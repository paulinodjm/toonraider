using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;

public class LaraCroft : MonoBehaviour, ICharacter<LaraCroft>
{
    #region Parameters

    /// <summary>
    /// The camera target
    /// </summary>
    public Transform CameraTarget;

    /// <summary>
    /// The walk speed
    /// </summary>
    public float WalkSpeed;

    /// <summary>
    /// The run speed
    /// </summary>
    public float RunSpeed;

    /// <summary>
    /// The rotation speed
    /// </summary>
    public float RotationSpeed;

    /// <summary>
    /// The vertical jump speed
    /// </summary>
	public float JumpSpeed;

    /// <summary>
    /// The horizontal acceleration when jumping
    /// </summary>
    public float JumpAccel;

    /// <summary>
    /// The gravity
    /// </summary>
	public float Gravity;

    /// <summary>
    /// The falling speed threshold (when landing hurts)
    /// </summary>
    public float FallingSpeed;

    /// <summary>
    /// The distance needed to pick up something
    /// </summary>
    public float PickupDistance;

    /// <summary>
    /// The player camera
    /// </summary>
	public Transform Camera { get; set; }

    /// <summary>
    /// The point to attach picked items
    /// </summary>
    public Transform HandItem;

    /// <summary>
    /// The inventory
    /// </summary>
    public Inventory Inventory { get; private set; }

    /// <summary>
    /// The player input
    /// </summary>
    public PlayerInput InputSettings;

    #endregion

    #region Components

    private CharacterController _characterController;

    private Animator _animator;

    #endregion

    #region Movement

    /// <summary>
    /// The current velocity
    /// </summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>
    /// Tell if the character is currently grounded
    /// </summary>
    public bool IsGrounded { get; private set; }

    #endregion

    #region Interactions

    private List<Interactible> _availableInteractibles = new List<Interactible>();

    /// <summary>
    /// The current interaction, if any
    /// </summary>
    private Interactible _currentInteractible;

    private Interaction _nearestInteraction;

    private Interaction _currentInteraction;

    #endregion

    #region Picking up

    /// <summary>
    /// Tell if lara can use an interaction
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public bool CanInteractWith(Interactible interaction)
    {
        if (_currentLogic == null) return false;

        return _currentLogic.CanInteractWith(interaction);
    }

    /// <summary>
    /// The item being hold
    /// </summary>
    public GameObject HoldItem { get; private set; }

    #endregion

    #region States

    /// <summary>
    /// The current state
    /// </summary>
    private CharacterLogic<LaraCroft> _currentLogic;

    /// <summary>
    /// The next state, if any
    /// </summary>
    private CharacterLogic<LaraCroft> _pendingLogic;

    /// <summary>
    /// The state used for move transitions (can be inactive)
    /// </summary>
    private LaraGroundTransition _transitionLogic;

    /// <summary>
    /// The state dictionnary, by type
    /// </summary>
    private Dictionary<Type, CharacterLogic<LaraCroft>> _logics = new Dictionary<Type,CharacterLogic<LaraCroft>>();
    
    private int _interactionCount;

    /// <summary>
    /// Jump to the next state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public T GotoState<T>() where T : CharacterLogic<LaraCroft>
    {
        _pendingLogic = _logics[typeof(T)];
        return (T)_pendingLogic;
    }

    /// <summary>
    /// Instanciate a new state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T SpawnLogic<T>() where T : CharacterLogic<LaraCroft>
    {
        var logic = gameObject.AddComponent<T>();
        _logics[typeof(T)] = logic;
        return logic;
    }

    /// <summary>
    /// Returns a state instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T GetState<T>() where T : CharacterLogic<LaraCroft>
    {
        return (T)_logics[typeof(T)];
    }

    #endregion

    void Awake()
    {
        if (!Application.isEditor) Screen.lockCursor = true;

        Inventory = GetComponent<Inventory>();

        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        SpawnLogic<LaraWalking>();
        SpawnLogic<LaraJumping>();
        SpawnLogic<LaraFalling>();
        _transitionLogic = SpawnLogic<LaraGroundTransition>();
        SpawnLogic<LaraInteracting>();

        GotoState<LaraWalking>();
    }

	void Update () 
	{
        if (Camera == null) print("pas de caméra");

        if (_pendingLogic)
        {
            _currentLogic = _pendingLogic;
            _pendingLogic = null;
        }

        InputSettings.ProcessInput();
        SortInteractions();

        ProcessMove();
        ProcessAction();
	}

    void OnGUI()
    {
        GUILayout.Label(Inventory.Items.Count() + " items dans l'inventaire");
        GUILayout.Label(_interactionCount + "/" + _availableInteractibles.Count);

        if (_nearestInteraction != null)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append(_nearestInteraction.IsAvailable ? " > " : " ? ");
            strBuilder.Append(_nearestInteraction.Caption);
            
            GUILayout.Label(strBuilder.ToString());
        }
    }

    #region Interactions

    /// <summary>
    /// Find the nearest interaction
    /// </summary>
    private void SortInteractions()
    {
        _nearestInteraction = null;

        var interactions = new List<Interaction>();

        for (var i = 0; i < _availableInteractibles.Count; i++)
        {
            var interactible = _availableInteractibles[i];
            if (!CanInteractWith(interactible)) continue;

            var interaction = interactible.GetInteractionFor(this);
            if (interaction == null) continue;

            interactions.Add(interaction);
            _nearestInteraction = interaction;
        }

        _interactionCount = interactions.Count;
    }

    /// <summary>
    /// Interact with the nearest interaction, if the player asks for it.
    /// </summary>
    private void ProcessAction()
    {
        if (!InputSettings.ButAction || _nearestInteraction == null) return;

        _currentInteraction = _nearestInteraction;
        _nearestInteraction = null;

        MakeMoveTransition(
            _currentInteraction.UsePosition,
            _currentInteraction.UseRotation,
            AnimateInteraction,
            true
        );
    }

    /// <summary>
    /// Parent a given item to the character hand.
    /// </summary>
    /// <param name="item"></param>
    private void GrabItem(GameObject item)
    {
        HoldItem = item;
        HoldItem.transform.parent = HandItem.transform;
        HoldItem.transform.localPosition = Vector3.zero;
        HoldItem.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Process the interaction, and plays the associated animation
    /// </summary>
    private void AnimateInteraction()
    {
        var triggerName = _currentInteraction.Use();
        if (triggerName != null)
        {
            _animator.SetTrigger(triggerName);
        }
        else
        {
            var interactionState = GetState<LaraInteracting>();
            interactionState.ForceQuit = true;

            AllowMove();
        }
    }

    #endregion

    #region Movements

    /// <summary>
    /// Return the two transform axis, with vector.y clamped to 0
    /// </summary>
    /// <param name="t"></param>
    /// <param name="forward"></param>
    /// <param name="right"></param>
    public void GetAxis(Transform t, out Vector3 forward, out Vector3 right)
    {
        if (t == null)
        {
            forward = transform.forward;
            right = transform.right;
            return;
        }

        forward = t.forward;
        forward.y = 0;

        right = t.right;
        right.y = 0;
    }

    /// <summary>
    /// Shortcut for <code>GetAxis(Camera, forward, right)</code>
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="right"></param>
    public void GetAxis(out Vector3 forward, out Vector3 right)
    {
        GetAxis(Camera, out forward, out right);
    }

    /// <summary>
    /// Process the move calculations regarding the inputs, and call ApplyMove to make the character moving.
    /// </summary>
    private void ProcessMove()
    {
        if (_currentLogic != null)
        {
            var move = _characterController.velocity;
            var rotation = transform.rotation;

            _currentLogic.CalcVelocityAndRotation(ref move, ref rotation);

            ApplyMove(move);
            transform.rotation = rotation;
        }

        AnimCharacterWalking();
    }

    /// <summary>
    /// Apply a movement to the character and check if it is grounded.
    /// Here comes all the inertia stuff.
    /// </summary>
    /// <param name="move">Move to apply</param>
    private void ApplyMove(Vector3 move)
    {
        var hits = _characterController.Move(move * Time.deltaTime);
        var isGrounded = (hits & CollisionFlags.Below) == CollisionFlags.Below;
        if (isGrounded != IsGrounded)
        {
            if (isGrounded)
            {
                _currentLogic.Landed();
            }
            else
            {
                _currentLogic.Falling();
            }
        }
        IsGrounded = isGrounded;

        if ((hits & CollisionFlags.Above) == CollisionFlags.Above)
        {
            move.y = -1f;
        }
        Velocity = move;
    }

    /// <summary>
    /// Plays the animation when walking (or jumping, or falling)
    /// </summary>
    private void AnimCharacterWalking() // <- this job has to be done by the current state
    {
        var velocity = Velocity;
        velocity.y = 0;

        _animator.SetBool("Move", velocity.magnitude > .0f);
        _animator.SetBool("Walk", InputSettings.ButWalk);
        _animator.SetBool("Fall", !IsGrounded);
    }

    /// <summary>
    /// Ask the character to move to a given point
    /// </summary>
    /// <param name="targetPosition">point to reach</param>
    /// <param name="targetRotation">target rotation</param>
    /// <param name="finalAction">action executed when the transition ends</param>
    /// <param name="run">true if the character should run, false to walk</param>
    private void MakeMoveTransition(Vector3 targetPosition, Quaternion targetRotation, Action finalAction, bool run = false)
    {
        _transitionLogic.InitializeTransition(targetPosition, targetRotation, finalAction, run);
        GotoState<LaraGroundTransition>();
    }

    #endregion

    #region Items triggering events

    void OnTriggerEnter(Collider other)
    {
        var interactible = (Interactible)other.gameObject;
        if (interactible != null)
        {
            _availableInteractibles.Add(interactible);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var interactible = (Interactible)other.gameObject;
        if (interactible != null)
        {
            _availableInteractibles.Remove(interactible);
        }
    }

    #endregion

    #region Animation events

    /// <summary>
    /// Called to play the foot step sound
    /// </summary>
    private void FootStep()
    {
        GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// Called when an animation needs to block the inputs
    /// </summary>
    public void FreezeMove()
    {
        Debug.LogWarning("Obsolete");
    }

    /// <summary>
    /// Called when an animation release the input lock.
    /// </summary>
    public void AllowMove()
    {
        GotoState<LaraWalking>();
    }

    /// <summary>
    /// Called when Lara takes an item on the ground
    /// </summary>
    private void PickupItem()
    {
        var pendingItem = _currentInteraction.Interactible as ItemPickable;

        pendingItem.NotifyUsed();
        GrabItem((GameObject)Instantiate(pendingItem.InventoryItem.Prefab));
    }

    /// <summary>
    /// Called when Lara drops an item from his hand to her bag.
    /// </summary>
    private void BagIn()
    {
        var pendingItem = _currentInteraction.Interactible as ItemPickable;

        Destroy(HoldItem);
        HoldItem = null;

        Inventory.Items.Add(pendingItem.InventoryItem);
        _currentInteraction = null;
    }

    /// <summary>
    /// Called when Lara takes an item in her bag.
    /// </summary>
    private void BagOut()
    {
        var pendingLock = (Lock)_currentInteraction.Interactible;
        var usingItem = Inventory.Items.Find(pendingLock.KeyName);

        GrabItem((GameObject)Instantiate(usingItem.Prefab));

        Inventory.Items.Remove(usingItem);
    }

    /// <summary>
    /// Called when Lara uses an item in her hand to unlock something.
    /// </summary>
    private void ItemUsed()
    {
        var nearestLock = (Lock)_currentInteraction.Interactible;

        HoldItem.transform.parent = nearestLock.KeyPosition;
        HoldItem.transform.localPosition = Vector3.zero;
        HoldItem.transform.localRotation = Quaternion.identity;
        HoldItem = null;

        nearestLock.NotifyUsed();
    }

    #endregion

}

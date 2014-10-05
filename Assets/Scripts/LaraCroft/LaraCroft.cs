using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;

public class LaraCroft : MonoBehaviour, ICharacter<LaraCroft>
{
    #region Parameters

    public Transform CameraTarget;

    public float WalkSpeed;
    public float RunSpeed;
    public float RotationSpeed;
	public float JumpSpeed;
    public float JumpAccel;
	public float Gravity;
    public float FallingSpeed;

    public float PickupDistance;

	public Transform Camera { get; set; }
    public Transform HandItem;

    public Inventory Inventory { get; private set; }

    public PlayerInput InputSettings;
    public PlayerInput PlayerInput 
    {
        get { return InputSettings; } 
    }

    #endregion
    #region Components

    private CharacterController _characterController;
    private Animator _animator;

    #endregion
    #region Movement

    public Vector3 Velocity { get; private set; }
    public bool IsGrounded { get; private set; }

    private bool _jump;

    #endregion
    #region Interactions

    private List<Interaction_Old> _availableInteractions = new List<Interaction_Old>();
    private Interaction_Old _nearestInteraction;
    private Interaction_Old _pendingInteraction;

    #endregion
    #region Picking up

    public bool CanInteractWith(Interaction_Old interaction)
    {
        if (_currentLogic == null) return false;

        return _currentLogic.CanInteractWith(interaction);
    }

    public GameObject HoldItem { get; private set; }

    #endregion
    #region States

    private CharacterLogic<LaraCroft> _currentLogic;
    private CharacterLogic<LaraCroft> _pendingLogic;
    private LaraGroundTransition _transitionLogic;
    private Dictionary<Type, CharacterLogic<LaraCroft>> _logics = new Dictionary<Type,CharacterLogic<LaraCroft>>();

    public void GotoState<T>() where T : CharacterLogic<LaraCroft>
    {
        _pendingLogic = _logics[typeof(T)];
    }

    private T SpawnLogic<T>() where T : CharacterLogic<LaraCroft>
    {
        var logic = gameObject.AddComponent<T>();
        _logics[typeof(T)] = logic;
        return logic;
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

        if (_nearestInteraction != null && CanInteractWith(_nearestInteraction))
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append((_nearestInteraction.GetStatusFor(this) == Interaction_Old.Status.Avalaible ? " > " : " ? "));
            strBuilder.Append(_nearestInteraction.Caption);
            GUILayout.Label(strBuilder.ToString());
        }
    }

    #region Interactions

    private void SortInteractions()
    {
        _nearestInteraction = null;

        for (var i = 0; i < _availableInteractions.Count; i++)
        {
            var interaction = _availableInteractions[i];

            if (interaction.GetStatusFor(this) == Interaction_Old.Status.Unavailable)
            {
                _availableInteractions.Remove(interaction);
            }
            else if (_nearestInteraction == null)
            {
                _nearestInteraction = interaction;
            }
        }
    }

    /// <summary>
    /// Interact with the nearest interaction, if the player asks for it.
    /// </summary>
    private void ProcessAction()
    {
        if (_nearestInteraction == null ||
            !CanInteractWith(_nearestInteraction) ||
            !InputSettings.ButAction
        ) return;

        if (_nearestInteraction.GetStatusFor(this) == Interaction_Old.Status.Incomplete)
        {
            print("I need something!");
            return;
        }

        _pendingInteraction = _nearestInteraction;

        Vector3 usePosition; Quaternion useRotation;
        _pendingInteraction.CalcUsePosRot(this, out usePosition, out useRotation);
        MakeMoveTransition(
            usePosition, useRotation,
            () => _animator.SetTrigger(_pendingInteraction.GetActionName(this)),
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
    private void AnimCharacterWalking()
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
        var interaction = (Interaction_Old)other.gameObject;
        if (interaction != null)
        {
            _availableInteractions.Add(interaction);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var interaction = (Interaction_Old)other.gameObject;
        if (interaction != null)
        {
            _availableInteractions.Remove(interaction);
        }
    }

    #endregion
    #region Animation events

    /// <summary>
    /// Called to play the foot step sound
    /// </summary>
    private void FootStep()
    {
        audio.Play();
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
        var pendingItem = _pendingInteraction as ItemPickable;

        pendingItem.NotifyUsed();
        GrabItem((GameObject)Instantiate(pendingItem.InventoryItem.Prefab));
    }

    /// <summary>
    /// Called when Lara drops an item from his hand to her bag.
    /// </summary>
    private void BagIn()
    {
        var pendingItem = _pendingInteraction as ItemPickable;

        Destroy(HoldItem);
        HoldItem = null;

        Inventory.Items.Add(pendingItem.InventoryItem);
        _pendingInteraction = null;
    }

    /// <summary>
    /// Called when Lara takes an item in her bag.
    /// </summary>
    private void BagOut()
    {
        var pendingLock = _pendingInteraction as Lock;
        var usingItem = Inventory.Items.Find(pendingLock.KeyName);

        GrabItem((GameObject)Instantiate(usingItem.Prefab));

        Inventory.Items.Remove(usingItem);
    }

    /// <summary>
    /// Called when Lara uses an item in her hand to unlock something.
    /// </summary>
    private void ItemUsed()
    {
        var nearestLock = _pendingInteraction as Lock;

        HoldItem.transform.parent = nearestLock.KeyPosition;
        HoldItem.transform.localPosition = Vector3.zero;
        HoldItem.transform.localRotation = Quaternion.identity;
        HoldItem = null;

        _pendingInteraction.NotifyUsed();
        _pendingInteraction = null;
    }

    #endregion

}

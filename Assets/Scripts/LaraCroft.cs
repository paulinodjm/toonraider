﻿using System.Security.Cryptography.X509Certificates;
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

    public bool InputFrozen { get; set; }
    public Vector3 Velocity { get; private set; }
    public bool IsGrounded { get; private set; }

    private MoveTransition _moveTransition;
    private bool _jump;

    #endregion
    #region Interactions

    private List<Interaction> _availableInteractions = new List<Interaction>();
    private Interaction _nearestInteraction;
    private Interaction _pendingInteraction;

    #endregion
    #region Picking up

    public bool CanInteract
    {
        get
        {
            return !InputFrozen && IsGrounded;
        }
    }

    public GameObject HoldItem { get; private set; }

    #endregion
    #region States

    private CharacterLogic<LaraCroft> _currentLogic;
    private CharacterLogic<LaraCroft> _pendingLogic;
    private Dictionary<Type, CharacterLogic<LaraCroft>> _logics = new Dictionary<Type,CharacterLogic<LaraCroft>>();

    public void GotoState<T>() where T : CharacterLogic<LaraCroft>
    {
        _pendingLogic = _logics[typeof(T)];
    }

    private void SpawnLogic<T>() where T : CharacterLogic<LaraCroft>
    {
        _logics[typeof(T)] = gameObject.AddComponent<T>();
    }

    #endregion

    void Awake()
    {
        if (!Application.isEditor) Screen.lockCursor = true;

        Inventory = GetComponent<Inventory>();

        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        SpawnLogic<CharacterWalking>();
        SpawnLogic<CharacterJumping>();
        SpawnLogic<CharacterFalling>();
        GotoState<CharacterWalking>();
    }

	void Update () 
	{
        if (Camera == null) print("pas de caméra");

        if (_pendingLogic)
        {
            _currentLogic = _pendingLogic;
            _pendingLogic = null;
        }

        if (_moveTransition != null)
        {
            ProcessMoveTransition();
            return;
        }

        InputSettings.ProcessInput();
        SortInteractions();

        if (!InputFrozen)
        {
            ProcessMove();
            ProcessAction();
        }
	}

    void OnGUI()
    {
        GUILayout.Label(Inventory.Items.Count() + " items dans l'inventaire");

        if (_nearestInteraction != null && CanInteract)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append((_nearestInteraction.GetStatusFor(this) == Interaction.Status.Avalaible ? " > " : " ? "));
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

            if (interaction.GetStatusFor(this) == Interaction.Status.Unavailable)
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
            !CanInteract ||
            !InputSettings.ButAction
        ) return;

        if (_nearestInteraction.GetStatusFor(this) == Interaction.Status.Incomplete)
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
        InputFrozen = true;
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

            _currentLogic.PerformMove(ref move, ref rotation);

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
        var moveSpeed = run ? RunSpeed : WalkSpeed;
        _moveTransition = new MoveTransition
        {
            TargetPosition = targetPosition,
            TargetRotation = targetRotation,
            RemainingTime = Vector3.Distance(transform.position, targetPosition) / moveSpeed,
            Action = finalAction,
            Run = run
        };
    }

    /// <summary>
    /// Move linearly the character to a given point
    /// </summary>
    private void ProcessMoveTransition()
    {
        var position = Vector3.Lerp(transform.position, _moveTransition.TargetPosition, Time.deltaTime / _moveTransition.RemainingTime);
        var delta = position - transform.position;
        _characterController.Move(delta);
        transform.rotation = Quaternion.Lerp(transform.rotation, _moveTransition.TargetRotation, Time.deltaTime / _moveTransition.RemainingTime);

        _moveTransition.RemainingTime -= Time.deltaTime;

        if (_moveTransition.RemainingTime <= .0f)
        {
            _animator.SetBool("Move", false);
            if (_moveTransition.Action != null) _moveTransition.Action();
            _moveTransition = null;
        }
        else
        {
            _animator.SetBool("Move", true);
            _animator.SetBool("Walk", !_moveTransition.Run);
        }
    }

    #endregion
    #region Items triggering events

    void OnTriggerEnter(Collider other)
    {
        var interaction = (Interaction)other.gameObject;
        if (interaction != null)
        {
            _availableInteractions.Add(interaction);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var interaction = (Interaction)other.gameObject;
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
        InputFrozen = true;
    }

    /// <summary>
    /// Called when an animation release the input lock.
    /// </summary>
    public void AllowMove()
    {
        InputFrozen = false;
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

public class MoveTransition
{
    public Vector3 TargetPosition;
    public Quaternion TargetRotation;
    public float RemainingTime;
    public Action Action;
    public bool Run;
}

public class CharacterWalking : CharacterLogic<LaraCroft>
{
    public override void PerformMove(ref Vector3 velocity, ref Quaternion rotation)
    {
        var inputDir = ProcessInput();
        velocity = CalcVelocity(inputDir, velocity);
        rotation = CalcRotation(velocity, rotation);
        ApplyGravity(ref velocity);
    }

    protected virtual Vector3 ProcessInput()
    {
        Vector3 forward, right;
        Character.GetAxis(out forward, out right);

        forward *= Character.InputSettings.MoveDirection.z;
        right *= Character.InputSettings.MoveDirection.x;
        return (forward + right).normalized;
    }

    protected virtual Vector3 CalcVelocity(Vector3 inputDir, Vector3 velocity)
    {
        var moveSpeed = Character.InputSettings.ButWalk ? Character.WalkSpeed : Character.RunSpeed;
        return inputDir * moveSpeed;
    }

    protected virtual Quaternion CalcRotation(Vector3 velocity, Quaternion rotation)
    {
        velocity.y = 0;
        var desiredRotation = (velocity.magnitude != 0.0f) ? Quaternion.LookRotation(velocity) : rotation;
        return Quaternion.Lerp(rotation, desiredRotation, Character.RotationSpeed);
    }

    protected virtual void ApplyGravity(ref Vector3 move)
    {
        if (Character.InputSettings.ButJump)
        {
            move.y = Character.JumpSpeed;
        }
        else
        {
            move.y = -5;
        }
    }

    public override void Falling()
    {
        GotoState<CharacterJumping>();
    }
}

public class CharacterJumping : CharacterWalking
{
    protected override Vector3 CalcVelocity(Vector3 inputDir, Vector3 velocity)
    {
        var maxSpeed = Character.RunSpeed;
        if (inputDir.x == 0 && inputDir.y == 0)
        {
            maxSpeed = velocity.magnitude - (Character.JumpAccel * Time.deltaTime);
            if (maxSpeed < .0f)
                maxSpeed = .0f;
        }

        var move = velocity + (inputDir * Character.JumpAccel * Time.deltaTime);
        move.y = 0;
        move = Vector3.ClampMagnitude(move, maxSpeed);
        move.y = velocity.y;
        return move;

        /*
        var move = velocity + (inputDir * Player.JumpAccel * Time.deltaTime);
        move.y = 0;
        if (move.magnitude > Player.RunSpeed)
        {
            move = Vector3.ClampMagnitude(move, Player.RunSpeed);
        }
        move.y = velocity.y;
        return move;
        //*/
    }

    protected override void ApplyGravity(ref Vector3 move)
    {
        move.y -= Character.Gravity * Time.deltaTime;
        if (move.y <= -Character.FallingSpeed)
            GotoState<CharacterFalling>();
    }

    public override void Landed()
    {
        GotoState<CharacterWalking>();
    }
}

public class CharacterFalling : CharacterJumping
{
    public override void Landed()
    {
        base.Landed();
        print("Ouch!");
    }

    protected override void ApplyGravity(ref Vector3 move)
    {
        move.y -= Character.Gravity * Time.deltaTime;
    }
}
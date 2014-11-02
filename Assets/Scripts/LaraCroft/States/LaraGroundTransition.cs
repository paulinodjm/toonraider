using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Logic when lara tries to reach automatically a given point
/// </summary>
public class LaraGroundTransition : LaraWalking 
{
    /// <summary>
    /// The position to reach
    /// </summary>
    public Vector3 TargetPosition { get; private set; }

    /// <summary>
    /// The rotation to reach
    /// </summary>
    public Quaternion TargetRotation { get; private set; }

    /// <summary>
    /// The remaining time to reach the point
    /// </summary>
    public float RemainingTime { get; private set; }

    /// <summary>
    /// The action to execute when the point is reached
    /// </summary>
    public Action Action { get; private set; }

    /// <summary>
    /// Tell if lara should run or not during the transition
    /// </summary>
    public bool Run { get; private set; }

    private Animator _animator;

    /// <summary>
    /// Initialize the transition
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="targetRotation"></param>
    /// <param name="finalAction"></param>
    /// <param name="run"></param>
    public void InitializeTransition(Vector3 targetPosition, Quaternion targetRotation, Action finalAction, bool run = false)
    {
        var moveSpeed = run ? Character.RunSpeed : Character.WalkSpeed;

        TargetPosition = targetPosition;
        TargetRotation = targetRotation;
        RemainingTime = Vector3.Distance(transform.position, targetPosition) / moveSpeed;
        Action = finalAction;
        Run = run;
    }

    public override void CalcVelocityAndRotation(ref Vector3 velocity, ref Quaternion rotation)
    {
        base.CalcVelocityAndRotation(ref velocity, ref rotation);

        RemainingTime -= Time.deltaTime;

        if (RemainingTime <= .0f)
        {
            _animator.SetBool("Move", false);
            if (Action != null)
            {
                Action();
                Action = null;
            }
            
            GotoState<LaraInteracting>(); // <- remove this for a softer implementation
        }
        else
        {
            _animator.SetBool("Move", true);
            _animator.SetBool("Walk", !Run);
        }
    }

    public override bool CanInteractWith(Interactible interaction)
    {
        return false;
    }

    public override bool WantJump
    {
        get
        {
            return false;
        }
    }

    protected override Vector3 CalcVelocity(Vector3 inputDir, Vector3 velocity)
    {
        var position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime / RemainingTime);
        return (position - transform.position) / Time.deltaTime;
    }

    protected override Quaternion CalcRotation(Vector3 velocity, Quaternion rotation)
    {
        return Quaternion.Lerp(transform.rotation, TargetRotation, Time.deltaTime / RemainingTime);
    }

    public override void Falling()
    {
        // n'appelle pas base.Falling() car provoque la chute.
    }

    protected override void ApplyGravity(ref Vector3 move)
    {
        // rien à faire
    }

    protected new void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();
    }
}

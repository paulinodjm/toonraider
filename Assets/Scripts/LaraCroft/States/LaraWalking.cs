using UnityEngine;
using System.Collections;

/// <summary>
/// Base logic when grounded (idle, walking, running)
/// </summary>
public class LaraWalking : CharacterLogic<LaraCroft>
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
        GotoState<LaraJumping>();
    }
}
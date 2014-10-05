using UnityEngine;
using System.Collections;

/// <summary>
/// Logic when jumping
/// </summary>
public class LaraJumping : LaraWalking
{
    public override bool CanInteractWith(Interactible interaction)
    {
        var ledge = interaction as Ledge;
        return ledge != null;
    }

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
    }

    protected override void ApplyGravity(ref Vector3 move)
    {
        move.y -= Character.Gravity * Time.deltaTime;
        if (move.y <= -Character.FallingSpeed)
            GotoState<LaraFalling>();
    }

    public override void Landed()
    {
        GotoState<LaraWalking>();
    }
}
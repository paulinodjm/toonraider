using UnityEngine;
using System.Collections;

/// <summary>
/// Logic when falling (landing hurts)
/// </summary>
public class LaraFalling : LaraJumping
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

    public override bool CanInteractWith(Interactible interaction)
    {
        return false;
    }
}
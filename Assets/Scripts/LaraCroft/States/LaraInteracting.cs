using UnityEngine;
using System.Collections;

/// <summary>
/// Logic when interacting with something (should not move)
/// </summary>
public class LaraInteracting : LaraWalking 
{
    /// <summary>
    /// Force the state interruption, even if the animation has raised no event
    /// </summary>
    public bool ForceQuit { get; set; }

    public override void CalcVelocityAndRotation(ref Vector3 velocity, ref Quaternion rotation)
    {
        if (ForceQuit)
        {
            GotoState<LaraWalking>();
            ForceQuit = false;
        }

        base.CalcVelocityAndRotation(ref velocity, ref rotation);
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
        return Vector3.zero;
    }

    protected override Quaternion CalcRotation(Vector3 velocity, Quaternion rotation)
    {
        return rotation;
    }
}

﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Logic when interacting with something (should not move)
/// </summary>
public class LaraInteracting : LaraWalking 
{
    protected override Vector3 CalcVelocity(Vector3 inputDir, Vector3 velocity)
    {
        return Vector3.zero;
    }

    protected override Quaternion CalcRotation(Vector3 velocity, Quaternion rotation)
    {
        return rotation;
    }
}

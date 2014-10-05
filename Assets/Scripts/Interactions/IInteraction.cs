using UnityEngine;
using System.Collections;

/// <summary>
/// Base interface for each possible interaction
/// </summary>
public interface IInteraction
{
    /// <summary>
    /// Position to reach to interact
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// Rotation to reach to interact
    /// </summary>
    Quaternion Rotation { get; }
}
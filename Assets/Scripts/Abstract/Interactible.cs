using System;
using UnityEngine;

/// <summary>
/// Base class for each object that can be used by the player
/// </summary>
public abstract class Interactible : MonoBehaviour
{
    /// <summary>
    /// Returns the current interaction for this user
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>The interaction, or null if no available</returns>
    public abstract Interaction GetInteractionFor(LaraCroft user);

    /// <summary>
    /// Cast a GameObject into an Interaction, if possible
    /// </summary>
    /// <param name="obj">GameObject to cast</param>
    /// <returns>The interactible</returns>
    public static explicit operator Interactible(GameObject obj)
    {
        return obj.GetComponent<Interactible>();
    }

    /// <summary>
    /// Notify the interactible that it has been used
    /// TODO : Move this to Interaction
    /// </summary>
    public virtual void NotifyUsed() { }
}
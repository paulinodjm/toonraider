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

    #region Old Interface (obsolete)

    [Obsolete()]
    public abstract string Caption { get; }

    /// <summary>
    /// Returns the nearest position to interact with gameObject
    /// </summary>
    [Obsolete()]
    public abstract void CalcUsePosRot(LaraCroft laraCroft, out Vector3 position, out Quaternion rotation);

    /// <summary>
    /// Return true if the player can use this interaction
    /// </summary>
    /// <param name="laraCroft"></param>
    /// <returns></returns>
    [Obsolete()]
    public virtual Status GetStatusFor(LaraCroft laraCroft)
    {
        return gameObject.activeInHierarchy ? Status.Avalaible : Status.Unavailable;
    }

    [Obsolete()]
    public abstract string GetActionName(LaraCroft laraCroft);

    [Obsolete()]
    public virtual void NotifyUsed() { }

    [Obsolete()]
    public enum Status
    {
        Avalaible,
        Incomplete,
        Unavailable,
    }

    #endregion
}
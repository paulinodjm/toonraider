using UnityEngine;

public abstract class Interaction : MonoBehaviour
{
    public abstract string Caption { get; }

    /// <summary>
    /// Returns the nearest position to interact with gameObject
    /// </summary>
    public abstract void CalcUsePosRot(LaraCroft laraCroft, out Vector3 position, out Quaternion rotation);

    /// <summary>
    /// Return true if the player can use this interaction
    /// </summary>
    /// <param name="laraCroft"></param>
    /// <returns></returns>
    public virtual Status GetStatusFor(LaraCroft laraCroft)
    {
        return gameObject.activeInHierarchy ? Status.Avalaible : Status.Unavailable;
    }

    public abstract string GetActionName(LaraCroft laraCroft);

    public virtual void NotifyUsed() { }

    /// <summary>
    /// Cast a GameObject into an Interaction, if possible
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static explicit operator Interaction(GameObject obj)
    {
        return obj.GetComponent<Interaction>();
    }


    public enum Status
    {
        Avalaible,
        Incomplete,
        Unavailable,
    }
}
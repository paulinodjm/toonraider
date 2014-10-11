using UnityEngine;

/// <summary>
/// Base class for each type of interaction
/// </summary>
public abstract class Interaction
{
    /// <summary>
    /// Lara Object that can use this interaction
    /// </summary>
    public LaraCroft User { get; private set; }

    /// <summary>
    /// Interactible object related to this interaction
    /// </summary>
    public Interactible Interactible { get; private set; }

    /// <summary>
    /// Caption that can be displayed on the screen
    /// </summary>
    public abstract string Caption { get; }

    /// <summary>
    /// Tells if the interaction is currently available
    /// </summary>
    public abstract bool IsAvailable { get; }

    /// <summary>
    /// The position to reach to use this interaction
    /// </summary>
    public abstract Vector3 UsePosition { get; }

    /// <summary>
    /// The rotation to reach to use this interaction
    /// </summary>
    public abstract Quaternion UseRotation { get; }

    /// <summary>
    /// Creates a new interaction
    /// </summary>
    /// <param name="user"></param>
    /// <param name="interactible"></param>
    public Interaction(LaraCroft user, Interactible interactible)
    {
        User = user;
        Interactible = interactible;
    }

    /// <summary>
    /// Use this interaction
    /// </summary>
    public abstract string Use();
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Base interface for each interactible object
/// </summary>
public interface IIteractible
{
    /// <summary>
    /// Returns the interaction object (can be null)
    /// </summary>
    /// <param name="lara"></param>
    /// <returns></returns>
    IInteraction GetInteraction(LaraCroft lara);
}
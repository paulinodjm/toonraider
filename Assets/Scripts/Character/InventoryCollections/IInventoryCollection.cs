using UnityEngine;
using System.Collections.Generic;

public interface IInventoryCollection<T>
{
    /// <summary>
    /// Add a new element
    /// </summary>
    void Add(T elt);

    /// <summary>
    /// Remove an old element
    /// </summary>
    void Remove(T elt);

    /// <summary>
    /// Returns the item count
    /// </summary>
    int Count();

    /// <summary>
    /// Find a given Item
    /// </summary>
    T Find(string eltName);

    /// <summary>
    /// Count the number of occurences of a given item
    /// </summary>
    /// <param name="eltName"></param>
    /// <returns></returns>
    int Count(string eltName);
}
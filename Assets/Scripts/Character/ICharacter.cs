using UnityEngine;

/// <summary>
/// Basic interface for a character type, which implements a machine-state system.
/// </summary>
public interface ICharacter<Character_T> where Character_T : Component, ICharacter<Character_T>
{
    /// <summary>
    /// Ask the player to go to a given state.
    /// </summary>
    State_T GotoState<State_T>() where State_T : CharacterLogic<Character_T>;
}
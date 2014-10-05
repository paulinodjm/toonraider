using UnityEngine;
using System.Collections;

public abstract class CharacterLogic<Character_T> : MonoBehaviour, ICharacter<Character_T> 
    where Character_T : Component, ICharacter<Character_T>
{
    /// <summary>
    /// Player that the CharacterLogic belongs to.
    /// </summary>
    public Character_T Character { get; private set; }

    /// <summary>
    /// Tell if the character can interact with the given interaction
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public abstract bool CanInteractWith(Interaction_Old interaction);

    protected void Awake()
    {
        Character = GetComponent<Character_T>();
    }

    public void GotoState<State_T>() where State_T : CharacterLogic<Character_T>
    {
        Character.GotoState<State_T>();
    }

    /// <summary>
    /// Called once per frame to set the news character velocity and rotation 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="rotation"></param>
    public abstract void CalcVelocityAndRotation(ref Vector3 velocity, ref Quaternion rotation);

    /// <summary>
    /// Called once when the character touchs the ground
    /// </summary>
    public virtual void Landed() 
    {
        // nothing to do
    }

    /// <summary>
    /// Called once when the character leaves the ground
    /// </summary>
    public virtual void Falling() 
    {
        // nothing to do
    }
}

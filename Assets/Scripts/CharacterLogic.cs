using UnityEngine;
using System.Collections;

public abstract class CharacterLogic<Character_T> : MonoBehaviour, ICharacter<Character_T> 
    where Character_T : Component, ICharacter<Character_T>
{
    /// <summary>
    /// Player that the CharacterLogic belongs to.
    /// </summary>
    public Character_T Character { get; private set; }

    protected void Awake()
    {
        Character = GetComponent<Character_T>();
    }

    public abstract void PerformMove(ref Vector3 velocity, ref Quaternion rotation);

    public void GotoState<State_T>() where State_T : CharacterLogic<Character_T>
    {
        Character.GotoState<State_T>();
    }

    public virtual void Landed() { }
    public virtual void Falling() { }
}

using UnityEngine;

public class Lock : Interactible 
{
    public bool IsUsed;

    public string KeyName;

    public Transform KeyPosition;

    public Transform InteractPoint;

    public void Using()
    {
        audio.Play();
    }

    public override void NotifyUsed()
    {
        IsUsed = true;
        Using();
    }

    public override Interaction GetInteractionFor(LaraCroft user)
    {
        if (IsUsed) return null;

        var hasKey = user.Inventory.Items.Find(KeyName) != null;
        var interaction = (Interaction)null;
        
        if (hasKey)
        {
            interaction = new UseKey(user, this);
        }
        else
        {
            interaction = new MissingItem(user, this);
        }

        return interaction;
    }
}

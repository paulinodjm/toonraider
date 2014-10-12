using UnityEngine;

public class Lock : Interactible 
{
    public bool IsUsed;
    public string KeyName;

    public Transform KeyPosition;
    public Transform InteractPoint;

    public override string Caption
    {
        get { return "Utiliser " + KeyName; }
    }

    public void Using()
    {
        audio.Play();
    }

    public override void CalcUsePosRot(LaraCroft laraCroft, out Vector3 position, out Quaternion rotation)
    {
        position = InteractPoint.position;
        rotation = InteractPoint.rotation;
    }

    public override Interactible.Status GetStatusFor(LaraCroft laraCroft)
    {
        if (base.GetStatusFor(laraCroft) == Status.Unavailable || IsUsed)
        {
            return Status.Unavailable;
        }
        else
        {
            return laraCroft.Inventory.Items.Find(KeyName) != null ? Status.Avalaible : Status.Incomplete;
        }
    }

    public override string GetActionName(LaraCroft laraCroft)
    {
        return "UseKey";
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

using UnityEngine;
using System.Collections;

public class Lock : Interaction_Old 
{
    public bool Used;
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

    public override Interaction_Old.Status GetStatusFor(LaraCroft laraCroft)
    {
        if (base.GetStatusFor(laraCroft) == Status.Unavailable || Used)
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
        Used = true;
        Using();
    }
}

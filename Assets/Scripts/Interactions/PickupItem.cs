using UnityEngine;
using System.Collections;

public class PickupItem : Interaction 
{
    public PickupItem(LaraCroft user, Interactible interactible) :
        base(user, interactible)
    {
        // nothing to do
    }

    public override string Caption
    {
        get { return "Ramasser"; }
    }

    public override bool IsAvailable
    {
        get { return true; }
    }

    public override Vector3 UsePosition
    {
        get { return User.transform.position; }
    }

    public override Quaternion UseRotation
    {
        get { return User.transform.rotation; }
    }

    public override string Use()
    {
        return "PickupGround";
    }
}

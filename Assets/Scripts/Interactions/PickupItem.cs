using UnityEngine;
using System.Collections;

public class PickupItem : Interaction 
{
    public PickupItem(LaraCroft user, Interactible interactible) :
        base(user, interactible)
    {
        var transform = interactible.transform;

        var useDirection = (transform.position - user.transform.position).normalized;
        _usePosition = transform.position - useDirection * user.PickupDistance;

        useDirection.y = 0;
        _useRotation = Quaternion.LookRotation(useDirection);
    }

    public override string Caption
    {
        get { return "Ramasser"; }
    }

    public override bool IsAvailable
    {
        get { return true; }
    }

    private Vector3 _usePosition;

    public override Vector3 UsePosition 
    {
        get { return _usePosition; }
    }

    private Quaternion _useRotation;

    public override Quaternion UseRotation 
    {
        get { return _useRotation; }
    }

    public override string Use()
    {
        return "PickupGround";
    }
}

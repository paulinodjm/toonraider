using UnityEngine;
using System.Collections;

/// <summary>
/// Item dropped in the world that can be picked by the character.
/// </summary>
[AddComponentMenu("TombRaider/ItemPickable")]
public class ItemPickable : Interactible 
{
    public Item InventoryItem;

    public override string Caption
    {
        get { return "Ramasser " + InventoryItem.Name; }
    }

    public override void CalcUsePosRot(LaraCroft laraCroft, out Vector3 position, out Quaternion rotation)
    {
        var useDirection = (transform.position - laraCroft.transform.position).normalized;
        position = transform.position - useDirection * laraCroft.PickupDistance;

        useDirection.y = 0;
        rotation = Quaternion.LookRotation(useDirection);
    }

    public override string GetActionName(LaraCroft laraCroft)
    {
        return "PickupGround";
    }

    public override void NotifyUsed()
    {
        gameObject.SetActive(false);
    }

    public static explicit operator ItemPickable(GameObject gameObject)
    {
        return gameObject.GetComponent<ItemPickable>();
    }
}

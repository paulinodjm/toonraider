using UnityEngine;
using System.Collections;

/// <summary>
/// Item dropped in the world that can be picked by the character.
/// </summary>
[AddComponentMenu("TombRaider/ItemPickable")]
public class ItemPickable : Interactible 
{
    public Item InventoryItem;

    public GameObject VisibleItem;

    public bool IsPickable 
    { 
        get
        {
            return VisibleItem.activeSelf;
        }
        set
        {
            VisibleItem.SetActive(value);
        }
    }

    public override void NotifyUsed()
    {
        IsPickable = false; 
    }

    public static explicit operator ItemPickable(GameObject gameObject)
    {
        return gameObject.GetComponent<ItemPickable>();
    }

    public override Interaction GetInteractionFor(LaraCroft user)
    {
        return IsPickable ? new PickupItem(user, this) : null;
    }
}

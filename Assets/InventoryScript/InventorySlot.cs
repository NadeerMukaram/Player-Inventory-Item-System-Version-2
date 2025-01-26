using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Get the dragged item
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        // Get the DraggableItem component
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
        if (draggableItem == null) return;

        // Check if the slot is empty or contains items with the same name
        if (CanDropItem(dropped))
        {
            // Set the parent of the dragged item to this slot
            draggableItem.parentAfterDrag = transform;

        }
    }

    // Check if the item can be dropped into this slot
    private bool CanDropItem(GameObject droppedItem)
    {
        // If the slot is empty, allow the drop
        if (transform.childCount == 0)
        {
            return true;
        }

        // Get the name of the dropped item
        string droppedItemName = droppedItem.name;

        // Check if all items in the slot have the same name as the dropped item
        foreach (Transform child in transform)
        {
            if (child.name != droppedItemName)
            {
                Debug.Log("Cannot drop item: Items in the slot must have the same name.");
                return false;
            }
        }

        return true;
    }

}
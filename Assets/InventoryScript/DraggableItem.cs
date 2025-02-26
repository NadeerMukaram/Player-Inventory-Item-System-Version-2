using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;
    [HideInInspector] public Transform parentAfterDrag;

    private List<Transform> childClones = new List<Transform>(); // List to store child objects with "(Clone)" in their names

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;

        // Find all child objects with "(Clone)" in their names
        FindAllChildClones();

        // Set all child clones to follow the parent's drag behavior
        foreach (var child in childClones)
        {
            child.SetParent(transform.root); // Set parent to root during drag
            child.SetAsLastSibling(); // Ensure it renders on top
            child.GetComponent<Image>().raycastTarget = false; // Disable raycast target for dragging
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;

        // Move all child clones with the parent
        foreach (var child in childClones)
        {
            child.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;

        // Reset all child clones to their original parent
        foreach (var child in childClones)
        {
            child.SetParent(parentAfterDrag); // Reset parent to the original slot
            child.GetComponent<Image>().raycastTarget = true; // Re-enable raycast target
        }

        childClones.Clear(); // Clear the list for the next drag operation
    }

    private void FindAllChildClones()
    {
        childClones.Clear(); // Clear the list before populating it
        foreach (Transform child in parentAfterDrag)
        {
            if (child.name.Contains("(Clone)"))
            {
                childClones.Add(child); // Add child objects with "(Clone)" in their names
            }
        }
    }
}

using UnityEngine;
using TMPro;

public class ItemCount : MonoBehaviour
{
    public Transform targetSlot; 
    public TextMeshProUGUI countText;

    void Update()
    {

        int itemCount = targetSlot.childCount;

        // Update the TextMeshProUGUI with the current count
        if (itemCount > 0)
        {
            countText.text = itemCount.ToString(); 
        }
        else
        {
            countText.text = ""; 
        }
    }
}
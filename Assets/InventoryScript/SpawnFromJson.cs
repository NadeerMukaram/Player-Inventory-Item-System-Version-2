using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;

public class SpawnFromJson : MonoBehaviour
{
    [SerializeField] private List<RectTransform> uibuttonPrefab;
    [SerializeField] private List<Transform> slots;

    private ItemsEffects effects;

    [SerializeField] private TextMeshProUGUI saveIndicator; // Add this variable
    [SerializeField] private Button saveData; // New public Button variable

    void Start()
    {
        // Assign the method to the saveData button click event
        saveData.onClick.AddListener(SaveDataButtonClick);

        effects = FindObjectOfType<ItemsEffects>();

        string filePath = Path.Combine(Application.persistentDataPath, "spawnedPrefabNames.json");

        // Check if the JSON file exists
        if (File.Exists(filePath))
        {
            // Read the JSON file content
            string jsonContent = File.ReadAllText(filePath);

            // Deserialize the JSON data
            SlotData slotData = JsonUtility.FromJson<SlotData>(jsonContent);

            // Check if the deserialization was successful
            if (slotData != null && slotData.Slots != null)
            {
                // Iterate through the slots in the JSON data
                for (int slotIndex = 0; slotIndex < slotData.Slots.Count; slotIndex++)
                {
                    List<string> itemNamesInSlot = slotData.Slots[slotIndex].ItemNames;

                    // Iterate through the item names in the current slot
                    foreach (string prefabName in itemNamesInSlot)
                    {
                        if (prefabName == "Empty")
                        {
                            // Handle Empty slot (skip instantiation or perform specific logic)
                            continue;
                        }

                        RectTransform prefabToSpawn = uibuttonPrefab.Find(prefab => prefab.name == prefabName);

                        if (prefabToSpawn != null && slotIndex < slots.Count)
                        {
                            // Instantiate the prefab in the current slot
                            RectTransform instantiatedPrefab = Instantiate(prefabToSpawn, slots[slotIndex].position, Quaternion.identity, slots[slotIndex]);

                            // Get the Button component from the instantiated prefab
                            Button button = instantiatedPrefab.GetComponent<Button>();

                            if (button != null)
                            {
                                // Add a listener to the button to handle the delete action
                                button.onClick.AddListener(() => OnPrefabButtonClick(prefabName, filePath));
                                button.onClick.AddListener(() => DeletePrefab(instantiatedPrefab, prefabName, filePath));
                            }
                            else
                            {
                                Debug.LogWarning($"Prefab with name '{prefabName}' is missing a Button component.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Prefab with name '{prefabName}' not found or not enough slots.");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to deserialize JSON data.");
            }
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + filePath);
            InitializeEmptyJsonData(); // Initialize with empty slots if the file doesn't exist
        }
    }

    private void InitializeEmptyJsonData()
    {
        SlotData emptySlotData = new SlotData
        {
            Slots = new List<SlotItem>()
        };

        for (int i = 0; i < slots.Count; i++)
        {
            emptySlotData.Slots.Add(new SlotItem { ItemNames = new List<string> { "Empty" } });
        }

        string filePath = Path.Combine(Application.persistentDataPath, "spawnedPrefabNames.json");
        string jsonData = JsonUtility.ToJson(emptySlotData);

        File.WriteAllText(filePath, jsonData);
    }

    public void SaveDataButtonClick()
    {
        // Load existing JSON data
        string filePath = Path.Combine(Application.persistentDataPath, "spawnedPrefabNames.json");
        string jsonContent = File.ReadAllText(filePath);
        SlotData slotData = JsonUtility.FromJson<SlotData>(jsonContent);

        // If slotData is null, initialize it
        if (slotData == null)
        {
            slotData = new SlotData();
        }

        // Iterate through slots
        for (int i = 0; i < slots.Count; i++)
        {
            Transform currentSlot = slots[i];
            SlotItem slotItem = new SlotItem { ItemNames = new List<string>() };

            // Iterate through the child objects of the slot
            foreach (Transform child in currentSlot)
            {
                RectTransform prefabInSlot = child.GetComponent<RectTransform>();

                if (prefabInSlot != null)
                {
                    // Get the name of the prefab
                    string prefabName = prefabInSlot.name.Replace("(Clone)", "").Trim();

                    // Add the prefab name to the slot's item list
                    slotItem.ItemNames.Add(prefabName);
                }
            }

            // If no prefab is found, mark the slot as "Empty"
            if (slotItem.ItemNames.Count == 0)
            {
                slotItem.ItemNames.Add("Empty");
            }

            // Update the slot data
            if (slotData.Slots.Count > i)
            {
                slotData.Slots[i] = slotItem;
            }
            else
            {
                slotData.Slots.Add(slotItem);
            }
        }

        // Trim the slotData.Slots list to match the number of slots
        slotData.Slots = slotData.Slots.Take(slots.Count).ToList();

        // Convert slotData to JSON and save it to the file
        string jsonData = JsonUtility.ToJson(slotData);
        File.WriteAllText(filePath, jsonData);

        // Display the save indicator
        if (saveIndicator != null)
        {
            saveIndicator.text = "Data Saved!";
        }
    }

    private void OnPrefabButtonClick(string prefabName, string jsonFilePath)
    {
        // Call the HandleButtonClick method from PrefabButtonClickHandler
        effects.HandleButtonClick(prefabName);

        // Remove the prefab name from the JSON data
        RemovePrefabNameFromJson(prefabName, jsonFilePath);
    }

    // Method to delete the prefab and its corresponding name from JSON data
    public void DeletePrefab(RectTransform prefabToDelete, string prefabName, string jsonFilePath)
    {
        // Remove the instantiated prefab
        Destroy(prefabToDelete.gameObject);

        // Remove the prefab name from the JSON data
        RemovePrefabNameFromJson(prefabName, jsonFilePath);
    }

    // Method to remove the prefab name from JSON data
    void RemovePrefabNameFromJson(string prefabName, string jsonFilePath)
    {
        // Read the JSON file content
        string jsonContent = File.ReadAllText(jsonFilePath);

        // Deserialize the JSON data
        SlotData slotData = JsonUtility.FromJson<SlotData>(jsonContent);

        // Check if the deserialization was successful
        if (slotData != null && slotData.Slots != null)
        {
            // Iterate through the slots to find and remove the prefab name
            foreach (var slot in slotData.Slots)
            {
                slot.ItemNames.Remove(prefabName);
            }

            // Serialize the updated JSON data
            string updatedJsonContent = JsonUtility.ToJson(slotData);

            // Write the updated JSON data back to the file
            File.WriteAllText(jsonFilePath, updatedJsonContent);
        }
        else
        {
            Debug.LogError("Failed to update JSON data while removing prefab name.");
        }
    }
}

[System.Serializable]
public class SlotData
{
    public List<SlotItem> Slots;
}

[System.Serializable]
public class SlotItem
{
    public List<string> ItemNames;
}
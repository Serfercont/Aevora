using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHUD : MonoBehaviour
{
    [Header("Textos de Cantidad")]
    [SerializeField] private TextMeshProUGUI lockpicksText;
    [SerializeField] private TextMeshProUGUI medkitsText;
    [SerializeField] private TextMeshProUGUI keysText;
    [SerializeField] private TextMeshProUGUI livesText;

    private PlayerInventory inventory;

    private void Start()
    {
        inventory = FindAnyObjectByType<PlayerInventory>();

        if (inventory == null)
        {
            return;
        }

        inventory.OnItemAdded += UpdateItemHUD;
        inventory.OnItemUsed += UpdateItemHUD;
        inventory.OnKeyAdded += UpdateKeyHUD;
        inventory.OnKeysCleared += RefreshAll;
        inventory.OnLivesChanged += UpdateLivesHUD;

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnItemAdded -= UpdateItemHUD;
            inventory.OnItemUsed -= UpdateItemHUD;
            inventory.OnKeyAdded -= UpdateKeyHUD;
            inventory.OnKeysCleared -= RefreshAll;
            inventory.OnLivesChanged -= UpdateLivesHUD;
        }
    }

    private void RefreshAll()
    {
        UpdateTextsAndIcons();
    }

    private void UpdateItemHUD(string itemName, float amount)
    {
        UpdateTextsAndIcons();
    }

    private void UpdateKeyHUD(string keyID)
    {
        UpdateTextsAndIcons();
    }

    private void UpdateLivesHUD(int currentLives)
    {
        if (livesText != null)
        {
            livesText.text = ""+currentLives;
        }
    }

    private void UpdateTextsAndIcons()
    {
        if (inventory == null) return;

        lockpicksText.text = "x " + inventory.Lockpicks;

        medkitsText.text = "x " + inventory.Medkits;

        int totalKeys = inventory.GetKeyCount();
        keysText.text = "x " + totalKeys;

        if (livesText != null)
        {
            livesText.text = "" + inventory.Lives;
        }
    }
}
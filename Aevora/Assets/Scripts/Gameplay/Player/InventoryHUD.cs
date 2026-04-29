using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Sprite lockpickIcon;
    [SerializeField] private Sprite medkitIcon;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform barParent;

    private PlayerInventory inventory;
    private GameObject lockpickItem;
    private GameObject medkitItem;

    private int lastLockpicks = -1;
    private int lastMedkits = -1;

    private void Start()
    {
        inventory = FindAnyObjectByType<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("No se encontró PlayerInventory en la escena.");
            return;
        }

        CreateBar();
        RefreshBar(true);
    }

    private void Update()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<PlayerInventory>();
            if (inventory == null) return;

            CreateBar();
            RefreshBar(true);
            return;
        }

        if (inventory.lockpicks != lastLockpicks || inventory.medkits != lastMedkits)
        {
            RefreshBar();
        }
    }

    private void CreateBar()
    {
        foreach (Transform child in barParent)
        {
            Destroy(child.gameObject);
        }

        lockpickItem = Instantiate(itemPrefab, barParent);
        medkitItem = Instantiate(itemPrefab, barParent);

        SetupItem(lockpickItem, lockpickIcon);
        SetupItem(medkitItem, medkitIcon);
    }

    private void SetupItem(GameObject itemGO, Sprite icon)
    {
        Image iconImage = itemGO.GetComponentInChildren<Image>();
        TextMeshProUGUI amountText = itemGO.GetComponentInChildren<TextMeshProUGUI>();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }

        if (amountText != null)
        {
            amountText.text = "x0";
        }
    }

    private void RefreshBar(bool force = false)
    {
        if (inventory == null) return;

        lastLockpicks = inventory.lockpicks;
        lastMedkits = inventory.medkits;

        UpdateItem(lockpickItem, lastLockpicks);
        UpdateItem(medkitItem, lastMedkits);
    }

    private void UpdateItem(GameObject itemGO, int amount)
    {
        if (itemGO == null) return;

        TextMeshProUGUI amountText = itemGO.GetComponentInChildren<TextMeshProUGUI>();
        if (amountText != null)
        {
            amountText.text = "x" + amount;
        }

        itemGO.SetActive(amount > 0);
    }
}
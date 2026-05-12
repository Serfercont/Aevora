using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    private static PlayerInventory instance;

    public event Action<string, float> OnItemAdded;
    public event Action<string, float> OnItemUsed;

    [Header("Starting Resources")]
    [SerializeField] private int startLockpicks = 2;
    [SerializeField] private int startMedkits   = 1;

    public int Lockpicks { get; private set; }
    public int Medkits   { get; private set; }

    private readonly HashSet<string> _keys = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[PlayerInventory] Duplicate detected, destroying.", this);
            Destroy(this);
            return;
        }
        instance  = this;
        Lockpicks = startLockpicks;
        Medkits   = startMedkits;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    [YarnCommand("add_item")]
    public void AddItem(string itemName, float amount = 1f)
    {
        int qty = Mathf.Max(0, (int)amount);
        switch (itemName.ToLower())
        {
            case "ganzua":   Lockpicks += qty; break;
            case "botiquin": Medkits   += qty; break;
            default: Debug.LogWarning($"[PlayerInventory] Unknown item '{itemName}'"); return;
        }
        OnItemAdded?.Invoke(itemName, amount);
    }

    [YarnCommand("use_item")]
    public void UseItem(string itemName, float amount = 1f)
    {
        int qty = Mathf.Max(0, (int)amount);
        switch (itemName.ToLower())
        {
            case "ganzua":   Lockpicks = Mathf.Max(0, Lockpicks - qty); break;
            case "botiquin": Medkits   = Mathf.Max(0, Medkits   - qty); break;
            default: Debug.LogWarning($"[PlayerInventory] Unknown item '{itemName}'"); return;
        }
        OnItemUsed?.Invoke(itemName, amount);
    }

    [YarnCommand("add_key")]
    public void AddKey(string keyID) => _keys.Add(keyID);

    [YarnFunction("has_item")]
    public static bool HasItem(string itemName, float amount = 1f)
    {
        if (instance == null) return false;
        int qty = (int)amount;
        return itemName.ToLower() switch
        {
            "ganzua"   => instance.Lockpicks >= qty,
            "botiquin" => instance.Medkits   >= qty,
            _          => false
        };
    }

    [YarnFunction("has_key")]
    public static bool HasKey(string keyID)
        => instance != null && instance._keys.Contains(keyID);
}

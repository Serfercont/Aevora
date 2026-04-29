using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    private static PlayerInventory instance;

    public event Action<string, float> OnInventoryAdded;
    public event Action<string, float> OnInventoryRemoved;

    [Header("Recursos Actuales")]
    public int lockpicks = 2;
    public int medkits = 1;
    
    private HashSet<string> keys = new HashSet<string>();

    private void Awake()
    {
        instance = this;
    }

    [YarnCommand("add_item")]
    public void AddItem(string itemName, float amount = 1f)
    {
        int intAmount = (int)amount;
        switch (itemName.ToLower())
        {
            case "ganzua": lockpicks += intAmount; break;
            case "botiquin": medkits += intAmount; break;
        }
        OnInventoryAdded?.Invoke(itemName, amount);
    }

    [YarnFunction("has_item")]
    public static bool HasItem(string itemName, float amount = 1f)
    {
        if (instance == null) return false;

        return itemName.ToLower() switch
        {
            "ganzua" => instance.lockpicks >= (int)amount,
            "botiquin" => instance.medkits >= (int)amount,
            _ => false
        };
    }

    [YarnCommand("use_item")]
    public void UseItem(string itemName, float amount = 1f)
    {
        int intAmount = (int)amount;
        if (itemName.ToLower() == "ganzua") lockpicks = Mathf.Max(0, lockpicks - intAmount);
        if (itemName.ToLower() == "botiquin") medkits = Mathf.Max(0, medkits - intAmount);
        OnInventoryRemoved?.Invoke(itemName, amount);
    }

    [YarnCommand("add_key")]
    public void AddKey(string keyID) { keys.Add(keyID); }

    [YarnFunction("has_key")]
    public static bool HasKey(string keyID) => instance != null && instance.keys.Contains(keyID);
}
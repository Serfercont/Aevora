using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    private static PlayerInventory instance;

    public event Action<string, float> OnItemAdded;
    public event Action<string, float> OnItemUsed;
    public event Action<string> OnKeyAdded;
    public event Action OnKeysCleared;
    public event Action<int> OnLivesChanged;

    [Header("Starting Resources")]
    [SerializeField] private int startLockpicks = 2;
    [SerializeField] private int startMedkits   = 1;
    [SerializeField] private int maxLives       = 5;

    private static int _currentLockpicks = -1;
    private static int _currentMedkits = -1;
    private static int _currentLives = -1;
    private static HashSet<string> _keys = new();

    public int Lockpicks => _currentLockpicks;
    public int Medkits   => _currentMedkits;
    public int Lives     => _currentLives;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (_currentLockpicks == -1) _currentLockpicks = startLockpicks;
        if (_currentMedkits == -1)   _currentMedkits = startMedkits;
        if (_currentLives == -1)     _currentLives = maxLives;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            _currentLockpicks = startLockpicks;
            _currentMedkits = startMedkits;
            _currentLives = maxLives;
            _keys.Clear();
        }
        else
        {
            _keys.Clear();
            OnKeysCleared?.Invoke();
            Debug.Log("[Inventory] Llaves reiniciadas para el nuevo nivel.");
        }
    }

    public void TryUseMedkit()
    {
        if (_currentMedkits > 0 && _currentLives < maxLives)
        {
            _currentMedkits--;
            _currentLives = Mathf.Min(maxLives, _currentLives + 1);
            
            OnItemUsed?.Invoke("botiquin", 1f);
            OnLivesChanged?.Invoke(_currentLives);
            Debug.Log($"[Inventory] Botiquín usado. Vidas actuales: {_currentLives}");
        }
    }

    public void TakeDamage()
    {
        _currentLives = Mathf.Max(0, _currentLives - 1);
        OnLivesChanged?.Invoke(_currentLives);
        Debug.Log($"[Inventory] ¡Jugador detectado! Vidas restantes: {_currentLives}");

        if (_currentLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("[Inventory] ¡0 Vidas! Volviendo al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }

    [YarnCommand("add_item")]
    public void AddItem(string itemName, float amount = 1f)
    {
        int qty = Mathf.Max(0, (int)amount);
        switch (itemName.ToLower())
        {
            case "ganzua":   _currentLockpicks += qty; break;
            case "botiquin": _currentMedkits   += qty; break;
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
            case "ganzua":   _currentLockpicks = Mathf.Max(0, _currentLockpicks - qty); break;
            case "botiquin": _currentMedkits   = Mathf.Max(0, _currentMedkits   - qty); break;
            default: Debug.LogWarning($"[PlayerInventory] Unknown item '{itemName}'"); return;
        }
        OnItemUsed?.Invoke(itemName, amount);
    }

    [YarnCommand("add_key")]
    public void AddKey(string keyID) 
    {
        if (_keys.Add(keyID))
        {
            OnKeyAdded?.Invoke(keyID);
        }
    }
    
    [YarnFunction("has_item")]
    public static bool HasItem(string itemName, float amount = 1f)
    {
        if (instance == null) return false;
        int qty = (int)amount;
        return itemName.ToLower() switch
        {
            "ganzua"   => _currentLockpicks >= qty,
            "botiquin" => _currentMedkits   >= qty,
            _          => false
        };
    }

    [YarnFunction("has_key")]
    public static bool HasKey(string keyID)
        => _keys != null && _keys.Contains(keyID);

    public int GetKeyCount()
    {
        return _keys.Count;
    }
}
using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    public enum ItemType { Ganzua, Botiquin, Llave }
    
    [SerializeField] private ItemType type;
    [SerializeField] private string keyID;
    [SerializeField] private int amount = 1;

    public void Interact(GameObject player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        
        if (inventory != null)
        {
            if (type == ItemType.Llave) 
                inventory.AddKey(keyID);
            else 
                inventory.AddItem(type.ToString(), amount);

            Destroy(gameObject);
        }
    }

    public string GetInteractionPrompt()
    {
        return type == ItemType.Llave ? $"Presiona 'E' para recoger Llave: {keyID}" : $"Presiona 'E' para recoger {type}";
    }
}
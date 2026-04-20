using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Documento Secreto";

    public void Interact(GameObject player)
    {
        Debug.Log($"Has recogido: {itemName}");
        Destroy(gameObject); 
    }
    public string GetInteractionPrompt()
    {
        return $"Presiona 'E' para recoger {itemName}";
    }
}

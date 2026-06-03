using UnityEngine;


public class Key : MonoBehaviour, IInteractable
{
    [Header("Identificador")]
    [Tooltip("ID único que debe coincidir con el doorKeyID de la puerta correspondiente.")]
    [SerializeField] private string keyID = "key_door_1";

    [Header("Prompt")]
    [SerializeField] private string interactionPrompt = "Recoger llave";

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact(GameObject interactor)
    {
        PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("[Key] El interactor no tiene PlayerInventory.");
            return;
        }

        inventory.AddKey(keyID);
        Debug.Log($"[Key] Llave '{keyID}' recogida.");

        gameObject.SetActive(false);
    }
}

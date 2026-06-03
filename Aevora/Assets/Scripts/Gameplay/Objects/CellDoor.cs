using UnityEngine;

public class CellDoor : MonoBehaviour, IInteractable
{
    [Header("Prompts")]
    [SerializeField] private string promptLocked   = "Necesitas una ganzúa";
    [SerializeField] private string promptUnlocked = "Forzar celda (Ganzúa)";


    [Header("Comportamiento")]
    [Tooltip("Si está marcado, la puerta de la celda se desactiva al abrirse.")]
    [SerializeField] private bool disableOnOpen = false;

    private bool _isOpen = false;

    public string GetInteractionPrompt()
    {
        if (_isOpen) return "";
        return PlayerInventory.HasItem("ganzua", 1f) ? promptUnlocked : promptLocked;
    }

    public void Interact(GameObject player)
    {
        if (_isOpen) return;

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            return;
        }

        if (!PlayerInventory.HasItem("ganzua", 1f))
        {
            return;
        }

        inventory.UseItem("ganzua", 1f);
        OpenCell();
    }

    private void OpenCell()
    {
        _isOpen = true;

        if (disableOnOpen)
        {
            gameObject.SetActive(false);
        }
    }
}
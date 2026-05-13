using UnityEngine;

/// <summary>
/// Coloca este script en el GameObject de la puerta.
/// Asigna el mismo doorKeyID que el keyID de la llave correspondiente.
/// La animación de apertura es opcional: asigna un Animator con el trigger "Open".
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    [Header("Identificador")]
    [Tooltip("Debe coincidir con el keyID de la llave correspondiente.")]
    [SerializeField] private string doorKeyID = "key_door_1";

    [Header("Prompts")]
    [SerializeField] private string promptLocked   = "Necesitas una llave";
    [SerializeField] private string promptUnlocked = "Abrir puerta";

    [Header("Animación (opcional)")]
    [Tooltip("Animator con un trigger llamado 'Open'. Déjalo vacío si no usas animación.")]
    [SerializeField] private Animator doorAnimator;

    [Header("Comportamiento")]
    [Tooltip("Si está marcado, la puerta se desactiva al abrirse en vez de animarse.")]
    [SerializeField] private bool disableOnOpen = false;

    private bool _isOpen = false;

    public string GetInteractionPrompt()
    {
        if (_isOpen) return "";   // Ya abierta, no mostrar prompt
        return PlayerInventory.HasKey(doorKeyID) ? promptUnlocked : promptLocked;
    }

    public void Interact(GameObject interactor)
    {
        if (_isOpen) return;

        if (!PlayerInventory.HasKey(doorKeyID))
        {
            Debug.Log($"[Door] Puerta '{doorKeyID}' bloqueada. El jugador no tiene la llave.");
            return;
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        _isOpen = true;
        Debug.Log($"[Door] Puerta '{doorKeyID}' abierta.");

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }

        if (disableOnOpen)
        {
            gameObject.SetActive(false);
        }
    }
}

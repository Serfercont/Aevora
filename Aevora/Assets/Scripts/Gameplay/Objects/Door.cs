using UnityEngine;


public class Door : MonoBehaviour, IInteractable
{
    [Header("Identificador")]
    [Tooltip("Debe coincidir con el keyID de la llave correspondiente.")]
    [SerializeField] private string KeyID = "key_door_1";
    [Tooltip("ID de la segunda llave si la puerta necesita dos. Si se deja vacío, se usa KeyID + \"_second\".")]
    [SerializeField] private string secondKeyID = "";

    [Header("Prompts")]
    [SerializeField] private string promptLocked   = "Necesitas una llave";
    [SerializeField] private string promptLockedTwoKeys = "Necesitas dos llaves";
    [SerializeField] private string promptUnlocked = "Abrir puerta";

    [Header("Animación (opcional)")]
    [Tooltip("Animator con un trigger llamado 'Open'. Déjalo vacío si no usas animación.")]
    [SerializeField] private Animator doorAnimator;

    [Header("Comportamiento")]
    [Tooltip("Si está marcado, la puerta se desactiva al abrirse en vez de animarse.")]
    [SerializeField] private bool disableOnOpen = false;
    [SerializeField] private bool needs2Keys = false;

    private bool _isOpen = false;

    public string GetInteractionPrompt()
    {
        if (_isOpen) return "";

        if (needs2Keys)
        {
            return HasRequiredKeys() ? promptUnlocked : promptLockedTwoKeys;
        }

        return PlayerInventory.HasKey(KeyID) ? promptUnlocked : promptLocked;
    }

    public void Interact(GameObject interactor)
    {
        if (_isOpen) return;

        if (!HasRequiredKeys())
        {
            return;
        }

        OpenDoor();
    }

    private bool HasRequiredKeys()
    {
        if (!PlayerInventory.HasKey(KeyID))
        {
            return false;
        }

        if (!needs2Keys)
        {
            return true;
        }

        string requiredSecondKeyID = string.IsNullOrWhiteSpace(secondKeyID)
            ? KeyID + "_second"
            : secondKeyID;

        return PlayerInventory.HasKey(requiredSecondKeyID);
    }

    private void OpenDoor()
    {
        _isOpen = true;

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

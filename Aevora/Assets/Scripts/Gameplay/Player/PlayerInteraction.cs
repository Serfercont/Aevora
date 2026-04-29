using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [Tooltip("Distancia máxima para interactuar con objetos.")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    [SerializeField] private FloatingInteractionUI interactionUI;
    private IInteractable currentInteractable;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player != null && (player.isDead || !player.canMove))
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                if (interactionUI != null)
                    interactionUI.Hide();
            }

            return;
        }

        CheckNearbyInteractables();
    }
    public void TryInteract()
    {
        if(currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    private void CheckNearbyInteractables()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);

        if (hitColliders.Length > 0)
        {
            IInteractable interactableObject = hitColliders[0].GetComponent<IInteractable>();

            if (interactableObject != null)
            {
                if (currentInteractable != interactableObject)
                {
                    currentInteractable = interactableObject;
                    if (interactionUI != null)
                        interactionUI.Show(currentInteractable.GetInteractionPrompt(), hitColliders[0].transform.position);
                    return;
                }
            }
        }
        currentInteractable = null;
        if (interactionUI != null)
            interactionUI.Hide();
    }
}
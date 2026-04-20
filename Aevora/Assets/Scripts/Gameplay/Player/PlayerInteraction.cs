using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [Tooltip("Distancia máxima para interactuar con objetos.")]
    [SerializeField] private float interactRange = 2f;
    [Tooltip("Capa (Layer) donde se encuentran los objetos interactuables para optimizar la búsqueda.")]
    [SerializeField] private LayerMask interactableLayer;

    // Metodo que llama el player para intentar interactuar con objetos 
    public void TryInteract()
    {
        // Esfera de detección para encontrar objetos interactuables dentro del rango
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);

        if (hitColliders.Length > 0)
        {
            // Primer objeto interactuable encontrado
            IInteractable interactableObject = hitColliders[0].GetComponent<IInteractable>();

            if (interactableObject != null)
            {
                interactableObject.Interact(gameObject);
            }
        }
        else
        {
            Debug.Log("No hay nada con lo que interactuar cerca.");
        }
    }

    // Dibujamos la esfera en el editor para visualizar el rango de interacción
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
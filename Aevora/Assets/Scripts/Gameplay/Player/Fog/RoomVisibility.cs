using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomVisibility : MonoBehaviour
{
    [Header("Componentes Visuales de la Habitación")]
    [Tooltip("Arrastra aquí el objeto padre que contiene las luces o el contenido de la habitación.")]
    [SerializeField] private GameObject roomContent;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (roomContent != null)
        {
            roomContent.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomContent != null) roomContent.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomContent != null) roomContent.SetActive(false);
        }
    }
}
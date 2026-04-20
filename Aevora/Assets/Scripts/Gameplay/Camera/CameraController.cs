using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("El Transform del jugador al que la cámara debe seguir.")]
    [SerializeField] private Transform target;

    [Header("Configuración de Cámara")]
    [Tooltip("La distancia y ángulo relativo de la cámara respecto al jugador.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [Tooltip("Qué tan suave será el seguimiento. Mayor número = más rápido.")]
    [SerializeField] private float smoothSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
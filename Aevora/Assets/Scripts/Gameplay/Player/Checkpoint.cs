using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.LastCheckpointPosition = transform.position;
                
                Debug.Log("¡Checkpoint guardado!");
            }
        }
    }
}

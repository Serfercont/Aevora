using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player detected!");
            
            other.transform.position = new Vector3(0, 0, 0);
            
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }
}

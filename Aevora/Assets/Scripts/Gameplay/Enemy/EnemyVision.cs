using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    private Enemy enemyScript;
    private void Start()
    {
        enemyScript = GetComponentInParent<Enemy>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player detected!");
            
            enemyScript.DetectAndFire(other.gameObject);
        }
    }
}

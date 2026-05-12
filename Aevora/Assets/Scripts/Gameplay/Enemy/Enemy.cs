using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    [SerializeField] private Transform [] waypoints;
    [SerializeField] private float waitTime = 3f;
    private int currentWaypointIndex = 0;

    private bool isWaiting= false;
    private bool isAttacking = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        if(waypoints.Length > 0)
        {
            MoveToNextWaypoint();
        }
    }

    void Update()
    {
        if (isAttacking || isWaiting) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
        else
        {
            if(animator != null)
            {
                bool isWalking = navMeshAgent.velocity.magnitude > 0.1f;
                animator.SetBool("is_walking", isWalking);
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        navMeshAgent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        if(animator != null)
        {
            animator.SetBool("is_walking", false);
            animator.SetBool("issearching", true);
        }
        yield return new WaitForSeconds(waitTime);

        animator.SetBool("issearching", false);
        navMeshAgent.isStopped = false;
        isWaiting = false;
        MoveToNextWaypoint();
    }

    public void DetectAndFire(GameObject player)
    {
        if(isAttacking) return;
        StartCoroutine(AttackAndRespawnPlayer(player));
    }

    private IEnumerator AttackAndRespawnPlayer(GameObject player)
    {
        isAttacking = true;

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        animator.SetBool("is_walking", false);
        animator.SetBool("issearching", false);
        animator.SetTrigger("playerfound");

        float timer = 0f;
        while (timer < 2f)
        {
            if (player != null)
            {
                Vector3 currentLookPosition = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                Vector3 direction = (currentLookPosition - transform.position).normalized;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            player.transform.position = playerScript.LastCheckpointPosition;
        }


        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;

        animator.ResetTrigger("playerfound");
        

        animator.SetTrigger("playerdead");
        yield return new WaitForSeconds(1f);

        animator.ResetTrigger("playerdead");
        isAttacking = false;

        navMeshAgent.isStopped = false;
        MoveToNextWaypoint();
    }
}

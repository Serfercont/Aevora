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

    private Vector3 initialModelLocalPos;
    private Quaternion initialModelLocalRot;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        initialModelLocalPos = animator.transform.localPosition;
        initialModelLocalRot = animator.transform.localRotation;
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
                bool isWalking = navMeshAgent.velocity.magnitude > 0.05f || (!navMeshAgent.isStopped && navMeshAgent.hasPath);
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

        StopAllCoroutines();
        isWaiting = false;
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

        Player playerScript = player?.GetComponent<Player>();
        Rigidbody playerRb = player?.GetComponent<Rigidbody>();

        if (playerRb != null && playerScript != null)
        {
            playerRb.position = playerScript.LastCheckpointPosition;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        animator.ResetTrigger("playerfound");
        

        animator.SetTrigger("playerdead");
        yield return new WaitForSeconds(1f);

        

        if (animator != null)
        {
            animator.ResetTrigger("playerdead");
            animator.transform.localPosition = initialModelLocalPos;
            animator.transform.localRotation = initialModelLocalRot;
        }

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);


        isAttacking = false;

        navMeshAgent.isStopped = false;
        MoveToNextWaypoint();
    }
}

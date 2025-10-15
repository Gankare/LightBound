using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GoblinAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    // Patrol
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attack
    [Tooltip("Optional manual override for attack cooldown. Leave 0 to use animation length.")]
    public float timeBetweenAttacks = 0f;
    bool alreadyAttacked;
    public float attackRange = 2f;
    public int attackDamage = 10;

    // States
    public float sightRange = 10f;
    bool playerInSightRange, playerInAttackRange;

    public Animator animator; // must have "Attack" animation

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check ranges
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrol();
        else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patrol()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;

        animator?.SetBool("Walking", agent.velocity.magnitude > 0.1f); // Check if the agent is moving
    }

    private void SearchWalkPoint()
    {
        // Random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        animator?.SetBool("Walking", agent.velocity.magnitude > 0.1f); // Check if the agent is moving
    }

    private void AttackPlayer()
    {
        agent.isStopped = true; // Stop movement
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;

            // Attack animation
            animator?.SetTrigger("Attack");

            // Damage player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);

            // Use animation length as cooldown if available
            float cooldown = timeBetweenAttacks;
            if (animator != null && cooldown <= 0f)
            {
                AnimationClip attackClip = GetAnimationClip("Attack");
                if (attackClip != null)
                    cooldown = attackClip.length;
                else
                    cooldown = 1f; // fallback
            }

            StartCoroutine(ResetAttackAfterCooldown(cooldown));
        }
    }

    private AnimationClip GetAnimationClip(string name)
    {
        if (animator == null) return null;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
                return clip;
        }
        return null;
    }

    private IEnumerator ResetAttackAfterCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        alreadyAttacked = false;
        agent.isStopped = false; // Resume movement
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
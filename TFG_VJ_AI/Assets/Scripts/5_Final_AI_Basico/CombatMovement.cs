using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CombatMovement : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    public float followRadius = 2.5f;
    public float enemyScanRadius = 8f;
    public LayerMask enemyMask;
    public float attackRange = 2.0f;

    [Header("Debug")]
    public Transform currentEnemy;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public bool TickFollowAndHelp()
    {
        if (!player || !agent) return false;

        float d = Vector3.Distance(transform.position, player.position);
        if (d > followRadius)
            agent.SetDestination(player.position);

        Collider[] hits = Physics.OverlapSphere(player.position, enemyScanRadius, enemyMask);
        if (hits.Length > 0)
        {
            var enemy = hits[0].transform;
            currentEnemy = enemy;
            ApproachAndAttack(enemy);
        }
        else
        {
            currentEnemy = null;
        }
        return true;
    }

    public bool TickAttackPlayer()
    {
        if (!player || !agent) return false;
        ApproachAndAttack(player);
        return true;
    }

    void ApproachAndAttack(Transform target)
    {
        float d = Vector3.Distance(transform.position, target.position);
        if (d > attackRange)
        {
            agent.SetDestination(target.position);
        }
        else
        {
        }
    }
}
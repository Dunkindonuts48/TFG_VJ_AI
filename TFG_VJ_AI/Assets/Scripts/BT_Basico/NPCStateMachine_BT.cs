using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BT_Basico
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCStateMachine_BT : MonoBehaviour
    {
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 3f;
        [SerializeField] private Transform player;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float chaseSpeed = 6f;
        [SerializeField] private float attackDuration = 1f;
        private int currentWP = 0;
        private NavMeshAgent agent;
        private NPCBehaviorTree bt;

        public Transform[] PatrolPoints => patrolPoints;
        public Transform Player => player;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float ChaseSpeed => chaseSpeed;
        public float PatrolSpeed => patrolSpeed;
        public float AttackDuration => attackDuration;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.autoBraking = false;
            bt = GetComponent<NPCBehaviorTree>();
            bt.ConstructTree(this);
        }

        private void Start()
        {
            StartPatrol();
        }

        private void Update()
        {
            bt.Tick();
        }

        public void StartPatrol()
        {
            agent.speed = patrolSpeed;
            if (patrolPoints.Length == 0) return;
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[currentWP].position);
        }

        private void LateUpdate()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
            {
                currentWP = (currentWP + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentWP].position);
            }
        }

        public void StartChase()
        {
            agent.speed = chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        public IEnumerator AttackRoutine()
        {
            agent.isStopped = true;
            yield return new WaitForSeconds(attackDuration);
        }
    }
}

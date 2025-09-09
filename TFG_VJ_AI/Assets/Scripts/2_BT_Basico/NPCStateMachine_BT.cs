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
        [SerializeField] private Material materialObjetivo;
        [SerializeField] private Material materialDefault;

        private int currentWP = 0;
        private NavMeshAgent agent;
        private NPCBehaviorTree bt;
        private int ultimoResaltado = -1;
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

            ResaltarSolo(currentWP);
        }

        private void LateUpdate()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
            {
                currentWP = (currentWP + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentWP].position);

                ResaltarSolo(currentWP);
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

        private void ResaltarSolo(int idxObjetivo)
        {
            if (ultimoResaltado >= 0 && ultimoResaltado < patrolPoints.Length)
            {
                var rendPrev = patrolPoints[ultimoResaltado]?.GetComponent<Renderer>();
                if (rendPrev) rendPrev.sharedMaterial = materialDefault;
            }

            var rend = patrolPoints[idxObjetivo]?.GetComponent<Renderer>();
            if (rend) rend.sharedMaterial = materialObjetivo;

            ultimoResaltado = idxObjetivo;
        }
    }
}
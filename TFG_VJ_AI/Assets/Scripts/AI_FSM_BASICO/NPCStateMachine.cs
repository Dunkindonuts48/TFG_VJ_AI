using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float chaseSpeed = 60.0f;
    [SerializeField] private GameObject NPC_Enemy;
    //private Transform[] patrolPoint;
    [SerializeField] private int lastPatrolIndex = 0;
    public Transform NPCTransform { get; private set; }
    private NPCState currentState;
    private Transform playerTransform;


    private void Awake()
    {
        NPCTransform = transform;
        //int count = patrolPointsParent.childCount;
        //patrolPoints = new Transform[count];
        //for (int i = 0; i < count; i++)
        //{
        //    patrolPoints[i] = patrolPointsParent.GetChild(i);
        //}

        if (NPC_Enemy != null) playerTransform = NPC_Enemy.transform;

        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Start()
    {
        ChangeState(new IdleState(this, idleDuration: 3f));
    }

    private void Update()
    {
        currentState?.Execute();
    }

    public void ChangeState(NPCState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    //public Transform[] GetNPCTransform() => patrolPoint;
    public Transform[] GetPatrolPoints() => patrolPoints;
    public Transform GetPlayer() => playerTransform;
    public float GetChaseSpeed() => chaseSpeed;

    public int GetLastPatrolIndex() => lastPatrolIndex;
    public void SetLastPatrolIndex(int idx) => lastPatrolIndex = idx;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            if (currentState is PatrolState patrol)
                lastPatrolIndex = patrol.CurrentWaypointIndex;

            ChangeState(new ChaseState(this));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {          
            ChangeState(new PatrolState(this, lastPatrolIndex));
        }
    }
}
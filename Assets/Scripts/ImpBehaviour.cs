using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class ImpBehaviour : MonoBehaviour
{
    //  NPC States
    public enum FSMState
    {
        None,
        Idle,
        RunAway,
        TrapPlacing,
        Dead
    }

    //  Current NPC state
    public FSMState curState;

    //  Total times the NPC can get hit until destruction
    public int health = 1;

    //  Destination buffer variables
    private float setDestinationTime = 0;
    //  rate at which destination is checked (sec)
    private float setDestinationWaitTime = 0.1f;

    //  Line-of-sight variables
    private float playerOutOfSightTime = 0;
    private float maxOutOfSightTime = 2f;

    private Transform playerTransform;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;

    // Range variables
    [Header("Ranges")]
    public float trapPlacingRange = 20;

    // Movement variables
    [Header("Movement")]
    public float runningSpeed = 20f;

    public Queue<GameObject> trapPool;
    private int totalTraps;
/*
    [Header("Trap Object")]
    public GameObject trapObject;*/

    /*// Imports the bullet manager
    [Header("Bullet Related Variables")]
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.2f;
    private float elapsedTime;*/

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        nav.speed = runningSpeed;

        trapPool = new Queue<GameObject>();
        totalTraps = transform.childCount;

        // Instantiate all the traps and make them invisible @ frame 0 (zero)
        for (int i = 0; i < totalTraps; i++)
        {
/*            GameObject trap = Instantiate(trapObject);
            trap.SetActive(false);
            trap.transform.SetParent(transform, false);
            trapPool.Enqueue(trap);*/
            
            GameObject trap = transform.GetChild(i).gameObject;
            trap.SetActive(false);
            trapPool.Enqueue(trap);

        }

        curState = FSMState.Idle;

        // Locates the player before initialisation
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Calls rigidbody before initialisation
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (curState)
        {
            case FSMState.Idle: UpdateIdleState(); break;
            case FSMState.RunAway: UpdateRunAwayState(); break;
            case FSMState.TrapPlacing: UpdateTrapPlacingState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
        }

        /*elapsedTime += Time.deltaTime;*/

        if (health <= 0)
        {
            curState = FSMState.Dead;
        }
    }

    //
    //
    //  NPC STATES
    //
    //

    /*
     *   Idle State
     */
    void UpdateIdleState()
    {
        nav.isStopped = true;
        nav.SetDestination(transform.position);
        IdleActions();
    }

    /*
     *   Running Away State
     */
    void UpdateRunAwayState()
    {
        nav.isStopped = false;
        nav.speed = runningSpeed;

        //  NPC runs some units in the opposite direction to player 
        Vector3 oppositeDirection = -(playerTransform.position - transform.position).normalized;
        oppositeDirection.y = 0;

        Vector3 runAwayDestination = transform.position + oppositeDirection * 5f;

        nav.SetDestination(runAwayDestination);

        IdleActions();
    }

    /*
     *   Trap Placing State
     */
    void UpdateTrapPlacingState()
    {
        nav.isStopped = true;

        //  Set traps to a random location within range
        ValidTrapPlacing();
        //  ACTIVATE (NOT instantiate) 3-5 traps within the range, trap total determined by rng

        //  When the IMP sees the player, the traps shoot. Therefore, the traps are controlled and NOT autonomous. Possibly an idle action?
        //print("shooting now");

        IdleActions();
    }

    /*
     *   Dead State
     */
    void UpdateDeadState()
    {

    }

    /*  Returns if the player is in view via raycast  */
    public bool PlayerInView()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit))
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red);

            if (hit.transform == playerTransform)
            {
                return true;
            }
        }
        return false;
    }

    private void IdleActions()
    {
        // Transitions
        // NPC can see player
        if (PlayerInView())
        {
            playerOutOfSightTime = 0;
            Transitions();
        }
        //  NPC cannot see player
        else
        {
            playerOutOfSightTime += Time.deltaTime;

            //  NPC reaction buffer to no longer seeing player
            if (playerOutOfSightTime > maxOutOfSightTime)
            {
                curState = FSMState.Idle;
            }
        }
    }

    private void Transitions()
    {
        //  Enters trap placing range
        if (Vector3.Distance(transform.position, playerTransform.position) <= trapPlacingRange)
        {
            curState = FSMState.TrapPlacing;
            
            //  Exits trap placing range
        } 
        else if (Vector3.Distance(transform.position, playerTransform.position) > trapPlacingRange)
        {
            curState = FSMState.RunAway;
        }
    }

    private void ValidTrapPlacing()
    {
        //  Assign positions (and activates) for all traps in the pool
        while (trapPool.Count > 0)
        {
            GameObject trap = trapPool.Dequeue();
            NavMeshAgent agent = trap.GetComponent<NavMeshAgent>();

            if (agent != null)
            {
                Vector3 randomPosition = GetRandomNavMeshPosition();

                //  If the random position is valid, set a trap to that position and activate it
                if (randomPosition != Vector3.zero)
                {
                    agent.Warp(randomPosition);
                    trap.SetActive(true);
                }
            }
        }
    }

    Vector3 GetRandomNavMeshPosition()
    {
        //  Checks for points within the circular range
        Vector2 randomPointWithinCircularRange = Random.insideUnitCircle * trapPlacingRange;

        //  Assigns random position within the range
        Vector3 randomPosition = new Vector3((transform.position.x + randomPointWithinCircularRange.x), 0, (transform.position.z + randomPointWithinCircularRange.y));

        NavMeshHit hit;

        //  Try to find valid position on the NavMesh around randomPosition
        if (NavMesh.SamplePosition(randomPosition, out hit, trapPlacingRange, NavMesh.AllAreas))
        {
            //  Return valid position on NavMesh
            return hit.position;
        }

        //  Return if no valid positions
        return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trapPlacingRange);
    }

}

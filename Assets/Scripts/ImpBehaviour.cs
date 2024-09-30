using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    // Imports the sinewave bullet manager
    [Header("Bullet Related Variables")]
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.2f;
    private float elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        // Error management + logging
        if (bulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }

        nav = GetComponent<NavMeshAgent>();
        nav.speed = runningSpeed;

        curState = FSMState.Idle;

        elapsedTime = shootRate;

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

        elapsedTime += Time.deltaTime;

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
        print("placing traps");
        IdleActions();
    }

    /*
     *   Dead State
     */
    void UpdateDeadState()
    {

    }

    /*  Returns if the player is in view via raycast  */
    private bool PlayerInView()
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trapPlacingRange);
    }

}

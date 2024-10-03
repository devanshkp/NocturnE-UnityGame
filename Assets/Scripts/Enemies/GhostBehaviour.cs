using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostBehaviour : MonoBehaviour
{
    //
    //  Ghosts need a special navmesh that is baked without walls, and a special accompanying navmesh agent 
    //


    // NPC states
    public enum FSMState
    {
        None,
        Patrol,
        Attack,
        Dead,
    }

    public GameObject[] destinationList;
    private Vector3 destinationPos;
    private int currentDestination = 0;

    //  Destination buffer variables
    private float setDestinationTime = 0;
    //  rate at which destination is checked (sec)
    private float setDestinationWaitTime = 0.2f;

    private Transform playerTransform;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;

    // Current NPC state
    public FSMState curState;

    // Total times the NPC can get hit until destruction
    public int health = 1;

    // Range variables
    [Header("Ranges")]
    public float attackRange = 20;

    // Movement variables
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float targettingSpeed = 2f;

    // Bullet variables (NOTE: most bullet variables are in their respective bullet script)
    [Header("Bullet Related Variables")]
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.5f;
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
        // Set first destination
        nav.SetDestination(destinationList[0].transform.position);
        nav.speed = moveSpeed;

        // NPC initialises in patrol state
        curState = FSMState.Patrol;

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
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
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
     *   Patrol State
     */
    void UpdatePatrolState()
    {
        //  NPC reaches the current destination
        if (Vector3.Distance(transform.position, destinationList[currentDestination].transform.position)
            < 2.5f)
        {
            //  Increment destination
            currentDestination++;

            //  NPC reaches final destination
            if (currentDestination > destinationList.Length - 1)
            {
                //  Reset to first destination
                currentDestination = 0;
            }

            nav.SetDestination(destinationList[currentDestination].transform.position);
        }

        //  Transitions
        //  Attack state when player enters attack range
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            nav.speed = targettingSpeed;
            curState = FSMState.Attack;
        }

    }

    /*
     *  Attack State
     */
    void UpdateAttackState()
    {
        setDestinationTime += Time.deltaTime;

        if (setDestinationTime > setDestinationWaitTime)
        {
            //  Follow player
            nav.SetDestination(playerTransform.position);
            setDestinationTime = 0;

            //  Shoot when no obstructions
            if (PlayerInView())
            {
                ShootingPlayer();
            }
        }
        //  Transitions
        //  Patrol state when player exits attack range
        if (Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            nav.SetDestination(destinationList[currentDestination].transform.position);
            nav.speed = moveSpeed;
            curState = FSMState.Patrol;
        }
    }

    /*
     *  Dead State
     */
    void UpdateDeadState()
    {

    }

    /*  Returns if the player is in view via raycast  */
    private bool PlayerInView()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, attackRange))
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red);

            if (hit.transform == playerTransform)
            {
                return true;
            }
        }
        return false;
    }

    /*  Shooting method for the skeletons: shoots at the shoot rate  */
    private void ShootingPlayer()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= shootRate)
        {
            elapsedTime = 0;
            bulletPoolManager.Shooting();
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

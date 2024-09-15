using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SkeletonBehaviour : MonoBehaviour
{
    // NPC states
    public enum FSMState
    {
        None,
        Patrol,
        Attack,
        Dead,
    }

    public GameObject[] destinationList;
    protected Vector3 destinationPos;

    // Current NPC state
    public FSMState curState;

    // Total times the NPC can get hit until destruction
    public int health = 1;

    // Range variables
    [Header("Ranges")]
    public float attackRange = 20;

    // Movement variables
    [Header("Movement")]
    public float moveSpeed = 10f;

    // Bullet variables (NOTE: most bullet variables are in their respective bullet script)
    [Header("Bullet Related Variables")]
    public float shootRate = 0.5f;
    public float elapsedTime;

    protected Transform playerTransform;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;


    // Start is called before the first frame update
    void Start()
    {
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

    }

    /*
     *  Attack State
     */
    void UpdateAttackState()
    {

    }


    /*
     *  Dead State
     */
    void UpdateDeadState()
    {

    }

    public void Movement()
    {
        // Movement is incomplete, fix this future me

        nav.SetDestination(destinationPos);
    }

    public void LocateDestinationWaypoint(GameObject location)
    {

    }
}

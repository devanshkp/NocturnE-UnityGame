using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WerewolfBehaviour : MonoBehaviour
{
    //  NPC States
    public enum FSMState
    {
        None,
        Idle,
        DayRunAway,
        DayAttack,
        NightAttack,
        NightChase,
        Dead
    }

    //  Current NPC state
    public FSMState curState;

    //  Total times the NPC can get hit until destruction
    public int health = 2;

    //  Line-of-sight variables
    private float playerOutOfSightTime = 0;
    private float maxOutOfSightTime = 2f;

    private Transform playerTransform;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;

    // Range variables
    [Header("Ranges")]
    public float attackRange = 20;

    // Movement variables
    [Header("Movement")]
    public float dayMoveSpeed = 20f;
    public float nightMoveSpeed = 10f;
    public float nightTargettingSpeed = 3f;

    // Imports the sinewave bullet manager
    [Header("Bullet Related Variables")]
    public SineBulletPoolManager sinBulletPoolManager;
    public float shootRate = 0.2f;
    private float elapsedTime;

    [Header("Level Manager")]
    public LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        // Error management + logging
        if (sinBulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }

        // When a manager is applied, set NPC to idle
        if (levelManager == null)
        {
            Debug.Log("No level manager, please add the manager");
        }
        else
        {
            curState = FSMState.Idle;
        }

        nav = GetComponent<NavMeshAgent>();
        nav.speed = dayMoveSpeed;

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
            case FSMState.DayRunAway: UpdateDayRunAwayState(); break;
            case FSMState.DayAttack: UpdateDayAttackState(); break;
            case FSMState.NightAttack: UpdateNightAttackState(); break;
            case FSMState.NightChase: UpdateNightChaseState(); break;
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
        IdleActions();
    }

    /*
     *   Run Away State
     */
    void UpdateDayRunAwayState()
    {


        IdleActions();
    }

    /*
     *   Daytime Attacking State
     */
    void UpdateDayAttackState()
    {


        IdleActions();
    }

    /*
     *   Night time Attacking State
     */
    void UpdateNightAttackState()
    {


        IdleActions();
    }

    /*
     *   Chasing State
     */
    void UpdateNightChaseState()
    {


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

    /*  Shooting method for the skeletons: shoots at the shoot rate  */
    private void ShootingPlayer()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= shootRate)
        {
            elapsedTime = 0;
            sinBulletPoolManager.Shooting();
        }
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
        else
        {
            curState = FSMState.Idle;
        }
    }

    private void Transitions()
    {
        //  Day time & not attackable
        if ((levelManager.isNightTime == false) && Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            curState = FSMState.DayRunAway;

            //  Day time & attackable
        }
        else if ((levelManager.isNightTime == false) && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            curState = FSMState.DayAttack;
        }

        //  Night time & not attackable
        else if ((levelManager.isNightTime == true) && Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            curState = FSMState.NightChase;

            //  Night time & attackable
        }
        else if ((levelManager.isNightTime == true) && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            curState = FSMState.NightAttack;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

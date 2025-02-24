using System.Collections;
using System.Collections.Generic;
//using TreeEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class ImpBehaviour : MonoBehaviour, InterfaceEnemy
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

    private bool isDead = false;

    [Header("Score Settings")]
    public int points = 50;

    //  Total times the NPC can get hit until destruction
    [Header("Health Settings")]
    public float health = 10;
    public float Health => health;

    //  Line-of-sight variables
    private float playerOutOfSightTime = 0;
    private float maxOutOfSightTime = 2f;

    private Transform playerTransform;
    private PlayerController playerController;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;
    private Animator _animator;

    [Header("Health")]
    private HealthManager healthManager;

    // Range variables
    [Header("Ranges")]
    public float trapPlacingRange = 20;

    // Movement variables
    [Header("Movement")]
    public float runningSpeed = 20f;

    [Header("Imported Animation Objects")]
    public GameObject deathAnimation;
    public float deathAnimationLifeTime = 3f;
    public float fadeDuration = 1f;
    private GameObject deathAnimationInstance;

    [Header("Trap Pool")]
    public Queue<GameObject> trapPool;
    private int totalTraps;


    // Start is called before the first frame update
    void Start()
    {
        InitializeHealthManager();
        nav = GetComponent<NavMeshAgent>();
        nav.speed = runningSpeed;

        trapPool = new Queue<GameObject>();
        totalTraps = transform.childCount;

        // Collect all the traps into the queue and make them invisible @ frame 0 (zero)
        for (int i = 0; i < totalTraps; i++)
        {
            GameObject trap = transform.GetChild(i).gameObject;

            //  Skip the asset gameobjects (only disable the trap children)
            if(trap.name.Contains("cap") || trap.name.Contains("RigGob1") || trap.name.Contains("Canvas") || trap.name.Contains("trigger"))
            {
                continue;
            }

            /*// remove the asset gameobjects from totalTraps count
            totalTraps =- 2;*/

            trap.SetActive(false);
            trapPool.Enqueue(trap);
        }

        curState = FSMState.Idle;

        // Locates the player before initialisation
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObj.transform;
        playerController = playerObj.GetComponent<PlayerController>();

        // Calls rigidbody before initialisation
        _rigidbody = GetComponent<Rigidbody>();

        _animator = GetComponent<Animator>();
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

        if (healthManager.GetHealth() <= 0)
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
        //  NPC running animation
        _animator.Play("Base Layer.running");

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
        //  NPC idle animation
        _animator.Play("Base Layer.idle");

        nav.isStopped = true;

        //  Set traps to a random location within range
        ValidTrapPlacing();

        IdleActions();
    }

    /*
     *   Dead State
     */
    void UpdateDeadState()
    {
        if (!isDead)
        {
            isDead = true;
            playerController.AddMoneyAndScore(25, 100);

            _animator.Play("Base Layer.idle");

            //  Spawn effect at enemy position
            deathAnimationInstance = Instantiate(deathAnimation, transform.position, Quaternion.identity);

            //  Play effect and fade out
            StartCoroutine(DestroyEffect());

            //
            //  Reward points to player score here
            //
        }
    }

    private IEnumerator DestroyEffect()
    {
        //  Let animation play for its life time
        yield return new WaitForSeconds(deathAnimationLifeTime);

        //  Destroy enemy
        Destroy(gameObject);

        //
        //  Fade effect logic if shader and gameobject supports
        //

        /*float fadeStartTime = Time.time;

        //  Get renderers in the effect to apply transparency to
        Renderer[] renderers = deathAnimationInstance.GetComponentsInChildren<Renderer>();

        while (Time.time < fadeStartTime + fadeDuration)
        {
            float time = (Time.time - fadeStartTime) / fadeDuration;

            foreach (Renderer renderer in renderers)
            {
                Color color = renderer.material.color;
                //  Decrease alpha (become transparent)
                color.a = Mathf.Lerp(1f, 0f, time);
                renderer.material.color = color;
            }
            yield return null;
        }*/

        Destroy(deathAnimationInstance);
    }

    public void TakeDamage(int damage)
    {
        healthManager.UpdateHealth(-damage);
    }

    private void IdleActions()
    {
        // Transitions
        // NPC can see player
        if (PlayerInView(transform.position))
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
    
    /*  Returns if the player is in view via raycast | Casts ray FROM a defined position TO the player  */
    public bool PlayerInView(Vector3 position)
    {
        Vector3 directionToPlayer = playerTransform.position - position;

        LayerMask visionMask = ~LayerMask.GetMask("Bullets", "Enemy");

        if (Physics.Raycast(position, directionToPlayer, out RaycastHit hit, Mathf.Infinity, visionMask))
        {
            Debug.DrawLine(position, playerTransform.position, Color.red);

            if (hit.transform == playerTransform)
            {
                return true;
            }
        }
        return false;
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
                Vector3 randomPosition = Vector3.zero;

                bool isPositionVisible = false;

                //  Generate new random positions until the player can see it
                while (!isPositionVisible)
                {
                    randomPosition = GetRandomNavMeshPosition();

                    //  Check if player can see it
                    if (randomPosition != Vector3.zero)
                    {
                        isPositionVisible = PlayerInView(randomPosition);
                    }
                }

                //  Once player can see it, set trap to the position and activate
                agent.Warp(randomPosition);
                trap.SetActive(true);
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

    void InitializeHealthManager()
    {
        if (healthManager == null)
        {
            healthManager = GetComponentInChildren<HealthManager>();
            if (healthManager != null)
            {
                healthManager.SetMaxHealth(health);
                healthManager.TurnOffHealthBar();
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trapPlacingRange);
    }

}

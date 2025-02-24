using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class SkeletonBehaviour : MonoBehaviour, InterfaceEnemy
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
    private Vector3 destinationPos;
    private int currentDestination = 0;

    //  Destination buffer variables
    private float setDestinationTime = 0;
    //  rate at which destination is checked (sec)
    private float setDestinationWaitTime = 0.2f;

    //  Line-of-sight variables
    private float playerOutOfSightTime = 0;
    private float maxOutOfSightTime = 2f;

    private Transform playerTransform;
    private PlayerController playerController;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;
    private Animator _animator;

    // Current NPC state
    public FSMState curState;

    private bool isDead = false;

    [Header("Score Settings")]
    public int points = 100;

    [Header("Health")]
    // Total times the NPC can get hit until destruction
    public float health = 10;
    public float Health => health;
    private HealthManager healthManager;

    // Range variables
    [Header("Ranges")]
    public float attackRange = 20;

    // Movement variables
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float targettingSpeed = 3f;

    // Bullet variables (NOTE: most bullet variables are in their respective bullet script)
    [Header("Bullet Related Variables")]
    public GameObject bulletSpawnpoint;
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.5f;
    private float elapsedTime;

    [Header("Imported Animation Objects")]
    public GameObject deathAnimation;
    public float deathAnimationLifeTime = 3f;
    public float fadeDuration = 1f;
    private GameObject deathAnimationInstance;


    // Start is called before the first frame update
    void Start()
    {
        // Error management + logging
        if (bulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }
        
        InitializeHealthManager();
        nav = GetComponent<NavMeshAgent>();
        // Set first destination
        nav.SetDestination(destinationList[0].transform.position);
        nav.speed = moveSpeed;

        // NPC initialises in patrol state
        curState = FSMState.Patrol;

        elapsedTime = shootRate;

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
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
        }

        elapsedTime += Time.deltaTime;

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
     *   Patrol State
     */
    void UpdatePatrolState()
    {
        //  NPC running animation
        _animator.Play("Base Layer.running");

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
        //  Attack state when player enters attack range and is in view
        if ((Vector3.Distance(transform.position, playerTransform.position) <= attackRange) && PlayerInView())
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
        //  NPC walking animation
        _animator.Play("Base Layer.walking");

        setDestinationTime += Time.deltaTime;

        if (setDestinationTime > setDestinationWaitTime)
        {
            //  NPC can see player
            if (PlayerInView())
            {
                playerOutOfSightTime = 0f;

                //  Follow player
                nav.isStopped = false;
                nav.SetDestination(playerTransform.position);
                setDestinationTime = 0;

                ShootingPlayer();

            }
            //  NPC cannot see player
            else
            {
                //  NPC freezes for a moment
                playerOutOfSightTime += Time.deltaTime;
                nav.isStopped = true;
                _animator.CrossFade("Base Layer.idle", maxOutOfSightTime, 0, 0);

                //  NPC reaction buffer to no longer seeing player
                if (playerOutOfSightTime > maxOutOfSightTime)
                {
                    nav.isStopped = false;
                    nav.SetDestination(destinationList[currentDestination].transform.position);
                    nav.speed = moveSpeed;
                    curState = FSMState.Patrol;
                    return;
                }
            }
        }

        //  Transitions
        //  Patrol state when player exits attack range
        if (Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            nav.speed = moveSpeed;
            curState = FSMState.Patrol;
        }

    }


    /*
     *  Dead State
     */
    void UpdateDeadState()
    {
        if (!isDead)
        {
            isDead = true;
            playerController.AddMoneyAndScore(50, 100);

            _animator.Play("Base Layer.dies");

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

    /*  Returns if the player is in view via raycast  */
    private bool PlayerInView()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;

        LayerMask visionMask = ~LayerMask.GetMask("Bullets", "Enemy");

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, attackRange, visionMask))
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
            bulletPoolManager.Shooting(bulletSpawnpoint.transform.position);
        }
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
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

using System.Collections;
using System.Collections.Generic;
//using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GhostBehaviour : MonoBehaviour, InterfaceEnemy
{
    //
    //  Ghosts need a special navmesh that is baked without walls, and a special accompanying navmesh agent ("Ghost" Agent Type)
    //


    //  NPC states
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
    private PlayerController playerController;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;
    private Animator _animator;

    //  Current NPC state
    public FSMState curState;

    //  Total times the NPC can get hit until destruction
    public float health = 10;
    public float Health => health;

    private bool isDead = false;

    [Header("Score Settings")]
    public int points = 200;

    [Header("Health")]
    private HealthManager healthManager;

    //  Range variables
    [Header("Ranges")]
    public float attackRange = 20;

    //  Movement variables
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float targettingSpeed = 2f;

    //  Bullet variables (NOTE: most bullet variables are in their respective bullet script)
    [Header("Bullet Related Variables")]
    public GameObject bulletSpawnpoint;
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.5f;
    private float elapsedTime;

    //  Animation states
    [Header("Imported Animation Objects")]
    public GameObject deathAnimation;
    public float deathAnimationLifeTime = 3f;
    public float fadeDuration = 1f;
    private GameObject deathAnimationInstance;
    private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
    private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
    private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");

    // Start is called before the first frame update
    void Start()
    {
        //  Error management + logging
        if (bulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }

        InitializeHealthManager();
        nav = GetComponent<NavMeshAgent>();
        //  Set first destination
        nav.SetDestination(destinationList[0].transform.position);
        nav.speed = moveSpeed;

        //  NPC initialises in patrol state
        curState = FSMState.Patrol;

        elapsedTime = shootRate;

        //  Locates the player before initialisation
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObj.transform;
        playerController = playerObj.GetComponent<PlayerController>();

        //  Calls rigidbody before initialisation
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
        //  NPC walking animation
        //_animator.CrossFade("Base Layer.move", 0.1f, 0, 0);

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
        if (!isDead)
        {
            isDead = true;
            playerController.AddMoneyAndScore(100, 200);

            _animator.Play("Base Layer.dissolve");

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
            //  attacking animation per shot
            _animator.CrossFade(AttackState, shootRate, 0, 0);
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

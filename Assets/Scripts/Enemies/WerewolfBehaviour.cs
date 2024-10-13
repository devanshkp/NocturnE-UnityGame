using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WerewolfBehaviour : MonoBehaviour, InterfaceEnemy
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

    private bool isDead = false;

    //  Total times the NPC can get hit until destruction
    public float health = 20;
    public float Health => health;

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
    private Animator werewolf_animator;
    private Animator human_animator;

    [Header("Score Settings")]
    public int wereworlfPoints = 500;
    public int humanPoints = 100;

    [Header("Health")]
    private HealthManager healthManager;

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
    public GameObject werewolfBulletSpawnpoint;
    public GameObject humanBulletSpawnpoint;
    public BulletPoolManager bulletPoolManager;
    public SineBulletPoolManager sinBulletPoolManager;
    public float shootRate = 0.2f;
    private float elapsedTime;

    [Header("Level Manager and NPC Models")]
    public LevelManager levelManager;
    public GameObject humanModel;
    public GameObject werewolfModel;

    [Header("Imported Animation Objects")]
    public GameObject smallDeathAnimation;
    public GameObject largeDeathAnimation;
    public float deathAnimationLifeTime = 3f;
    public float fadeDuration = 1f;
    private GameObject deathAnimationInstance;


    // Start is called before the first frame update
    void Start()
    {
        // Error management + logging
        if (sinBulletPoolManager == null)
        {
            Debug.Log("No sine bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        } else if (bulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }

        //  When a manager is applied, set NPC to idle
        //  Determine which model to use depending on the time
        if (levelManager == null)
        {
            Debug.Log("No level manager, please add the manager");
        }
        else
        {
            curState = FSMState.Idle;

            if (levelManager.isNightTime)
            {
                humanModel.SetActive(false);
                werewolfModel.SetActive(true);
            }
            else
            {
                humanModel.SetActive(true);
                werewolfModel.SetActive(false);
            }
        }

        InitializeHealthManager();
        nav = GetComponent<NavMeshAgent>();
        nav.speed = dayMoveSpeed;
        nav.isStopped = true;

        elapsedTime = shootRate;

        // Locates the player before initialisation
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Calls rigidbody before initialisation
        _rigidbody = GetComponent<Rigidbody>();

        werewolf_animator = werewolfModel.GetComponent<Animator>();
        human_animator = humanModel.GetComponent<Animator>();
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
        if (levelManager.isNightTime)
        {
            werewolfModel.SetActive(true);
            humanModel.SetActive(false);

            werewolf_animator.Play("Base Layer.lookaround");
        }
        else
        {
            werewolfModel.SetActive(false);
            humanModel.SetActive(true);

            human_animator.Play("Base Layer.idle");
        }

        nav.isStopped = true;
        nav.SetDestination(transform.position);
        IdleActions();
    }

    /*
     *   Run Away State
     */
    void UpdateDayRunAwayState()
    {
        //  NPC running animation
        human_animator.Play("Base Layer.running");

        nav.isStopped = false;
        nav.speed = dayMoveSpeed;

        //  NPC runs some units in the opposite direction to player 
        Vector3 oppositeDirection = -(playerTransform.position - transform.position).normalized;
        oppositeDirection.y = 0;

        Vector3 runAwayDestination = transform.position + oppositeDirection * 5f;

        nav.SetDestination(runAwayDestination);

        IdleActions();
    }

    /*
     *   Daytime Attacking State
     */
    void UpdateDayAttackState()
    {
        //  NPC idles
        human_animator.CrossFade("Base Layer.idle", 0.1f, 0, 0);

        //  NPC freezes position while attacking
        nav.isStopped = true;
        nav.speed = 0;

        Vector3 direction = (playerTransform.position - transform.position).normalized;

        direction.y = 0;

        //  Rotates to face player
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        //  Light of sight check + transitions
        IdleActions();

        setDestinationTime += Time.deltaTime;

        if(setDestinationTime > setDestinationWaitTime)
        {
            //  Follow player
            nav.SetDestination(playerTransform.position);
            setDestinationTime = 0;

            DayShootingPlayer();
        }

        //  NPC doesn't chase to the player after attacking
        if (Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            nav.SetDestination(transform.position);
        }
    }

    /*
     *   Night time Attacking State
     */
    void UpdateNightAttackState()
    {
        nav.isStopped = false;
        nav.speed = nightTargettingSpeed;

        //  Light of sight check + transitions
        IdleActions();

        setDestinationTime += Time.deltaTime;

        if (setDestinationTime > setDestinationWaitTime)
        {
            //  Follow player
            nav.SetDestination(playerTransform.position);
            setDestinationTime = 0;

            NightShootingPlayer();
        }

        //  NPC doesn't chase to the player after attacking
        if (Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            nav.SetDestination(transform.position);
        }

        IdleActions();
    }

    /*
     *   Chasing State
     */
    void UpdateNightChaseState()
    {
        //  NPC running animation
        werewolf_animator.Play("Base Layer.running2");

        nav.isStopped = false;
        nav.speed = nightMoveSpeed;

        IdleActions();

        setDestinationTime += Time.deltaTime;

        if (setDestinationTime > setDestinationWaitTime)
        {
            //  Follow player
            nav.SetDestination(playerTransform.position);
            setDestinationTime = 0;
        }
    }

    /*
     *   Dead State
     */
    void UpdateDeadState()
    {
        if (!isDead && werewolfModel.activeInHierarchy)
        {
            isDead = true;

            werewolf_animator.Play("Base Layer.down");

            //  Spawn effect at enemy position
            deathAnimationInstance = Instantiate(largeDeathAnimation, transform.position, Quaternion.identity);

            //  Play effect and fade out
            StartCoroutine(DestroyEffect());

            //
            //  Reward werewolf specific points to player score here
            //
        }
        else if (!isDead && humanModel.activeInHierarchy)
        {
            isDead = true;

            human_animator.Play("Base Layer.dies");

            //  Spawn effect at enemy position
            deathAnimationInstance = Instantiate(smallDeathAnimation, transform.position, Quaternion.identity);

            //  Play effect and fade out
            StartCoroutine(DestroyEffect());

            //
            //  Reward human specific points to player score here
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

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, Mathf.Infinity, visionMask))
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red);

            if (hit.transform == playerTransform)
            {
                return true;
            }
        }
        return false;
    }

    /*  Shooting method for the night  */
    private void NightShootingPlayer()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= shootRate)
        {
            elapsedTime = 0;
            //  NPC attacking animation
            werewolf_animator.CrossFade("Base Layer.attack2", 0.1f, 0, 0);
            sinBulletPoolManager.Shooting(werewolfBulletSpawnpoint.transform.position);
        }
    }

    /*  Shooting method for the day  */
    private void DayShootingPlayer()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= shootRate)
        {
            elapsedTime = 0;
            bulletPoolManager.Shooting(humanBulletSpawnpoint.transform.position);
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
        //  NPC cannot see player
        else
        {
            playerOutOfSightTime += Time.deltaTime;

            //  NPC reaction buffer to no longer seeing player
            if(playerOutOfSightTime > maxOutOfSightTime)
            {
                curState = FSMState.Idle;
            }
        }
    }

    private void Transitions()
    {
        //  Day time & not attackable
        if ((levelManager.isNightTime == false) && Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            werewolfModel.SetActive(false);
            humanModel.SetActive(true);
            curState = FSMState.DayRunAway;

            //  Day time & attackable
        }
        else if ((levelManager.isNightTime == false) && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            werewolfModel.SetActive(false);
            humanModel.SetActive(true);
            curState = FSMState.DayAttack;

            //  Night time & not attackable
        }
        else if ((levelManager.isNightTime == true) && Vector3.Distance(transform.position, playerTransform.position) > attackRange)
        {
            werewolfModel.SetActive(true);
            humanModel.SetActive(false);
            curState = FSMState.NightChase;

            //  Night time & attackable
        }
        else if ((levelManager.isNightTime == true) && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            werewolfModel.SetActive(true);
            humanModel.SetActive(false);
            curState = FSMState.NightAttack;
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

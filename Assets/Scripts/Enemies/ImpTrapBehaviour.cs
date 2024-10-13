using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ImpTrapBehaviour : MonoBehaviour, InterfaceEnemy
{
    //  Total times the NPC can get hit until destruction
    public float health = 10;
    public float Health => health;

    private bool isDead = false;

    //  Destination buffer variables
    private float setDestinationTime = 0;
    //  rate at which destination is checked (sec)
    private float setDestinationWaitTime = 0.1f;

    private Transform playerTransform;
    private PlayerController playerController;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;

    [Header("Score Settings")]
    public int points = 50;

    [Header("Health")]
    private HealthManager healthManager;

    // Imports the bullet manager
    [Header("Bullet Related Variables")]
    public GameObject bulletSpawnpoint;
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.5f;
    private float elapsedTime;

    [Header("Imp Controller")]
    public GameObject impController;
    public ImpBehaviour impBehaviour;

    [Header("Imported Animation Objects")]
    public GameObject deathAnimation;
    public float deathAnimationLifeTime = 1.5f;
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
        nav.isStopped = true;

        elapsedTime = shootRate;

        // Locates the player before initialisation
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObj.transform;
        playerController = playerObj.GetComponent<PlayerController>();

        // Calls rigidbody before initialisation
        _rigidbody = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    void Update()
    {
        setDestinationTime += Time.deltaTime;

        if (setDestinationTime > setDestinationWaitTime)
        {
            //  Imp controlling trap can see player
            if (impBehaviour.PlayerInView(impController.transform.position))
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;

                direction.y = 0;

                //  Rotates to face player
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

                Shooting();
            }

        }

        if (healthManager.GetHealth() <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        if (!isDead)
        {
            isDead = true;
            playerController.AddMoneyAndScore(10, 50);

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

    private void Shooting()
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
}

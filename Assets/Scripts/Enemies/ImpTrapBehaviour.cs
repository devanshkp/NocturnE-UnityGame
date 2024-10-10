using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ImpTrapBehaviour : MonoBehaviour, InterfaceEnemy
{
    //  Total times the NPC can get hit until destruction
    public float health = 10;
    public float Health => health;

    //  Destination buffer variables
    private float setDestinationTime = 0;
    //  rate at which destination is checked (sec)
    private float setDestinationWaitTime = 0.1f;

    private Transform playerTransform;
    private Rigidbody _rigidbody;
    private NavMeshAgent nav;

    // Imports the bullet manager
    [Header("Bullet Related Variables")]
    public GameObject bulletSpawnpoint;
    public BulletPoolManager bulletPoolManager;
    public float shootRate = 0.5f;
    private float elapsedTime;

    [Header("Imp Controller")]
    public GameObject impController;
    public ImpBehaviour impBehaviour;

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
        nav.isStopped = true;

        elapsedTime = shootRate;

        // Locates the player before initialisation
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

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

        if (health <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        //  Destroy (or deactivate?) here
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
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
}

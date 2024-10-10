using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBullet : MonoBehaviour
{
    // BULLET INFORMATION
    public float speed = 10.0f;
    public float lifeTime = 5.0f;
    public int damage = 10;

    // FIRE DAMAGE INFORMATION
    public float fireTickDamage = 1f;           // damage taken every 'tick'
    public float fireTickRate = 0.1f;           // The rate each 'tick' of damage occurs in (each x seconds)
    public int fireLifeTime = 3;                // Total time fire is damaging

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //  Use a wrapper class to get all the vars
            other.SendMessage("TakeFireDamage", damage, fireTickDamage, fireTickRate, fireLifeTime);
        }
    }
}

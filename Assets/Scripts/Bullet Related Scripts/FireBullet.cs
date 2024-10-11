using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBulletInfo
{
    [Header("Fire Settings")]
    public float fireTickDamage = 0.5f;           // damage taken every 'tick'
    public float fireTickRate = 0.3f;           // delay per damage tick
    public int fireLifeTime = 5;                // Total time fire is damaging

    public FireBulletInfo(float fireTickDamage, float fireTickRate,int fireLifeTime)
    {
        this.fireTickDamage = fireTickDamage;
        this.fireTickRate = fireTickRate;
        this.fireLifeTime = fireLifeTime;
    }
}

public class FireBullet : MonoBehaviour
{
    [Header("Fire Settings")]
    public float fireTickDamage = 0.5f;           // damage taken every 'tick'
    public float fireTickRate = 0.3f;           // delay per damage tick
    public int fireLifeTime = 5;                // Total time fire is damaging

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FireBulletInfo damageInfo = new FireBulletInfo(fireTickDamage, fireTickRate, fireLifeTime);
            other.SendMessage("TakeFireDamage", damageInfo);
        }
    }
}

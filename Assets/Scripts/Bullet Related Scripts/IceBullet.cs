using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBulletInfo
{
    [Header("Ice Settings")]
    public float iceTickDamage = 0.2f;           //   Damage taken every 'tick'
    public float iceTickRate = 0.3f;             //  Delay per tick of damage
    public float movementModifier = 0.5f;     //   Modifier to multiply the player speed, decreasing movement speed
    public float iceLifeTime = 2;                //   Total time ice is in effect (damage and movement modifier)

    public IceBulletInfo(float iceTickDamage, float iceTickRate, float movementModifier, float icelifeTime)
    {
        this.iceTickDamage = iceTickDamage;
        this.iceTickRate = iceTickRate;
        this.movementModifier = movementModifier;
        this.iceLifeTime = icelifeTime;
    }
}

public class IceBullet : MonoBehaviour
{
    [Header("Ice Settings")]
    public float iceTickDamage = 0.2f;           //   Damage taken every 'tick'
    public float iceTickRate = 0.3f;             //  Delay per tick of damage
    public float movementModifier = 0.5f;     //   Modifier to multiply the player speed, decreasing movement speed
    public float iceLifeTime = 2;                //   Total time ice is in effect (damage and movement modifier)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            IceBulletInfo damageInfo = new IceBulletInfo(iceTickDamage, iceTickRate, movementModifier, iceLifeTime);
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.TakeIceDamage(damageInfo);
        }
    }
}

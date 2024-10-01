using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBullet : MonoBehaviour
{
    // BULLET INFORMATION
    public float speed = 10.0f;
    public float lifeTime = 5.0f;
    public int damage = 10;

    // ICE DAMAGE INFORMATION
    public float iceTickDamage = 1f;           //   Damage taken every 'tick'
    public float iceTickRate = 0.1f;           //   Rate each 'tick' of damage occurs in (each x seconds)
    public float movementModifier = 0.75f;     //   Modifier to multiply the player speed, decreasing movement speed
    public int iceLifeTime = 3;                //   Total time ice is in effect (damage and movement modifier)

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

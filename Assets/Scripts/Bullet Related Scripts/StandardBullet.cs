using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardBullet : MonoBehaviour
{
    // BULLET INFORMATION
    public float speed = 10.0f;
    public float lifeTime = 5.0f;
    public int damage = 10;

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
            gameObject.SetActive(false);
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.TakeDamage(damage);
        }
    }
}


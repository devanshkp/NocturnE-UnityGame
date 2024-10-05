using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int damageAmount = 10;  // Amount of damage dealt to enemies

    // Detect enemies when they enter the sword's trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we collided with has the Enemy component (or any other tag you use)
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit!");
            // Try to get the enemy's health component
            // EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            // if (enemyHealth != null)
            // {
            //     // Deal damage to the enemy
            //     enemyHealth.TakeDamage(damageAmount);
            // }

            // // Send the "TakeDamage" message to the object with the amount of damage as a parameter
            // other.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
        }
    }
}


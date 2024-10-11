using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int damageAmount = 10;  // Amount of damage dealt to enemies
    public PlayerController playerController; //  Player Controller script
    private bool isSlashing;
    private bool isDamaging = false;
    private float slashCooldown = 0.5f;  // Cooldown time between each slash to apply damage
    private float cooldownTimer = 0f;

    private void Update()
    {
        isSlashing = playerController.isSlashing;
        // If currently in cooldown, update the cooldown timer
        if (isDamaging){
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0){
                // Reset the damaging flag
                isDamaging = false;
                cooldownTimer = 0f;
            }
        }
    }

    // Detect enemies when they enter the sword's trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Object collides with enemy while slashing, checking once per slash
        if (other.CompareTag("Enemy") && isSlashing && !isDamaging)
        {
            isDamaging = true;
            cooldownTimer = slashCooldown;

            //  Checks enemy uses health interface, remembering trigger is a child of the actual enemy
            InterfaceEnemy enemy = other.gameObject.GetComponentInParent<InterfaceEnemy>();

            //  Get enemy health from trigger's parent and deal damage on that parent
            if(enemy != null)
            {
                float enemyHealth = enemy.Health;

                print("Hit!");

                // Imported Imp asset error handling
                if (other.name.Contains("Imp"))
                {
                    other.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    other.transform.parent.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
                }
                


            }

            // Try to get the enemy's health component
            // EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            // if (enemyHealth != null)
            // {
            //     // Deal damage to the enemy
            //     enemyHealth.TakeDamage(damageAmount);
            // }

            // // Send the "TakeDamage" message to the object with the amount of damage as a parameter
            // other.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);

        //  Sword idling (no slash)
        } else if (other.CompareTag("Enemy") && isSlashing == false)
        {
            isDamaging = false;
            //Debug.Log("Idling");
        }
        //  Update damaging bool
        else
        {
            isDamaging = false;
        }
    }
}


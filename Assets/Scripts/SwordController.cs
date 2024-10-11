using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int damageAmount = 10;  // Amount of damage dealt to enemies
    public PlayerController playerController; //  Player Controller script
    private Collider swordCollider; // Reference to the sword's collider
    private bool isSlashing;
    private bool isDamaging = false;
    private float slashCooldown = 1.5f;  // Cooldown time between each slash to apply damage
    private float stationarySlashCooldown = 1.875f;
    private float cooldownTimer = 0f;

    // Keep track of already hit enemies
    private HashSet<Collider> hitEnemies = new HashSet<Collider>();

    void Start()
    {
        // Get the sword's collider component
        swordCollider = GetComponent<Collider>();
        swordCollider.enabled = false;
    }

    // Detect enemies when they enter the sword's trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Object collides with enemy while slashing, checking once per slash
        if (other.CompareTag("Enemy") && !isDamaging && !hitEnemies.Contains(other))
        {
            isDamaging = true;
            cooldownTimer = playerController.stationarySlash ? stationarySlashCooldown : slashCooldown;

            hitEnemies.Add(other);

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

        }
        else
        {
            isDamaging = false;
        }
    }

    // Functions to be called as animation events
    public void ResetCoolDown()
    {
        isDamaging = false;
        hitEnemies.Clear(); // Clear registry for next attack
    }

    public void EnableSwordCollider()
    {
        swordCollider.enabled = true;
    }

    public void DisableSwordCollider()
    {
        swordCollider.enabled = false;
    }
}


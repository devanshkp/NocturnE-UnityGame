using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public GameObject character;
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float lerpSpeed = 20f;
    private float diffMultiplier;
    public bool isEnemy;
    private GameObject healthBar;

    void Start()
    {
        currentHealth = maxHealth;
    
        if (isEnemy){
            Transform enemyCanvas = character.GetComponentInChildren<Canvas>().transform;
            healthBar = enemyCanvas.Find("Health Bar").gameObject;
        }
    }

    void Update()
    {
        if (healthSlider.value != currentHealth)
            healthSlider.value = currentHealth;
        if (healthSlider.value != easeHealthSlider.value){
            if (healthSlider.value == 0){
                healthBar.SetActive(false);
            }
            else{
                SetLerpSpeed(currentHealth - easeHealthSlider.value);
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed * Time.deltaTime);
            }
        }
    }

    void SetLerpSpeed(float healthDifference)
    {
        // Calculate the percentage of health difference relative to maxHealth
        float healthPercentage = (healthDifference / maxHealth) * 100f;

        if (healthPercentage >= 60)
            lerpSpeed = 3f;
        else if (healthPercentage >= 20 && healthPercentage < 60)
            lerpSpeed = 2.5f;
        else
            lerpSpeed = 2f;
    }


    public void TurnOffHealthBar()
    {
        healthBar.SetActive(false);
    }

    public void SetMaxHealth(float maxEnemyHealth)
    {
        maxHealth = maxEnemyHealth;
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void UpdateHealth(float healthDelta)
    {
        currentHealth += healthDelta;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}

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
    private EnemyUIManager enemyUIManager;
    private GameObject healthBar;
    private bool isHealthBarVisible;

    void Start()
    {
        currentHealth = maxHealth;
    
        if (isEnemy)
            enemyUIManager = character.GetComponentInChildren<EnemyUIManager>();
    }

    void Update()
    {
        if (healthSlider.value != currentHealth)
            healthSlider.value = currentHealth;

        if (easeHealthSlider.value != currentHealth){
            if (isEnemy) {
                Debug.Log("Difference");
                Debug.Log($"Ease Health Slider Value: {easeHealthSlider.value}, Current Health: {currentHealth}");
            }
            SetLerpSpeed(easeHealthSlider.value - currentHealth);
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed * Time.deltaTime);
        }
    }

    // IEnumerator Lerp()
    // {
    //     float time = 0;
    //     while (time < 1){
    //         easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, time);
    //         time += Time.deltaTime;
    //         yield return null;
    //     }   
    //     easeHealthSlider.value = currentHealth;
    // }

    void SetLerpSpeed(float healthDifference)
    {
        // Calculate the percentage of health difference relative to maxHealth
        if (isEnemy) Debug.Log(healthDifference);
        float healthPercentage = (healthDifference / maxHealth) * 100f;
        if (healthPercentage >= 60)
            lerpSpeed = 3f;
        else if (healthPercentage >= 20 && healthPercentage < 60)
            lerpSpeed = 2.5f;
        else
            lerpSpeed = 2f;

        // if (healthPercentage >= 60)
        //     lerpSpeed = .075f;
        // else if (healthPercentage >= 20 && healthPercentage < 60)
        //     lerpSpeed = .0625f;
        // else
        //     lerpSpeed = 0.05f;
    }


    public void TurnOffHealthBar()
    {
        if (enemyUIManager != null)
            enemyUIManager.DisableHealthBar(); 
    }

    public void SetMaxHealth(float maxEnemyHealth)
    {
        maxHealth = maxEnemyHealth;
        currentHealth = maxHealth;
        // Set slider maximum values
        healthSlider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;

        // Initialize sliders to max health
        healthSlider.value = maxHealth;
        easeHealthSlider.value = maxHealth;
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void UpdateHealth(float healthDelta)
    {  
        currentHealth += healthDelta;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (isEnemy && currentHealth < maxHealth && !isHealthBarVisible){
            isHealthBarVisible = true;
            enemyUIManager.EnableHealthBar();
        }
    }
}

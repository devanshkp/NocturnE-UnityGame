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

    private bool healthBarVisible = false;
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
            setLerpSpeed(currentHealth - easeHealthSlider.value);
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed * Time.deltaTime);
        }
    }

    void setLerpSpeed(float healthDifference)
    {
        if (healthDifference >= 60 && healthDifference <= 100)
            lerpSpeed = 3f;
        else if (healthDifference >= 20 && healthDifference < 60)
            lerpSpeed = 2.5f;
        else
            lerpSpeed = 2f;
    }

    public void applyDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0){
            currentHealth = 0;
            character.SendMessage("Die");
        }
        if (isEnemy && currentHealth < maxHealth && !healthBarVisible)
            healthBar.SetActive(true);
    }
}

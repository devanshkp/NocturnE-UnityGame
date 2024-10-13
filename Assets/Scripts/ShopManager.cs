using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopPrompt;  // Reference to the UI prompt to open the shop
    public GameObject shopUI;      // Reference to the shop UI that displays shop items
    public GameObject shopAlert;
    public Transform player;       // Reference to the player transform
    private bool isPlayerInRange = false;   // Tracks if the player is in range of the shop

    private PlayerController playerController; // Reference to the player's controller for buffs
    private BuffManager buffManager;

    void Start()
    {
        if (shopPrompt == null){
            shopPrompt = transform.Find("ShopCanvas/ShopPrompt").gameObject;
        }
        if (shopUI == null){
            shopUI = transform.Find("ShopCanvas/Shop").gameObject;
        }
        if (shopAlert == null){
            shopAlert = transform.Find("ShopCanvas/Shop/Alert").gameObject;
        }
        shopPrompt.SetActive(false);
        shopAlert.SetActive(false);
        shopUI.SetActive(false);

    }

    void Update()
    {
        // Check if the player is in range and presses E to open the shop
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        // Toggle the shop UI on or off
        shopUI.SetActive(!shopUI.activeSelf);
        if (shopUI.activeSelf)
        {
            playerController.isShopOpen = true;
        }
        else
        {
            playerController.isShopOpen = false;
        }
    }

    public void ApplyDoubleJump()
    {
        if (playerController.doubleJump){
            if (!shopAlert.activeSelf){
                StartCoroutine(DisplayAlert());
            }
            return;
        }
        if (playerController.money < 500){
            return;
        }
        playerController.doubleJump = true;
        playerController.money -= 500;
        buffManager.EnableBuff(BuffType.JumpBuff);
    }

    public void ApplySpeedBuff()
    {
        if (playerController.speedBuff){
            if (!shopAlert.activeSelf){
                StartCoroutine(DisplayAlert());
            }
            return;
        }
        if (playerController.money < 1000){
            return;
        }
        playerController.speedBuff = true;
        playerController.money -= 1000;
        buffManager.EnableBuff(BuffType.SpeedBuff);
    }

    public void ApplyDamageBuff()
    {
        if (playerController.damageBuff){
            if (!shopAlert.activeSelf){
                StartCoroutine(DisplayAlert());
            }
            return;
        }
        if (playerController.money < 2000){
            return;
        }
        playerController.damageBuff = true;
        playerController.money -= 2000;
        buffManager.EnableBuff(BuffType.DamageBuff);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ensure the player enters the trigger collider
        if (other.CompareTag("Player"))
        {
            if (playerController == null){
                Debug.Log("Getting player and buff manager script objects.");
                playerController = other.GetComponent<PlayerController>();
                buffManager = other.GetComponentInChildren<BuffManager>();
            }
            isPlayerInRange = true;
            shopPrompt.SetActive(true);  // Show the prompt to the player
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Ensure the player exits the trigger collider
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            shopPrompt.SetActive(false);  // Hide the prompt when player leaves
            shopUI.SetActive(false);  // Ensure the shop UI is hidden if the player leaves the area
        }
    }

    IEnumerator DisplayAlert()
    {
        shopAlert.SetActive(true);
        float timer = 0f;
        while (timer < 3f){
            timer += Time.deltaTime;
            yield return null;
        }
        shopAlert.SetActive(false);
    }
}

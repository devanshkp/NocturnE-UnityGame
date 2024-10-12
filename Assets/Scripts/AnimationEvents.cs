using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public SwordController swordController;
    public PlayerController playerController;

    void Start()
    {
        if (swordController == null){
            swordController = GetComponentInChildren<SwordController>();
        }
        if (playerController == null){
            playerController = GetComponentInParent<PlayerController>();
        }
    }

    public void EnableSwordCollider()
    {
        swordController.EnableSwordCollider();
    }

    public void DisableSwordCollider()
    {
        swordController.DisableSwordCollider();
    }

    public void ResetCoolDown()
    {
        swordController.ResetCoolDown();
    }

    public void RestartLevel()
    {
        playerController.EndLevel();
    }

    public void EndAutoTargetState()
    {
        playerController.StopAutoTargeting();
    }
}

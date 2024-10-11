using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public SwordController swordController;

    void Start()
    {
        if (swordController == null){
            swordController = GetComponentInChildren<SwordController>();
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
}

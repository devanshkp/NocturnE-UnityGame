using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    public bool isCompleted = false;

    private void OnTriggerEnter(Collider other)
    {
        //  When the player exits, trigger
        if (other.CompareTag("Player") && isCompleted == false)
        {
            print("level completed");
            isCompleted = true;
        }
    }
}


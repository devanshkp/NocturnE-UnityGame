using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Leaderboard : MonoBehaviour
{
    public TMP_Text leaderboardText;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        playerController = FindObjectOfType<PlayerController>();

        if (playerController == null)
        {
            Debug.Log("Error, cannot find playercontroller");
        }

        DisplayLeaderboard();
    }

    public void DisplayLeaderboard()
    {
        leaderboardText.text = "";

        float score = playerController.score;
        
        if (score != 0)
        {
            leaderboardText.text = score.ToString();
        }
         
        else
        {
            leaderboardText.text = "No score recorded yet";
        }
    }
}

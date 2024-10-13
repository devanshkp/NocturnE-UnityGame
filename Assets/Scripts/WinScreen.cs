using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinScreen : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text scoreText;

    void Start()
    {
        float playerScore = PlayerPrefs.GetFloat("PlayerScore", 0f);
        scoreText.text = playerScore.ToString("N0");
    }
}

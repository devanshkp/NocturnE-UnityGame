using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GUIManager : MonoBehaviour
{
    public TMP_Text scoreText;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        if (SceneManager.GetActiveScene().name == "Win Screen"){
            float playerScore = PlayerPrefs.GetFloat("PlayerScore");
            scoreText.text = playerScore.ToString("N0");
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        print("Playerprefs gone");
    }

    public void LoadMenu(string menu)
    {
        SceneManager.LoadScene(menu);
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void LoadLeaderboard(string leaderboard)
    {
        SceneManager.LoadScene(leaderboard);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}


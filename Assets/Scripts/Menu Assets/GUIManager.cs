using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
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


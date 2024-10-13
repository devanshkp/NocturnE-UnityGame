using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//
//  Use this for levels following the rest zone, otherwise use LevelManager
//

public class LevelLoader : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //  Add new else if for future levels
        if (PlayerPrefs.GetInt("LevelsCompleted", 0) == 0)
        {
            SceneManager.LoadScene("Level 1");
        }
        else if (PlayerPrefs.GetInt("LevelsCompleted", 0) == 1)
        {
            SceneManager.LoadScene("Level 2");
        }
        else if (PlayerPrefs.GetInt("LevelsCompleted", 0) == 2)
        {
            SceneManager.LoadScene($"Level 3");
        }
    }
}

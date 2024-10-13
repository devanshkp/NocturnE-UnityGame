using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//
//  Use this for levels following the rest zone, otherwise use LevelManager
//

public class LevelLoader : MonoBehaviour
{
    public LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        if (levelManager == null)
        {
            Debug.Log("No level manager assigned - please assign a level manager. Deactivating the level loader");
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //  Add new else if for future levels
        if (levelManager.levelNumber == 0)
        {
            SceneManager.LoadScene("Level 1");
        }else if (levelManager.levelNumber == 1)
        {
            SceneManager.LoadScene($"Level {levelManager.levelNumber}");
        }else if (levelManager.levelNumber == 2)
        {
            SceneManager.LoadScene($"Level {levelManager.levelNumber}");
        }
    }
}

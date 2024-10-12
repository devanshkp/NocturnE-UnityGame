using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the tag "Player"
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "SampleScene"){
                SceneManager.LoadScene("Test Level of Ghosts");
            }
            else{
                SceneManager.LoadScene("Level");
            }
        }
    }
}

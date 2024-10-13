using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Gate Variables")]
    public GameObject gate;
    public LevelEndTrigger levelEndTrigger;
    public bool gateOpen = false;
    public int levelNumber;

    [Header("Level Enemies")]
    public GameObject[] enemiesList;

    [Header("In-Game Time Settings")]
    public bool isNightTime = false;
    public Light dayLight;
    public Material daySkybox;
    public Material nightSkybox;
    public float currentTime = 0f;
    public float lightTransitionDuration = 20f;
    public float dayHoldTime = 60f;     //  time to stay fully day in seconds
    public float nightHoldTime = 60f;   //  time to stay fully night in seconds
    private float lightIntensity = 1f;
    private enum CyclePhase { DayHold, DayToNight, NightHold, NightToDay }
    private CyclePhase currentPhase;

    private Transform playerTransform;


    // Start is called before the first frame update
    void Start()
    {
        if (enemiesList.Length == 0)
        {
            Debug.Log("No enemies assigned to the level manager - please add all the enemies to the level manager. Deactivating the level manager");
            gameObject.SetActive(false);
        }

        if (levelNumber == 0)
        {
            Debug.Log("No level number assigned - please assign a level number. Deactivating the level manager");
            gameObject.SetActive(false);
        }

        currentTime = 0f;
        currentPhase = CyclePhase.DayHold;

        isNightTime = false;

        UpdateTimeCycle();
    }

    // Update is called once per frame
    void Update()
    {
        //  Level win condition check
        if(gateOpen == true)
        {
            //  Let player through the gate
            /*gate.SetActive(false);*/

            //  Deactivate ALL level enemies on exitting level, but no score rewarded
            if(levelEndTrigger.isCompleted)
                
                PlayerClearsLevel();
                /*for (int i = 0; i < enemiesList.Length; i++)
                {
                    //  Transition to next scene -> Rest Area or something else
                    //Destroy(enemiesList[i]);
                }*/
        }


        currentTime += Time.deltaTime;

        // Check for phase progression
        if (currentPhase == CyclePhase.DayHold && currentTime >= dayHoldTime)
        {
            currentPhase = CyclePhase.DayToNight;
            currentTime = 0f; // Reset timer for the next phase
        }
        else if (currentPhase == CyclePhase.DayToNight && currentTime >= lightTransitionDuration)
        {
            currentPhase = CyclePhase.NightHold;
            currentTime = 0f; // Reset timer for the next phase
            lightIntensity = 0f; // Ensure the intensity is set to 0 after transition
            isNightTime = true; // Set isNightTime to true
        }
        else if (currentPhase == CyclePhase.NightHold && currentTime >= nightHoldTime)
        {
            currentPhase = CyclePhase.NightToDay;
            currentTime = 0f; // Reset timer for the next phase
        }
        else if (currentPhase == CyclePhase.NightToDay && currentTime >= lightTransitionDuration)
        {
            currentPhase = CyclePhase.DayHold;
            currentTime = 0f; // Reset timer for the next phase
            lightIntensity = 1f; // Ensure the intensity is set to 1 after transition
            isNightTime = false; // Set isNightTime to false
        }

        UpdateTimeCycle();
    }

    public void UpdateTimeCycle()
    {
        switch (currentPhase)
        {
            case CyclePhase.DayHold:
                lightIntensity = 1f;
                RenderSettings.skybox = daySkybox;
                break;

            case CyclePhase.DayToNight:
                // Transitioning from Day to Night
                float dayToNightProgress = currentTime / lightTransitionDuration;
                lightIntensity = Mathf.Lerp(1f, 0f, dayToNightProgress);
                RenderSettings.skybox.Lerp(daySkybox, nightSkybox, dayToNightProgress);
                break;

            case CyclePhase.NightHold:
                lightIntensity = 0f;
                RenderSettings.skybox = nightSkybox;
                break;

            case CyclePhase.NightToDay:
                // Transitioning from Night to Day
                float nightToDayProgress = currentTime / lightTransitionDuration;
                lightIntensity = Mathf.Lerp(0f, 1f, nightToDayProgress);
                RenderSettings.skybox.Lerp(nightSkybox, daySkybox, nightToDayProgress);
                break;
        }

        // Set the light intensity
        if (dayLight != null)
            dayLight.intensity = lightIntensity;
    }

    public void PlayerClearsLevel()
    {
        if (SceneManager.GetActiveScene().name == "Level 3")
        {
            SceneManager.LoadScene("Win Screen");
        }
        else
        {
            SceneManager.LoadScene("Rest Zone");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class LevelManager : MonoBehaviour
{
    [Header("Gate Variables")]
    public GameObject level1Gate;
    public LevelEndTrigger level1EndTrigger;
    public bool level1GateOpen = false;
    public GameObject level2Gate;
    public LevelEndTrigger level2EndTrigger;
    public bool level2GateOpen = false;
    public GameObject level3Gate;
    public LevelEndTrigger level3EndTrigger;
    public bool level3GateOpen = false;

    [Header("Level Enemies")]
    public GameObject[] level1EnemiesList;
    public GameObject[] level2EnemiesList;
    public GameObject[] level3EnemiesList;

    /*public bool level2GateOpen = false;
    public bool level3GateOpen = false;*/

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
        /*if (enemiesList.Length == 0)
        {
            Debug.Log("No enemies assigned to the level manager - please add all the enemies to the level manager. Deactivating the level manager");
            gameObject.SetActive(false);
        }*/

        currentTime = 0f;
        currentPhase = CyclePhase.DayHold;

        isNightTime = false;

        UpdateTimeCycle();
    }

    // Update is called once per frame
    void Update()
    {
        if(level1GateOpen == true)
        {
            //  Let player through the gate
            level1Gate.SetActive(false);

            //  Deactivate ALL level enemies on exitting level, but no score rewarded
            if(level1EndTrigger.isCompleted)
                for (int i = 0; i < level1EnemiesList.Length; i++)
                {
                    level1EnemiesList[i].SetActive(false);
                }
        } else if (level2GateOpen == true)
        {
            //  Let player through the gate
            level2Gate.SetActive(false);

            //  Deactivate ALL level enemies on exitting level, but no score rewarded
            if (level2EndTrigger.isCompleted)
                for (int i = 0; i < level2EnemiesList.Length; i++)
                {
                    level2EnemiesList[i].SetActive(false);
                }
        } else if (level3GateOpen == true)
        {
            //  Let player through the gate
            level3Gate.SetActive(false);

            //  Deactivate ALL level enemies on exitting level, but no score rewarded
            if (level3EndTrigger.isCompleted)
                for (int i = 0; i < level3EnemiesList.Length; i++)
                {
                    level3EnemiesList[i].SetActive(false);
                }
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
        dayLight.intensity = lightIntensity;
    }
}

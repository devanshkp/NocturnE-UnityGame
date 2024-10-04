using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Gate Variables")]
    public GameObject level1Gate;
    public LevelEndTrigger levelEndTrigger;
    public bool level1GateOpen = false;

    [Header("Level Enemies")]
    public GameObject[] enemiesList;

    /*public bool level2GateOpen = false;
    public bool level3GateOpen = false;*/

    [Header("In-Game Time Check")]
    public bool isNightTime = false;

    private Transform playerTransform;


    // Start is called before the first frame update
    void Start()
    {
        if (enemiesList.Length == 0)
        {
            Debug.Log("No enemies assigned to the level manager - please add all the enemies to the level manager. Deactivating the level manager");
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(level1GateOpen == true)
        {
            //  Let player through the gate
            level1Gate.SetActive(false);

        //  Deactivate ALL level enemies on exitting level, but no score rewarded
        if(levelEndTrigger.isCompleted)
            for (int i = 0; i < enemiesList.Length; i++)
            {
                enemiesList[i].SetActive(false);
            }
        }
    }
}

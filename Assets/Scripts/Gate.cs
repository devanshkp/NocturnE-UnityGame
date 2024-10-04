using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("Gate Variables")]
    public float gateTimeMax;
    public float gateTime = 0;
    public float gateActivationRange = 30f;

    /*public bool level2GateOpen = false;
    public bool level3GateOpen = false;*/

    [Header("Gate Enemies")]
    public GameObject[] gateEnemyList;

    public LevelManager levelManager;

    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (gateEnemyList.Length == 0)
        {
            Debug.Log("No gate enemies assigned - please add the gate enemies to the list in the gate gameObject. Deactivating gate");
            gameObject.SetActive(false);
        }

        //  Deactivate gate enemies on start
        for (int i = 0; i < gateEnemyList.Length; i++)
        {
            gateEnemyList[i].gameObject.SetActive(false);
        }

        // Locates the player before initialisation
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        //  Activate defenses when player sees and is within gate activation range
        if (PlayerInView() && Vector3.Distance(transform.position, playerTransform.position) <= gateActivationRange)
        {
            //  Spawn gate enemy units when gate is under attack
            for (int i = 0; i < gateEnemyList.Length; i++)
            {
                gateEnemyList[i].gameObject.SetActive(true);
            }

            //  Start incrementing the gate timer
            gateTime += Time.deltaTime;

            if (gateTime >= gateTimeMax)
            {
                print("timer done");
                levelManager.level1GateOpen = true;
            }
        }
    }

    private bool PlayerInView()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;

        int layerMask = 1 << LayerMask.NameToLayer("Gate");
        layerMask = ~layerMask;

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, 100f, layerMask))
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red);

            if (hit.transform == playerTransform)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gateActivationRange);
    }
}

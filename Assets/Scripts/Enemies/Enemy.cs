using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 
    //
    //      Proto-Enemy tester, used for testing the relationship enemies have with bullet managers
    //      Will delete
    //
    //

    public BulletPoolManager BulletPoolManager;
    // How fast a bullet spawns (in seconds)
    public float shootRate = 0.5f;

    private float elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        // Error management + logging
        if (BulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //
        //  Delete debugging code and replace with actual enemy AI
        //

        elapsedTime += Time.deltaTime;

        /*      DEBUG: SHOOTING W/OUT INPUT      */

        if (elapsedTime > shootRate)
        {
            elapsedTime = 0;
            BulletPoolManager.Shooting();
        }

        /*      DEBUG: SHOOTING USING LEFT MOUSE BUTTON        */

        /*if(Input.GetButtonDown("Fire1") && elapsedTime > shootRate)
        {
            elapsedTime = 0;
            BulletPoolManager.Shooting();
        }*/
    }
}

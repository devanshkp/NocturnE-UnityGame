using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnemy : MonoBehaviour
{
    public NewBulletPoolManager BulletPoolManager;
    public float shootRate = 1.0f;

    private float elapsedTime;
    // Start is called before the first frame update
    void Start()
    {
        if (BulletPoolManager == null)
        {
            Debug.Log("No bullet pool manager assigned to the enemy - Disabling enemy");
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Attempt to shoot (without input) every frame

        if (elapsedTime > shootRate)
        {
            elapsedTime = 0;
            BulletPoolManager.Shooting();
        }


        // Attempt to shoot (with user input)

        /*if(Input.GetButtonDown("Fire1") && elapsedTime > shootRate)
        {
            elapsedTime = 0;
            BulletPoolManager.Shooting();
        }*/
    }
}

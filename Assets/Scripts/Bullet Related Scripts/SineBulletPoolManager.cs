using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SineBulletPoolManager : MonoBehaviour
{
    float centreY;

    // BULLET POOL INFORMATION
    public Queue<GameObject> bulletPool;
    public int poolSize = 20;
    public GameObject bulletObject;

    public float waveAmplitude = 2.5f;
    public float frequency = 5.0f;

    public GameObject enemy;

    public StandardBullet StandardBullet;


    // Start is called before the first frame update
    void Start()
    {
        centreY = transform.position.y;

        bulletPool = new Queue<GameObject>();

        // Instantiates all the bullets in the manager and make them invisible @ frame 0 (zero)
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletObject);
            bullet.SetActive(false);
            bullet.transform.SetParent(transform, false);
            bulletPool.Enqueue(bullet);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     *  Shooting method
     */
    public void Shooting(Vector3 spawnpoint)
    {

        // Calls a bullet from the queue
        GameObject bullet = GetBullet();

        // 'Spawns' and moves the bullet
        if (bullet != null)
        {
            bullet.SetActive(true);
            bullet.transform.position = spawnpoint;
            bullet.transform.rotation = enemy.transform.rotation;

            // Makes the bullets follow a sine wave form
            StartCoroutine(SineMovementCoroutine(bullet, StandardBullet.lifeTime));
            // Bullet queuing
            StartCoroutine(ReturnBulletToPool(bullet, StandardBullet.lifeTime));
        }
    }

    // Picks the next sequential bullet to shoot
    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            return bulletPool.Dequeue();
        }
        else
        {
            Debug.Log("No bullets available in the bullet pool queue");
            return null;
        }
    }

    // Returns the bullet to the queue after its lifetime
    public IEnumerator ReturnBulletToPool(GameObject bullet, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    public IEnumerator SineMovementCoroutine(GameObject bullet, float lifeTime)
    {
        float timeElapsed = 0f;
        Vector3 startPos = bullet.transform.position;
        Vector3 initialForward = bullet.transform.forward;

        while (timeElapsed < lifeTime)
        {
            timeElapsed += Time.deltaTime;

            Vector3 forwardMovement = initialForward * StandardBullet.speed * timeElapsed;

            //  Calculates wave based local space relative to inital forward direction
            float sineOffset = waveAmplitude * Mathf.Sin(frequency * timeElapsed);
            Vector3 sineMovement = bullet.transform.right * sineOffset;

            bullet.transform.position = startPos + forwardMovement + sineMovement;

            // waits for next frame
            yield return null;      
        }

    }

    private void FixedUpdate()
        {
            
        }
}

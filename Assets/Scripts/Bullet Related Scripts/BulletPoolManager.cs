using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    // BULLET POOL INFORMATION
    public Queue<GameObject> bulletPool;
    public int poolSize = 20;
    public GameObject bulletObject;

    public GameObject enemy;

    public StandardBullet StandardBullet;

    // Start is called before the first frame update
    void Start()
    {

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

            // Bullets shoot towards the front of the enemy
            Vector3 shootDirection = enemy.transform.forward;

            bullet.GetComponent<Rigidbody>().velocity = shootDirection * StandardBullet.speed;

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

    
}

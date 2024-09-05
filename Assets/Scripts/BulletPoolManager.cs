using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public GameObject bulletToPool;
    public int poolSize = 20;
    private Queue<GameObject> bulletPool;

    // Start is called before the first frame update
    void Start()
    {
        PoolBullets();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PoolBullets()
    {
        print("STARTING");

        bulletPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletToPool);
            bullet.SetActive(false);
            bullet.transform.parent = transform;
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            return bulletPool.Dequeue();
        }
        else
        {
            Debug.LogWarning("Bruh");
            return null;
        }
        /*bullet.SetActive(true);*/
        /*bulletPool.Enqueue(bullet);*/
        //return bullet;
    }

    public IEnumerator ReturnBulletToPool(GameObject bullet, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    public void tester()
    {
        print("HIHIHI");
    }

}

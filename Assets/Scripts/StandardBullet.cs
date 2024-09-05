using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StandardBullet : MonoBehaviour
{
    public GameObject BulletObject;
    //public BulletPoolManager BulletPoolManager;
    //public GameObject BulletPoolManager;
    //private Rigidbody _rigidbody;



    public float speed = 10.0f;         // movement speed per unit
    public float lifeTime = 5.0f;       // life time in seconds
    //public int damage = 50;             // damage dealt on impact
    /*public float shootRate = 0.5f;      // shoot rate per second*/
    //protected float elapsedTime;

    private Vector3 spawn = Vector3.zero;
    private GameObject bullet;

    //public GameObject bulletToPool;
    public int poolSize = 20;
    private Queue<GameObject> bulletPool;

    //private Vector3 newPos;

    public void Start()
    {
        //bullet = BulletPoolManager.GetBullet();
        print("STARTING");

        bulletPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(BulletObject);
            bullet.SetActive(false);
            bullet.transform.parent = transform;
            bulletPool.Enqueue(bullet);
        }
        //_rigidbody = GetComponent<Rigidbody>();

        // GARBAGE COLLECTION
        // start the countdown for destroying the bullet WHEN instantiated
        // Destroy(gameObject, lifeTime);

        //GameObject bulletPool = BulletPoolManager

    }
    public void Shoot()
    {

        GameObject bullet = GetBullet();
        
        if (bullet != null )
        {
            bullet.transform.position = spawn;
            bullet.transform.rotation = Quaternion.identity;
            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody>().velocity = Vector3.forward * speed;

            StartCoroutine(ReturnBulletToPool(bullet, lifeTime));
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






}

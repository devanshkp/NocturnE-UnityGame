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
    public void Shooting()
    {

        // Calls a bullet from the queue
        GameObject bullet = GetBullet();

        // 'Spawns' and moves the bullet
        if (bullet != null)
        {
            bullet.SetActive(true);
            /*bullet.transform.position = Vector3.zero;               // SET POSITION TO THAT OF THE ENEMY*/




            /*Vector3 pos = bullet.transform.position;

            float sin = Mathf.Sin(pos.y);*/
            //pos.x = sin;

            //bullet.transform.position = pos;



            bullet.transform.position = Vector3.zero;
            bullet.transform.rotation = Quaternion.identity;        // SET ROTATION TO THAT OF THE ENEMY (have the bullets spawn always IN FRONT of the enemy)
            //bullet.GetComponent<Rigidbody>().velocity = new Vector3(Mathf.Sin(3 * Time.time),StandardBullet.speed);

            StartCoroutine(SineMovementCoroutine(StandardBullet.lifeTime));        // Makes the bullets follow a sine wave form
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

    public IEnumerator SineMovementCoroutine(float lifeTime)
    {
        float timeElapsed = 0f;

        while (timeElapsed < lifeTime)
        {
            timeElapsed += Time.deltaTime;

            Vector3 forwardMovement = transform.forward * StandardBullet.speed * timeElapsed;

            float sineOffset = Mathf.Sin(3 * timeElapsed);
            Vector3 sineMovement = new Vector3(0,sineOffset,0);

            transform.position = Vector3.zero + forwardMovement + sineMovement;

            yield return null;
        }

    }

    private void FixedUpdate()
        {
            
        }
}

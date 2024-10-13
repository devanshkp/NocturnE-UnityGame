using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUIManager : MonoBehaviour
{
    public GameObject lockOnIcon;
    public GameObject healthBar;
    private Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform.Find("EnemyUICamera").GetComponent<Camera>().transform;
        if (lockOnIcon == null)
            lockOnIcon =  transform.GetComponentInChildren<Transform>().Find("LockOnIcon").gameObject;
        if (healthBar != null)
            healthBar = transform.GetComponentInChildren<Transform>().Find("Health Bar").gameObject; 
        lockOnIcon.SetActive(false);
    }

    void LateUpdate()
    {
        transform.LookAt(cam.transform);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }

    public void EnableLockOnIcon()
    {
        lockOnIcon.SetActive(true);
    }

    public void DisableLockOnIcon()
    {
        lockOnIcon.SetActive(false);
    }

    public void EnableHealthBar()
    {
        healthBar.SetActive(true);
    }

    public void DisableHealthBar()
    {
        healthBar.SetActive(false);
    }

    public Transform GetLockOnIcon()
    {
        return lockOnIcon.transform;
    }
}



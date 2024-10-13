using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Skeleton_Menu : MonoBehaviour
{
    //
    //  Ghosts need a special navmesh that is baked without walls, and a special accompanying navmesh agent ("Ghost" Agent Type)
    //

    [Header("Destination List")]
    public GameObject[] destinationList;
    private Vector3 destinationPos;
    private int currentDestination = 0;

    //  Movement variables
    [Header("Movement")]
    public float moveSpeed = 4f;

    private NavMeshAgent nav;
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        //  Set first destination
        nav.SetDestination(destinationList[0].transform.position);
        nav.speed = moveSpeed;

        _animator = GetComponent<Animator>();

        MenuPatrol();
    }

    void Update()
    {
        MenuPatrol();
    }

    private void MenuPatrol()
    {
        //  NPC walking animation
        _animator.Play("Base Layer.running");

        //  NPC reaches the current destination
        if (Vector3.Distance(transform.position, destinationList[currentDestination].transform.position)
            < 2.5f)
        {
            //  Increment destination
            currentDestination++;

            //  NPC reaches final destination
            if (currentDestination > destinationList.Length - 1)
            {
                //  Reset to first destination
                currentDestination = 0;
            }

            nav.SetDestination(destinationList[currentDestination].transform.position);
        }
    }
}

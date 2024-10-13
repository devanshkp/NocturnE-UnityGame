using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Skeleton_Win_Screen : MonoBehaviour
{
    void Start()
    {
        Animator _animator = gameObject.GetComponent<Animator>();

        _animator.Play("Base Layer.dies");
    }
}

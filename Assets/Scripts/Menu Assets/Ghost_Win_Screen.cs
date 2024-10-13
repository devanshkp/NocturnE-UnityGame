using System.Collections;
using System.Collections.Generic;
//using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Ghost_Win_Screen : MonoBehaviour
{
    void Start()
    {
        Animator _animator = gameObject.GetComponent<Animator>();

        _animator.Play("Base Layer.dissolve");
    }
}

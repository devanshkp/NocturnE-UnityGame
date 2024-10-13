using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Werewolf_Win_Screen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animator _animator = gameObject.GetComponent<Animator>();

        _animator.Play("Base Layer.down");
    }
}

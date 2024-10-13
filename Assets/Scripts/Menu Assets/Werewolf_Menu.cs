using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Werewolf_Menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animator _animator = gameObject.GetComponent<Animator>();

        _animator.Play("Base Layer.lookaround");
    }
}

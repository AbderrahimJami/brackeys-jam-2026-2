using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OpenCloseAnimationInteraction : MonoBehaviour, InteractionInterface
{
    Animator animator;

    [SerializeField]
    string paramName = "isOpen";
    bool defaultValue = false;

    [SerializeField]
    public AudioSource soundToPlayOnInterect = null;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool(paramName, defaultValue);
    }
    public void Interact(GameObject interactor)
    {
        bool newState = !animator.GetBool(paramName);
        animator.SetBool(paramName, newState);

        if (soundToPlayOnInterect != null)
        {
            // Play audio
        }
    }
}

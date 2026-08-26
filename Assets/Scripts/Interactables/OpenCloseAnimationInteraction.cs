using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class OpenCloseAnimationInteraction : MonoBehaviour, InteractionInterface
{
    Animator animator;

    [SerializeField] 
    string paramName = "isOpen";
    bool defaultValue = false;
    [SerializeField] 
    private EventReference doorSoundEvent;


    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool(paramName, defaultValue);
    }
    public void Interact(GameObject interactor)
    {
        bool newState = !animator.GetBool(paramName);
        animator.SetBool(paramName, newState);
        playSound();
    }

    private void playSound()
    {
        RuntimeManager.StudioSystem.setParameterByName("isOpen", System.Convert.ToSingle(animator.GetBool(paramName)));
        EventInstance instance = RuntimeManager.CreateInstance(doorSoundEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
        instance.release();
    }
}

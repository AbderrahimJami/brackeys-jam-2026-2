using NUnit.Framework;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CameraController : MonoBehaviour
{
    float frequency;
    float amplitude;
    float cameraSensitivity = 10f;
    GameObject follow;
    float xRotation;
    bool startHeadBobbing = false;
    bool isInitialized = false;

    float timer = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
 

    public void initialize(float cameraSensitivity, GameObject follow)
    {
        isInitialized = true;
        startHeadBobbing = false;

        this.follow = follow;
        this.cameraSensitivity = cameraSensitivity;
        this.amplitude = 0f;
        this.frequency = 0f;

    }


    public void setHeadBobbing(bool val, float freq = 0f, float applitude = 0f)
    {
        startHeadBobbing = val;
        frequency = freq;
        this.amplitude = applitude;
    }

    // Update is called once per frame
    void Update()
    {

        if (isInitialized == false) return;


        float mouseX = Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 60f);

        if (startHeadBobbing)
        {
            timer += Time.deltaTime;
            float val = amplitude * Mathf.Sin(timer * frequency * 2f * Mathf.PI);
            xRotation += val;
        }

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        

        follow.transform.Rotate(Vector3.up * mouseX);


        Vector3 position = transform.position;

        

    }
}

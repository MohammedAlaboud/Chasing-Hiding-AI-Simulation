using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour //Unity class from Utility used for moving object with arrow keyes, and modified to only move on z plane and record speed as well
{
    //exposed to be modified from inspector
    public float speed = 10.0f; //speed to move object at (up/down arrow keyes)
    public float rotationSpeed = 100.0f; //speed to rotate object at (right/left arrow keyes)
    public float currentSpeed = 0; //to record the current speed of whats beings controlled

    void Update()
    {
        // Get the horizontal and vertical axis -> mapped to the arrow keys by default. The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        //Movements are frame rate independent
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        //Move along the object's z-axis 
        transform.Translate(0, 0, translation);
        currentSpeed = translation;
        // Rotate around y-axis
        transform.Rotate(0, rotation, 0);
    }
}

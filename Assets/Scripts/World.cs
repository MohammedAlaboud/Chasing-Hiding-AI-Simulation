using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class World //singleton class (no MonoBehaviour)
{
    private static readonly World instance = new World(); //const instance of the world (World class exists in Unity lib)
    private static GameObject[] hidingLocations; //stores gameobjects that can be utilized to hide behind

    static World() //constructor that stores all hiding spots on initialisation
    {
        hidingLocations = GameObject.FindGameObjectsWithTag("hide"); //finds all objects with the "hide" tag given to it in the scene (stored in hidingLocations list) -> objects/obstacles must be given hide tag to make it possible to be used by the runner
    }

    private World() { } //empty private constructor required for singleton classes

    public static World Instance //get an instance of the world using Unity implemented World method to do so
    {
        get { return instance; }
    }

    public GameObject[] GetHidingLocations() //get the list of game object hiding locations
    {
        return hidingLocations;
    }
}

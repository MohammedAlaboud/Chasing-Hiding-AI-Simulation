using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;

public class RunnerAIController : MonoBehaviour
{
    NavMeshAgent navMeshAgent; //nav mesh agent required to utlise unity's nav mesh system
    public GameObject chaser; //the chaser in this scenario (the runner bahaves based on the chaser)-> Must be assigned in inspector

    void Start() //on play, nav mesh agent is established 
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>(); //access navmesh component of this object (issues if not added manually)
    }

    void MoveTo(UnityEngine.Vector3 runnerPosition) //move towards target
    {
        navMeshAgent.SetDestination(runnerPosition);
    }

    void Flee(UnityEngine.Vector3 positionAwayFrom) //move away from target
    {
        UnityEngine.Vector3 vectorAwayFrom = positionAwayFrom - this.transform.position;
        navMeshAgent.SetDestination(this.transform.position - vectorAwayFrom);
    }

    void Chase() //chaser persues target and attempts to predict where it will be and intercept it on its path
    {
        UnityEngine.Vector3 runnerDirection = chaser.transform.position - this.transform.position; //direction towards what to chase

        float directionsAngleDifference = UnityEngine.Vector3.Angle(this.transform.forward, this.transform.TransformVector(chaser.transform.forward)); //angle difference between forward directions of runner and chaser
        float angleToRunner = UnityEngine.Vector3.Angle(this.transform.forward, this.transform.TransformVector(runnerDirection)); //angle between chaser forward direction and runner position

        if (navMeshAgent.speed + chaser.GetComponent<Drive>().currentSpeed < 0.01f || (angleToRunner > 90 && directionsAngleDifference < 20)) //simply move towards target without computing where chaser should be if the chaser is not moving or if the runner is position behind chaser and are not facing the same relative direction
        {
            MoveTo(chaser.transform.position);
            return;
        }

        float locationPredictionAhead = runnerDirection.magnitude / (navMeshAgent.speed + chaser.GetComponent<Drive>().currentSpeed); //calculate where the runner will be based on their speed and movement direction
        MoveTo(chaser.transform.position + chaser.transform.forward * locationPredictionAhead); //so that the chaser can predict and move to where the runner will be 
    }

    void Evade() //runner moves away taking into account that chaser is predicting where it will go, so the runner moves away from where chaser predicts it (runner) will be -> method not used in first scenario but can be incorporated within more complex or advanced scenarios
    {
        UnityEngine.Vector3 runnerDirection = chaser.transform.position - this.transform.position; //direction towards what to run from
        float locationPredictionAhead = runnerDirection.magnitude / (navMeshAgent.speed + chaser.GetComponent<Drive>().currentSpeed); //calculate where the runner will be based on their speed and movement direction
        Flee(chaser.transform.position + chaser.transform.forward * locationPredictionAhead); //so that the chaser can predict and move to where the runner will be 
    }

    UnityEngine.Vector3 roamAreaPosition = UnityEngine.Vector3.zero; //variable for storing position of wandering area -> initialise it to center to avoid issues when not assigned

    void Roam() //method to wander or roam randomly rather than sit still
    {
        //values to modify roaming behaviour 
        float roamAreaRadius = 10;
        float distanceToRoamingArea = 10;
        float roamingPositionShift = 1;

        roamAreaPosition += new UnityEngine.Vector3(UnityEngine.Random.Range(-1.0f, 1.0f) * roamingPositionShift, 0, UnityEngine.Random.Range(-1.0f, 1.0f) * roamingPositionShift); //get a new random position away from roaming object (runner in this case) based on given distance 

        //establish area of roaming to move within after normalizing position to avoid getting large values
        roamAreaPosition.Normalize();
        roamAreaPosition *= roamAreaRadius;

        UnityEngine.Vector3 positionToMoveTo = roamAreaPosition + new UnityEngine.Vector3(0, 0, distanceToRoamingArea); //find the position based on the roaming area and position of the roaming/wandering object
        UnityEngine.Vector3 convertedPosition = this.gameObject.transform.InverseTransformVector(positionToMoveTo);//convert the position calculated to a world position that the object can be moved towards 

        MoveTo(convertedPosition); //apply movement

    }

    void InitialHide() //initial version of the hiding method found below -> This is not used at the moment but is being kept for future development (check Hide method for implementation documentation)
    {
        float dist = Mathf.Infinity; 
        UnityEngine.Vector3 hidingLocationChosen = UnityEngine.Vector3.zero; 

        for (int i = 0; i < World.Instance.GetHidingLocations().Length; i++)
        {
            UnityEngine.Vector3 hidingLocationDirection = World.Instance.GetHidingLocations()[i].transform.position - chaser.transform.position;
            UnityEngine.Vector3 hidingPosition = World.Instance.GetHidingLocations()[i].transform.position + hidingLocationDirection.normalized * 10;

            if (UnityEngine.Vector3.Distance(this.transform.position, hidingPosition) < dist)
            {
                hidingLocationChosen = hidingPosition;
                dist = UnityEngine.Vector3.Distance(this.transform.position, hidingPosition);
            }
        }

        MoveTo(hidingLocationChosen);
    }

    void Hide()
    {
        float dist = Mathf.Infinity; //needed a large value to satisfy condition for now (had issues with initial way so this is being used instead)
        UnityEngine.Vector3 hidingLocationChosen = UnityEngine.Vector3.zero; //store the locaiton the runner will hide at and initialised to avoid empty value errors
        UnityEngine.Vector3 directionOfLocationChosen = UnityEngine.Vector3.zero; //store the direction of the hiding location and initialised to avoid empty value errors
        GameObject objectBeingHidBehind = World.Instance.GetHidingLocations()[0]; //store the object being used for hiding and also initialised for the same reaosn as the above variables

        for (int i = 0; i < World.Instance.GetHidingLocations().Length; i++) //for all hiding objects in the world
        {
            UnityEngine.Vector3 hidingLocationDirection = World.Instance.GetHidingLocations()[i].transform.position - chaser.transform.position; //get direction to that object
            UnityEngine.Vector3 hidingPosition = World.Instance.GetHidingLocations()[i].transform.position + hidingLocationDirection.normalized * 10; //get the hiding position that runner will move towards behind that object (magnitude of 10 introduced after tuning)

            if (UnityEngine.Vector3.Distance(this.transform.position, hidingPosition) < dist) //if the distance is shorter than the last found distance from the previous loop (closer hiding position)
            {
                //set the location, the direction of the hiding position and the object
                hidingLocationChosen = hidingPosition;
                directionOfLocationChosen = hidingLocationDirection;
                objectBeingHidBehind = World.Instance.GetHidingLocations()[i];

                //set the distance to the current hiding position
                dist = UnityEngine.Vector3.Distance(this.transform.position, hidingPosition);
            }
        }

        Collider hidingObjectCollider = objectBeingHidBehind.GetComponent<Collider>(); //get hiding object collider (objects in the scene must be assigned colliders manually)
        Ray backRay = new Ray(hidingLocationChosen, -directionOfLocationChosen.normalized); //establish ray to opposite direction of the object (so that runner hides behind it rather than on the wrong side)
        RaycastHit castResult; //out variable initialised during raycasting to determine the ray (or line of points that the runner can potentially move to)
        float rayCastRange = 100.0f; //range of ray casting (must be bigger than hiding position multiplier set)
        hidingObjectCollider.Raycast(backRay, out castResult, rayCastRange); //perform ray casting 

        MoveTo(castResult.point + directionOfLocationChosen.normalized * 5); //move towards hiding position
    }

    bool runnerInViewOfChaser() //if the runner is in view of the chaser (based on direct ray casting casting from one to the other)
    {
        RaycastHit raycastResult; //out paramters as the result of what was hit 
        UnityEngine.Vector3 rayToTarget = chaser.transform.position - this.transform.position; //direction to cast towards

        if (Physics.Raycast(this.transform.position, rayToTarget, out raycastResult)) //perform ray casting
        {
            if (raycastResult.transform.gameObject.tag == "chaser") //if direct cast from runner hits chaser then return true, otherwise false
            {
                return true; 
            }
        }

        return false;
    }

    bool chaserCanBeSeenByRunner() //if the chaser can be seen by the runner based on cone area of view rather than ray casting (this way to avoid runner tring to hide or move away when chaser is too far)
    {
        UnityEngine.Vector3 toNavMeshAgent = this.transform.position - chaser.transform.position; //direction towards runner
        float viewAngle = UnityEngine.Vector3.Angle(chaser.transform.forward, toNavMeshAgent); //view area on runner based on angle

        if (viewAngle < 60) //if chaser in view of runner (based on cone range with cone being 60 degrees wide)...
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //variables for delaying between hiding and persuing (specific behaviour)
    bool toDelay = false; //check when to stop cool down delay to move between actions or states
    int delayTime = 5; //time to delay

    void Delay()
    {
        toDelay = false;
    }

    bool runnerInRangeOfChaser() //to determine when the runner will hide or wander
    {
        if (UnityEngine.Vector3.Distance(this.transform.position, chaser.transform.position) < 20) //if the runner is in given range of chaser then return true
        {
            return true;
        }

        return false;
    }

    //implementation of a state machine scenario is found in this method
    //this is just one of several possible solutions implemented for this scenario and it is possible to base it on simpler or more complex state machines with the different combinations of the behaviour/actions coded above as well as include additional ones
    void Update()
    {
        
        //to reiterate, this is just an example of how the runner bahaves based on the chaser, this method has been modified to show other possibilities in the video
        if (!toDelay) //delay function included to give more time between actions rather than immediately jumping between states
        {
            if (!runnerInRangeOfChaser()) //if the runner is far away enough from the chaser, then just wander
            {
                Roam();
            }
            else if (runnerInViewOfChaser() && chaserCanBeSeenByRunner()) //if the runner is in the chaser's view range and if the chaser can be seen by the runner
            {
                Hide(); //then the runner will hide (can replace hide with evade or create more complex solution to include both)
                toDelay = true; //ensure that there is a delay to give time for the runner to complete its action
                Invoke("Delay", delayTime); //invoke delay method to stop delaying
            }
            else
            {
                Chase(); //runner will sneak up on chaser 
            }
        }
        
    }
}

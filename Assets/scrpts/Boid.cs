
using NUnit.Framework.Internal.Commands;
using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class Boid : MonoBehaviour
{
    public new Rigidbody rigidbody;

    [SerializeField] private float currentSpeed;
    public float maxSpeed = 2.0f;
    public float maxAcceleration = 3.0f;

    private Vector3 lastAcceleration;
    [SerializeField] private bool showObstacleRays = true;

    Vector3 targetVelocity = MouseTarget.Velocity;


    public float arriveRadius = 0.5f;
    [SerializeField] private float sensorLength = 5f; // for obstacle avoidance
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float neighborRadius = 2.0f;
    [SerializeField] private float separationRadius = 1.0f;

    private void Awake()
    {
        GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1));//make the fish different colour 
        rigidbody = GetComponent<Rigidbody>();//Get the Rigidbody and initialize the velocity.
        rigidbody.linearVelocity = Random.insideUnitSphere;
    }

    private void Start()

    {
       
        obstacleMask = LayerMask.GetMask("Obstacle");
        //Fish swim using variable maximum acceleration up to maximum speed.These parameters can be set per-fish
        maxSpeed = Random.Range(1.5f, 3.0f);              
        maxAcceleration = Random.Range(2.0f, 5.0f);       
    }
    
    private void FixedUpdate()
    {
        Vector3 acceleration = Vector3.zero;
        Vector3 target = MouseTarget.Position;
        Vector3 totalSteering = Vector3.zero;
       
        //Blend them together in a meaningful way.
        Vector3 cohesion = Cohesion() * 1.0f;
        Vector3 separation = Separation() * 1.5f;
        Vector3 alignment = Alignment() * 1.0f;
        acceleration += cohesion + separation + alignment;

        //limit max acceleration
        if (acceleration.magnitude > maxAcceleration)
            acceleration = acceleration.normalized * maxAcceleration;
        // update speed 
        rigidbody.linearVelocity += acceleration * Time.fixedDeltaTime;
        // limit max speed 
        if (rigidbody.linearVelocity.magnitude > maxSpeed)
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * maxSpeed;
        // Fish orient their model to match the direction of their current velocity
        if (rigidbody.linearVelocity != Vector3.zero)
            transform.forward = rigidbody.linearVelocity;

        currentSpeed = rigidbody.linearVelocity.magnitude;

        switch (BoidSimulationControl.Instance.controlMode)
        {
            case BoidSimulationControl.ControlMode.Seek:
                if (Input.GetMouseButton(1))
                    totalSteering = Flee(target);
                else if (Input.GetMouseButton(0)) 
               // else
                {
                    totalSteering = Seek(target);
                }
                break;

            case BoidSimulationControl.ControlMode.Pursue:
                if (Input.GetMouseButton(0))
                    totalSteering = Pursue(target,targetVelocity);
                else if (Input.GetMouseButton(1))
                    totalSteering = Evade(target, targetVelocity);
                break;

            case BoidSimulationControl.ControlMode.Food:
                GameObject food = FindNearestWithTag("Food");
                if (food != null) totalSteering = Arrive(food);
                break;

            case BoidSimulationControl.ControlMode.Obstacle:
                totalSteering = Seek(target); 
                break;
        }


        Vector3 avoidance = ObstacleAvoidance();
        //Vector3 avoidance = ObstacleAvoidance(sensorLength, maxAcceleration);
        if (avoidance != Vector3.zero)
        {
            Debug.Log("Avoiding obstacle!");
            totalSteering = avoidance;
        }
        lastAcceleration = totalSteering;
        Debug.Log("lastAcceleration: " + lastAcceleration);

        ApplySteering(totalSteering);

        if (rigidbody.linearVelocity.magnitude > 0.1f)
            transform.forward = rigidbody.linearVelocity.normalized;
    }
    /*private Vector3 Seek(Vector3 target)
    {    
        Vector3 desired = (target - transform.position).normalized * maxSpeed;
        return (desired - rigidbody.linearVelocity).normalized * maxAcceleration;
    }*/
    public Vector3 Seek(Vector3 target)
    { /*Vector3 toTarget = target - transform.position;
        Vector3 toTargetNoemalized = toTarget.normalized;
        Vector3 accel =toTargetNoemalized*maxAcceleration;
        return accel;*/
        Vector3 toTarget = target - transform.position;

        Vector3 toTargetNormalized = toTarget.normalized;

        Vector3 desiredVelocity = toTargetNormalized * maxSpeed;

        Vector3 deltaVel = desiredVelocity - rigidbody.linearVelocity;

        Vector3 accel = deltaVel.normalized * maxAcceleration;

        return accel;
    }
    public Vector3 Flee(Vector3 target)
    {
        /* Vector3 desired = (transform.position - target).normalized * maxSpeed;
         Vector3 steer = desired - rigidbody.linearVelocity;
         return steer.normalized * maxAcceleration;*/
        
{
            // 1. Calculate the direction away from the target
            Vector3 toTarget = transform.position - target;

            // 2. Normalize the direction
            Vector3 toTargetNormalized = toTarget.normalized;

            // 3. Calculate desired fleeing velocity
            Vector3 desiredVelocity = toTargetNormalized * maxSpeed;

            // 4. Find the difference between current velocity and desired velocity
            Vector3 deltaVel = desiredVelocity - rigidbody.linearVelocity;

            // 5. Clamp the result to max acceleration and return
            Vector3 accel = deltaVel.normalized * maxAcceleration;
            return accel;
        }
    }
    /*private Vector3 Pursue(Vector3 target)
    {
        

        Vector3 desiredDir = (target - transform.position).normalized;
        Vector3 currentDir = rigidbody.linearVelocity.normalized;
        Vector3 steer = (desiredDir - currentDir).normalized * maxAcceleration;
        return steer;
}*/
    public Vector3 Pursue(Vector3 targetPosition, Vector3 targetVelocity)
    {
        // 1. Calculate the distance between the target and the pursuer
        Vector3 toTarget = targetPosition - transform.position;

        // 2. Estimate how far the target will move (prediction time) = distance / pursuer's max speed
        float predictionTime = toTarget.magnitude / maxSpeed;

        // 3. Predict the target's future position (assuming linear motion)
        Vector3 futurePosition = targetPosition + targetVelocity * predictionTime;

        // 4. Seek the predicted future position
        return Seek(futurePosition);
    }
    /*(private Vector3 Evade(Vector3 target)
    {
        /*float safeDistance = 3.0f;
        Vector3 toTarget = transform.position - target;

        if (toTarget.magnitude > safeDistance)
            return Vector3.zero; 

        Vector3 fleeDir = (transform.position - target).normalized;
        Vector3 currentDir = rigidbody.linearVelocity.normalized;
        Vector3 steer = (fleeDir - currentDir).normalized * maxAcceleration;
        return steer;*/
    //}
    public Vector3 Evade(Vector3 targetPosition, Vector3 targetVelocity)
    {
        // 1. Calculate the distance between the target and the evader
        Vector3 toTarget = targetPosition - transform.position;

        // 2. Estimate prediction time based on distance and max speed
        float predictionTime = toTarget.magnitude / maxSpeed;

        // 3. Predict where the target will be in the future
        Vector3 futurePosition = targetPosition + targetVelocity * predictionTime;

        // 4. Flee from the predicted future position
        return Flee(futurePosition);
    }

    private Vector3 Arrive(GameObject targetObj)
    {
        Vector3 toTarget = targetObj.transform.position - transform.position;
        float distance = toTarget.magnitude;
        float speed = rigidbody.linearVelocity.magnitude;
        if (distance < arriveRadius && speed < 0.05f) 
        {
            //Destroy(targetObj)//;//eat food
            Destroy(targetObj, 0.1f);
            return Vector3.zero; // slow to stop
        }
        //reach the the edge of radius ,and slow speed
        float slowDownFactor = Mathf.Clamp01(distance / 2f); // 2f = slowing zone
        Vector3 desired = toTarget.normalized * maxSpeed * slowDownFactor;
        Vector3 steer = desired - rigidbody.linearVelocity;
        return steer.normalized * maxAcceleration;
    }
    private GameObject FindNearestWithTag(string tag)// find the near food 
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var t in targets)
        {
            float d = Vector3.Distance(transform.position, t.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = t;
            }
        }

        return nearest;
    }
    /* private Vector3 ObstacleAvoidance()
     {
         RaycastHit hit;
         //3 detection Ray
         Vector3[] directions = {
             transform.forward,
             Quaternion.AngleAxis(30, Vector3.up) * transform.forward,
             Quaternion.AngleAxis(-30, Vector3.up) * transform.forward
         };

         foreach (Vector3 dir in directions)
         {
             if (Physics.Raycast(transform.position, dir, out hit, sensorLength, obstacleMask))
             {
                 Vector3 avoidDir = Vector3.Reflect(dir, hit.normal);
                 return avoidDir.normalized * maxAcceleration;
             }
         }

         return Vector3.zero;
     }*/
    private Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        Vector3 avoidanceForce = Vector3.zero;

        // Add more detection angles
        float[] angles = { 0, 15, -15, 30, -30, 45, -45 };

        foreach (float angle in angles)
        {
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;

            if (Physics.Raycast(transform.position, dir, out hit, sensorLength, obstacleMask))
            {
                // Calculate avoidance strength - closer distance = stronger force
                float avoidanceStrength = (sensorLength - hit.distance) / sensorLength;
                Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up);

                // Adjust weight based on angle
                float weight = 1.0f / (1.0f + Mathf.Abs(angle) * 0.02f);

                avoidanceForce += avoidDir * avoidanceStrength * weight;
            }
        }

        return avoidanceForce.normalized * maxAcceleration;
    }
    /*public Vector3 ObstacleAvoidance(float lookaheadDistance ,float acceleration )
    {
        Vector3 accelOut = Vector3.zero;

       
        Ray whiskerLeft = new Ray(transform.position, Quaternion.AngleAxis(20, transform.up) * transform.forward);
        Ray whiskerRight = new Ray(transform.position, Quaternion.AngleAxis(-20, transform.up) * transform.forward);
        RaycastHit hitInfoLeft;
        RaycastHit hitInfoRight;
        

        bool didHitLeft = Physics.Raycast(whiskerLeft.origin, whiskerLeft.direction, out hitInfoLeft, lookaheadDistance);
        if (didHitLeft)
        {
            accelOut = transform.right * acceleration;
            Debug.DrawLine(whiskerLeft.origin, hitInfoLeft.point, Color.red);
        }
        else
        {
            Debug.DrawRay(whiskerLeft.origin, whiskerLeft.direction * lookaheadDistance, Color.yellow);
        }

       
        bool didHitRight = Physics.Raycast(whiskerRight.origin, whiskerRight.direction, out hitInfoRight, lookaheadDistance);
        if (didHitRight)
        {
            accelOut = -transform.right * acceleration;
            Debug.DrawLine(whiskerRight.origin, hitInfoRight.point, Color.red);
        }
        else
        {
            Debug.DrawRay(whiskerRight.origin, whiskerRight.direction * lookaheadDistance, Color.yellow);
        }

        return accelOut;

    }*/


    private void ApplySteering(Vector3 acceleration)// add a acceleration and get a linearVelocity
    {
        if (acceleration.magnitude > maxAcceleration)
            acceleration = acceleration.normalized * maxAcceleration;//limit max Acceleration

        Vector3 newVelocity = rigidbody.linearVelocity + acceleration * Time.fixedDeltaTime;

        if (newVelocity.magnitude > maxSpeed)
            newVelocity = newVelocity.normalized * maxSpeed;//limit max speed

        rigidbody.linearVelocity = newVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw 3 obstacle detection rays in BLUE
        if (showObstacleRays)
        {
            Gizmos.color = Color.blue;
            Vector3[] dirs = new Vector3[]
            {
        transform.forward,
        Quaternion.AngleAxis(30, Vector3.up) * transform.forward,
        Quaternion.AngleAxis(-30, Vector3.up) * transform.forward
            };
            foreach (var dir in dirs)
            {
                Gizmos.DrawRay(transform.position, dir * sensorLength);
            }
        }
        float velocityScale = 2f;
        float accelerationScale = 3f;
        // Draw velocity vector in RED
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, rigidbody.linearVelocity* velocityScale);

        // Draw last applied acceleration in GREEN
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, lastAcceleration* accelerationScale);
    }

    Vector3 Cohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (Boid other in BoidSimulationControl.Instance.boids)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < neighborRadius)
            {
                center += other.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        center /= count;
        Vector3 toCenter = center - transform.position;
        return toCenter.normalized * maxAcceleration;
    }

    Vector3 Separation()
    {
        Vector3 avoid = Vector3.zero;
        int count = 0;

        foreach (Boid other in BoidSimulationControl.Instance.boids)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < separationRadius)
            {
                avoid += (transform.position - other.transform.position).normalized / dist;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        return avoid.normalized * maxAcceleration;
    }

    Vector3 Alignment()
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;

        foreach (Boid other in BoidSimulationControl.Instance.boids)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < neighborRadius)
            {
                averageVelocity += other.rigidbody.linearVelocity;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        averageVelocity /= count;
        return (averageVelocity - rigidbody.linearVelocity).normalized * maxAcceleration;
    }
}





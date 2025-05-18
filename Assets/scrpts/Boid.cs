
using NUnit.Framework.Internal.Commands;
using UnityEngine;
using System.Collections.Generic;


public class Boid : MonoBehaviour
{
    private Rigidbody rigidbody;

    [SerializeField] private float currentSpeed;
    public float maxSpeed = 2.0f;
    public float maxAcceleration = 3.0f;

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
                    totalSteering = Seek(target);
                break;

            case BoidSimulationControl.ControlMode.Pursue:
                if (Input.GetMouseButton(0))
                    totalSteering = Pursue(target);
                else if (Input.GetMouseButton(1))
                    totalSteering = Evade(target);
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
        if (avoidance != Vector3.zero)
        {
            Debug.Log("Avoiding obstacle!");
            totalSteering = avoidance;
        }

        ApplySteering(totalSteering);

        if (rigidbody.linearVelocity.magnitude > 0.1f)
            transform.forward = rigidbody.linearVelocity.normalized;
    }
    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized * maxSpeed;
        return (desired - rigidbody.linearVelocity).normalized * maxAcceleration;
    }
    private Vector3 Flee(Vector3 target)
    {
        Vector3 desired = (transform.position - target).normalized * maxSpeed;
        Vector3 steer = desired - rigidbody.linearVelocity;
        return steer.normalized * maxAcceleration;
    }
    private Vector3 Pursue(Vector3 target)
    {
        Vector3 desiredDir = (target - transform.position).normalized;
        Vector3 currentDir = rigidbody.linearVelocity.normalized;
        Vector3 steer = (desiredDir - currentDir).normalized * maxAcceleration;
        return steer;
    }
    private Vector3 Evade(Vector3 target)
    {
        float safeDistance = 3.0f;
        Vector3 toTarget = transform.position - target;

        if (toTarget.magnitude > safeDistance)
            return Vector3.zero; 

        Vector3 fleeDir = (transform.position - target).normalized;
        Vector3 currentDir = rigidbody.linearVelocity.normalized;
        Vector3 steer = (fleeDir - currentDir).normalized * maxAcceleration;
        return steer;
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
    private Vector3 ObstacleAvoidance()
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
    }

    private void ApplySteering(Vector3 acceleration)// add a acceleration and get a linearVelocity
    {
        if (acceleration.magnitude > maxAcceleration)
            acceleration = acceleration.normalized * maxAcceleration;//limit max Acceleration

        Vector3 newVelocity = rigidbody.linearVelocity + acceleration * Time.fixedDeltaTime;

        if (newVelocity.magnitude > maxSpeed)
            newVelocity = newVelocity.normalized * maxSpeed;//limit max speed

        rigidbody.linearVelocity = newVelocity;
    }

    private void OnDrawGizmosSelected()//can see the ray :Red
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
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





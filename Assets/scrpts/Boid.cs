using UnityEngine;

public class Boid : MonoBehaviour
{
    private GameObject TargetObject;
    private Rigidbody rigidbody;

    public float maxSpeed = 2f;
    public float maxAcceleration = 3f;
    public Vector3 velocity;

    public float arriveRadius = 0.5f;
    public float sensorLength = 2f; // for obstacle avoidance
    public LayerMask obstacleMask;

    private void Start()
    {
        TargetObject = GameObject.Find("Target");
        rigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
      
        Vector3 toTarget = TargetObject.transform.position - transform.position;
        Vector3 toTargetNormalized = toTarget.normalized;
        Vector3 acceleration = toTargetNormalized * maxAcceleration;
        rigidbody.linearVelocity += acceleration * Time.fixedDeltaTime;
        Vector3 vel = rigidbody.linearVelocity;
        float speed = vel.magnitude;
        if (speed > maxSpeed)
        {
            vel =vel.normalized *maxSpeed;
        }

        transform.forward = rigidbody.linearVelocity;


    }
        /*Vector3 totalSteering = Vector3.zero;

        // Decide what behavior to run based on mode
        switch (BoidSimulationControl.Instance.controlMode)
        {
            case BoidSimulationControl.ControlMode.Seek:
                totalSteering += Seek(MouseTarget.Position);
                break;

            case BoidSimulationControl.ControlMode.Pursue:
                totalSteering += Pursue(MouseTarget.Position);
                break;

            case BoidSimulationControl.ControlMode.Food:
                GameObject food = FindNearestWithTag("Food");
                if (food != null) totalSteering += Arrive(food);
                break;

            case BoidSimulationControl.ControlMode.Obstacle:
                totalSteering += ObstacleAvoidance();
                break;
        }

        ApplySteering(totalSteering);
        transform.position += velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
            transform.forward = velocity.normalized;
    }
    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized * maxSpeed;
        return (desired - velocity).normalized * maxAcceleration;
    }
    private Vector3 Pursue(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        return steer.normalized * maxAcceleration;
    }
    private Vector3 Arrive(GameObject  targetObj)
    {
        Vector3 toTarget = targetObj.transform.position  - transform.position;
        float distance = toTarget.magnitude;

        if (distance < arriveRadius)
        {
            // "Eat" the food
            Destroy(targetObj);
            return -velocity; // slow to stop
        }

        float slowDownFactor = Mathf.Clamp01(distance / 2f); // 2f = slowing zone
        Vector3 desired = toTarget.normalized * maxSpeed * slowDownFactor;
        Vector3 steer = desired - velocity;
        return steer.normalized * maxAcceleration;
    }
    private Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        Vector3[] directions = {
            transform.forward,
            Quaternion.AngleAxis(30, Vector3.up) * transform.forward,
            Quaternion.AngleAxis(-30, Vector3.up) * transform.forward
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out hit, sensorLength, obstacleMask))
            {
                // Steer away from the obstacle surface normal
                Vector3 avoidDir = Vector3.Reflect(dir, hit.normal);
                return avoidDir.normalized * maxAcceleration;
            }
        }

        return Vector3.zero;
    }
    private void ApplySteering(Vector3 acceleration)
    {
        if (acceleration.magnitude > maxAcceleration)
            acceleration = acceleration.normalized * maxAcceleration;

        velocity += acceleration * Time.deltaTime;

        if (velocity.magnitude > maxSpeed)
            velocity = velocity.normalized * maxSpeed;
    }
    private GameObject FindNearestWithTag(string tag)
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
    }*/
}


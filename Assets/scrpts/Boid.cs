using UnityEngine;


public class Boid : MonoBehaviour
{
  
    private Rigidbody rigidbody;

    public float maxSpeed = 2.0f;
    public float maxAcceleration = 3.0f;

    public float arriveRadius = 0.2f;
    public float sensorLength = 2f; // for obstacle avoidance
    public LayerMask obstacleMask;

    private void Awake()
    {
        GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1));
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.linearVelocity = Random.insideUnitSphere;
    }

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 target = MouseTarget.Position;
        //bool LeftClick = MouseTarget.LeftClick;//
        Vector3 totalSteering = Vector3.zero;
        Debug.Log("Right Mouse Pressed: " + Input.GetMouseButton(1));
        // 控制模式处理（需要 BoidSimulationControl.cs）
        switch (BoidSimulationControl.Instance.controlMode)
        {
            case BoidSimulationControl.ControlMode.Seek:
                if (Input.GetMouseButton(1)) // 右键按住时使用 Flee 行为
                    totalSteering += Flee(target);

                else if(Input.GetMouseButton(0))
                    totalSteering += Seek(target);
                break;
            case BoidSimulationControl.ControlMode.Pursue:
                if (Input.GetMouseButton(0)) // 左键
                    totalSteering += Pursue(target);
                else if (Input.GetMouseButton(1)) // 右键
                    totalSteering += Evade(target);
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
        if (distance < arriveRadius && speed < 0.05f) // 加入速度判断
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

        Vector3 newVelocity = rigidbody.linearVelocity + acceleration * Time.fixedDeltaTime;

        if (newVelocity.magnitude > maxSpeed)
            newVelocity = newVelocity.normalized * maxSpeed;

        rigidbody.linearVelocity = newVelocity;
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
    }
}
/*public class Boid : MonoBehaviour
{
    private GameObject TargetObject;
    private Rigidbody rigidbody;

    public float maxSpeed = 2f;
    public float maxAcceleration = 3f;
    public Vector3 velocity;

    public float arriveRadius = 0.5f;
    public float sensorLength = 2f; // for obstacle avoidance
    public LayerMask obstacleMask;

    private void Awake()
    {
        GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1));
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.linearVelocity = Random.insideUnitSphere;
    }

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
            vel = vel.normalized * maxSpeed;
        }

        transform.forward = rigidbody.linearVelocity;


    }
}*/
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



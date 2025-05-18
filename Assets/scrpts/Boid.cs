
using UnityEngine;


public class Boid : MonoBehaviour
{

    private Rigidbody rigidbody;

    public float maxSpeed = 2.0f;
    public float maxAcceleration = 3.0f;

    public float arriveRadius = 0.2f;
    [SerializeField] private float sensorLength = 5f; // for obstacle avoidance
    [SerializeField] private LayerMask obstacleMask;

    private void Awake()
    {
        GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1));//make the fish different colour 
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.linearVelocity = Random.insideUnitSphere;
    }

    private void Start()
    {
        obstacleMask = LayerMask.GetMask("Obstacle");
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
   
        /*Vector3 target = MouseTarget.Position;
        //bool LeftClick = MouseTarget.LeftClick;//
        Vector3 totalSteering = Vector3.zero;
        Debug.Log("Right Mouse Pressed: " + Input.GetMouseButton(1));
        // Debug.DrawRay(transform.position, transform.forward * 5f, Color.red);
        Vector3 avoidance = ObstacleAvoidance();
        if (avoidance != Vector3.zero)
        {
            Debug.Log("Avoiding obstacle!");
        }
        // 控制模式处理（需要 BoidSimulationControl.cs）
        switch (BoidSimulationControl.Instance.controlMode)
        {
            case BoidSimulationControl.ControlMode.Seek:
                if (Input.GetMouseButton(1)) // 右键按住时使用 Flee 行为
                    totalSteering += Flee(target);

                else if (Input.GetMouseButton(0))
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
                /*case BoidSimulationControl.ControlMode.Obstacle:
                    GameObject nearestObstacle = FindNearestWithTag("Obstacle");
                    if (nearestObstacle != null)
                    {
                        Vector3 toObstacle = nearestObstacle.transform.position - transform.position;
                        Vector3 awayFromObstacle = -toObstacle.normalized * maxAcceleration;
                        totalSteering += awayFromObstacle;
                    }

                    totalSteering += Seek(target); // 继续朝目标移动
                    break;
            }*/

        //case BoidSimulationControl.ControlMode.Obstacle:
        //totalSteering += Seek(target); 

        // break;
        // Vector3 avoidance = ObstacleAvoidance();
        /*if (avoidance != Vector3.zero)
        {
            Debug.Log("Avoiding obstacle!");
            totalSteering = avoidance; // 优先避障
        }
        else
        {
            totalSteering = Seek(target); // 没有障碍才去目标
        }
}
totalSteering += ObstacleAvoidance();

ApplySteering(totalSteering);

if (rigidbody.linearVelocity.magnitude > 0.1f)
    transform.forward = rigidbody.linearVelocity.normalized;
}*/
private void FixedUpdate()
    {
        Vector3 target = MouseTarget.Position;
        Vector3 totalSteering = Vector3.zero;

        // 控制模式处理
        switch (BoidSimulationControl.Instance.controlMode)
        {
            case BoidSimulationControl.ControlMode.Seek:
                if (Input.GetMouseButton(1)) // 右键：逃离
                    totalSteering = Flee(target);
                else if (Input.GetMouseButton(0)) // 左键：追逐
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
                totalSteering = Seek(target); // 默认朝目标走
                break;
        }

        // ✅ 最后统一做避障检查，优先级最高
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



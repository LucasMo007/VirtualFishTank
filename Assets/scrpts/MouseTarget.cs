using UnityEngine;

public class MouseTarget : MonoBehaviour
{
    public static Vector3 Position = Vector3.zero;
    public static Vector3 Velocity { get; private set; }
    public static bool LeftClick { get; set; }

    private static Vector3 lastPosition;
    private static bool isFirstFrame = true;

    void Update()
    {
        LeftClick = Input.GetMouseButton(0);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 currentPosition = ray.GetPoint(distance);

            if (isFirstFrame)
            {
                lastPosition = currentPosition;
                isFirstFrame = false;
            }

            Velocity = (currentPosition - lastPosition) / Time.deltaTime;
            Position = currentPosition;
            lastPosition = currentPosition;
        }
        else
        {
            Velocity = Vector3.zero;
        }
    }
}




/*public class MouseTarget : MonoBehaviour
{
    public static Vector3 Position { get;  set; }
    public static Vector3 Velocity { get; private set; }
    public static bool LeftClick { get; set; }

    public GameObject follower;
    public LayerMask waterLayer;

    private static Vector3 lastPosition;
    private static bool isFirstFrame = true;

    void Update()
    {
        LeftClick = Input.GetMouseButtonDown(0);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, waterLayer))
        {
            Position = hit.point;

            if (isFirstFrame)
            {
                lastPosition = Position;
                isFirstFrame = false;
            }

            Velocity = (Position - lastPosition) / Time.deltaTime;
            lastPosition = Position;

            if (follower != null)
                follower.transform.position = hit.point;

            Debug.DrawLine(ray.origin, hit.point, Color.green);
        }
        else
        {
            Velocity = Vector3.zero;
        }
    }
}*/

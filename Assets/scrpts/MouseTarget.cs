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





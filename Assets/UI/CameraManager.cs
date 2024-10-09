using UnityEngine;

public enum CameraFocus { Fields, House };

public static class CameraManager
{
    public static CameraFocus currentCameraFocus;
    public static Vector3 targetPosition;
    public static Quaternion targetRotation;
    public static float speed; // Speed of Camera movement
    private static Camera mainCamera;

    public static void init()
    {
        currentCameraFocus = CameraFocus.Fields;
        targetPosition = new Vector3(0, 250, -950);
        targetRotation = Quaternion.Euler(25, 0, 0);
        speed = 1000.0f;
        mainCamera = Camera.main;
    }

    public static void switchCamera()
    {
        switch (currentCameraFocus)
        {
            case CameraFocus.Fields:
                CameraFocusHouse();
                break;
            case CameraFocus.House:
                CameraFocusFields();
                break;
            default:
                Debug.LogWarning($"CameraFocus not processed: {currentCameraFocus}");
                break;
        }
    }

    public static void CameraFocusHouse()
    {
        targetPosition = new Vector3(0, 200, -1260);
        targetRotation = Quaternion.Euler(45, 0, 0);
        currentCameraFocus = CameraFocus.House;
    }

    public static void CameraFocusFields()
    {
        targetPosition = new Vector3(0, 250, -950);
        targetRotation = Quaternion.Euler(25, 0, 0);
        currentCameraFocus = CameraFocus.Fields;
    }

    public static Vector3 GetMouseWorldPos(Transform planeReferenceObject)
    {
        // Define the plane using the normal and position
        Plane plane = new Plane(planeReferenceObject.up, planeReferenceObject.position);

        // Get mouse position
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Create a ray from the camera through the mouse position
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);

        // Project the mouse position onto the plane
        plane.Raycast(ray, out float distance);
            
        // Get the point where the ray intersects the plane
        Vector3 pointOnPlane = ray.GetPoint(distance);

        return pointOnPlane;
    }

    public static Vector3 GetMouseWorldPosAtDepth(float distance)
    {
        // Get mouse position
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert mouse position to world point
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);

        Vector3 worldPoint = ray.GetPoint(distance);

        return worldPoint;
    }

    public static Vector3 GetCameraPos()
    {
        return mainCamera.transform.position;
    }

    public static Vector3 GetTransformScreenPos(Transform trs)
    {
        return mainCamera.WorldToScreenPoint(trs.position);
    }

    public static Ray GetMousePosRayCast()
    {
        // Cast a ray from the camera to the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        return ray;
    }
}

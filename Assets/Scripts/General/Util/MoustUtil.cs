using UnityEngine;

public static class MouseUtil
{
    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f)
    {
        Camera camera = Camera.main;
        
        if (camera == null)
        {
            camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                return Vector3.zero;
            }
        }
        
        Plane dragPlane = new(camera.transform.forward, new Vector3(0, 0, zValue));
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
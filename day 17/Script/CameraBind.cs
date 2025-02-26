using UnityEngine;

public class CameraBind : MonoBehaviour
{
    public Transform camera;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (camera != null)
        {
            transform.position = new Vector3(camera.position.x, camera.position.y, transform.position.z);
        }
    }
}

using UnityEngine;

public class CameraBind : MonoBehaviour
{
    public Transform camera;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (camera != null)
        {
            transform.position = new Vector3(camera.position.x, transform.position.y, camera.position.z);
        }
    }
}

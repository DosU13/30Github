using UnityEngine;

public class BackShaderHelper : MonoBehaviour
{
    public Transform cameraTransform;
    private Material backgroundMaterial;

    void Start()
    {
        backgroundMaterial = GetComponent<Renderer>().material;
    }

    void Update()
    {
        Vector3 cameraPos = cameraTransform.position;
        backgroundMaterial.SetVector("_CameraPosition", new Vector4(cameraPos.x, cameraPos.y, 0, 0));
    }
}
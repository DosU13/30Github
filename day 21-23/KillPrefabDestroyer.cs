using UnityEngine;

public class KillPrefabDestroyer : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 0.5f);
    }
}

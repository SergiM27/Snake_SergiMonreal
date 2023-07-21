using UnityEngine;


//Used by cameras so they don't rotate with player movement.
public class LockRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }
}

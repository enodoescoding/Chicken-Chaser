using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    
    // Update is called once per frame
    void LateUpdate()
    {
        //rotate about the up axis based on some speed relative to the world
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}

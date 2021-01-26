using UnityEngine;

///<summary>Class used to rotate a gameObject at a constant rate</summary>
public class Rotate : MonoBehaviour
{
    ///<summary>speed of the rotation, assigned in inspector</summary>
    public float rotationSpeed;

    ///<summary>Update is called by unity every frame</summary>
    void Update()
    {
        transform.RotateAround(transform.position, Vector3.up, rotationSpeed);
    }
}

using UnityEngine;

///<summary>Script for Camera Movement</summary>
public class CameraController : MonoBehaviour
{
    ///<summary> RotationSpeed for the Camera. Public so it can be set in the Inspector</summary>
    public float rotationSpeed;

    ///<summary> Allows inversion of the Camera Movement on the X axis</summary>
    public bool invertX;
    ///<summary> Allows inversion of the Camera Movement on the Y axis</summary>
    public bool invertY;

    ///<summary> Update is called once per Frame while the application is running</summary>
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, Input.GetAxis("Horizontal") * rotationSpeed * (invertX ? 1 : -1));
        transform.RotateAround(Vector3.zero, transform.right, Input.GetAxis("Vertical") * rotationSpeed * (invertY ? -1 : 1));
        transform.position -= transform.forward * Input.GetAxis("Mouse ScrollWheel");
        transform.LookAt(Vector3.zero);
    }
}

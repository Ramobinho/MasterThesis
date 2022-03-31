using System.Collections;
using UnityEngine;

class MovingTest : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float MovementSpeed = 1.0f;
    [SerializeField] private float RotationSpeed = 1.0f;

    [SerializeField] private KeyCode ForwardKey = KeyCode.Z;
    [SerializeField] private KeyCode BackwardKey = KeyCode.S;
    [SerializeField] private KeyCode LeftKey = KeyCode.Q;
    [SerializeField] private KeyCode RightKey = KeyCode.D;

    [SerializeField] private KeyCode AnchoredMoveKey = KeyCode.Mouse2;
    [SerializeField] private KeyCode AnchoredRotateKey = KeyCode.Mouse1;

    private void FixedUpdate()
    {
        Vector3 Move = Vector3.zero; //fixedupdate gaat elke frame updaten, moet niet als klasse variabele. dit gebeurt elke seconde

        //move and rotate the camera

        //move the camera foward, abck, left & right
        if (Input.GetKey(ForwardKey)){
            Move += Vector3.forward * MovementSpeed;
        }
        if (Input.GetKey(BackwardKey)){
            Move += Vector3.back * MovementSpeed;
        }
        if (Input.GetKey(LeftKey)){
            Move += Vector3.left * MovementSpeed;
        }
        if (Input.GetKey(RightKey)){
            Move += Vector3.right * MovementSpeed;
        }

        float MouseMoveY = Input.GetAxis("Mouse Y");
        float MouseMoveX = Input.GetAxis("Mouse X");

        //rotate the camera when anchored
        if (Input.GetKey(AnchoredRotateKey))
        {
            transform.RotateAround(transform.position, transform.right, MouseMoveY * -RotationSpeed);
            transform.RotateAround(transform.position, transform.up, MouseMoveX * RotationSpeed);
        }



        //move the camera when anchored
        if (Input.GetKey(AnchoredMoveKey))
        {
            Move -= Vector3.up * MouseMoveY * MovementSpeed;
            Move -= Vector3.right * MouseMoveX * MovementSpeed;
        }


        
        transform.Translate(Move);
    }
}

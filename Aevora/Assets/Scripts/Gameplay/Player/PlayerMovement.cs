using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    private Rigidbody rb;
    private Vector3 currentInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    public void SetMovementInput(Vector3 input)
    {
        currentInput = input;
    }
    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }
    private void MovePlayer()
    {
        Vector3 targetVelocity = currentInput * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    private void RotatePlayer()
    {
        if (currentInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

}
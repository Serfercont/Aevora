using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveForce = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;
    private PlayerInput playerInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }
    private void FixedUpdate()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        rb.AddForce(new Vector3(moveInput.x, 0, moveInput.y) * moveForce);
    }

}
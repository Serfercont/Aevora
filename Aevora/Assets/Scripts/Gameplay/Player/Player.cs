using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]

public class Player : MonoBehaviour
{
    private PlayerMovement movementModule;
    private PlayerInteraction interactionModule;
    private InputSystem_Actions playerControls;

    public bool isDead = false;
    public bool canMove = true;

    private void Awake()
    {
        movementModule = GetComponent<PlayerMovement>();
        interactionModule = GetComponent<PlayerInteraction>();
        playerControls = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.Player.Interact.performed += OnInteractInput;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.Interact.performed -= OnInteractInput;
    }
    
    private void Update()
    {
        if (isDead || !canMove)
        {
            movementModule.SetMovementInput(Vector3.zero);
            return;
        }
        HandleMovementInput();
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HandleInteractionInput();
        }
    }

    private void HandleMovementInput()
    {
        Vector2 input = playerControls.Player.Move.ReadValue<Vector2>();
        Vector3 inputDir = new Vector3(input.x, 0f, input.y).normalized;
        movementModule.SetMovementInput(inputDir);
    }

    private void HandleInteractionInput()
    {
        if (isDead || !canMove) return;
        
        interactionModule.TryInteract();
    }

    [YarnCommand("toggle_move")]
    public void ToggleMovement(bool state)
    {
        canMove = state;
        if(!state)
        {
            movementModule.SetMovementInput(Vector3.zero);
        }
    }
}

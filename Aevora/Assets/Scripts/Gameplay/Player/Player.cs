using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
public class Player : MonoBehaviour, IPlayerState
{
    public bool IsDead  { get; private set; } = false;
    public bool CanMove { get; private set; } = true;

    private PlayerMovement movementModule;
    private PlayerInteraction interactionModule;
    private InputSystem_Actions controls;

    private void Awake()
    {
        movementModule = GetComponent<PlayerMovement>();
        interactionModule = GetComponent<PlayerInteraction>();
        controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.Player.Interact.performed -= OnInteractPerformed;
    }

    private void Update()
    {
        if (IsDead || !CanMove)
        {
            movementModule.SetInput(Vector3.zero);
            return;
        }

        Vector2 raw = controls.Player.Move.ReadValue<Vector2>();
        movementModule.SetInput(new Vector3(raw.x, 0f, raw.y).normalized);
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (IsDead || !CanMove) return;
        interactionModule.TryInteract();
    }



    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        CanMove = false;
        movementModule.SetInput(Vector3.zero);
    }

    [YarnCommand("toggle_move")]
    public void ToggleMovement(bool state)
    {
        if (IsDead) return;
        CanMove = state;
        if (!state) movementModule.SetInput(Vector3.zero);
    }
}

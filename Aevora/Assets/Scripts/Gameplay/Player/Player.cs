using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerInventory))]
public class Player : MonoBehaviour, IPlayerState
{
    public bool IsDead  { get; private set; } = false;
    public bool CanMove { get; private set; } = true;

    private PlayerMovement movementModule;
    private PlayerInteraction interactionModule;
    private PlayerInventory inventoryModule;
    private InputSystem_Actions controls;
    public Vector3 LastCheckpointPosition { get; set; }

    private void Awake()
    {
        movementModule = GetComponent<PlayerMovement>();
        interactionModule = GetComponent<PlayerInteraction>();
        inventoryModule = GetComponent<PlayerInventory>();
        controls = new InputSystem_Actions();

        LastCheckpointPosition = transform.position;
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Interact.performed += OnInteractPerformed;
        controls.Player.Heal.performed += OnHealPerformed;
    }
        
    private void OnDisable()
    {
        controls.Disable();
        controls.Player.Interact.performed -= OnInteractPerformed;
        controls.Player.Heal.performed -= OnHealPerformed;
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

    private void OnHealPerformed(InputAction.CallbackContext ctx)
    {
        if (IsDead || !CanMove) return;
        inventoryModule.TryUseMedkit();
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

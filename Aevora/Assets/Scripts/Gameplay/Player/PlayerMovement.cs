using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed     = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    public event Action OnStartMoving;
    public event Action OnStopMoving;

    public bool IsMoving => _input.sqrMagnitude > 0.001f;

    private Rigidbody _rb;
    private Vector3   _input;
    private bool      _wasMoving;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints  = RigidbodyConstraints.FreezeRotation;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetInput(Vector3 input) => _input = input;

    private void FixedUpdate()
    {
        ApplyVelocity();
        ApplyRotation();
        NotifyMovementStateChange();
    }

    private void ApplyVelocity()
    {
        Vector3 target   = _input * moveSpeed;
        _rb.linearVelocity = new Vector3(target.x, _rb.linearVelocity.y, target.z);
    }

    private void ApplyRotation()
    {
        if (_input == Vector3.zero) return;
        Quaternion target = Quaternion.LookRotation(_input);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
    }

    private void NotifyMovementStateChange()
    {
        bool moving = IsMoving;
        if (moving == _wasMoving) return;

        _wasMoving = moving;
        if (moving) OnStartMoving?.Invoke();
        else        OnStopMoving?.Invoke();
    }
}

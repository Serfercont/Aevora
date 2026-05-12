using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimations : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private Animator _animator;
    private PlayerMovement _movement;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _movement.OnStartMoving += OnStartMoving;
        _movement.OnStopMoving  += OnStopMoving;
    }

    private void OnDisable()
    {
        _movement.OnStartMoving -= OnStartMoving;
        _movement.OnStopMoving  -= OnStopMoving;
    }

    private void OnStartMoving() => _animator.SetBool(IsMovingHash, true);
    private void OnStopMoving()  => _animator.SetBool(IsMovingHash, false);
}

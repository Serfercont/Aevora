using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float     interactRange    = 2f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("References")]
    [SerializeField] private FloatingInteractionUI interactionUI;

    private IPlayerState      _state;
    private IInteractable     _current;

    private void Awake()
    {

        _state = GetComponent<IPlayerState>();
    }

    private void Update()
    {
        if (_state.IsDead || !_state.CanMove)
        {
            ClearCurrent();
            return;
        }

        TrackNearestInteractable();
    }

    public void TryInteract()
    {
        if (_current == null) return;
        _current.Interact(gameObject);
        ClearCurrent();
    }



    private void TrackNearestInteractable()
    {
        IInteractable nearest = FindNearest();

        if (nearest == _current) return;    // Nothing changed

        _current = nearest;

        if (_current != null)
        {
            Vector3 pos = (_current as Component)?.transform.position ?? transform.position;
            interactionUI.Show(_current.GetInteractionPrompt(), pos);
        }
        else
        {
            interactionUI.Hide();
        }
    }

    private IInteractable FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);
        foreach (Collider hit in hits)
        {
            IInteractable found = hit.GetComponent<IInteractable>();
            if (found != null) return found;
        }
        return null;
    }

    private void ClearCurrent()
    {
        if (_current == null) return;
        _current = null;
        interactionUI.Hide();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}

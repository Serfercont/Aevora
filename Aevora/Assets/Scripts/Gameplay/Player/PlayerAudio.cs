using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource stepsAudioSource;

    [Header("Footsteps Settings")]
    [SerializeField] private AudioClip[] stepClips;
    [SerializeField] private float stepInterval = 0.45f;  

    private PlayerMovement _movement;
    private Coroutine _footstepsCoroutine;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();

        if (stepsAudioSource != null)
        {
            stepsAudioSource.playOnAwake = false;
            stepsAudioSource.loop = false;
        }
    }

    private void OnEnable()
    {
        _movement.OnStartMoving += StartFootsteps;
        _movement.OnStopMoving  += StopFootsteps;
    }

    private void OnDisable()
    {
        _movement.OnStartMoving -= StartFootsteps;
        _movement.OnStopMoving  -= StopFootsteps;
        StopFootsteps();
    }

    private void StartFootsteps()
    {
        if (_footstepsCoroutine == null)
        {
            _footstepsCoroutine = StartCoroutine(FootstepsRoutine());
        }
    }

    private void StopFootsteps()
    {
        if (_footstepsCoroutine != null)
        {
            StopCoroutine(_footstepsCoroutine);
            _footstepsCoroutine = null;
        }
    }

    private IEnumerator FootstepsRoutine()
    {
        while (_movement.IsMoving)
        {
            PlayRandomFootstep();
            yield return new WaitForSeconds(stepInterval);
        }
        _footstepsCoroutine = null;
    }

    private void PlayRandomFootstep()
    {
        if (stepClips == null || stepClips.Length == 0 || stepsAudioSource == null) return;

        int randomIndex = Random.Range(0, stepClips.Length);
        
        stepsAudioSource.PlayOneShot(stepClips[randomIndex]);
    }
}
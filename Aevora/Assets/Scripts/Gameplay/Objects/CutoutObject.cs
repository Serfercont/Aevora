using System.Collections.Generic;
using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private Renderer targetRenderer;

    private Camera mainCamera;
    private readonly HashSet<Renderer> activeRenderers = new HashSet<Renderer>();

    private static readonly int CutoutPosId = Shader.PropertyToID("_CutoutPos");
    private static readonly int CutoutSizeId = Shader.PropertyToID("_CutoutSize");
    private static readonly int FalloffSizeId = Shader.PropertyToID("_FalloffSize");

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    private void Start()
    {
        ClearCutout();
    }

    private void Update()
    {
        if (mainCamera == null || targetObject == null)
            return;

        Bounds bounds = targetRenderer != null
            ? targetRenderer.bounds
            : new Bounds(targetObject.position, Vector3.one);

        Vector3 cameraPos = mainCamera.transform.position;

        Vector3[] samplePoints =
        {
            bounds.center,
            bounds.center + Vector3.up * bounds.extents.y,
            bounds.center - Vector3.up * bounds.extents.y
        };

        bool isBlocked = false;
        foreach (Vector3 point in samplePoints)
        {
            Vector3 direction = point - cameraPos;
            if (Physics.Raycast(cameraPos, direction.normalized, direction.magnitude, wallMask))
            {
                isBlocked = true;
                break;
            }
        }

        if (!isBlocked)
        {
            ClearCutout();
            return;
        }

        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(targetObject.position);

        RaycastHit[] hitObjects = Physics.RaycastAll(
            cameraPos,
            (targetObject.position - cameraPos).normalized,
            Vector3.Distance(cameraPos, targetObject.position),
            wallMask
        );

        // Primero limpia los renderers que ya no están bloqueando
        HashSet<Renderer> newRenderers = new HashSet<Renderer>();
        for (int i = 0; i < hitObjects.Length; i++)
        {
            Renderer r = hitObjects[i].collider.GetComponent<Renderer>();
            if (r != null) newRenderers.Add(r);
        }

        // Apaga cutout en renderers que salieron del rayo
        foreach (Renderer r in activeRenderers)
        {
            if (r == null || newRenderers.Contains(r)) continue;
            foreach (Material m in r.materials)
            {
                m.SetFloat(CutoutSizeId, 0f);
                m.SetFloat(FalloffSizeId, 0f);
            }
        }

        activeRenderers.Clear();

        // Aplica cutout solo a los nuevos
        foreach (Renderer r in newRenderers)
        {
            if (!activeRenderers.Add(r)) continue;
            foreach (Material m in r.materials)
            {
                m.SetVector(CutoutPosId, cutoutPos);
                m.SetFloat(CutoutSizeId, 0.1f);
                m.SetFloat(FalloffSizeId, 0.0f);
            }
        }
    }

    private void ClearCutout()
    {
        foreach (Renderer renderer in activeRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetFloat(CutoutSizeId, 0f);
                materials[i].SetFloat(FalloffSizeId, 0f);
            }
        }

        activeRenderers.Clear();
    }
}

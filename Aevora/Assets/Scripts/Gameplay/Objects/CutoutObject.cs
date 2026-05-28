using System.Collections.Generic;
using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private Renderer targetRenderer;

    private Camera mainCamera;

    private readonly HashSet<Renderer> activeRenderers = new HashSet<Renderer>();
    private readonly HashSet<Renderer> nextRenderers   = new HashSet<Renderer>();

    private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

    private readonly Vector3[] samplePoints = new Vector3[3];

    private static readonly int CutoutPosId    = Shader.PropertyToID("_CutoutPos");
    private static readonly int CutoutSizeId   = Shader.PropertyToID("_CutoutSize");
    private static readonly int FalloffSizeId  = Shader.PropertyToID("_FalloffSize");

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

        Vector3 center  = bounds.center;
        float   extentY = bounds.extents.y;

        samplePoints[0] = center;
        samplePoints[1] = center + new Vector3(0f,  extentY, 0f);
        samplePoints[2] = center + new Vector3(0f, -extentY, 0f);

        Vector3 cameraPos = mainCamera.transform.position;

        nextRenderers.Clear();

        foreach (Vector3 point in samplePoints)
        {
            Vector3 dir      = point - cameraPos;
            float   distance = dir.magnitude;

            int hitCount = Physics.RaycastNonAlloc(
                cameraPos,
                dir / distance,   
                HitBuffer,
                distance,
                wallMask,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < hitCount; i++)
            {
                Renderer r = HitBuffer[i].collider.GetComponent<Renderer>();
                if (r != null) nextRenderers.Add(r);
            }
        }

        if (nextRenderers.Count == 0)
        {
            ClearCutout();
            return;
        }

        foreach (Renderer r in activeRenderers)
        {
            if (r == null || nextRenderers.Contains(r)) continue;

            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetFloat(CutoutSizeId,  0f);
                mats[i].SetFloat(FalloffSizeId, 0f);
            }
        }

        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(targetObject.position);

        foreach (Renderer r in nextRenderers)
        {
            bool isNew = !activeRenderers.Contains(r);

            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (isNew)
                    mats[i].SetVector(CutoutPosId, cutoutPos);

                mats[i].SetFloat(CutoutSizeId,  0.1f);
                mats[i].SetFloat(FalloffSizeId, 0.0f);
            }
        }

        activeRenderers.Clear();
        foreach (Renderer r in nextRenderers)
            activeRenderers.Add(r);
    }

    private void ClearCutout()
    {
        foreach (Renderer r in activeRenderers)
        {
            if (r == null) continue;

            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetFloat(CutoutSizeId,  0f);
                mats[i].SetFloat(FalloffSizeId, 0f);
            }
        }
        activeRenderers.Clear();
    }
}
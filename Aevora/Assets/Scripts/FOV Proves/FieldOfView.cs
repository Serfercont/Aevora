using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour 
{
    [Header("Configuración de Visión")]
    public float viewRadius;
    [Range(0, 360)] public float viewAngle;
    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;
    public float maskCutawayDst = .1f;

    [Header("Capas (Layers)")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Referencias")]
    public MeshFilter viewMeshFilter;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    private List<Transform> previouslyVisibleTargets = new List<Transform>();

    private Mesh viewMesh;
    private Collider[] targetsInViewRadius = new Collider[50];
    private List<Vector3> viewPoints  = new List<Vector3>();
    private List<Vector3> vertices    = new List<Vector3>();
    private List<int>     triangles   = new List<int>();

    private int hiddenLayerIndex;

    // ─── Ciclo de vida ────────────────────────────────────────────────────────

    void Start() 
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        hiddenLayerIndex = LayerMask.NameToLayer("OcultoPorFOV");

        InitializeTargets();
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    void LateUpdate() 
    {
        DrawFieldOfView();
    }

    // ─── Inicialización ───────────────────────────────────────────────────────

    void InitializeTargets()
    {
        Collider[] all = new Collider[200];
        int found = Physics.OverlapSphereNonAlloc(transform.position, viewRadius * 10f, all, targetMask);
        for (int i = 0; i < found; i++)
        {
            if (all[i] != null)
                SetLayer(all[i].transform.gameObject, hiddenLayerIndex, hiddenLayerIndex);
        }
    }

    // ─── Detección ────────────────────────────────────────────────────────────

    IEnumerator FindTargetsWithDelay(float delay) 
    {
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (true) 
        {
            yield return wait;
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets() 
    {
        visibleTargets.Clear();

        int targetsFound = Physics.OverlapSphereNonAlloc(transform.position, viewRadius, targetsInViewRadius, targetMask);

        for (int i = 0; i < targetsFound; i++) 
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) 
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask)) 
                {
                    visibleTargets.Add(target);
                }
            }
        }

        foreach (Transform prev in previouslyVisibleTargets)
        {
            if (prev == null || visibleTargets.Contains(prev)) continue;

            FOVTarget fovTarget = prev.GetComponent<FOVTarget>();
            if (fovTarget != null && fovTarget.staysVisible) continue;

            int visibleLayer = GetVisibleLayer(prev, fovTarget);
            SetLayer(prev.gameObject, hiddenLayerIndex, visibleLayer);
        }

        foreach (Transform target in visibleTargets)
        {
            FOVTarget fovTarget = target.GetComponent<FOVTarget>();
            int visibleLayer = GetVisibleLayer(target, fovTarget);
            SetLayer(target.gameObject, visibleLayer, visibleLayer);
        }

        previouslyVisibleTargets.Clear();
        previouslyVisibleTargets.AddRange(visibleTargets);
    }

    // ─── Helpers de capas ─────────────────────────────────────────────────────

    int GetVisibleLayer(Transform target, FOVTarget fovTarget)
    {
        if (fovTarget != null && fovTarget.VisibleLayer != -1)
            return fovTarget.VisibleLayer;

        return target.gameObject.layer != hiddenLayerIndex
            ? target.gameObject.layer
            : hiddenLayerIndex;
    }

    void SetLayer(GameObject obj, int toLayer, int fromLayer)
    {
        if (obj == null) return;

        if (obj.layer == hiddenLayerIndex || obj.layer == fromLayer)
            obj.layer = toLayer;

        foreach (Transform child in obj.transform)
            SetLayer(child.gameObject, toLayer, fromLayer);
    }

    // ─── Dibujo del mesh de visión ────────────────────────────────────────────

    void DrawFieldOfView() 
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        viewPoints.Clear();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++) 
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0) 
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) 
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        vertices.Clear();
        triangles.Clear();
        vertices.Add(Vector3.zero);

        for (int i = 0; i < vertexCount - 1; i++) 
        {
            Vector3 dir = (viewPoints[i] - transform.position).normalized;
            vertices.Add(transform.InverseTransformPoint(viewPoints[i] + dir * maskCutawayDst));

            if (i < vertexCount - 2) 
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }
        }

        viewMesh.Clear();
        viewMesh.SetVertices(vertices);
        viewMesh.SetTriangles(triangles, 0);
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) 
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++) 
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) 
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            } 
            else 
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle) 
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        else
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) 
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // ─── Structs ──────────────────────────────────────────────────────────────

    public struct ViewCastInfo 
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) 
        {
            hit = _hit; point = _point; dst = _dst; angle = _angle;
        }
    }

    public struct EdgeInfo 
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) 
        {
            pointA = _pointA; pointB = _pointB;
        }
    }
}
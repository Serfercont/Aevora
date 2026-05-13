using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour 
{
    [Header("Configuración de Visión")]
    public float viewRadius;
    [Range(0,360)] public float viewAngle;
    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;
    public float maskCutawayDst = .1f;

    [Header("Gestión de Capas de Enemigos")]
    [Tooltip("El nombre de la capa cuando el enemigo NO está a la vista.")]
    public string hiddenLayerName = "OcultoPorFOV";
    [Tooltip("El nombre de la capa cuando el enemigo SÍ está a la vista.")]
    public string visibleLayerName = "Enemy";

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
    private List<Vector3> viewPoints = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private int hiddenLayerIndex;
    private int visibleLayerIndex;

    void Start() 
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        hiddenLayerIndex = LayerMask.NameToLayer(hiddenLayerName);
        visibleLayerIndex = LayerMask.NameToLayer(visibleLayerName);

        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay) 
    {
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (true) 
        {
            yield return wait;
            FindVisibleTargets();
        }
    }

    void LateUpdate() 
    {
        DrawFieldOfView();
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

        foreach (Transform prevTarget in previouslyVisibleTargets)
        {
            if (prevTarget != null && !visibleTargets.Contains(prevTarget))
            {
                SetLayerRecursively(prevTarget.gameObject, hiddenLayerIndex);
            }
        }

        foreach (Transform target in visibleTargets)
        {
            if (!previouslyVisibleTargets.Contains(target))
            {
                SetLayerRecursively(target.gameObject, visibleLayerIndex);
            }
        }

        previouslyVisibleTargets.Clear();
        previouslyVisibleTargets.AddRange(visibleTargets);
    }
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }

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
            Vector3 pushedPoint = viewPoints[i] + (dir * maskCutawayDst);
            vertices.Add(transform.InverseTransformPoint(pushedPoint));

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
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        } 
        else 
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) 
    {
        if (!angleIsGlobal) 
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo 
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) 
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo 
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) 
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
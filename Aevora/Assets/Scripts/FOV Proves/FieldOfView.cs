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

    [Tooltip("Distancia que penetra la malla en los muros para revelar sus bordes.")]
    public float maskCutawayDst = .1f;

    [Header("Capas (Layers)")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Referencias")]
    public MeshFilter viewMeshFilter;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    private Mesh viewMesh;

    // OPTIMIZACIÓN: Caching de variables para no generar "Basura" (Garbage Collection) en el Update
    private Collider[] targetsInViewRadius = new Collider[50]; // Soporta hasta 50 objetivos simultáneos
    private List<Vector3> viewPoints = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    void Start() 
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay) 
    {
        // Optimización: Cacheamos el WaitForSeconds
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
        
        // OPTIMIZACIÓN: OverlapSphereNonAlloc no crea arrays nuevos, recicla el que le damos.
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
    }

    void DrawFieldOfView() 
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        
        viewPoints.Clear(); // Reutilizamos la lista en vez de crear una nueva
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
            // CORRECCIÓN VISUAL CRÍTICA:
            // Calculamos la dirección real hacia el punto para empujarlo dentro del muro correctamente.
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
        // Usamos SetVertices y SetTriangles con listas, otra optimización nativa de Unity.
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
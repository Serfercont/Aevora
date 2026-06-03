using UnityEngine;
using System.Collections.Generic;

public class WallCutoutShader : MonoBehaviour
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float cutoutRadius = 0.15f;

    private Camera _cam;
    private Renderer[] _wallRenderers;
    private MaterialPropertyBlock _propBlock;

    void Start()
    {
        _cam = Camera.main;
        _propBlock = new MaterialPropertyBlock();

        var all = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var walls = new System.Collections.Generic.List<Renderer>();
        foreach (var r in all)
            if (((1 << r.gameObject.layer) & wallLayer) != 0)
                walls.Add(r);
        _wallRenderers = walls.ToArray();
    }

    void LateUpdate()
    {
        Vector3 screen = _cam.WorldToScreenPoint(transform.position);
        Vector2 pos = new Vector2(screen.x / Screen.width, screen.y / Screen.height);

        bool hidden = false;
        Vector3 dir = transform.position - _cam.transform.position;

        if (Physics.Raycast(_cam.transform.position, dir.normalized,
            out RaycastHit hitInfo, dir.magnitude, wallLayer, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(hitInfo.normal.z) < 0.5f) 
                hidden = true;
        }

        _propBlock.SetVector("_CutoutPos", new Vector4(pos.x, pos.y, 0, 0));
        _propBlock.SetFloat("_Cutoff", hidden ? cutoutRadius : 0f);

        foreach (var r in _wallRenderers)
            r.SetPropertyBlock(_propBlock);
    }
}
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
        // Posición del jugador en viewport (0-1)
        Vector2 pos = _cam.WorldToViewportPoint(transform.position);

        // No hace falta raycast: el shader dibuja el círculo en la posición de pantalla
        // del jugador. Solo las paredes renderizadas en esa posición lo muestran —
        // las que no tapan al jugador quedan fuera del círculo automáticamente.
        _propBlock.SetVector("_CutoutPos", new Vector4(pos.x, pos.y, 0, 0));
        _propBlock.SetFloat("_Cutoff", cutoutRadius);

        foreach (var r in _wallRenderers)
            r.SetPropertyBlock(_propBlock);
    }
}
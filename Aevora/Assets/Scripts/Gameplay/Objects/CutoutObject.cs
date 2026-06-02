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

        // Recoge todos los renderers que estén en el wallLayer
        var all = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var walls = new System.Collections.Generic.List<Renderer>();
        foreach (var r in all)
            if (((1 << r.gameObject.layer) & wallLayer) != 0)
                walls.Add(r);
        _wallRenderers = walls.ToArray();
    }

    void LateUpdate()
    {
        // Posición del jugador en pantalla normalizada (0-1)
        Vector3 screen = _cam.WorldToScreenPoint(transform.position);
        Vector2 pos = new Vector2(screen.x / Screen.width, screen.y / Screen.height);

        // ¿Hay una pared entre la cámara y el jugador?
        bool hidden = false;
        Vector3 dir = transform.position - _cam.transform.position;

        if (Physics.Raycast(_cam.transform.position, dir.normalized,
            out RaycastHit hitInfo, dir.magnitude, wallLayer, QueryTriggerInteraction.Ignore))
        {
            // Solo activar si la pared es vertical (normal horizontal, no techo/suelo)
            if (Mathf.Abs(hitInfo.normal.z) < 0.5f) 
                hidden = true;
        }

        // Actualizar el PropertyBlock (aplica a TODOS los material slots del renderer)
        _propBlock.SetVector("_CutoutPos", new Vector4(pos.x, pos.y, 0, 0));
        _propBlock.SetFloat("_Cutoff", hidden ? cutoutRadius : 0f);

        foreach (var r in _wallRenderers)
            r.SetPropertyBlock(_propBlock);
    }
}
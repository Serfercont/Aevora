using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WallCutout : MonoBehaviour
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField, Range(0f, 1f)] private float cutoffThreshold = 0.5f;
    [SerializeField] private float adjacentPanelRadius = 1.0f;
    [SerializeField] private Transform playerTransform;

    private readonly Dictionary<Renderer, MaterialSnapshot[]> _activeCutouts = new();
    private readonly HashSet<Renderer> _hitThisFrame = new();
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[32];
    private readonly Collider[] _overlapBuffer = new Collider[16];

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 camPos = transform.position;
        Vector3 dir = playerTransform.position - camPos;
        float dist = dir.magnitude;

        // 1. Detectar paredes en la línea directa cámara→jugador
        _hitThisFrame.Clear();

        int hitCount = Physics.RaycastNonAlloc(camPos, dir.normalized, _hitBuffer,
                                               dist, wallLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            // Ignorar paredes laterales: solo actuar si la pared mira hacia la cámara.
            // Usamos transform.forward (dirección fija de la cámara) como referencia,
            // no dir.normalized (que cambia con la posición del jugador).
            if (Vector3.Dot(_hitBuffer[i].normal, transform.forward) > -0.6f) continue;

            // Pared directamente en la línea de visión
            AddRendererFromCollider(_hitBuffer[i].collider);

            // Paneles adyacentes cerca del punto de impacto (no del jugador)
            int nearCount = Physics.OverlapSphereNonAlloc(
                _hitBuffer[i].point, adjacentPanelRadius, _overlapBuffer, wallLayer);

            for (int j = 0; j < nearCount; j++)
                AddRendererFromCollider(_overlapBuffer[j]);
        }

        // 2. Aplicar cutout a paredes nuevas
        foreach (Renderer rend in _hitThisFrame)
            if (!_activeCutouts.ContainsKey(rend))
                ApplyCutout(rend);

        // 3. Restaurar paredes que ya no ocluyen
        List<Renderer> toRestore = null;
        foreach (var kvp in _activeCutouts)
            if (!_hitThisFrame.Contains(kvp.Key))
                (toRestore ??= new()).Add(kvp.Key);
        toRestore?.ForEach(RestoreMaterials);
    }

    private void OnDisable()
    {
        foreach (var kvp in new Dictionary<Renderer, MaterialSnapshot[]>(_activeCutouts))
            RestoreMaterials(kvp.Key);
    }

    private void AddRendererFromCollider(Collider col)
    {
        Renderer rend = col.GetComponentInChildren<Renderer>()
                     ?? col.GetComponentInParent<Renderer>();
        if (rend != null) _hitThisFrame.Add(rend);
    }

    private void ApplyCutout(Renderer rend)
    {
        Material[] mats = rend.materials;
        var snapshots = new MaterialSnapshot[mats.Length];
        for (int i = 0; i < mats.Length; i++)
        {
            snapshots[i] = new MaterialSnapshot(mats[i]);
            SetMaterialCutout(mats[i], cutoffThreshold);
        }
        rend.materials = mats;
        _activeCutouts[rend] = snapshots;
    }

    private void RestoreMaterials(Renderer rend)
    {
        if (!_activeCutouts.TryGetValue(rend, out var snapshots) || rend == null)
        { _activeCutouts.Remove(rend); return; }

        Material[] mats = rend.materials;
        for (int i = 0; i < mats.Length && i < snapshots.Length; i++)
            snapshots[i].RestoreTo(mats[i]);
        rend.materials = mats;
        _activeCutouts.Remove(rend);
    }

    private static void SetMaterialCutout(Material mat, float cutoff)
    {
        mat.SetFloat("_AlphaClip", 1f);
        mat.SetFloat("_AlphaToMask", 0.03f);
        mat.SetFloat("_Cutoff", cutoff);
        Color col = mat.GetColor("_BaseColor");
        col.a = 0.2f;
        mat.SetColor("_BaseColor", col);
        mat.SetColor("_Color", col);
        mat.EnableKeyword("_ALPHATEST_ON");
        mat.renderQueue = (int)RenderQueue.AlphaTest;
        mat.SetOverrideTag("RenderType", "TransparentCutout");
    }

    private struct MaterialSnapshot
    {
        private readonly float _alphaClip, _alphaToMask, _cutoff;
        private readonly Color _baseColor;
        private readonly int _renderQueue;
        private readonly bool _hadAlphaTestKw;

        public MaterialSnapshot(Material mat)
        {
            _alphaClip      = mat.GetFloat("_AlphaClip");
            _alphaToMask    = mat.GetFloat("_AlphaToMask");
            _cutoff         = mat.GetFloat("_Cutoff");
            _baseColor      = mat.GetColor("_BaseColor");
            _renderQueue    = mat.renderQueue;
            _hadAlphaTestKw = mat.IsKeywordEnabled("_ALPHATEST_ON");
        }

        public void RestoreTo(Material mat)
        {
            mat.SetFloat("_AlphaClip",   _alphaClip);
            mat.SetFloat("_AlphaToMask", _alphaToMask);
            mat.SetFloat("_Cutoff",      _cutoff);
            mat.SetColor("_BaseColor",   _baseColor);
            mat.SetColor("_Color",       _baseColor);
            mat.renderQueue = _renderQueue;
            if (_hadAlphaTestKw) mat.EnableKeyword("_ALPHATEST_ON");
            else                 mat.DisableKeyword("_ALPHATEST_ON");
            mat.SetOverrideTag("RenderType", _alphaClip > 0.5f ? "TransparentCutout" : "Opaque");
        }
    }
}

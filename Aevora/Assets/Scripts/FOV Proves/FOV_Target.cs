using UnityEngine;

public class FOVTarget : MonoBehaviour
{
    [Tooltip("Nombre de la capa cuando este objeto está visible (ej: 'Enemy', 'Interactable')")]
    public string visibleLayerName;

    [Tooltip("Si está activo, el objeto NO vuelve a ocultarse al salir del FOV")]
    public bool staysVisible = false;

    public int VisibleLayer  { get; private set; }
    public int HiddenLayer   { get; private set; }

    void Awake()
    {
        HiddenLayer  = LayerMask.NameToLayer("OcultoPorFOV");
        VisibleLayer = LayerMask.NameToLayer(visibleLayerName);

        if (VisibleLayer == -1)
            Debug.LogWarning($"[FOVTarget] La capa '{visibleLayerName}' no existe en el proyecto.", this);
    }
}
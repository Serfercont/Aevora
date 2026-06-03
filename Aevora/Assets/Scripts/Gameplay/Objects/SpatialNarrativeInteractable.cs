using UnityEngine;

public class SpatialNarrativeInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private string visualPrompt = "Presiona 'E' para inspeccionar";

    [Header("Contenido")]
    [SerializeField] private Sprite image;

    [Header("Texto (Opcional)")]
    [TextArea(3, 30)]
    [SerializeField] private string readableText;
    [SerializeField] private TextAsset readableTextAsset;

    [Header("Comportamiento")]
    [SerializeField] private bool lockPlayerMovement = true;

    [SerializeField] private bool isZekeTortureReport = false;
    public static bool FoundZekeReport { get; private set; } = false;

    public void Interact(GameObject player)
    {
        SpatialNarrativeUI ui = SpatialNarrativeUI.GetOrCreate();
        if (ui == null)
            return;

        if (isZekeTortureReport)
        {
            FoundZekeReport = true;
        }

        Player playerController = player != null ? player.GetComponent<Player>() : null;
        ui.Open(image, ResolveReadableText(), playerController, lockPlayerMovement);
    }

    public string GetInteractionPrompt()
    {
        return visualPrompt;
    }

    private string ResolveReadableText()
    {
        if (readableTextAsset != null && !string.IsNullOrWhiteSpace(readableTextAsset.text))
            return readableTextAsset.text;

        return readableText;
    }
}

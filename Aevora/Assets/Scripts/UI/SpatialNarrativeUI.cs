using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpatialNarrativeUI : MonoBehaviour
{
    public static SpatialNarrativeUI Instance { get; private set; }

    [Header("UI References (Optional)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image artworkImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button readToggleButton;
    [SerializeField] private TextMeshProUGUI readToggleButtonLabel;
    [SerializeField] private GameObject textRoot;
    [SerializeField] private TextMeshProUGUI bodyText;

    private Player currentPlayer;
    private bool restoreMovementOnClose;
    private bool previousCanMove;

    private bool isOpen;
    private bool hasReadableText;
    private bool frameJustOpened;

    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private bool restoreCursorOnClose;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureUI();
        HideImmediate();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (frameJustOpened)
        {
            frameJustOpened = false;
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (textRoot.activeSelf)
                {
                    textRoot.SetActive(false);

                    if (readToggleButton != null) 
                        readToggleButton.gameObject.SetActive(true);
                }
                else
                {
                    Close();
                }
                return;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (hasReadableText && !textRoot.activeSelf)
                {
                    textRoot.SetActive(true);
                    if (readToggleButton != null) 
                        readToggleButton.gameObject.SetActive(false);
                }
            }
        }
    }

    public static SpatialNarrativeUI GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        SpatialNarrativeUI existing = FindFirstObjectByType<SpatialNarrativeUI>();
        if (existing != null)
        {
            Instance = existing;
            existing.EnsureUI();
            existing.HideImmediate();
            return existing;
        }

        GameObject root = new GameObject("SpatialNarrativeUI");
        SpatialNarrativeUI created = root.AddComponent<SpatialNarrativeUI>();
        created.EnsureUI();
        created.HideImmediate();
        return created;
    }

    public void Open(Sprite image, string readableText, Player player, bool lockPlayerMovement = true)
    {
        EnsureUI();

        currentPlayer = player;
        restoreMovementOnClose = false;

        if (lockPlayerMovement && currentPlayer != null)
        {
            previousCanMove = currentPlayer.CanMove;
            currentPlayer.ToggleMovement(false);
            restoreMovementOnClose = true;
        }

        if (!isOpen)
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            restoreCursorOnClose = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        isOpen = true;
        frameJustOpened = true;

        canvas.enabled = true;

        artworkImage.enabled = image != null;
        artworkImage.sprite = image;

        hasReadableText = !string.IsNullOrWhiteSpace(readableText);
        readToggleButton.gameObject.SetActive(hasReadableText);
        textRoot.SetActive(false);
        
        if (readToggleButton != null)
            readToggleButton.gameObject.SetActive(hasReadableText);
            
        textRoot.SetActive(false);

        if (hasReadableText)
        {
            bodyText.text = readableText;
            SetReadToggleLabel("Presiona 'E' para leer");
        }
        else
        {
            bodyText.text = string.Empty;
            SetReadToggleLabel(string.Empty);
        }
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        canvas.enabled = false;
        textRoot.SetActive(false);

        if (restoreMovementOnClose && currentPlayer != null)
        {
            currentPlayer.ToggleMovement(previousCanMove);
        }

        currentPlayer = null;
        restoreMovementOnClose = false;

        if (restoreCursorOnClose)
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
            restoreCursorOnClose = false;
        }
    }

    public void ToggleRead()
    {
        if (!isOpen || !hasReadableText)
            return;

        bool show = !textRoot.activeSelf;
        textRoot.SetActive(show);
        SetReadToggleLabel(show ? "Ocultar" : "Leer");
    }

    private void SetReadToggleLabel(string text)
    {
        if (readToggleButtonLabel != null)
        {
            readToggleButtonLabel.text = text;
        }
    }

    private void HideImmediate()
    {
        isOpen = false;
        if (canvas != null)
        {
            canvas.enabled = false;
        }

        if (textRoot != null)
        {
            textRoot.SetActive(false);
        }
    }

    private void EnsureUI()
    {
        if (canvas != null && artworkImage != null && closeButton != null && readToggleButton != null && textRoot != null && bodyText != null)
        {
            EnsureEventSystemExists();
            return;
        }

        if (canvas == null)
        {
            canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (gameObject.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        BuildDefaultUIIfNeeded();
        EnsureEventSystemExists();
    }

    private void BuildDefaultUIIfNeeded()
    {
        Transform existingRoot = transform.Find("_SpatialNarrativeRoot");
        if (existingRoot != null)
        {
            CacheDefaultRefs(existingRoot);
            return;
        }

        GameObject root = new GameObject("_SpatialNarrativeRoot", typeof(RectTransform));
        root.transform.SetParent(transform, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(root.transform, false);
        RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        Image backdropImage = backdrop.GetComponent<Image>();
        backdropImage.color = new Color(0f, 0f, 0f, 0.75f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.15f);
        panelRect.anchorMax = new Vector2(0.85f, 0.85f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(1f, 1f, 1f, 0.95f);

        GameObject artwork = new GameObject("Artwork", typeof(RectTransform), typeof(Image));
        artwork.transform.SetParent(panel.transform, false);
        RectTransform artworkRect = artwork.GetComponent<RectTransform>();
        artworkRect.anchorMin = new Vector2(0.05f, 0.35f);
        artworkRect.anchorMax = new Vector2(0.95f, 0.95f);
        artworkRect.offsetMin = Vector2.zero;
        artworkRect.offsetMax = Vector2.zero;
        Image artworkImg = artwork.GetComponent<Image>();
        artworkImg.preserveAspect = true;

        GameObject readButtonGO = CreateButton(panel.transform, "ReadButton", "Leer");
        RectTransform readButtonRect = readButtonGO.GetComponent<RectTransform>();
        readButtonRect.anchorMin = new Vector2(0.05f, 0.05f);
        readButtonRect.anchorMax = new Vector2(0.45f, 0.15f);
        readButtonRect.offsetMin = Vector2.zero;
        readButtonRect.offsetMax = Vector2.zero;

        GameObject closeButtonGO = CreateButton(panel.transform, "CloseButton", "Cerrar");
        RectTransform closeButtonRect = closeButtonGO.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.55f, 0.05f);
        closeButtonRect.anchorMax = new Vector2(0.95f, 0.15f);
        closeButtonRect.offsetMin = Vector2.zero;
        closeButtonRect.offsetMax = Vector2.zero;

        GameObject textPanel = new GameObject("TextRoot", typeof(RectTransform), typeof(Image));
        textPanel.transform.SetParent(panel.transform, false);
        RectTransform textPanelRect = textPanel.GetComponent<RectTransform>();
        textPanelRect.anchorMin = new Vector2(0.05f, 0.05f);
        textPanelRect.anchorMax = new Vector2(0.95f, 0.95f);
        
        textPanelRect.offsetMin = Vector2.zero;
        textPanelRect.offsetMax = Vector2.zero;
        Image textPanelBg = textPanel.GetComponent<Image>();
        textPanelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewport.transform.SetParent(textPanel.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(10f, 10f);
        viewportRect.offsetMax = new Vector2(-10f, -10f);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);

        GameObject bodyTextGO = new GameObject("BodyText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ContentSizeFitter));
        bodyTextGO.transform.SetParent(viewport.transform, false);
        RectTransform bodyRect = bodyTextGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.offsetMin = new Vector2(0f, 0f);
        bodyRect.offsetMax = new Vector2(0f, 0f);

        TextMeshProUGUI tmp = bodyTextGO.GetComponent<TextMeshProUGUI>();
        tmp.text = string.Empty;
        tmp.enableWordWrapping = true;
        tmp.fontSize = 24f;
        tmp.color = Color.white;

        ContentSizeFitter fitter = bodyTextGO.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGO.transform.SetParent(textPanel.transform, false);
        RectTransform scrollRectT = scrollGO.GetComponent<RectTransform>();
        scrollRectT.anchorMin = Vector2.zero;
        scrollRectT.anchorMax = Vector2.one;
        scrollRectT.offsetMin = Vector2.zero;
        scrollRectT.offsetMax = Vector2.zero;

        ScrollRect scrollRect = scrollGO.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = bodyRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        viewport.transform.SetParent(scrollGO.transform, false);

        CacheDefaultRefs(root.transform);
    }

    private void CacheDefaultRefs(Transform root)
    {
        Transform panel = root.Find("Panel");
        if (panel == null)
            return;

        Transform artwork = panel.Find("Artwork");
        Transform readBtn = panel.Find("ReadButton");
        Transform closeBtn = panel.Find("CloseButton");
        Transform textPanel = panel.Find("TextRoot");
        Transform body = root.Find("Panel/TextRoot/Scroll/Viewport/BodyText");

        artworkImage = artwork != null ? artwork.GetComponent<Image>() : artworkImage;

        if (readBtn != null)
        {
            readToggleButton = readBtn.GetComponent<Button>();
            readToggleButtonLabel = readBtn.Find("Label") != null ? readBtn.Find("Label").GetComponent<TextMeshProUGUI>() : readToggleButtonLabel;
        }

        if (closeBtn != null)
        {
            closeButton = closeBtn.GetComponent<Button>();
        }

        textRoot = textPanel != null ? textPanel.gameObject : textRoot;
        bodyText = body != null ? body.GetComponent<TextMeshProUGUI>() : bodyText;
    }

    private GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        Image image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(buttonGO.transform, false);
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10f, 6f);
        labelRect.offsetMax = new Vector2(-10f, -6f);

        TextMeshProUGUI tmp = labelGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28f;
        tmp.color = Color.black;

        return buttonGO;
    }

    private void EnsureEventSystemExists()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}

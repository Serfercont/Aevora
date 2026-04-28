using UnityEngine;
using TMPro;

public class FloatingInteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        Hide();
    }

    public void Show(string text, Vector3 targetPosition)
    {
        canvas.enabled = true;
        promptText.text = text;
        transform.position = targetPosition + offset;
    }

    public void Hide()
    {
        canvas.enabled = false;
    }

    void LateUpdate()
    {
        if (canvas.enabled)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }
}
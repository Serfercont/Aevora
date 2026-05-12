using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [SerializeField] private string nodeName = "GuardiasHablando";
    
    private DialogueRunner dialogueRunner;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.StartDialogue(nodeName);
                Destroy(gameObject);
            }
        }
    }
}
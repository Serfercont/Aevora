using UnityEngine;
using Yarn.Unity;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string interactionPrompt = "ZekeEncuentro";

    [SerializeField] 
    private string visualPrompt = "Press E to talk";
    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public void Interact(GameObject player)
    {
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(interactionPrompt);
        }
    }

    public string GetInteractionPrompt()
    {
        return visualPrompt;
    }
}

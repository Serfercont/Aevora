using UnityEngine;

public interface IInteractable
{
    // This method will be called when the player interacts with the object.
    public void Interact(GameObject player);

    // This method will return a string that can be displayed to the player as a prompt for interaction.
    string GetInteractionPrompt();
} 


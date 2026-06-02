using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void LoadScene()
    {
        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego..."); 
        Application.Quit();
    }

    public void OnMainMenu(InputValue value)
    {
        // Conservamos tu regla de seguridad: solo funciona en el menú principal
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            // Comprobamos si la acción se ha pulsado (isPressed)
            if (value.isPressed)
            {
                LoadScene();
            }
        }
    }
}

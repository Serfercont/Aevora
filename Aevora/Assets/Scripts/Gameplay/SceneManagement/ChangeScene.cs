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
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (value.isPressed)
            {
                LoadScene();
            }
        }
    }
}

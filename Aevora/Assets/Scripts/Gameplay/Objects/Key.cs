using UnityEngine;


public class Key : MonoBehaviour, IInteractable
{
    private static readonly System.Collections.Generic.List<Transform> availableSpawnPoints = new();
    private static int spawnPoolSceneHandle = int.MinValue;

    [Header("Identificador")]
    [Tooltip("ID único que debe coincidir con el doorKeyID de la puerta correspondiente.")]
    [SerializeField] private string keyID = "key_door_1";

    [Header("Spawn")]
    [Tooltip("Si está marcado, la llave se queda en su posición actual en la escena.")]
    [SerializeField] private bool noRandom = false;
    [Tooltip("Puntos de spawn disponibles para esta llave. Se usan en orden aleatorio si noRandom está desmarcado.")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Prompt")]
    [SerializeField] private string interactionPrompt = "Recoger llave";

    private void Start()
    {
        if (noRandom)
        {
            return;
        }

        EnsureSpawnPoolInitialized();
        MoveToUniqueSpawn();
    }

    public string GetInteractionPrompt() => interactionPrompt;

    private void EnsureSpawnPoolInitialized()
    {
        int currentSceneHandle = gameObject.scene.handle;
        if (spawnPoolSceneHandle == currentSceneHandle && availableSpawnPoints.Count > 0)
        {
            return;
        }

        availableSpawnPoints.Clear();
        spawnPoolSceneHandle = currentSceneHandle;

        Key[] keysInScene = FindObjectsByType<Key>(FindObjectsSortMode.None);
        System.Collections.Generic.HashSet<Transform> uniqueSpawnPoints = new();

        foreach (Key key in keysInScene)
        {
            if (key.spawnPoints == null)
            {
                continue;
            }

            foreach (Transform spawnPoint in key.spawnPoints)
            {
                if (spawnPoint != null)
                {
                    uniqueSpawnPoints.Add(spawnPoint);
                }
            }
        }

        availableSpawnPoints.AddRange(uniqueSpawnPoints);

        for (int i = 0; i < availableSpawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, availableSpawnPoints.Count);
            (availableSpawnPoints[i], availableSpawnPoints[randomIndex]) = (availableSpawnPoints[randomIndex], availableSpawnPoints[i]);
        }
    }

    private void MoveToUniqueSpawn()
    {
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[Key] No hay spawn points disponibles para asignar.");
            return;
        }

        Transform selectedSpawn = availableSpawnPoints[0];
        availableSpawnPoints.RemoveAt(0);
        transform.SetPositionAndRotation(selectedSpawn.position, selectedSpawn.rotation);
    }

    public void Interact(GameObject interactor)
    {
        PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("[Key] El interactor no tiene PlayerInventory.");
            return;
        }

        inventory.AddKey(keyID);
        Debug.Log($"[Key] Llave '{keyID}' recogida.");

        gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Singleton que persiste entre escenas. Guarda el progreso y el mensaje final armado.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progreso del Juego")]
    public List<string> collectedMessages = new List<string>();

    private int currentLevelIndex = 0; // Suponiendo que el índice 1 es el primer nivel

    private void Awake()
    {
        // Configuración clásica de Singleton Persistente
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMessagePart(string part)
    {
        if (!collectedMessages.Contains(part))
        {
            collectedMessages.Add(part);
        }
    }

    public string GetFinalMessage()
    {
        return string.Join(" ", collectedMessages);
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentLevelIndex);
        }
        else
        {
            // Fin del juego, cargar escena de mensaje final (o el final que decidas)
            Debug.Log("Juego Terminado. Mensaje final: " + GetFinalMessage());
            // SceneManager.LoadScene("EndMessage"); // Reemplaza con el nombre de tu escena final
        }
    }
}
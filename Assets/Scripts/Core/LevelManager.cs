using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestiona la lógica de un nivel individual (Guitarra, Cuadro o Mapa).
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Configuración del Nivel")]
    public int totalPieces;
    public string messagePart; // El mensaje que se desbloquea al final (ej: "Eres mi nota perfecta, ")

    [Header("UI y Eventos")]
    public GameObject levelCompleteUI;
    public UnityEvent OnLevelComplete;

    private int currentPiecesPlaced = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (levelCompleteUI != null) levelCompleteUI.SetActive(false);

        // Auto-detectar piezas si no se configuró manualmente
        if (totalPieces == 0)
        {
            totalPieces = FindObjectsOfType<PuzzlePiece>().Length;
        }
    }

    public void PiecePlaced()
    {
        currentPiecesPlaced++;

        if (currentPiecesPlaced >= totalPieces)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        Debug.Log("¡Nivel Completado!");

        // Guardar la parte del mensaje en el GameManager
        GameManager.Instance.AddMessagePart(messagePart);

        // Mostrar la interfaz de victoria con animación
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }

        OnLevelComplete?.Invoke();
    }

    // Este método se llamará desde un botón en la UI al completar el nivel
    public void GoToNextLevel()
    {
        GameManager.Instance.LoadNextLevel();
    }
}
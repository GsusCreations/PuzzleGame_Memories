using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using System.Security.Cryptography;
using System.Text;
using DG.Tweening; // Por si quieres añadir animaciones extra luego

/// <summary>
/// Controla el flujo del Menú Principal: Login con validación segura (Hash) y transición al panel de inicio.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject loginPanel;
    public GameObject startPanel;

    [Header("Campos de Entrada (Login)")]
    public TMP_InputField nameInputField;
    public TMP_InputField passwordInputField;
    public TextMeshProUGUI feedbackText; // Para mostrar errores (ej: "Contraseña incorrecta")

    [Header("Datos Esperados")]
    [Tooltip("El nombre que esperas que ingrese (ignora mayúsculas y espacios extra).")]
    public string expectedName = "Juan Perez";

    [Tooltip("Pega aquí el Hash generado de la contraseña.")]
    public string expectedPasswordHash = "";

    [Header("Herramienta de Desarrollo (Solo para ti)")]
    [Tooltip("Escribe tu contraseña aquí, haz CLIC DERECHO en el nombre de este script (arriba) y elige 'Generar Hash'. Luego cópialo en Expected Password Hash y borra esto.")]
    public string passwordToHashTemp;

    private void Start()
    {
        // Estado inicial: Mostrar login, ocultar inicio
        loginPanel.SetActive(true);
        startPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.text = "";

        // Ocultar la contraseña con asteriscos en la UI
        if (passwordInputField != null)
            passwordInputField.inputType = TMP_InputField.InputType.Password;
    }

    /// <summary>
    /// Se llama al presionar el botón de "Ingresar" en el panel de Login.
    /// </summary>
    public void AttemptLogin()
    {
        string inputName = nameInputField.text.Trim().ToLower();
        string inputPassword = passwordInputField.text.Trim();

        string expectedNameFormatted = expectedName.Trim().ToLower();

        // 1. Validar Nombre
        if (string.IsNullOrEmpty(inputName) || inputName != expectedNameFormatted)
        {
            ShowFeedback("Nombre no reconocido. Intenta de nuevo.");
            return;
        }

        // 2. Validar Contraseña (comparando Hashes)
        string inputHash = ComputeSha256Hash(inputPassword);

        if (inputHash == expectedPasswordHash)
        {
            // Login exitoso
            ShowFeedback("¡Acceso concedido!", Color.green);
            TransitionToStartPanel();
        }
        else
        {
            // Contraseña incorrecta
            ShowFeedback("Contraseña incorrecta.", Color.red);
        }
    }

    /// <summary>
    /// Se llama al presionar el botón de "Iniciar Nivel" en el panel de Inicio.
    /// </summary>
    public void StartFirstLevel()
    {
        // Asumiendo que usas el GameManager que creamos antes para cargar el nivel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("No se encontró el GameManager en la escena.");
        }
    }

    /// <summary>
    /// Alterna la visibilidad de la contraseña (entre asteriscos y texto normal).
    /// Asigna esta función al evento OnClick de un botón (ej. un ícono de ojo) junto al input.
    /// </summary>
    public void TogglePasswordVisibility()
    {
        if (passwordInputField == null) return;

        if (passwordInputField.inputType == TMP_InputField.InputType.Password)
        {
            passwordInputField.inputType = TMP_InputField.InputType.Standard;
        }
        else
        {
            passwordInputField.inputType = TMP_InputField.InputType.Password;
        }

        // Forzar la actualización visual para que los asteriscos cambien a texto al instante
        passwordInputField.ForceLabelUpdate();
    }

    private void TransitionToStartPanel()
    {
        // Aquí podrías agregar animaciones con DoTween, por ahora hacemos un cambio simple
        loginPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    private void ShowFeedback(string message, Color? color = null)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color ?? Color.white;
        }
    }

    // ==========================================
    // SISTEMA DE SEGURIDAD (HASHING)
    // ==========================================

    /// <summary>
    /// Convierte un texto normal en una cadena encriptada SHA256 irrompible.
    /// </summary>
    private string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// FUNCIÓN MÁGICA PARA EL INSPECTOR: 
    /// Te permite generar el hash sin tener que programar nada extra.
    /// </summary>
    [ContextMenu("Generar Hash de la Contraseña")]
    private void GenerateHashFromInspector()
    {
        if (string.IsNullOrEmpty(passwordToHashTemp))
        {
            Debug.LogWarning("Primero escribe una contraseña en 'Password To Hash Temp'.");
            return;
        }

        string hash = ComputeSha256Hash(passwordToHashTemp.Trim());
        Debug.Log("<color=cyan><b>TU HASH ES:</b></color>\n" + hash);
        Debug.Log("Copia el texto de arriba y pégalo en la variable 'Expected Password Hash'. Luego borra la contraseña temporal.");
    }
}
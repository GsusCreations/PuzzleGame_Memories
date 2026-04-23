using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

/// <summary>
/// Controla el comportamiento de la pieza con un efecto "Imán" suave al acercarse a su destino.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PuzzlePiece : MonoBehaviour
{
    [Header("Configuración del Destino")]
    [Tooltip("El objeto vacío en la escena que marca dónde debe ir esta pieza.")]
    public Transform targetPosition;
    
    [Tooltip("Distancia a la que el 'imán' empieza a hacer efecto.")]
    public float snapTolerance = 1.5f; 
    
    [Header("Efecto Imán (DoTween)")]
    public float magnetDuration = 0.3f; // Tiempo que tarda en ser atraída
    public Ease magnetEase = Ease.OutBack; // Movimiento con un leve rebote al encajar, como un imán real
    
    [Header("Eventos")]
    public UnityEvent OnPieceSnapped;

    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isLocked = false;
    
    private SpriteRenderer spriteRenderer;
    private Material pieceMaterial;

    // Colores para el feedback del Shader
    private readonly Color correctColor = Color.green;
    private readonly Color incorrectColor = Color.red;
    private readonly Color magnetZoneColor = Color.yellow; // Color cuando estás cerca del imán

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pieceMaterial = spriteRenderer.material; 
    }

    private void Start()
    {
        startPosition = transform.position;
        SetFeedbackColor(Color.white, 0f);
    }

    private void OnMouseDown()
    {
        if (isLocked) return;
        
        isDragging = true;
        // Animación al agarrar
        transform.DOScale(transform.localScale * 1.1f, 0.1f);
        spriteRenderer.sortingOrder = 10; 
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;

        // --- NUEVO: Feedback de "Zona Magnética" ---
        // Mientras arrastras, chequeamos si ya estás en la zona de encaje
        float distance = Vector2.Distance(transform.position, targetPosition.position);
        if (distance <= snapTolerance)
        {
            // La pieza brilla indicando que el imán la está jalando
            SetFeedbackColor(magnetZoneColor, 0.6f);
        }
        else
        {
            // Apagamos el brillo si te alejas
            SetFeedbackColor(Color.white, 0f);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        
        isDragging = false;
        transform.DOScale(transform.localScale / 1.1f, 0.1f); // Restaurar tamaño
        spriteRenderer.sortingOrder = 1;

        CheckMagnetSnap();
    }

    private void CheckMagnetSnap()
    {
        float distance = Vector2.Distance(transform.position, targetPosition.position);

        if (distance <= snapTolerance)
        {
            // ¡Zona de Imán alcanzada!
            isLocked = true;
            SetFeedbackColor(correctColor, 1f); // Verde de éxito
            
            // --- EFECTO IMÁN con DoTween ---
            // Se mueve suavemente hacia el centro de la otra pieza
            transform.DOMove(targetPosition.position, magnetDuration)
                .SetEase(magnetEase)
                .OnComplete(() => {
                    // Desvanecer el borde verde suavemente tras encajar
                    DOVirtual.Float(1f, 0f, 0.5f, (v) => SetFeedbackColor(correctColor, v));
                    OnPieceSnapped?.Invoke();
                    LevelManager.Instance.PiecePlaced();
                });
        }
        else
        {
            // Si está muy lejos, efecto de error y regresa a la posición inicial
            SetFeedbackColor(incorrectColor, 1f);
            
            transform.DOMove(startPosition, 0.4f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => {
                    DOVirtual.Float(1f, 0f, 0.3f, (v) => SetFeedbackColor(incorrectColor, v));
                });
        }
    }

    private void SetFeedbackColor(Color color, float outlineIntensity)
    {
        if (pieceMaterial != null)
        {
            pieceMaterial.SetColor("_OutlineColor", color);
            pieceMaterial.SetFloat("_OutlineIntensity", outlineIntensity);
        }
    }
}
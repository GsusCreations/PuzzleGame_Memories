using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.InputSystem; // Añadido para el nuevo Input System

/// <summary>
/// Controla el comportamiento de la pieza usando Physics2D manual y el Nuevo Input System para máxima precisión en clics.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PuzzlePiece : MonoBehaviour
{
    [Header("Configuración del Destino")]
    public Transform targetPosition;
    public float snapTolerance = 1.5f;

    [Header("Efecto Imán")]
    public float magnetDuration = 0.3f;
    public Ease magnetEase = Ease.OutBack;

    [Header("Eventos")]
    public UnityEvent OnPieceSnapped;

    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isLocked = false;

    private SpriteRenderer spriteRenderer;
    private Material pieceMaterial;
    private Collider2D myCollider;

    private readonly Color correctColor = Color.green;
    private readonly Color incorrectColor = Color.red;
    private readonly Color magnetZoneColor = Color.yellow;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        pieceMaterial = spriteRenderer.material;
    }

    private void Start()
    {
        startPosition = transform.position;
        SetFeedbackColor(Color.white, 0f);
    }

    private void Update()
    {
        if (isLocked) return;

        // Referencias al mouse en el nuevo Input System
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 1. Detectar el inicio del clic (Equivalente a Input.GetMouseButtonDown(0))
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            // Comprobamos si el clic tocó exactamente el collider de ESTA pieza
            if (myCollider == Physics2D.OverlapPoint(mousePos))
            {
                StartDragging();
            }
        }

        // 2. Mientras se mantiene presionado (Equivalente a Input.GetMouseButton(0))
        if (mouse.leftButton.isPressed && isDragging)
        {
            DragPiece(mouse.position.ReadValue());
        }

        // 3. Al soltar el clic (Equivalente a Input.GetMouseButtonUp(0))
        if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            StopDragging();
        }
    }

    private void StartDragging()
    {
        Debug.Log("¡Pieza agarrada exitosamente!");
        isDragging = true;
        transform.DOScale(transform.localScale * 1.1f, 0.1f);
        spriteRenderer.sortingOrder = 10;
    }

    private void DragPiece(Vector2 screenMousePos)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
        mousePos.z = 0;
        transform.position = mousePos;

        // Feedback magnético visual
        float distance = Vector2.Distance(transform.position, targetPosition.position);
        if (distance <= snapTolerance)
        {
            SetFeedbackColor(magnetZoneColor, 0.6f);
        }
        else
        {
            SetFeedbackColor(Color.white, 0f);
        }
    }

    private void StopDragging()
    {
        isDragging = false;
        transform.DOScale(transform.localScale / 1.1f, 0.1f);
        spriteRenderer.sortingOrder = 1;

        CheckMagnetSnap();
    }

    private void CheckMagnetSnap()
    {
        float distance = Vector2.Distance(transform.position, targetPosition.position);

        if (distance <= snapTolerance)
        {
            isLocked = true;
            SetFeedbackColor(correctColor, 1f);

            transform.DOMove(targetPosition.position, magnetDuration)
                .SetEase(magnetEase)
                .OnComplete(() => {
                    DOVirtual.Float(1f, 0f, 0.5f, (v) => SetFeedbackColor(correctColor, v));
                    OnPieceSnapped?.Invoke();
                    LevelManager.Instance.PiecePlaced();
                });
        }
        else
        {
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
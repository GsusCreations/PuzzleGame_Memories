using UnityEngine;
using DG.Tweening;

/// <summary>
/// Script modular para añadir animaciones de entrada/salida a cualquier objeto (UI o Sprites 2D).
/// </summary>
public class DoTweenAnimator : MonoBehaviour
{
    public enum AnimationType { PopIn, FadeIn, SlideInFromBottom, Bounce }

    [Header("Configuración de Animación")]
    public AnimationType animationType = AnimationType.PopIn;
    public float duration = 0.5f;
    public float delay = 0f;
    public Ease easeType = Ease.OutBack;

    [Header("Auto Play")]
    public bool playOnAwake = true;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    private void Start()
    {
        if (playOnAwake)
        {
            PlayEntranceAnimation();
        }
    }

    public void PlayEntranceAnimation()
    {
        switch (animationType)
        {
            case AnimationType.PopIn:
                transform.localScale = Vector3.zero;
                transform.DOScale(originalScale, duration).SetDelay(delay).SetEase(easeType);
                break;

            case AnimationType.FadeIn:
                CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0;
                    canvasGroup.DOFade(1, duration).SetDelay(delay).SetEase(easeType);
                }
                else
                {
                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 0;
                        sr.color = c;
                        sr.DOFade(1, duration).SetDelay(delay).SetEase(easeType);
                    }
                }
                break;

            case AnimationType.SlideInFromBottom:
                transform.localPosition = originalPosition - new Vector3(0, 1000f, 0); // Ajustar según escala
                transform.DOLocalMove(originalPosition, duration).SetDelay(delay).SetEase(easeType);
                break;

            case AnimationType.Bounce:
                transform.localScale = originalScale;
                transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), duration, 5, 1).SetDelay(delay);
                break;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Mueve el objeto actual a través de una serie de puntos (Transforms) definidos en el Inspector usando DoTween.
/// </summary>
public class PathAnimator : MonoBehaviour
{
    [System.Serializable]
    public class WaypointData
    {
        [Tooltip("El punto hacia donde se moverá.")]
        public Transform targetTransform;

        [Tooltip("Multiplicador de escala. Escala objetivo = Escala previa * multiplicador.")]
        public float scaleMultiplier = 1f;
    }

    [Header("Configuración de la Ruta")]
    [Tooltip("Añade los puntos de la ruta y su multiplicador de escala.")]
    public List<WaypointData> waypoints = new List<WaypointData>();

    [Tooltip("Tiempo en segundos que tarda en ir de un punto a otro.")]
    public float durationPerPoint = 1f;

    [Tooltip("Tipo de suavizado del movimiento (Linear es velocidad constante).")]
    public Ease easeType = Ease.Linear;

    [Header("Comportamiento")]
    [Tooltip("¿Iniciar la animación apenas carga la escena?")]
    public bool playOnStart = false;

    [Tooltip("¿Repetir la ruta infinitamente?")]
    public bool loop = false;

    public enum PathLoopMode { Reiniciar, IrYVenir }
    [Tooltip("Si Loop está activado: 'Reiniciar' vuelve al inicio de golpe. 'IrYVenir' hace que regrese punto por punto.")]
    public PathLoopMode loopMode = PathLoopMode.IrYVenir;

    // Guardamos la secuencia para poder detenerla si es necesario
    private Sequence pathSequence;
    private Vector3 initialPosition;
    private Vector3 initialScale; // Guardamos la escala inicial para el Reset

    private void Awake()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayPath();
        }
    }

    /// <summary>
    /// Inicia la animación de traslado punto por punto. 
    /// Puedes llamarlo desde un UnityEvent (ConditionalExecutor o EventScheduler).
    /// </summary>
    public void PlayPath()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("PathAnimator: No hay puntos (waypoints) asignados en " + gameObject.name);
            return;
        }

        // Si ya hay una animación corriendo, la matamos para no superponerlas
        if (pathSequence != null && pathSequence.IsActive())
        {
            pathSequence.Kill();
        }

        // Creamos una nueva secuencia de DoTween
        pathSequence = DOTween.Sequence();

        // Usamos una variable temporal para ir calculando la escala "actual" paso por paso
        Vector3 currentStepScale = transform.localScale;

        // Agregamos cada punto a la secuencia
        foreach (WaypointData point in waypoints)
        {
            if (point != null && point.targetTransform != null)
            {
                // scaleTarget = current scale * multiplier
                currentStepScale = currentStepScale * point.scaleMultiplier;

                // .Append añade la animación a la cola
                pathSequence.Append(transform.DOMove(point.targetTransform.position, durationPerPoint).SetEase(easeType));

                // .Join hace que el DOScale ocurra EXACTAMENTE al mismo tiempo que el DOMove anterior
                pathSequence.Join(transform.DOScale(currentStepScale, durationPerPoint).SetEase(easeType));
            }
        }

        // Configuramos el bucle si está activado
        if (loop)
        {
            LoopType dotweenLoopType = (loopMode == PathLoopMode.IrYVenir) ? LoopType.Yoyo : LoopType.Restart;
            pathSequence.SetLoops(-1, dotweenLoopType);
        }
    }

    /// <summary>
    /// Detiene la animación donde sea que esté.
    /// </summary>
    public void StopPath()
    {
        if (pathSequence != null && pathSequence.IsActive())
        {
            pathSequence.Pause();
        }
    }

    /// <summary>
    /// Detiene la animación y devuelve el objeto a su posición original antes de empezar.
    /// </summary>
    public void ResetToStart()
    {
        if (pathSequence != null)
        {
            pathSequence.Kill();
        }
        transform.position = initialPosition;
        transform.localScale = initialScale; // Reseteamos también la escala
    }
}
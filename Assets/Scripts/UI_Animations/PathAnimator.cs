using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Mueve el objeto actual a través de una serie de puntos (Transforms) definidos en el Inspector usando DoTween.
/// </summary>
public class PathAnimator : MonoBehaviour
{
    [Header("Configuración de la Ruta")]
    [Tooltip("Arrastra aquí los GameObjects/Transforms vacíos que servirán como puntos de la ruta.")]
    public List<Transform> waypoints = new List<Transform>();

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

    private void Awake()
    {
        initialPosition = transform.position;
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

        // Agregamos cada punto a la secuencia
        foreach (Transform point in waypoints)
        {
            if (point != null)
            {
                // .Append añade la animación a la cola, así va una después de la otra
                pathSequence.Append(transform.DOMove(point.position, durationPerPoint).SetEase(easeType));
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
    }
}
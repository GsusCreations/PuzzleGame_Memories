using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Permite programar una secuencia de UnityEvents con un delay específico antes de cada uno.
/// Ideal para animaciones de entrada, cinemáticas ligeras o setup de escena.
/// </summary>
public class EventScheduler : MonoBehaviour
{
    [System.Serializable]
    public class ScheduledEvent
    {
        [Tooltip("Tiempo de espera en segundos ANTES de ejecutar el evento.")]
        public float delayBeforeExecute = 1f;

        [Tooltip("Los eventos a ejecutar (puedes añadir múltiples llamadas aquí).")]
        public UnityEvent onExecute;
    }

    [Header("Secuencia de Eventos")]
    [Tooltip("Lista de eventos que se ejecutarán en orden.")]
    public List<ScheduledEvent> eventSequence = new List<ScheduledEvent>();

    [Header("Configuración")]
    [Tooltip("¿Comenzar la secuencia automáticamente al iniciar la escena?")]
    public bool playOnStart = true;

    [Tooltip("¿Repetir la secuencia en bucle al terminar? (Útil para animaciones cíclicas).")]
    public bool loopSequence = false;

    private Coroutine sequenceCoroutine;

    private void Start()
    {
        if (playOnStart)
        {
            PlaySequence();
        }
    }

    /// <summary>
    /// Inicia o reinicia la secuencia de eventos desde el principio.
    /// Puedes llamar a este método desde un botón en la UI o desde otro script.
    /// </summary>
    public void PlaySequence()
    {
        StopSequence(); // Asegurarse de no tener dos secuencias corriendo a la vez
        sequenceCoroutine = StartCoroutine(SequenceRoutine());
    }

    /// <summary>
    /// Detiene la secuencia actual inmediatamente.
    /// </summary>
    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
    }

    private IEnumerator SequenceRoutine()
    {
        if (eventSequence == null || eventSequence.Count == 0)
        {
            Debug.LogWarning("EventScheduler: La secuencia está vacía en " + gameObject.name);
            yield break;
        }

        do
        {
            foreach (ScheduledEvent scheduledEvent in eventSequence)
            {
                // Esperar el tiempo configurado ANTES de disparar el evento
                if (scheduledEvent.delayBeforeExecute > 0)
                {
                    yield return new WaitForSeconds(scheduledEvent.delayBeforeExecute);
                }

                // Ejecutar todos los UnityEvents configurados en este paso
                scheduledEvent.onExecute?.Invoke();
            }

        } while (loopSequence); // Repetir si loop está activado
    }
}
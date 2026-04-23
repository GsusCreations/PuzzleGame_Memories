using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Evalúa una condición específica en un momento determinado y ejecuta eventos si se cumple.
/// Su Inspector se adapta dinámicamente según el tipo de condición seleccionada.
/// </summary>
public class ConditionalExecutor : MonoBehaviour
{
    public enum CheckMoment
    {
        OnStart,
        OnEnable,
        OnDisable,
        OnUpdate,
        OnTriggerEnter2D,
        Manual
    }

    public enum ConditionType
    {
        ActiveObject,
        TriggerCollider
    }

    [Header("Configuración General")]
    public CheckMoment checkMoment = CheckMoment.OnStart;
    public ConditionType conditionType = ConditionType.ActiveObject;
    public bool fireOnlyOnce = true;

    // --- Variables para ActiveObject ---
    public GameObject targetGameObject;
    public bool expectedActiveState = true;

    // --- Variables para TriggerCollider ---
    public Collider2D mainCollider;
    public Collider2D targetCollider;

    [Header("Eventos a Ejecutar")]
    public UnityEvent onConditionMet;

    private bool hasFired = false;

    private void Start()
    {
        if (checkMoment == CheckMoment.OnStart) EvaluateCondition();
    }

    private void OnEnable()
    {
        if (checkMoment == CheckMoment.OnEnable) EvaluateCondition();
    }

    private void OnDisable()
    {
        if (checkMoment == CheckMoment.OnDisable) EvaluateCondition();
    }

    private void Update()
    {
        if (checkMoment == CheckMoment.OnUpdate) EvaluateCondition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (checkMoment == CheckMoment.OnTriggerEnter2D && conditionType == ConditionType.TriggerCollider)
        {
            if (targetCollider != null && other == targetCollider)
            {
                ExecuteEvents();
            }
        }
    }

    /// <summary>
    /// Verifica si la condición actual se cumple. Puede ser llamado manualmente.
    /// </summary>
    public void EvaluateCondition()
    {
        if (hasFired && fireOnlyOnce) return;

        bool conditionMet = false;

        switch (conditionType)
        {
            case ConditionType.ActiveObject:
                if (targetGameObject != null)
                {
                    conditionMet = (targetGameObject.activeInHierarchy == expectedActiveState);
                }
                else
                {
                    Debug.LogWarning("ConditionalExecutor: Target GameObject no asignado.", this);
                }
                break;

            case ConditionType.TriggerCollider:
                // Para validaciones en Update, Start o Enable, comprobamos si se están tocando físicamente.
                if (mainCollider != null && targetCollider != null)
                {
                    conditionMet = mainCollider.IsTouching(targetCollider);
                }
                break;
        }

        if (conditionMet)
        {
            ExecuteEvents();
        }
    }

    private void ExecuteEvents()
    {
        if (hasFired && fireOnlyOnce) return;

        hasFired = true;
        onConditionMet?.Invoke();
    }

    /// <summary>
    /// Reinicia el estado para permitir que se vuelva a ejecutar si fireOnlyOnce estaba activado.
    /// </summary>
    public void ResetExecutor()
    {
        hasFired = false;
    }
}

// ==============================================================================
// CUSTOM EDITOR: Se encarga de mostrar/ocultar variables dinámicamente en el Inspector
// ==============================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(ConditionalExecutor))]
public class ConditionalExecutorEditor : Editor
{
    SerializedProperty checkMoment;
    SerializedProperty conditionType;
    SerializedProperty fireOnlyOnce;

    SerializedProperty targetGameObject;
    SerializedProperty expectedActiveState;

    SerializedProperty mainCollider;
    SerializedProperty targetCollider;

    SerializedProperty onConditionMet;

    private void OnEnable()
    {
        checkMoment = serializedObject.FindProperty("checkMoment");
        conditionType = serializedObject.FindProperty("conditionType");
        fireOnlyOnce = serializedObject.FindProperty("fireOnlyOnce");

        targetGameObject = serializedObject.FindProperty("targetGameObject");
        expectedActiveState = serializedObject.FindProperty("expectedActiveState");

        mainCollider = serializedObject.FindProperty("mainCollider");
        targetCollider = serializedObject.FindProperty("targetCollider");

        onConditionMet = serializedObject.FindProperty("onConditionMet");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Momento de Verificación", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(checkMoment, new GUIContent("Evaluar en"));
        EditorGUILayout.PropertyField(fireOnlyOnce, new GUIContent("Ejecutar solo una vez"));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Condición", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(conditionType, new GUIContent("Tipo de Condición"));

        EditorGUILayout.Space();

        // Lógica de visualización dinámica
        ConditionalExecutor.ConditionType type = (ConditionalExecutor.ConditionType)conditionType.enumValueIndex;

        EditorGUI.indentLevel++;
        if (type == ConditionalExecutor.ConditionType.ActiveObject)
        {
            EditorGUILayout.PropertyField(targetGameObject, new GUIContent("Objeto a verificar"));
            EditorGUILayout.PropertyField(expectedActiveState, new GUIContent("Estado esperado"));

            EditorGUILayout.HelpBox("Los eventos se ejecutarán si el 'Objeto a verificar' tiene el estado activo indicado.", MessageType.Info);
        }
        else if (type == ConditionalExecutor.ConditionType.TriggerCollider)
        {
            EditorGUILayout.PropertyField(mainCollider, new GUIContent("Mi Collider"));
            EditorGUILayout.PropertyField(targetCollider, new GUIContent("Collider Objetivo"));

            if (checkMoment.enumValueIndex == (int)ConditionalExecutor.CheckMoment.OnTriggerEnter2D)
            {
                EditorGUILayout.HelpBox("Detectará automáticamente cuando el 'Collider Objetivo' entre en 'Mi Collider' (Mi Collider debe ser Trigger).", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Verificará si ambos Colliders se están tocando (Overlapping) en el momento seleccionado (ej: OnEnable o Update).", MessageType.Info);
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onConditionMet, new GUIContent("Ejecutar si se cumple"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

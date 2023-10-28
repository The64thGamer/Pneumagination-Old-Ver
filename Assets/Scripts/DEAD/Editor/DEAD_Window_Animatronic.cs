using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(DEAD_Animatronic))]
public class DEAD_Window_Animatronic : Editor
{
    DEAD_Animatronic dead_Animatronic;

    void OnEnable()
    {
        dead_Animatronic = target as DEAD_Animatronic;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        // Check if the variable has changed
        if (GUI.changed)
        {
            UpdateAnimatorController();
        }
    }

    void UpdateAnimatorController()
    {

        //Check for empty Animator Controller
        Animator animator = dead_Animatronic.GetComponent<Animator>();
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("'" + dead_Animatronic.name + "' needs an Animator Controller in the Animator script.");
            return;
        }

        //Setup Animator Controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(animator.runtimeAnimatorController), typeof(AnimatorController)) as AnimatorController;


        //Add Layers
        for (int i = 0; i < controller.layers.Length; i++)
        {
            controller.RemoveLayer(i);
        }
        for (int i = 0; i < controller.parameters.Length; i++)
        {
            controller.RemoveParameter(i);
        }

        DEAD_Actuator[] actuators = dead_Animatronic.GetActuatorInfoCopy();
        for (int i = 0; i < actuators.Length; i++)
        {
            AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
            string uniqueName = actuators[i].actuationName + " ID(" + i + ")";
            newLayer.name = controller.MakeUniqueLayerName(uniqueName);
            newLayer.stateMachine = new AnimatorStateMachine();
            newLayer.stateMachine.name = newLayer.name;
            newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
            AnimatorState state = newLayer.stateMachine.AddState(actuators[i].actuationName);
            state.name = actuators[i].actuationName;
            state.speed = 0;
            state.timeParameterActive = true;
            state.timeParameter = uniqueName;
            state.motion = actuators[i].animation;
            newLayer.blendingMode = AnimatorLayerBlendingMode.Additive;
            newLayer.defaultWeight = 1f;

            controller.AddLayer(newLayer);
            controller.AddParameter(uniqueName, AnimatorControllerParameterType.Float);
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

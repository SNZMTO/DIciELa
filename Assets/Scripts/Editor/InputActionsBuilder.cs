using System.IO;
using Diciela.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée l'asset d'actions du jeu s'il manque (ARCHITECTURE, section 11) et retire celui du modèle Unity, qu'il
    /// remplace. Les touches sont liées à leur position sur un clavier QWERTY : ZQSD tombe juste en AZERTY, et la
    /// rotation (Q/E en QWERTY) tombe sur A/E. Un asset qui existe n'est jamais réécrit : ses liaisons se règlent dans
    /// la fenêtre de l'Input System.
    /// </summary>
    internal static class InputActionsBuilder
    {
        internal const string ActionsPath = "Assets/Input/Diciela.inputactions";
        private const string TemplateActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string ProjectWideActionsKey = "com.unity.input.settings.actions";

        public static void CreateIfMissing()
        {
            RemoveTemplateActions();
            if (EditorAssets.Exists(ActionsPath))
                return;

            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap camera = asset.AddActionMap(CameraControls.MapName);

            InputAction move = camera.AddAction(CameraControls.MoveAction, InputActionType.Value);
            move.expectedControlType = "Vector2";
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");

            InputAction rotate = camera.AddAction(CameraControls.RotateAction, InputActionType.Value);
            rotate.expectedControlType = "Axis";
            rotate.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");

            InputAction zoom = camera.AddAction(CameraControls.ZoomAction, InputActionType.Value, "<Mouse>/scroll/y");
            zoom.expectedControlType = "Axis";

            camera.AddAction(CameraControls.DragAction, InputActionType.Button, "<Mouse>/rightButton");

            InputAction pointer = camera.AddAction(CameraControls.PointerAction, InputActionType.Value, "<Pointer>/position");
            pointer.expectedControlType = "Vector2";

            EditorAssets.EnsureFolder("Assets/Input");
            File.WriteAllText(ActionsPath, asset.ToJson());
            Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(ActionsPath);
            Debug.Log("DICIELA : actions du jeu créées dans " + ActionsPath + ".");
        }

        private static void RemoveTemplateActions()
        {
            if (!EditorAssets.Exists(TemplateActionsPath))
                return;

            EditorBuildSettings.RemoveConfigObject(ProjectWideActionsKey);
            AssetDatabase.DeleteAsset(TemplateActionsPath);
            Debug.Log("DICIELA : l'asset d'actions du modèle Unity est retiré ; celui du jeu le remplace.");
        }
    }
}

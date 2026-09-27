using System;
using System.IO;
using Diciela.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Diciela.EditorTools
{
    /// <summary>
    /// L'asset d'actions du jeu (ARCHITECTURE, section 11) : il le crée s'il manque, lui ajoute les actions qui lui
    /// manquent, et retire celui du modèle Unity, qu'il remplace. Une action qui existe n'est jamais touchée : ses
    /// liaisons se règlent dans la fenêtre de l'Input System. Les touches sont liées à leur position sur un clavier
    /// QWERTY : ZQSD tombe juste en AZERTY, et la rotation (Q/E en QWERTY) tombe sur A/E.
    /// </summary>
    internal static class InputActionsBuilder
    {
        internal const string ActionsPath = "Assets/Input/Diciela.inputactions";
        private const string TemplateActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string ProjectWideActionsKey = "com.unity.input.settings.actions";

        public static void EnsureActions()
        {
            RemoveTemplateActions();

            // L'asset se lit depuis son fichier, se complète en mémoire, puis s'écrit et se réimporte : Unity garde une
            // copie vivante de l'asset importé, qu'on ne modifie jamais directement (SAVOIR, section 3).
            bool exists = EditorAssets.Exists(ActionsPath);
            InputActionAsset asset = exists
                ? InputActionAsset.FromJson(File.ReadAllText(ActionsPath))
                : ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap camera = asset.FindActionMap(CameraControls.MapName) ??
                                    asset.AddActionMap(CameraControls.MapName);

            int added = AddIfMissing(camera, CameraControls.MoveAction, AddMove) +
                        AddIfMissing(camera, CameraControls.RotateAction, AddRotate) +
                        AddIfMissing(camera, CameraControls.ZoomAction, AddZoom) +
                        AddIfMissing(camera, CameraControls.DragAction, AddDrag) +
                        AddIfMissing(camera, CameraControls.OrbitAction, AddOrbit) +
                        AddIfMissing(camera, CameraControls.PointerAction, AddPointer);

            if (added > 0)
            {
                EditorAssets.EnsureFolder("Assets/Input");
                File.WriteAllText(ActionsPath, asset.ToJson());
                AssetDatabase.ImportAsset(ActionsPath);
                Debug.Log("DICIELA : " + added + " action(s) ajoutée(s) à " + ActionsPath + ".");
            }
            Object.DestroyImmediate(asset);
        }

        private static int AddIfMissing(InputActionMap map, string actionName, Action<InputActionMap> add)
        {
            if (map.FindAction(actionName) != null)
                return 0;
            add(map);
            return 1;
        }

        private static void AddMove(InputActionMap map)
        {
            InputAction move = map.AddAction(CameraControls.MoveAction, InputActionType.Value);
            move.expectedControlType = "Vector2";
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        }

        private static void AddRotate(InputActionMap map)
        {
            InputAction rotate = map.AddAction(CameraControls.RotateAction, InputActionType.Value);
            rotate.expectedControlType = "Axis";
            rotate.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");
        }

        private static void AddZoom(InputActionMap map)
        {
            InputAction zoom = map.AddAction(CameraControls.ZoomAction, InputActionType.Value, "<Mouse>/scroll/y");
            zoom.expectedControlType = "Axis";
        }

        private static void AddDrag(InputActionMap map)
        {
            map.AddAction(CameraControls.DragAction, InputActionType.Button, "<Mouse>/rightButton");
        }

        private static void AddOrbit(InputActionMap map)
        {
            map.AddAction(CameraControls.OrbitAction, InputActionType.Button, "<Mouse>/middleButton");
        }

        private static void AddPointer(InputActionMap map)
        {
            InputAction pointer = map.AddAction(CameraControls.PointerAction, InputActionType.Value, "<Pointer>/position");
            pointer.expectedControlType = "Vector2";
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

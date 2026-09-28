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
    /// manquent, et retire celui du modèle Unity, qu'il remplace. Une action qui existe n'est pas touchée (ses liaisons
    /// se règlent dans la fenêtre de l'Input System), sauf la position du pointeur, lue telle quelle (voir plus bas).
    /// Les touches sont liées à leur position sur un clavier
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
                        AddButtonIfMissing(camera, CameraControls.DragAction, "<Mouse>/rightButton") +
                        AddButtonIfMissing(camera, CameraControls.OrbitAction, "<Mouse>/middleButton") +
                        AddPointerIfMissing(camera, CameraControls.PointerAction) +
                        AddLookIfMissing(camera, CameraControls.LookAction);

            // La pose : R, un raccourci mnémonique, se lie au caractère (SAVOIR, section 3).
            InputActionMap placement = asset.FindActionMap(PlacementControls.MapName) ??
                                       asset.AddActionMap(PlacementControls.MapName);
            added += AddButtonIfMissing(placement, PlacementControls.PlaceAction, "<Mouse>/leftButton") +
                     AddButtonIfMissing(placement, PlacementControls.RotateAction, "<Keyboard>/#(r)") +
                     AddButtonIfMissing(placement, PlacementControls.SecondaryAction, "<Mouse>/rightButton") +
                     AddButtonIfMissing(placement, PlacementControls.CancelAction, "<Keyboard>/escape") +
                     AddPointerIfMissing(placement, PlacementControls.PointerAction) +
                     AddLookIfMissing(placement, PlacementControls.LookAction);

            // Toucher ce qu'on voit : le clic gauche, quand l'outil de pose est au repos.
            InputActionMap choose = asset.FindActionMap(ChooseControls.MapName) ??
                                    asset.AddActionMap(ChooseControls.MapName);
            added += AddButtonIfMissing(choose, ChooseControls.TouchAction, "<Mouse>/leftButton") +
                     AddPointerIfMissing(choose, ChooseControls.PointerAction);

            // Valider ce que l'interface propose : Entrée, celle du pavé numérique comprise.
            InputActionMap confirm = asset.FindActionMap(ConfirmControls.MapName) ??
                                     asset.AddActionMap(ConfirmControls.MapName);
            added += AddIfMissing(confirm, ConfirmControls.ConfirmAction, m =>
                m.AddAction(ConfirmControls.ConfirmAction, InputActionType.Button, "<Keyboard>/enter")
                    .AddBinding("<Keyboard>/numpadEnter"));

            if (added > 0)
            {
                EditorAssets.EnsureFolder("Assets/Input");
                File.WriteAllText(ActionsPath, asset.ToJson());
                AssetDatabase.ImportAsset(ActionsPath);
                Debug.Log("DICIELA : " + added + " action(s) ajoutée(s) ou mise(s) à jour dans " + ActionsPath + ".");
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

        private static int AddButtonIfMissing(InputActionMap map, string actionName, string binding)
        {
            return AddIfMissing(map, actionName, m => m.AddAction(actionName, InputActionType.Button, binding));
        }

        /// <summary>
        /// La position du pointeur ; chaque carte a la sienne, pour vivre sans les autres. Elle se lit telle quelle
        /// (PassThrough) : une action « Value » départagerait la souris, le stylet et le doigt par leur grandeur, et
        /// pourrait rester sur la dernière position d'un doigt. Une ancienne action « Value » est remplacée.
        /// </summary>
        private static int AddPointerIfMissing(InputActionMap map, string actionName)
        {
            InputAction existing = map.FindAction(actionName);
            if (existing != null && existing.type == InputActionType.PassThrough)
                return 0;
            existing?.RemoveAction();
            map.AddAction(actionName, InputActionType.PassThrough, "<Pointer>/position").expectedControlType = "Vector2";
            return 1;
        }

        /// <summary>Le déplacement du pointeur à chaque image, qui continue quand le curseur est tenu (CursorGrip).</summary>
        private static int AddLookIfMissing(InputActionMap map, string actionName)
        {
            return AddIfMissing(map, actionName, m =>
                m.AddAction(actionName, InputActionType.PassThrough, "<Pointer>/delta").expectedControlType = "Vector2");
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

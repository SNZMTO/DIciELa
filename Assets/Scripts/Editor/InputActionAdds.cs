using System;
using Diciela.Controls;
using UnityEngine.InputSystem;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les façons d'ajouter une action à l'asset d'actions du jeu (InputActionsBuilder), chacune seulement si elle
    /// manque : une action qui existe n'est pas touchée (ses liaisons se règlent dans la fenêtre de l'Input System), sauf
    /// la position du pointeur, lue telle quelle. Chacune rend combien d'actions elle a ajoutées ou refaites.
    /// </summary>
    internal static class InputActionAdds
    {
        public static InputActionMap Map(InputActionAsset asset, string name)
        {
            return asset.FindActionMap(name) ?? asset.AddActionMap(name);
        }

        public static int IfMissing(InputActionMap map, string actionName, Action<InputActionMap> add)
        {
            if (map.FindAction(actionName) != null)
                return 0;
            add(map);
            return 1;
        }

        public static int Button(InputActionMap map, string actionName, string binding)
        {
            return IfMissing(map, actionName, m => m.AddAction(actionName, InputActionType.Button, binding));
        }

        /// <summary>
        /// La position du pointeur ; chaque carte a la sienne, pour vivre sans les autres. Elle se lit telle quelle
        /// (PassThrough) : une action « Value » départagerait la souris, le stylet et le doigt par leur grandeur, et
        /// pourrait rester sur la dernière position d'un doigt. Une ancienne action « Value » est remplacée.
        /// </summary>
        public static int Pointer(InputActionMap map, string actionName)
        {
            InputAction existing = map.FindAction(actionName);
            if (existing != null && existing.type == InputActionType.PassThrough)
                return 0;
            existing?.RemoveAction();
            map.AddAction(actionName, InputActionType.PassThrough, "<Pointer>/position").expectedControlType = "Vector2";
            return 1;
        }

        /// <summary>Le déplacement du pointeur à chaque image, qui continue quand le curseur est tenu (CursorGrip).</summary>
        public static int Look(InputActionMap map, string actionName)
        {
            return IfMissing(map, actionName, m =>
                m.AddAction(actionName, InputActionType.PassThrough, "<Pointer>/delta").expectedControlType = "Vector2");
        }

        public static int Move(InputActionMap map)
        {
            return IfMissing(map, CameraControls.MoveAction, m =>
            {
                InputAction move = m.AddAction(CameraControls.MoveAction, InputActionType.Value);
                move.expectedControlType = "Vector2";
                move.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
                move.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            });
        }

        /// <summary>Un axe sur les touches Q et E, liées à leur position : A et E en AZERTY (SAVOIR, section 3).</summary>
        public static int Turn(InputActionMap map, string actionName)
        {
            return IfMissing(map, actionName, m =>
            {
                InputAction rotate = m.AddAction(actionName, InputActionType.Value);
                rotate.expectedControlType = "Axis";
                rotate.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");
            });
        }

        /// <summary>La molette, seule (le zoom de la caméra).</summary>
        public static int Scroll(InputActionMap map, string actionName)
        {
            return IfMissing(map, actionName, m =>
                m.AddAction(actionName, InputActionType.Value, "<Mouse>/scroll/y").expectedControlType = "Axis");
        }

        /// <summary>La molette, Ctrl tenu (décision 206) : la taille du pinceau de la Place ; la caméra laisse alors la molette.</summary>
        public static int CtrlScroll(InputActionMap map, string actionName)
        {
            return IfMissing(map, actionName, m =>
            {
                InputAction size = m.AddAction(actionName, InputActionType.Value);
                size.expectedControlType = "Axis";
                size.AddCompositeBinding("OneModifier").With("Modifier", "<Keyboard>/ctrl")
                    .With("Binding", "<Mouse>/scroll/y");
            });
        }
    }
}

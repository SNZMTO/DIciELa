using System.IO;
using Diciela.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static Diciela.EditorTools.InputActionAdds;
using Object = UnityEngine.Object;

namespace Diciela.EditorTools
{
    /// <summary>
    /// L'asset d'actions du jeu (ARCHITECTURE, section 11) : il le crée s'il manque, lui ajoute les actions qui lui
    /// manquent (InputActionAdds), et retire celui du modèle Unity, qu'il remplace. Une action qui existe n'est pas
    /// touchée (ses liaisons se règlent dans la fenêtre de l'Input System), sauf la position du pointeur, lue telle
    /// quelle. Les touches sont liées à leur position sur un clavier QWERTY : ZQSD tombe juste en AZERTY, et la rotation
    /// (Q/E en QWERTY) tombe sur A/E.
    /// </summary>
    internal static class InputActionsBuilder
    {
        internal const string ActionsPath = "Assets/Input/Diciela.inputactions";
        private const string TemplateActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string ProjectWideActionsKey = "com.unity.input.settings.actions";

        /// <summary>Complète l'asset d'actions sans toucher aux scènes ; « Monter les scènes » le fait aussi.</summary>
        [MenuItem("DICIELA/Données/Compléter les actions du clavier")]
        public static void CompleteActions()
        {
            EnsureActions();
        }

        public static void EnsureActions()
        {
            RemoveTemplateActions();

            // L'asset se lit depuis son fichier, se complète en mémoire, puis s'écrit et se réimporte : Unity garde une
            // copie vivante de l'asset importé, qu'on ne modifie jamais directement (SAVOIR, section 3).
            bool exists = EditorAssets.Exists(ActionsPath);
            InputActionAsset asset = exists
                ? InputActionAsset.FromJson(File.ReadAllText(ActionsPath))
                : ScriptableObject.CreateInstance<InputActionAsset>();

            // La caméra ; Ctrl tenu, la molette est à l'outil en main (décision 206).
            InputActionMap camera = Map(asset, CameraControls.MapName);
            int added = Move(camera) + Turn(camera, CameraControls.RotateAction) +
                        Scroll(camera, CameraControls.ZoomAction) +
                        Button(camera, CameraControls.DragAction, "<Mouse>/rightButton") +
                        Button(camera, CameraControls.OrbitAction, "<Mouse>/middleButton") +
                        Pointer(camera, CameraControls.PointerAction) + Look(camera, CameraControls.LookAction) +
                        Button(camera, CameraControls.ToolModifierAction, "<Keyboard>/ctrl");

            // La pose : R, un raccourci mnémonique, se lie au caractère (SAVOIR, section 3).
            InputActionMap placement = Map(asset, PlacementControls.MapName);
            added += Button(placement, PlacementControls.PlaceAction, "<Mouse>/leftButton") +
                     Button(placement, PlacementControls.RotateAction, "<Keyboard>/#(r)") +
                     Button(placement, PlacementControls.SecondaryAction, "<Mouse>/rightButton") +
                     Pointer(placement, PlacementControls.PointerAction) +
                     Look(placement, PlacementControls.LookAction);

            // L'outil de la Place (décision 206) : tracer au clic gauche, glissé ; R comme à la pose ; Ctrl + molette, la
            // taille du pinceau ; le clic droit bref abandonne le geste ou pose l'outil de côté.
            InputActionMap place = Map(asset, PlaceControls.MapName);
            added += Button(place, PlaceControls.PaintAction, "<Mouse>/leftButton") +
                     Button(place, PlaceControls.RotateAction, "<Keyboard>/#(r)") +
                     CtrlScroll(place, PlaceControls.SizeAction) +
                     Button(place, PlaceControls.SecondaryAction, "<Mouse>/rightButton") +
                     Pointer(place, PlaceControls.PointerAction) + Look(place, PlaceControls.LookAction);

            // Échap a son geste à lui, une pile (décision 152).
            added += Button(Map(asset, EscapeControls.MapName), EscapeControls.BackAction, "<Keyboard>/escape");

            // Toucher ce qu'on voit : le clic gauche, quand aucun outil n'est en main.
            InputActionMap choose = Map(asset, ChooseControls.MapName);
            added += Button(choose, ChooseControls.TouchAction, "<Mouse>/leftButton") +
                     Pointer(choose, ChooseControls.PointerAction);

            // Marquer des arbres, l'outil « Retirer » en main : le clic gauche, le glisser et le pointeur ; le clic droit
            // bref le pose de côté.
            InputActionMap clearing = Map(asset, ClearingControls.MapName);
            added += Button(clearing, ClearingControls.MarkAction, "<Mouse>/leftButton") +
                     Button(clearing, ClearingControls.SecondaryAction, "<Mouse>/rightButton") +
                     Pointer(clearing, ClearingControls.PointerAction);

            // Les outils (décision 168) : la roue, touche tenue (Tab, comme les roues d'armes sur PC) ; un raccourci
            // mnémonique par outil, lié au caractère (SAVOIR, section 3), lu dans la table des outils.
            InputActionMap tools = Map(asset, ToolControls.MapName);
            added += Button(tools, ToolControls.WheelAction, "<Keyboard>/tab");
            foreach (ToolAction tool in ToolAction.All)
                added += Button(tools, tool.Name, tool.DefaultBinding);
            // Les cartes du panneau Construire ouvert (décision 184), et les modes de l'outil de la Place en main
            // (décision 206) : les touches 1 à 9, liées à leur place sur la rangée des chiffres, qu'on atteint sans Maj
            // en AZERTY aussi.
            for (int i = 0; i < ToolControls.CardKeys; i++)
                added += Button(tools, ToolControls.CardAction(i), "<Keyboard>/" + (i + 1));
            added += Pointer(tools, ToolControls.PointerAction);

            // Valider ce que l'interface propose : Entrée, celle du pavé numérique comprise.
            added += IfMissing(Map(asset, ConfirmControls.MapName), ConfirmControls.ConfirmAction, m =>
                m.AddAction(ConfirmControls.ConfirmAction, InputActionType.Button, "<Keyboard>/enter")
                    .AddBinding("<Keyboard>/numpadEnter"));

            // Faire tourner la Chimère à l'atelier : les touches qui tournent la caméra en ville, le clic gauche glissé.
            InputActionMap preview = Map(asset, PreviewControls.MapName);
            added += Turn(preview, PreviewControls.TurnAction) +
                     Button(preview, PreviewControls.GrabAction, "<Mouse>/leftButton") +
                     Pointer(preview, PreviewControls.PointerAction);

            if (added > 0)
            {
                EditorAssets.EnsureFolder("Assets/Input");
                File.WriteAllText(ActionsPath, asset.ToJson());
                AssetDatabase.ImportAsset(ActionsPath);
                Debug.Log("DICIELA : " + added + " action(s) ajoutée(s) ou mise(s) à jour dans " + ActionsPath + ".");
            }
            Object.DestroyImmediate(asset);
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

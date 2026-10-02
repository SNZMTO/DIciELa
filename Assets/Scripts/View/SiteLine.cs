using System;
using Diciela.Data;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// La ligne d'un chantier qui monte, côté rendu (décisions 166 et 181) : sa hauteur est la valeur propre de chaque
    /// rendu (la Renderer Shader User Value d'Unity, qui porte ici les bits d'un flottant), que lisent les shaders du
    /// chantier (ChantierLigne) ; son ondulation et son effritement, réglages de la pose, sont des variables globales des
    /// shaders. Ni bloc de propriétés ni matériau par chantier : le SRP Batcher regroupe les chantiers.
    /// </summary>
    internal static class SiteLine
    {
        /// <summary>Une ligne si bas que tout le fantôme se voit et rien de la vraie matière : l'aperçu de pose.</summary>
        public const float Nowhere = -10000f;

        private static readonly int WaveAmplitudeId = Shader.PropertyToID("_WaveAmplitude");
        private static readonly int WaveLengthId = Shader.PropertyToID("_WaveLength");
        private static readonly int WaveSecondsId = Shader.PropertyToID("_WaveSeconds");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");

        /// <summary>Donne aux shaders du chantier les réglages de la ligne : son ondulation et son effritement.</summary>
        public static void Publish(PlacementSettings settings)
        {
            Shader.SetGlobalFloat(WaveAmplitudeId, settings.WaveAmplitude);
            Shader.SetGlobalFloat(WaveLengthId, settings.WaveLength);
            Shader.SetGlobalFloat(WaveSecondsId, settings.WaveSeconds);
            Shader.SetGlobalFloat(SoftnessId, settings.SiteSoftness);
        }

        /// <summary>Met la ligne de ce rendu à cette hauteur, en mètres.</summary>
        public static void Raise(Renderer renderer, float height)
        {
            uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(height));
            if (renderer is MeshRenderer mesh)
                mesh.SetShaderUserValue(bits);
            else if (renderer is SkinnedMeshRenderer skinned)
                skinned.SetShaderUserValue(bits);
        }
    }
}

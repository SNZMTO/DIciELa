using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Diciela.Tests.EditMode
{
    /// <summary>
    /// Les shaders du chantier (décision 181) et du fantôme d'un sol (décision 216) : le SRP Batcher d'URP les regroupe,
    /// comme le dit l'Inspector d'un shader. Leurs propriétés sont toutes dans le tampon par matériau ; la ligne de
    /// chaque chantier passe par la valeur propre de chaque rendu, ses réglages par des variables globales, jamais par un
    /// bloc de propriétés. Unity ne publie pas cette réponse : le test la lit là où l'Inspector la lit, et dit clairement
    /// si Unity l'a déplacée.
    /// </summary>
    public sealed class SiteShadersTests
    {
        [TestCase("Diciela/Chantier")]
        [TestCase("Diciela/ChantierFantome")]
        [TestCase("Diciela/SolFantome")]
        public void TheSrpBatcherGroupsTheSiteShader(string name)
        {
            const BindingFlags Hidden = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo code = typeof(ShaderUtil).GetMethod("GetSRPBatcherCompatibilityCode", Hidden);
            MethodInfo reason = typeof(ShaderUtil).GetMethod("GetSRPBatcherCompatibilityIssueReason", Hidden);
            MethodInfo active = typeof(ShaderUtil).GetMethod("GetShaderActiveSubshaderIndex", Hidden);
            Assert.IsNotNull(code, "Unity ne dit plus où il range la compatibilité au SRP Batcher : revoir ce test.");
            Shader shader = Shader.Find(name);
            Assert.IsNotNull(shader, "Le shader " + name + " manque.");

            int subShader = active != null ? (int)active.Invoke(null, new object[] { shader }) : 0;
            int result = (int)code.Invoke(null, new object[] { shader, subShader });

            string why = result != 0 && reason != null
                ? (string)reason.Invoke(null, new object[] { shader, subShader, result })
                : "";
            // En mode batch (une campagne sans éditeur ouvert, sur une copie fraîche), Unity n'a pas encore préparé le
            // shader pour le SRP Batcher et répond « Not initialized » : le test est passé, pas échoué. Dans l'éditeur, il
            // tourne ; toute autre raison d'incompatibilité échoue comme avant.
            if (result != 0 && why.StartsWith("Not initialized"))
                Assert.Ignore(name + " : le SRP Batcher n'est pas initialisé (mode batch) ; ce test se lit dans l'éditeur.");
            Assert.AreEqual(0, result, name + " n'est pas regroupé par le SRP Batcher : " + why);
        }
    }
}

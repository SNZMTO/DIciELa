using System;
using System.Collections.Generic;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Crée ce qui manque des habitants ordinaires (GDD, sections 5, 6 et 16 ; décision 187) : les dix humains du pack,
    /// chacun tel quel (Assets/Data/Apparence/Humains) ; la sorte des habitants (de 5 à 20 en arrivant, ils rentrent
    /// dormir la nuit) ; les quatre premiers habitants, dans l'ordre de leur arrivée, chacun fort dans son métier, avec
    /// son humain (décision 188) et, pour nom, son métier : le bâtisseur, la fermière, la charretière, le menuisier.
    /// Réexécutable : une fiche qui existe n'est jamais touchée.
    /// </summary>
    internal static class VillagerContentBuilder
    {
        public const string HumansFolder = "Assets/Data/Apparence/Humains";
        private const string KindPath = "Assets/Data/Habitants/Habitant.asset";
        private const string FirstFolder = "Assets/Data/Habitants/Premiers";

        // Les humains du pack, dans l'ordre de leur planche : le nom de la fiche, et le personnage du pack (SM_Chr_…).
        private static readonly (string File, string Pack)[] Humans =
        {
            ("Homme_01", "Male_01"), ("Homme_02", "Male_02"), ("Homme_03", "Male_03"), ("Homme_04", "Male_04"),
            ("Boucher", "Butcher_01"), ("Femme_01", "Female_01"), ("Femme_02", "Female_02"),
            ("Femme_03", "Female_03"), ("Femme_04", "Female_04"), ("Taverniere", "Barmaid_01")
        };

        // Les premiers habitants, dans l'ordre de leur arrivée (GDD, section 6) : le nom de la fiche, la sous-statistique
        // de leur métier, leur humain.
        private static readonly (string File, string Strength, string Human)[] FirstVillagers =
        {
            ("Batisseur", "construction", "Homme_02"), ("Fermiere", "culture", "Femme_04"),
            ("Charretiere", "transport", "Femme_03"), ("Menuisier", "travail_bois", "Boucher")
        };

        /// <summary>Ce que le monteur rend : la sorte des habitants, et les premiers dans l'ordre de leur arrivée.</summary>
        internal sealed class Made
        {
            public VillagerKindDefinition Kind;
            public List<FirstVillagerDefinition> FirstVillagers;
        }

        /// <summary>Les fiches des humains, dans l'ordre de leur planche.</summary>
        public static IEnumerable<string> HumanPaths()
        {
            foreach ((string file, string _) in Humans)
                yield return HumansFolder + "/" + file + ".asset";
        }

        /// <summary>Crée ce qui manque ; les métiers se lisent dans ces statistiques.</summary>
        public static Made CreateMissing(IReadOnlyList<StatisticDefinition> statistics)
        {
            var humans = new Dictionary<string, CharacterDefinition>();
            foreach ((string file, string pack) in Humans)
            {
                humans[file] = EditorAssets.CreateIfMissing<CharacterDefinition>(HumansFolder + "/" + file + ".asset",
                    fiche =>
                    {
                        EditorAssets.Property(fiche, "_id").stringValue = file.ToLowerInvariant();
                        EditorAssets.Property(fiche, "_model").objectReferenceValue =
                            EditorAssets.Load<GameObject>(PackPaths.Characters + "/SM_Chr_" + pack + ".prefab");
                    });
            }
            VillagerKindDefinition kind = EditorAssets.CreateIfMissing<VillagerKindDefinition>(KindPath, fiche =>
            {
                EditorAssets.Property(fiche, "_id").stringValue = "habitant";
                EditorAssets.Property(fiche, "_nameKey").stringValue = "habitant.ordinaire";
                EditorAssets.Property(fiche, "_startingStatisticMin").floatValue = 5f;
                EditorAssets.Property(fiche, "_startingStatisticMax").floatValue = 20f;
                EditorAssets.Property(fiche, "_sleepsAtNight").boolValue = true;
            });
            var firsts = new List<FirstVillagerDefinition>();
            foreach ((string file, string strength, string human) in FirstVillagers)
            {
                firsts.Add(EditorAssets.CreateIfMissing<FirstVillagerDefinition>(FirstFolder + "/" + file + ".asset",
                    fiche =>
                    {
                        EditorAssets.Property(fiche, "_id").stringValue = file.ToLowerInvariant();
                        EditorAssets.Property(fiche, "_nameKey").stringValue = "habitant." + file.ToLowerInvariant();
                        EditorAssets.Property(fiche, "_strength").objectReferenceValue = SubStatistic(statistics, strength);
                        EditorAssets.Property(fiche, "_character").objectReferenceValue = humans[human];
                    }));
            }
            return new Made { Kind = kind, FirstVillagers = firsts };
        }

        private static SubStatisticDefinition SubStatistic(IReadOnlyList<StatisticDefinition> statistics, string id)
        {
            foreach (StatisticDefinition statistic in statistics)
            {
                foreach (SubStatisticDefinition subStatistic in statistic.SubStatistics)
                {
                    if (subStatistic != null && subStatistic.Id == id)
                        return subStatistic;
                }
            }
            throw new ArgumentException("Aucune statistique n'a la sous-statistique « " + id + " ».", nameof(id));
        }
    }
}

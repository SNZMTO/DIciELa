namespace Diciela.Data
{
    /// <summary>
    /// Les clés de textes qui ne sont pas rangées dans une fiche : celles de l'interface, et celles déduites d'un nom,
    /// avec une seule façon de les former, pour l'interface comme pour le test qui vérifie qu'elles existent.
    /// </summary>
    public static class TextKeys
    {
        private const string PlacementRefusalPrefix = "pose.refus.";
        private const string PlazaRefusalPrefix = "place.refus.";
        private const string FloorRefusalPrefix = "sol.refus.";
        private const string PaintModePrefix = "place.outil.";
        private const string PaintModeDetailPrefix = "place.outil.suite.";
        private const string PaintShapePrefix = "place.forme.";
        private const string PaintShapeDetailPrefix = "place.forme.suite.";
        private const string PaintStylePrefix = "place.style.";
        private const string PaintStyleDetailPrefix = "place.style.suite.";
        private const string PaintSymmetryPrefix = "place.symetrie.";
        private const string PaintSymmetryDetailPrefix = "place.symetrie.suite.";
        private const string DayPhasePrefix = "journee.";
        private const string DayNextPrefix = "journee.suite.";
        private const string DayNotePrefix = "journee.note.";

        // Les textes de l'écran de création (GDD, section 6) : son titre, ses deux premières lignes, le choix vide d'un
        // emplacement, le bouton qui valide.
        public const string CreationTitle = "creation.titre";
        public const string CreationHead = "creation.tete";
        public const string CreationBody = "creation.corps";
        public const string CreationNone = "creation.aucun";
        public const string CreationConfirm = "creation.valider";

        // La fiche d'un lieu (GDD, section 16 ; décision 141) : ses boutons, et un poste que personne ne tient.
        public const string SheetPutChimera = "fiche.mettre_chimere";
        public const string SheetRemoveChimera = "fiche.retirer_chimere";
        public const string SheetFreeSlot = "fiche.poste_libre";
        public const string SheetSleep = "fiche.dormir";
        public const string SheetWake = "fiche.reveiller";

        // L'astuce (GDD, section 16 ; décision 154) : ce qu'il y a à faire ensuite, au début d'une partie.
        public const string TipCamp = "astuce.campement";
        public const string TipPost = "astuce.poste";
        public const string TipCabin = "astuce.cabane";

        // La roue des outils (GDD, section 16 ; décision 168) : le nom et l'usage de chaque outil, dans la bulle du
        // milieu ; l'infobulle de l'icône des outils, où {touche} est remplacé par la touche de la roue.
        public const string BuildTool = "outil.construire";
        public const string BuildToolDetail = "outil.construire.suite";
        public const string ClearingTool = "outil.retirer";
        public const string ClearingToolDetail = "outil.retirer.suite";
        public const string MoveTool = "outil.deplacer";
        public const string MoveToolDetail = "outil.deplacer.suite";
        public const string PathsTool = "outil.chemins";
        public const string PathsToolDetail = "outil.chemins.suite";
        public const string PlazaTool = "outil.place";
        public const string PlazaToolDetail = "outil.place.suite";
        public const string ToolsTooltip = "outils.infobulle";
        public const string ToolsTooltipDetail = "outils.infobulle.suite";
        public const string KeyParameter = "{touche}";

        /// <summary>Les cases de la roue d'un outil de sol qui ne sont pas des modes (décision 221).</summary>
        public const string WheelBack = "roue.outils";
        public const string WheelBackDetail = "roue.outils.suite";
        public const string WheelSymmetry = "roue.symetrie";
        public const string WheelSymmetryDetail = "roue.symetrie.suite";
        public const string WheelInk = "roue.sol";
        public const string WheelInkDetail = "roue.sol.suite";

        // La carte de l'outil en main (décision 221) : ce qui est marqué pour être retiré ({nombre} y est remplacé), et
        // l'étoile avec ses branches ({branches}).
        public const string CardMarked = "carte.outil.marques";
        public const string CountParameter = "{nombre}";
        public const string CardStar = "carte.outil.etoile";

        // Les fanions du stock (GDD, section 16) : l'infobulle d'un fanion ({marchandise} y est remplacé).
        public const string StockTooltip = "fanion.infobulle";
        public const string GoodParameter = "{marchandise}";

        // Une construction dans la roue de Construire et sur la carte (GDD, section 16 ; décision 226) : ce qu'elle
        // offre, ses lits ({lits} y est remplacé) ; ce qu'elle coûte, chaque marchandise avec combien ({nombre} et
        // {marchandise}), à la suite ; l'usage d'une famille, dans la bulle.
        public const string CardBeds = "carte.lits";
        public const string BedsParameter = "{lits}";
        public const string CostAmount = "construction.cout";
        public const string ListSeparator = "liste.separateur";
        public const string WheelFamilyDetail = "roue.famille.suite";

        // La journée, en haut à gauche (GDD, section 16) : son numéro ({jour} y est remplacé).
        public const string DayNumber = "journee.jour";
        public const string DayNumberParameter = "{jour}";

        // Les outils de sol (décisions 206 et 221) : ce que leur carte écrit ({superficie}, {max}, {marques}, {taille} y
        // sont remplacés), et ce qu'elle dit quand un clic retirerait une place de charrette.
        public const string PlazaArea = "place.superficie";
        public const string AreaParameter = "{superficie}";
        public const string MaxParameter = "{max}";

        /// <summary>L'usage d'un nombre de branches de l'étoile ({branches} y est remplacé).</summary>
        public const string PaintStarDetail = "place.etoile.suite";
        public const string BranchesParameter = "{branches}";
        public const string PlazaMarks = "place.marques";
        public const string MarksParameter = "{marques}";
        public const string PaintBrushSize = "place.taille";
        public const string SizeParameter = "{taille}";
        public const string PlazaRemoveMark = "place.retirer_marque";
        public const string PlazaZoneInk = "place.zone";
        public const string PlazaZoneInkDetail = "place.zone.suite";
        public const string FloorInkDetail = "sol.suite";

        /// <summary>La clé du nom d'un mode d'un outil de sol, d'après son nom (Brush → place.outil.Brush).</summary>
        public static string PaintMode(string modeName)
        {
            return PaintModePrefix + modeName;
        }

        /// <summary>La clé de l'usage d'un mode d'un outil de sol (Brush → place.outil.suite.Brush).</summary>
        public static string PaintModeDetail(string modeName)
        {
            return PaintModeDetailPrefix + modeName;
        }

        /// <summary>La clé du nom d'une forme du mode Forme, d'après son nom (Disc → place.forme.Disc).</summary>
        public static string PaintShape(string shapeName)
        {
            return PaintShapePrefix + shapeName;
        }

        /// <summary>La clé de l'usage d'une forme du mode Forme (Disc → place.forme.suite.Disc).</summary>
        public static string PaintShapeDetail(string shapeName)
        {
            return PaintShapeDetailPrefix + shapeName;
        }

        /// <summary>La clé du nom d'un style de tracé, d'après son nom (Curved → place.style.Curved).</summary>
        public static string PaintStyle(string styleName)
        {
            return PaintStylePrefix + styleName;
        }

        /// <summary>La clé de l'usage d'un style de tracé (Curved → place.style.suite.Curved).</summary>
        public static string PaintStyleDetail(string styleName)
        {
            return PaintStyleDetailPrefix + styleName;
        }

        /// <summary>La clé du nom d'une symétrie, d'après son nom (Mirror → place.symetrie.Mirror).</summary>
        public static string PaintSymmetry(string symmetryName)
        {
            return PaintSymmetryPrefix + symmetryName;
        }

        /// <summary>La clé de l'usage d'une symétrie (Mirror → place.symetrie.suite.Mirror).</summary>
        public static string PaintSymmetryDetail(string symmetryName)
        {
            return PaintSymmetryDetailPrefix + symmetryName;
        }

        /// <summary>La clé de la raison d'un refus sur la Place, d'après son nom (TooLarge → place.refus.TooLarge).</summary>
        public static string PlazaRefusal(string reasonName)
        {
            return PlazaRefusalPrefix + reasonName;
        }

        /// <summary>La clé de la raison d'un refus sur les sols, d'après son nom (Unknown → sol.refus.Unknown).</summary>
        public static string FloorRefusal(string reasonName)
        {
            return FloorRefusalPrefix + reasonName;
        }

        /// <summary>La clé de la raison d'un refus de pose, d'après son nom (OutsideMap → pose.refus.OutsideMap).</summary>
        public static string PlacementRefusal(string reasonName)
        {
            return PlacementRefusalPrefix + reasonName;
        }

        /// <summary>L'infobulle de la journée, pendant cette phase : ce qui suit (Work → journee.suite.Work).</summary>
        public static string DayNext(string phaseName)
        {
            return DayNextPrefix + phaseName;
        }

        /// <summary>La suite de l'infobulle de la journée, pendant cette phase (Night → journee.note.Night).</summary>
        public static string DayNote(string phaseName)
        {
            return DayNotePrefix + phaseName;
        }

        /// <summary>La clé du nom d'une phase de la journée, d'après son nom (Evening → journee.Evening).</summary>
        public static string DayPhase(string phaseName)
        {
            return DayPhasePrefix + phaseName;
        }
    }
}

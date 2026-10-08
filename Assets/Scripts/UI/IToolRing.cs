using System.Collections.Generic;

namespace Diciela.UI
{
    /// <summary>
    /// La roue des réglages d'un outil en main (GDD, section 16 ; décisions 221 et 226) : ses cases, chacune à sa
    /// place, et ce que fait chacune quand on la prend, au chiffre ou à la molette ; sa case Outils, toujours en bas à
    /// gauche, ramène aux outils. Les outils de sol en ont une (PaintRing), Construire aussi (BuildRing).
    /// </summary>
    internal interface IToolRing
    {
        /// <summary>Ses cases, telles que la roue les montre.</summary>
        List<WheelSlot> Slots();

        /// <summary>
        /// Prend cette case (option -1) ou cette option ; faux pour la case Outils, qui ramène aux outils.
        /// </summary>
        bool Pick(int place, int option);

        /// <summary>
        /// Le chiffre de cette case : la prendre, ou, déjà prise, passer à son option suivante ; faux pour Outils.
        /// </summary>
        bool Key(int place);

        /// <summary>La molette sur cette case ou cette option : vrai si elle a réglé quelque chose.</summary>
        bool Scroll(int place, int option, int notches);
    }
}

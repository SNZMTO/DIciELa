namespace Diciela.UI
{
    /// <summary>
    /// Les chiffres sur la roue de l'outil en main (décision 221) : prendre sa case de ce rang (0 : la première, en haut),
    /// ou, déjà prise, son option suivante ; la case Outils ouvre la roue des outils.
    /// </summary>
    public interface IRingKeys
    {
        void TakeSlot(int index);
    }
}

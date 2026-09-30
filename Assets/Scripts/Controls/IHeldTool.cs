namespace Diciela.Controls
{
    /// <summary>
    /// Un outil qu'on a en main (poser, retirer) : tant qu'il l'est, le clic gauche sur la ville est à lui, pas au geste
    /// « toucher ».
    /// </summary>
    public interface IHeldTool
    {
        bool IsActive { get; }
    }
}

namespace Diciela.EditorTools
{
    /// <summary>
    /// Comment se pose, au départ, une pièce d'un emplacement que le pack ne pose nulle part (AttachmentFitter) : la règle
    /// de sa place sur la tête de référence, et de combien elle s'enfonce sur l'axe de contact de l'emplacement.
    /// </summary>
    internal readonly struct SlotRule
    {
        public enum Place
        {
            /// <summary>Au pied de la queue du chat, telle que le pack la pose.</summary>
            TailRoot,

            /// <summary>Sur la place de l'os de l'emplacement.</summary>
            Bone,

            /// <summary>Centrée sur le haut du crâne.</summary>
            Crown,

            /// <summary>Au niveau des yeux.</summary>
            Eyes,

            /// <summary>Au bout du nez.</summary>
            Nose,

            /// <summary>Au milieu des oreilles.</summary>
            Ears
        }

        public SlotRule(Place place, float sink)
        {
            Where = place;
            Sink = sink;
        }

        public Place Where { get; }

        /// <summary>Sur l'axe de contact, de combien de mètres elle entre dans la surface qu'elle touche.</summary>
        public float Sink { get; }
    }
}

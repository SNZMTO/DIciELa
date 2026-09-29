using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.Data
{
    /// <summary>
    /// Le réglage d'une pièce sur des porteurs qui ne ressemblent pas aux autres : des têtes pour une pièce de la tête (les
    /// dômes des yeux de la grenouille, le bec de l'oiseau), des corps pour une pièce du corps (la tunique évasée du raton
    /// sous une cape). Un décalage de plus, sur eux seulement, et au besoin un autre modèle (la barbe réduite au menton). Les deux variantes d'un animal ont la même forme : un
    /// réglage les nomme toutes les deux. Réglé à la main, dans la fiche de la pièce.
    /// </summary>
    [Serializable]
    public struct WearerAdjustment
    {
        [Tooltip("Les têtes sur lesquelles ce réglage vaut, pour une pièce de la tête.")]
        [SerializeField] private List<HeadDefinition> _heads;

        [Tooltip("Les corps sur lesquels ce réglage vaut, pour une pièce du corps (cou, dos, ceinture, queue).")]
        [SerializeField] private List<BodyDefinition> _bodies;

        [Tooltip("Le décalage ajouté sur ces porteurs, en mètres (x à droite, y en haut, z devant). Pour une paire " +
                 "(des oreilles), x écarte ses moitiés, chacune de son côté : négatif, elles se rapprochent.")]
        [SerializeField] private Vector3 _offset;

        [Tooltip("Le modèle qu'elle prend sur ces porteurs, s'il n'est pas le sien (la barbe réduite au menton sous un bec " +
                 "ou un gros museau) : une copie posée de la même façon. Vide : le sien.")]
        [SerializeField] private GameObject _model;

        public Vector3 Offset => _offset;
        public GameObject Model => _model;

        /// <summary>Vrai si ce réglage vaut sur ce corps (onBody : une pièce du corps) ou sur cette tête.</summary>
        public bool AppliesTo(HeadDefinition head, BodyDefinition body, bool onBody)
        {
            return onBody
                ? body != null && _bodies != null && _bodies.Contains(body)
                : head != null && _heads != null && _heads.Contains(head);
        }
    }
}

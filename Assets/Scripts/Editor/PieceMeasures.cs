using System.Globalization;
using Diciela.Data;
using UnityEngine;

namespace Diciela.EditorTools
{
    /// <summary>
    /// Les mesures d'une pièce posée sur un personnage habillé, pour juger ses proportions en chiffres à côté des
    /// planches (PieceSheets) : sa boîte (taille, centre) rapportée à celle du crâne choisi, ou du corps pour une pièce du
    /// corps (la boîte de leur maillage, dans le repère du personnage, posé à l'origine). Une ligne de tableau par pièce
    /// et par animal, en mètres.
    /// </summary>
    internal static class PieceMeasures
    {
        public const string Header = "emplacement;piece;animal;largeur;hauteur;profondeur;largeur/ref;hauteur/ref;" +
                                     "profondeur/ref;centre-ref x;centre-ref y;centre-ref z;bas-haut ref;ref\n";

        /// <summary>La ligne de cette pièce, telle qu'elle est posée sous ce personnage ; vide si elle n'y est pas.</summary>
        public static string Row(Transform holder, AttachmentDefinition piece, HeadDefinition head, BodyDefinition body)
        {
            bool onBody = piece.Slot.Frame == AttachmentFrame.Body || piece.Slot.Frame == AttachmentFrame.Neck;
            Bounds reference = onBody ? body.Mesh.bounds : head.Mesh.bounds;
            string name = piece.Model.name + "(Clone)";
            Bounds? shown = null;
            foreach (Renderer renderer in holder.GetComponentsInChildren<Renderer>())
            {
                if (!IsUnder(renderer.transform, name))
                    continue;
                if (shown is Bounds box)
                {
                    box.Encapsulate(renderer.bounds);
                    shown = box;
                }
                else
                    shown = renderer.bounds;
            }
            string animal = head.Id;
            if (!(shown is Bounds b))
                return piece.Slot.name + ";" + piece.name + ";" + animal + ";cachée ou absente\n";
            Vector3 size = b.size, center = b.center - reference.center;
            return string.Join(";", piece.Slot.name, piece.name, animal, F(size.x), F(size.y), F(size.z),
                       F(size.x / reference.size.x), F(size.y / reference.size.y), F(size.z / reference.size.z),
                       F(center.x), F(center.y), F(center.z), F(b.min.y - reference.max.y), onBody ? "corps" : "crâne") +
                   "\n";
        }

        private static bool IsUnder(Transform transform, string name)
        {
            for (Transform current = transform; current != null; current = current.parent)
            {
                if (current.name == name)
                    return true;
            }
            return false;
        }

        private static string F(float value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}

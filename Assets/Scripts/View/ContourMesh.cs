using System.Collections.Generic;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Le dedans d'un champ lissé (CellField ; décision 227), en faces drapées sur le relief, par la méthode des
    /// « carrés qui marchent » (ContourSquares) : chaque carré garde ses coins dedans et les points où ses côtés
    /// passent le niveau du contour, un polygone convexe qui se fait en éventail ; une selle dont le milieu est dehors
    /// se coupe en deux triangles. Deux carrés voisins partagent leurs sommets. Ses listes servent d'un dedans à
    /// l'autre.
    /// </summary>
    internal sealed class ContourMesh
    {
        private readonly List<int> _ring = new List<int>();
        private readonly bool[] _inside = new bool[4];

        /// <summary>Le dedans de ce champ, en faces vues d'en haut.</summary>
        public void AddFill(DrapedMesh mesh, CellField field)
        {
            for (int j = 0; j < field.CornersY - 1; j++)
            {
                for (int i = 0; i < field.CornersX - 1; i++)
                    FillSquare(mesh, field, i, j);
            }
        }

        private void FillSquare(DrapedMesh mesh, CellField field, int i, int j)
        {
            int count = ContourSquares.Classify(field, i, j, _inside);
            if (count == 0)
                return;
            if (ContourSquares.IsSaddle(count, _inside) && !ContourSquares.CenterInside(field, i, j))
            {
                // Une selle dont le milieu est dehors : chaque coin dedans a son triangle.
                for (int corner = 0; corner < 4; corner++)
                {
                    if (!_inside[corner])
                        continue;
                    _ring.Clear();
                    _ring.Add(Crossing(mesh, field, i, j, (corner + 3) % 4));
                    _ring.Add(Corner(mesh, field, i, j, corner));
                    _ring.Add(Crossing(mesh, field, i, j, corner));
                    Fan(mesh);
                }
                return;
            }
            // Le tour du carré, dans le sens des aiguilles d'une montre : chaque coin dedans, chaque côté coupé.
            _ring.Clear();
            for (int corner = 0; corner < 4; corner++)
            {
                if (_inside[corner])
                    _ring.Add(Corner(mesh, field, i, j, corner));
                if (_inside[corner] != _inside[(corner + 1) % 4])
                    _ring.Add(Crossing(mesh, field, i, j, corner));
            }
            Fan(mesh);
        }

        private static int Corner(DrapedMesh mesh, CellField field, int i, int j, int corner)
        {
            long key = ContourSquares.CornerKey(field, i, j, corner, out Vector2 position);
            return mesh.Point(key, position);
        }

        private static int Crossing(DrapedMesh mesh, CellField field, int i, int j, int side)
        {
            return mesh.Point(ContourSquares.SideKey(field, i, j, side), ContourSquares.SidePoint(field, i, j, side));
        }

        // Le polygone noté dans _ring, en éventail depuis son premier sommet : il est convexe, ses sommets sur le tour
        // du carré, dans le sens des aiguilles d'une montre.
        private void Fan(DrapedMesh mesh)
        {
            for (int k = 1; k < _ring.Count - 1; k++)
                mesh.AddFace(_ring[0], _ring[k], _ring[k + 1]);
        }
    }
}

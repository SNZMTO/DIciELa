using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Ce que chaque sol peint donne au sol de la carte autour de lui (GDD, section 9 ; décision 227), sur une région
    /// de mailles. Le sol couvre son champ lissé (CellField), son bord fondu sur la douceur des réglages et effiloché
    /// par un bruit (GroundNoise) ; son liant, s'il en a un, couvre une bande de sa largeur tout autour, d'après la
    /// distance au sol (CellDistance), son bord effiloché aussi. Il ne lit que les sols qui servent (bâtis, ou qu'on
    /// efface). Ses champs servent d'une région à l'autre, une paire par sorte de sol.
    /// </summary>
    internal sealed class FloorWeights
    {
        // Le bord du liant tire son bruit d'une autre graine que celui du sol.
        private const int BorderSeed = 0x2C1B3C6D;

        private readonly List<Kind> _kinds = new List<Kind>();
        private int _count;
        private float _softness;
        private float _fieldSoftness;

        // Une sorte de sol présente dans la région : sa fiche, son champ, la distance à lui, sa graine.
        private sealed class Kind
        {
            public FloorDefinition Floor;
            public readonly CellField Field = new CellField();
            public readonly CellDistance Distance = new CellDistance();
            public int Seed;
        }

        /// <summary>Combien de sortes de sol touchent la région, depuis le dernier Build.</summary>
        public int Count => _count;

        /// <summary>La fiche de la sorte de ce rang.</summary>
        public FloorDefinition FloorOf(int kind)
        {
            return _kinds[kind].Floor;
        }

        /// <summary>
        /// Lit les sols qui servent sur ces mailles (la région, et ce qui peut peindre jusqu'à elle), lissés sur ce
        /// rayon en mailles, leur bord fondu sur cette douceur en mètres ; vrai si l'un d'eux touche la région. En
        /// prévision, ce sont les sols tels qu'ils seront une fois leurs chantiers finis (Serves).
        /// </summary>
        public bool Build(Floors floors, NavGrid grid, RectInt cells, int radiusCells, float softnessMeters,
            bool foresight)
        {
            _count = 0;
            float size = grid.Metrics.CellSize;
            _softness = softnessMeters;
            // Le champ passe de 0 à 1 sur la largeur du flou : la douceur, en parts de champ.
            _fieldSoftness = softnessMeters / (2f * radiusCells * size);
            for (int y = cells.yMin; y < cells.yMax; y++)
            {
                for (int x = cells.xMin; x < cells.xMax; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!grid.Contains(cell))
                        continue;
                    if (!Serves(floors.StateAt(cell), foresight))
                        continue;
                    FloorDefinition floor = floors.FloorAt(cell);
                    if (floor == null || floor.Layer == null)
                        continue;
                    Kind kind = KindOf(floor, cells, size, radiusCells);
                    kind.Field.Add(cell);
                    kind.Distance.Add(cell);
                }
            }
            for (int k = 0; k < _count; k++)
            {
                _kinds[k].Field.Smooth();
                if (_kinds[k].Floor.BorderMeters > 0f)
                    _kinds[k].Distance.Spread();
            }
            return _count > 0;
        }

        /// <summary>Ce que la sorte de ce rang couvre en ce point, de 0 à 1 : son bord fondu et effiloché.</summary>
        public float FloorAt(int kind, Vector2 point)
        {
            Kind sort = _kinds[kind];
            float field = sort.Field.Sample(Fray(point, sort, sort.Seed));
            return Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(CellField.Edge - _fieldSoftness, CellField.Edge + _fieldSoftness, field));
        }

        /// <summary>Ce que le liant de la sorte de ce rang couvre en ce point, de 0 à 1 ; 0 sans liant.</summary>
        public float BorderAt(int kind, Vector2 point)
        {
            Kind sort = _kinds[kind];
            float width = sort.Floor.BorderMeters;
            if (width <= 0f)
                return 0f;
            float distance = sort.Distance.Sample(Fray(point, sort, sort.Seed ^ BorderSeed));
            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(width - _softness, width + _softness, distance));
        }

        /// <summary>
        /// Vrai si un sol dans cet état se peint : bâti, ou qu'on efface (il sert jusqu'à la fin de son chantier) ; en
        /// prévision (décision 228), bâti ou tracé, et plus celui qu'on efface.
        /// </summary>
        public static bool Serves(FloorState state, bool foresight)
        {
            if (state == FloorState.Built)
                return true;
            return foresight ? state == FloorState.Planned : state == FloorState.Erasing;
        }

        // Le point où lire le champ ou la distance : déplacé par le bruit de la sorte, de son effilochage au plus.
        private static Vector2 Fray(Vector2 point, Kind sort, int seed)
        {
            FloorDefinition floor = sort.Floor;
            if (floor.FrayMeters <= 0f)
                return point;
            return point + GroundNoise.Warp(point, floor.FrayScaleMeters, seed) * floor.FrayMeters;
        }

        // La sorte de ce sol dans la région, commencée à sa première maille.
        private Kind KindOf(FloorDefinition floor, RectInt cells, float size, int radiusCells)
        {
            for (int k = 0; k < _count; k++)
            {
                if (ReferenceEquals(_kinds[k].Floor, floor))
                    return _kinds[k];
            }
            if (_count == _kinds.Count)
                _kinds.Add(new Kind());
            Kind kind = _kinds[_count++];
            kind.Floor = floor;
            kind.Seed = GroundNoise.SeedOf(floor.Id);
            kind.Field.Begin(cells, size, radiusCells);
            kind.Distance.Begin(cells, size);
            return kind;
        }
    }
}

using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Ce que chaque sol peint donne au sol de la carte autour de lui (GDD, sections 9 et 16 ; décisions 227 et 232),
    /// sur une région de mailles. Chaque point a sa distance au bord du sol, négative dedans : près du bord, celle du
    /// champ lissé de ses mailles (CellField), dont le niveau moitié arrondit les coins et efface l'escalier des
    /// diagonales ; plus loin, celle de deux distances de mailles (CellDistance), aux mailles du sol et aux autres. Le
    /// sol se fond sur la largeur de son fondu, centrée sur son bord, comme peint au pinceau doux dans la démo du
    /// pack ; son liant, s'il en a un, couvre une bande de sa largeur depuis ce bord, qui se fond à son tour sur la
    /// largeur du sien. Un bruit (GroundNoise) effiloche le bord et le fondu. Il ne lit que les sols qui se peignent
    /// (Serves). Ses distances servent d'une région à l'autre, une paire par sorte de sol.
    /// </summary>
    internal sealed class FloorWeights
    {
        // Le bord du liant tire son bruit d'une autre graine que celui du sol.
        private const int BorderSeed = 0x2C1B3C6D;

        // Jusqu'où, en part de sa rampe, on suit le champ seul ; au-delà, on passe en douceur à la distance des
        // mailles.
        private const float NearEdge = 0.6f;

        private readonly List<Kind> _kinds = new List<Kind>();
        private int _count;
        private float _halfCell;
        private float _rampHalf;

        // Une sorte de sol présente dans la région : sa fiche, son champ, la distance à ses mailles, celle aux autres,
        // sa graine.
        private sealed class Kind
        {
            public FloorDefinition Floor;
            public readonly CellField Field = new CellField();
            public readonly CellDistance Outside = new CellDistance();
            public readonly CellDistance Inside = new CellDistance();
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
        /// Lit les sols qui se peignent sur ces mailles (la région, et ce qui peut peindre jusqu'à elle), lissés sur ce
        /// rayon en mailles ; vrai si l'un d'eux touche la région. En prévision, ce sont les sols tels qu'ils seront
        /// une fois leurs chantiers finis.
        /// </summary>
        public bool Build(Floors floors, NavGrid grid, RectInt cells, int radiusCells, bool foresight)
        {
            _count = 0;
            float size = grid.Metrics.CellSize;
            _halfCell = size / 2f;
            // Le long d'un bord droit, le champ passe de 1 à 0 sur deux rayons de flou, centrés sur le bord.
            _rampHalf = radiusCells * size;
            for (int y = cells.yMin; y < cells.yMax; y++)
            {
                for (int x = cells.xMin; x < cells.xMax; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!grid.Contains(cell) || !Serves(floors.StateAt(cell), foresight))
                        continue;
                    FloorDefinition floor = floors.FloorAt(cell);
                    if (floor == null || floor.Layer == null)
                        continue;
                    Kind kind = KindOf(floor, cells, size, radiusCells);
                    kind.Field.Add(cell);
                    kind.Outside.Add(cell);
                }
            }
            for (int k = 0; k < _count; k++)
            {
                Kind kind = _kinds[k];
                kind.Field.Smooth();
                kind.Outside.Spread();
                // Les autres mailles, celles d'un autre sol comprises : le bord du sol passe entre elles et les
                // siennes.
                for (int y = cells.yMin; y < cells.yMax; y++)
                {
                    for (int x = cells.xMin; x < cells.xMax; x++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (kind.Outside.At(cell) > 0f)
                            kind.Inside.Add(cell);
                    }
                }
                kind.Inside.Spread();
            }
            return _count > 0;
        }

        /// <summary>
        /// Ce que la sorte de ce rang couvre en ce point, de 0 à 1 : tout dedans, rien dehors, fondue entre les deux.
        /// </summary>
        public float FloorAt(int kind, Vector2 point)
        {
            Kind sort = _kinds[kind];
            float half = sort.Floor.FadeMeters / 2f;
            float edge = DistanceToEdge(sort, Fray(point, sort, sort.Seed));
            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-half, half, edge));
        }

        /// <summary>Ce que le liant de la sorte de ce rang couvre en ce point, de 0 à 1 ; 0 sans liant.</summary>
        public float BorderAt(int kind, Vector2 point)
        {
            Kind sort = _kinds[kind];
            float width = sort.Floor.BorderMeters;
            if (width <= 0f)
                return 0f;
            float half = sort.Floor.BorderFadeMeters / 2f;
            float edge = DistanceToEdge(sort, Fray(point, sort, sort.Seed ^ BorderSeed));
            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(width - half, width + half, edge));
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

        // La distance de ce point au bord du sol, en mètres, négative dedans : celle du champ, où sa rampe passe ;
        // celle des mailles, où il ne bouge plus ; entre les deux, de l'une à l'autre en douceur.
        private float DistanceToEdge(Kind sort, Vector2 point)
        {
            float field = sort.Field.Sample(point);
            float smooth = (CellField.Edge - field) * 2f * _rampHalf;
            float away = Mathf.Abs(field - CellField.Edge) * 2f;
            if (away <= NearEdge)
                return smooth;
            float cells = CellsDistance(sort, point);
            return Mathf.Lerp(smooth, cells, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(NearEdge, 1f, away)));
        }

        // La distance de ce point au bord des mailles du sol, en mètres, négative dedans. Entre le milieu d'une de ses
        // mailles et celui d'une autre, le bord passe à mi-chemin : la moitié de l'écart de leurs distances ; plus
        // loin, la distance à la plus proche, moins une demi-maille. Hors de la région, rien du sol.
        private float CellsDistance(Kind sort, Vector2 point)
        {
            float outside = sort.Outside.Sample(point);
            if (float.IsPositiveInfinity(outside))
                return float.PositiveInfinity;
            float inside = sort.Inside.Sample(point);
            if (float.IsPositiveInfinity(inside))
                return float.NegativeInfinity;
            if (outside > 0f && inside > 0f)
                return (outside - inside) / 2f;
            return outside > 0f ? outside - _halfCell : _halfCell - inside;
        }

        // Le point où lire les distances : déplacé par le bruit de la sorte, de son effilochage au plus.
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
            kind.Outside.Begin(cells, size);
            kind.Inside.Begin(cells, size);
            return kind;
        }
    }
}

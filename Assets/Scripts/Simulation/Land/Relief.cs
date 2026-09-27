using System;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Le relief de la carte : une grille régulière de hauteurs en mètres, dans le repère du monde (plan XZ, origine au
    /// coin de la carte). Entre quatre points, la hauteur se lit par interpolation bilinéaire, et la pente est celle de
    /// cette même surface : une seule façon de lire le sol.
    /// </summary>
    public sealed class Relief
    {
        private const float DegreesPerRadian = 180f / MathF.PI;

        // Les divisions par deux qui affinent le point où un rayon touche le sol : 20 divisent l'écart par un million.
        private const int RaycastRefinements = 20;

        // Une marge autour du plus haut et du plus bas du relief, en mètres : sans elle, un rayon qui touche le plus bas
        // (tout le sol, sur une carte plate) s'arrêterait parfois un cheveu au-dessus, par arrondi, et le raterait.
        private const float RaycastMarginMeters = 1e-3f;

        private readonly float[] _heights;
        private readonly float _stepX;
        private readonly float _stepZ;
        private readonly float _lowest;
        private readonly float _highest;

        /// <param name="heightsMeters">Les hauteurs, rangée par rangée : l'indice est z × samplesX + x. Copiées.</param>
        public Relief(float sizeX, float sizeZ, int samplesX, int samplesZ, float[] heightsMeters)
        {
            if (heightsMeters == null)
                throw new ArgumentNullException(nameof(heightsMeters));
            if (!(sizeX > 0f) || !(sizeZ > 0f))
                throw new ArgumentException("Une carte a une taille plus grande que zéro.");
            if (samplesX < 2 || samplesZ < 2)
                throw new ArgumentException("Un relief a au moins deux points de côté.");
            if (heightsMeters.Length != samplesX * samplesZ)
                throw new ArgumentException("Il faut une hauteur par point du relief.", nameof(heightsMeters));

            SizeX = sizeX;
            SizeZ = sizeZ;
            SamplesX = samplesX;
            SamplesZ = samplesZ;
            _heights = (float[])heightsMeters.Clone();
            _stepX = sizeX / (samplesX - 1);
            _stepZ = sizeZ / (samplesZ - 1);
            _lowest = float.MaxValue;
            _highest = float.MinValue;
            foreach (float height in _heights)
            {
                _lowest = Math.Min(_lowest, height);
                _highest = Math.Max(_highest, height);
            }
        }

        public float SizeX { get; }
        public float SizeZ { get; }
        public int SamplesX { get; }
        public int SamplesZ { get; }

        public bool Contains(float x, float z)
        {
            return x >= 0f && x <= SizeX && z >= 0f && z <= SizeZ;
        }

        /// <summary>La hauteur du sol en mètres ; hors de la carte, celle du bord le plus proche.</summary>
        public float HeightAt(float x, float z)
        {
            Locate(x, z, out int ix, out int iz, out float tx, out float tz);
            float h00 = Sample(ix, iz), h10 = Sample(ix + 1, iz), h01 = Sample(ix, iz + 1), h11 = Sample(ix + 1, iz + 1);
            return (1f - tz) * ((1f - tx) * h00 + tx * h10) + tz * ((1f - tx) * h01 + tx * h11);
        }

        /// <summary>La pente du sol en degrés : 0 à plat, 45 quand on monte d'un mètre par mètre.</summary>
        public float SlopeDegreesAt(float x, float z)
        {
            Locate(x, z, out int ix, out int iz, out float tx, out float tz);
            float h00 = Sample(ix, iz), h10 = Sample(ix + 1, iz), h01 = Sample(ix, iz + 1), h11 = Sample(ix + 1, iz + 1);
            float riseX = ((1f - tz) * (h10 - h00) + tz * (h11 - h01)) / _stepX;
            float riseZ = ((1f - tx) * (h01 - h00) + tx * (h11 - h10)) / _stepZ;
            return MathF.Atan(MathF.Sqrt(riseX * riseX + riseZ * riseZ)) * DegreesPerRadian;
        }

        /// <summary>
        /// Le premier point où ce rayon touche le sol de la carte, à moins de <paramref name="maxDistance"/> mètres de
        /// son départ : la visée du pointeur, qui suit le vrai relief, collines comprises (SAVOIR, 2.7). Faux si le
        /// rayon ne touche pas le sol, ou le touche hors de la carte.
        /// </summary>
        public bool TryRaycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 hit)
        {
            hit = default;
            Vector3 dir = direction.normalized;
            if (dir == Vector3.zero || !(maxDistance > 0f))
                return false;

            // Seule la tranche du rayon entre le plus haut et le plus bas du relief peut toucher le sol.
            float from = 0f, to = maxDistance;
            if (dir.y < 0f)
            {
                from = Math.Max(0f, (origin.y - (_highest + RaycastMarginMeters)) / -dir.y);
                to = Math.Min(to, (origin.y - (_lowest - RaycastMarginMeters)) / -dir.y);
            }
            else if (origin.y > _highest)
                return false;
            if (from > to)
                return false;
            if (IsBelowGround(origin + dir * from))
                return TryHitAt(origin + dir * from, out hit);

            // Un pas d'une demi-maille du relief : on ne saute pas une bosse ; le nombre de pas borne la boucle.
            float step = Math.Min(_stepX, _stepZ) * 0.5f;
            int steps = (int)Math.Ceiling((to - from) / step);
            float above = from;
            for (int i = 1; i <= steps; i++)
            {
                float along = Math.Min(from + i * step, to);
                if (IsBelowGround(origin + dir * along))
                    return TryHitAt(Refine(origin, dir, above, along), out hit);
                above = along;
            }
            return false;
        }

        private bool IsBelowGround(Vector3 point)
        {
            return point.y <= HeightAt(point.x, point.z);
        }

        /// <summary>Entre un point du rayon au-dessus du sol et un point dessous, le point où il le touche.</summary>
        private Vector3 Refine(Vector3 origin, Vector3 dir, float above, float below)
        {
            for (int i = 0; i < RaycastRefinements; i++)
            {
                float middle = (above + below) * 0.5f;
                if (IsBelowGround(origin + dir * middle))
                    below = middle;
                else
                    above = middle;
            }
            return origin + dir * below;
        }

        private bool TryHitAt(Vector3 point, out Vector3 hit)
        {
            hit = new Vector3(point.x, HeightAt(point.x, point.z), point.z);
            return Contains(point.x, point.z);
        }

        private float Sample(int ix, int iz)
        {
            return _heights[iz * SamplesX + ix];
        }

        /// <summary>La maille qui contient le point (ramené sur la carte) et la position du point dans cette maille.</summary>
        private void Locate(float x, float z, out int ix, out int iz, out float tx, out float tz)
        {
            float fx = Math.Clamp(x, 0f, SizeX) / _stepX;
            float fz = Math.Clamp(z, 0f, SizeZ) / _stepZ;
            ix = Math.Min((int)fx, SamplesX - 2);
            iz = Math.Min((int)fz, SamplesZ - 2);
            tx = fx - ix;
            tz = fz - iz;
        }
    }
}

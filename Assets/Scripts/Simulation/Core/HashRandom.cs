using System;

namespace Diciela.Simulation.Core
{
    /// <summary>
    /// Le hasard par hachage (SAVOIR, 2.4) : une suite de nombres qui ont l'air tirés au hasard, mais que les mêmes
    /// entrées rendent toujours pareils, sur toute machine ; jamais UnityEngine.Random ni System.Random, qu'une mise à
    /// jour pourrait faire bouger d'un bit. C'est SplitMix64 (Sebastiano Vigna) : un compteur qui avance d'un grand
    /// nombre impair, brassé à chaque tirage. Deux entiers (un numéro d'habitant, un numéro de pas…) donnent son
    /// départ. C'est une valeur, pas un objet partagé : chacun tire la sienne là où il en a besoin, sans rien allouer.
    /// </summary>
    public struct HashRandom
    {
        // Le pas du compteur : la partie décimale du nombre d'or, en 64 bits.
        private const ulong Gamma = 0x9E3779B97F4A7C15UL;

        private ulong _state;

        /// <summary>La suite qui part de ces deux entiers ; (0 ; 0) donne celle de SplitMix64 partie de zéro.</summary>
        public HashRandom(int a, int b)
        {
            // Les deux entiers côte à côte en 64 bits, puis brassés : deux paires différentes, deux départs différents.
            _state = Mix(((ulong)(uint)a << 32) | (uint)b);
        }

        /// <summary>
        /// Le brassage de SplitMix64 : chaque bit de l'entrée change en moyenne la moitié de ceux de la sortie, et deux
        /// entrées différentes ne donnent jamais la même sortie.
        /// </summary>
        public static ulong Mix(ulong value)
        {
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }

        /// <summary>Le tirage suivant, sur 64 bits.</summary>
        public ulong NextBits()
        {
            _state += Gamma;
            return Mix(_state);
        }

        /// <summary>Un entier de 0 (compris) à count (exclu).</summary>
        public int Range(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Il faut au moins une valeur possible.");
            // Les 32 bits du haut, mis à l'échelle par une multiplication : pas de modulo, qui favoriserait les petits.
            return (int)(((NextBits() >> 32) * (ulong)count) >> 32);
        }

        /// <summary>Un nombre de 0 (compris) à 1 (exclu).</summary>
        public double Unit()
        {
            // Les 53 bits du haut : toute la précision d'un double.
            return (NextBits() >> 11) * (1.0 / (1UL << 53));
        }
    }
}

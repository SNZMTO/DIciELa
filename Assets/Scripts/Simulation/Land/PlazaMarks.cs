using System;
using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation.Core;
using UnityEngine;

namespace Diciela.Simulation.Land
{
    /// <summary>
    /// Les places de charrette de la Place (décision 204) : les marques que le joueur pose dans la zone, à la taille de la
    /// plus grande charrette, dans l'ordre où il les a posées, et celles posées ou retirées pendant le pas, que la Place
    /// livre avec ses événements. Seule la Place s'en sert ; elle dit si une marque se pose.
    /// </summary>
    internal sealed class PlazaMarks
    {
        private readonly CartDefinition _cart;
        private readonly List<PlazaMark> _marks = new List<PlazaMark>();
        private readonly List<PlazaMark> _placed = new List<PlazaMark>();
        private readonly List<PlazaMark> _removed = new List<PlazaMark>();
        private int _nextId = 1;

        public PlazaMarks(CartDefinition cart)
        {
            _cart = cart;
        }

        /// <summary>Dans l'ordre où elles ont été posées : celui où elles appellent.</summary>
        public IReadOnlyList<PlazaMark> All => _marks;

        public PlazaMark Find(int id)
        {
            for (int i = 0; i < _marks.Count; i++)
            {
                if (_marks[i].Id == id)
                    return _marks[i];
            }
            return null;
        }

        /// <summary>La place que prendrait une marque posée ici, l'avant de la charrette ainsi tourné.</summary>
        public RotatedRect Footprint(Vector2 center, float headingDegrees)
        {
            Vector2 size = _cart == null ? Vector2.zero : new Vector2(_cart.SpotWidthMeters, _cart.SpotLengthMeters);
            return new RotatedRect(center, size, headingDegrees);
        }

        /// <summary>Où se tient celui qui gare la charrette d'une marque posée ici : au bout du timon, au bord avant.</summary>
        public Vector2 StandOf(Vector2 center, float headingDegrees)
        {
            return center + Heading.Direction(headingDegrees) * (Footprint(center, headingDegrees).Size.y * 0.5f);
        }

        /// <summary>Pose une marque ici ; la Place a vérifié qu'elle se pose.</summary>
        public PlazaMark Add(Vector2 center, float headingDegrees)
        {
            RotatedRect footprint = Footprint(center, headingDegrees);
            float ahead = footprint.Size.y * 0.5f - _cart.PullMeters;
            var mark = new PlazaMark(_nextId++, footprint, center + Heading.Direction(headingDegrees) * ahead);
            _marks.Add(mark);
            _placed.Add(mark);
            return mark;
        }

        public void Remove(PlazaMark mark)
        {
            _marks.Remove(mark);
            _removed.Add(mark);
        }

        /// <summary>Lève les marques posées puis celles retirées pendant le pas, et oublie les deux files.</summary>
        public void Deliver(Action<PlazaMark> placed, Action<PlazaMark> removed)
        {
            try
            {
                for (int i = 0; i < _placed.Count; i++)
                    placed?.Invoke(_placed[i]);
                for (int i = 0; i < _removed.Count; i++)
                    removed?.Invoke(_removed[i]);
            }
            finally
            {
                _placed.Clear();
                _removed.Clear();
            }
        }
    }
}

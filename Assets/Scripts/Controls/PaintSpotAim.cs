using System.Collections.Generic;
using Diciela.Data;
using Diciela.Simulation;
using Diciela.Simulation.Core;
using Diciela.Simulation.Land;
using Diciela.View;
using UnityEngine;

namespace Diciela.Controls
{
    /// <summary>
    /// Ce que vise un outil de sol au mode place de charrette (décisions 204, 216 et 220) : la place posée sous le
    /// pointeur, qu'un clic retirerait ; sinon celle qu'un clic poserait, avec ses copies par symétrie. Près de la place
    /// libre à côté d'une place posée, elle s'y aimante : au même cap, côte à côte, l'avant libre, à un écart qui ne leur
    /// laisse aucune maille commune. L'aimant coupé (Ctrl tenu), elle se pose où l'on vise, au cap de l'outil. La Place
    /// dit si elle se pose, par la même fonction que l'ordre.
    /// </summary>
    internal sealed class PaintSpotAim
    {
        private readonly World _world;
        private readonly PlacementSettings _settings;

        public PaintSpotAim(World world, PlacementSettings settings)
        {
            _world = world;
            _settings = settings;
        }

        /// <summary>La place posée sous le pointeur, qu'un clic retirerait ; null s'il n'y en a pas.</summary>
        public PlazaMark Hovered { get; private set; }

        /// <summary>Le milieu de la place qu'un clic poserait, aimantée ou non.</summary>
        public Vector2 Center { get; private set; }

        /// <summary>Le cap de la place qu'un clic poserait, en degrés.</summary>
        public float HeadingDegrees { get; private set; }

        /// <summary>La réponse de la Place à la place qu'un clic poserait ; Ok sur une place posée.</summary>
        public PaintAnswer Answer { get; private set; } = PaintAnswer.Ok;

        /// <summary>
        /// Vise ce point, l'outil tourné à ce cap, l'aimant en marche ou coupé ; met dans outlines les cadres à montrer :
        /// la place visée, ou celle qu'un clic poserait et ses copies.
        /// </summary>
        public void Aim(Vector2 aim, float headingDegrees, bool magnet, IReadOnlyList<GroundCopy> copies,
            List<PaintOutline> outlines)
        {
            PlazaZone plaza = _world.Plaza;
            outlines.Clear();
            Hovered = MarkAt(plaza, aim);
            if (Hovered != null)
            {
                Answer = PaintAnswer.Ok;
                outlines.Add(PaintOutline.Mark(Hovered.Footprint));
                return;
            }
            Center = aim;
            HeadingDegrees = headingDegrees;
            if (magnet)
                Snap(plaza, aim);
            Answer = PaintAnswer.Of(plaza.CheckMark(Center, HeadingDegrees));
            for (int i = 0; i < copies.Count; i++)
            {
                GroundCopy copy = copies[i];
                outlines.Add(PaintOutline.Mark(plaza.MarkFootprint(copy.Apply(Center), copy.HeadingOf(HeadingDegrees))));
            }
        }

        // L'aimant : la plus proche des places libres à côté d'une place posée, à droite ou à gauche, au même cap, si le
        // pointeur en est assez près.
        private void Snap(PlazaZone plaza, Vector2 aim)
        {
            float nearest = _settings.CartMagnetMeters;
            for (int i = 0; i < plaza.Marks.Count; i++)
            {
                RotatedRect footprint = plaza.Marks[i].Footprint;
                float spacing = footprint.Size.x + _settings.CartSpotGapMeters;
                Vector2 side = Heading.Direction(footprint.RotationDegrees + 90f) * spacing;
                for (int direction = -1; direction <= 1; direction += 2)
                {
                    Vector2 beside = footprint.Center + side * direction;
                    float distance = Vector2.Distance(aim, beside);
                    if (distance > nearest || MarkAt(plaza, beside) != null)
                        continue;
                    nearest = distance;
                    Center = beside;
                    HeadingDegrees = footprint.RotationDegrees;
                }
            }
        }

        // La place posée qui couvre ce point ; null s'il n'y en a pas.
        private static PlazaMark MarkAt(PlazaZone plaza, Vector2 point)
        {
            for (int i = 0; i < plaza.Marks.Count; i++)
            {
                if (plaza.Marks[i].Footprint.Overlaps(point, 0f))
                    return plaza.Marks[i];
            }
            return null;
        }
    }
}

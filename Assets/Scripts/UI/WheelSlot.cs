using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diciela.UI
{
    /// <summary>
    /// Une case d'une roue (décision 221) : sa place sur le plateau (0 en haut, puis dans le sens des aiguilles d'une
    /// montre, jusqu'à 7), son icône (une classe de la feuille de style) ou sa vignette, son pastel, sa touche, son nom et
    /// son usage, si elle est prise, et ses options, qu'elle déplie en éventail (aucune pour une case simple). Ce qu'elle
    /// fait quand on la prend, son présentateur le sait ; la vue ne fait que la montrer.
    /// </summary>
    public sealed class WheelSlot
    {
        private static readonly IReadOnlyList<WheelOption> NoOptions = Array.Empty<WheelOption>();

        public WheelSlot(int place, string icon, Texture2D picture, string pastel, string key, string name, string detail,
            bool isOn, IReadOnlyList<WheelOption> options = null)
        {
            Place = place;
            Icon = icon ?? "";
            Picture = picture;
            Pastel = pastel ?? "";
            Key = key ?? "";
            Name = name ?? "";
            Detail = detail ?? "";
            IsOn = isOn;
            Options = options ?? NoOptions;
        }

        public int Place { get; }
        public string Icon { get; }
        public Texture2D Picture { get; }
        public string Pastel { get; }
        public string Key { get; }
        public string Name { get; }
        public string Detail { get; }
        public bool IsOn { get; }
        public IReadOnlyList<WheelOption> Options { get; }
    }
}

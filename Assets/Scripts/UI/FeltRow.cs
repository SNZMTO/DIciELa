using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Diciela.UI
{
    /// <summary>
    /// Une rangée de feutres de la barre des outils de sol (décisions 206 et 216) : chacun une valeur, son visage (une
    /// icône ou une vignette), son nom et son usage au survol, sa touche s'il en a une ; celui qui est pris enfoncé. Un
    /// clic dit sa valeur. La barre en a une par rangée : ce que l'outil pose, ses modes, et ses options (PaintOptionsRow).
    /// </summary>
    internal sealed class FeltRow<T>
    {
        private static readonly string[] KeyClasses = { "creux", "pan-4", "touche" };
        private const string KeyTextClass = "touche-texte";
        private readonly VisualElement _row;
        private readonly Tooltips _tooltips;
        private readonly string[] _buttonClasses;
        private readonly List<T> _values = new List<T>();
        private readonly List<IDisposable> _tips = new List<IDisposable>();

        public FeltRow(VisualElement row, Tooltips tooltips, params string[] buttonClasses)
        {
            _row = row ?? throw new ArgumentNullException(nameof(row));
            _tooltips = tooltips ?? throw new ArgumentNullException(nameof(tooltips));
            _buttonClasses = buttonClasses;
        }

        /// <summary>Levé quand le joueur clique un feutre : sa valeur.</summary>
        public event Action<T> Chosen;

        /// <summary>Ajoute un feutre : sa valeur, son visage, son nom et son usage au survol, sa touche (vide : aucune).</summary>
        public void Add(T value, VisualElement face, string name, string detail, string key)
        {
            FeltButton button = Styled.With(new FeltButton(), _buttonClasses);
            button.Clicked += () => Chosen?.Invoke(value);
            face.pickingMode = PickingMode.Ignore;
            button.Add(face);
            if (!string.IsNullOrEmpty(key))
            {
                Facet hollow = Styled.With(new Facet { pickingMode = PickingMode.Ignore }, KeyClasses);
                hollow.Add(Styled.Text(key, KeyTextClass));
                button.Add(hollow);
            }
            (string, string) tip = (name, detail);
            _tips.Add(_tooltips.Attach(button, () => tip, true));
            _row.Add(button);
            _values.Add(value);
        }

        /// <summary>Enfonce le feutre de cette valeur, et lui seul.</summary>
        public void Select(T value)
        {
            for (int i = 0; i < _row.childCount; i++)
                _row[i].EnableInClassList(StyleClasses.Open, EqualityComparer<T>.Default.Equals(_values[i], value));
        }

        /// <summary>Retire tous les feutres et leurs infobulles.</summary>
        public void Clear()
        {
            foreach (IDisposable tip in _tips)
                tip.Dispose();
            _tips.Clear();
            _row.Clear();
            _values.Clear();
            _tooltips.Hide();
        }
    }
}

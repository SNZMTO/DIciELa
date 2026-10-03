using System;
using System.Collections.Generic;
using Diciela.Simulation.Economy;
using Diciela.Simulation.Land;
using UnityEngine;

namespace Diciela.View
{
    /// <summary>
    /// Les charrettes de l'étal, vues (GDD, section 11 ; décisions 197 et 198) : chacune le modèle du pack que la
    /// simulation lui a tiré, tel quel, l'avant du modèle à qui la tire. Rangée à son entrepôt, on ne la voit pas ;
    /// attelée, elle suit son charretier à la fraction du pas en cours (SAVOIR, 2.2) ; garée, elle glisse et se tourne
    /// jusqu'à sa place, sans à-coup, à l'allure que dit sa fiche ; garée, si sa Place est déplacée, elle la suit d'un
    /// coup. Elle épouse le relief : posée sur le sol sous son milieu, penchée comme lui sous ses roues.
    /// </summary>
    public sealed class CartViews : MonoBehaviour
    {
        private sealed class Shown
        {
            public Cart Cart;
            public Transform Root;
            // Le milieu du modèle et sa demi-taille au sol (largeur, longueur), dans le repère de la racine.
            public Vector3 Middle;
            public Vector2 HalfSize;
            public bool Seen;
            // Garée, et déjà vue garée : un changement la fait suivre sa Place, sans glisser.
            public bool Parked;
            public Vector2 At;
            public float Yaw;
            // La pose écrite en dernier : une charrette garée n'est pas réécrite à chaque image.
            public Vector2 PosedAt;
            public float PosedYaw;
        }

        private readonly List<Shown> _shown = new List<Shown>();
        private Carts _carts;
        private Relief _relief;

        /// <summary>Montre les charrettes de ce monde, puis suit leurs naissances.</summary>
        public void Show(Carts carts, Relief relief)
        {
            if (ReferenceEquals(carts, null))
                throw new ArgumentNullException(nameof(carts));
            if (ReferenceEquals(relief, null))
                throw new ArgumentNullException(nameof(relief));

            Hide();
            _carts = carts;
            _relief = relief;
            foreach (Cart cart in carts.All)
                OnCartAdded(cart);
            carts.CartAdded += OnCartAdded;
            carts.CartChanged += OnCartChanged;
        }

        /// <summary>Cesse de suivre le monde et retire ce qu'on montrait.</summary>
        public void Hide()
        {
            if (_carts != null)
            {
                _carts.CartAdded -= OnCartAdded;
                _carts.CartChanged -= OnCartChanged;
            }
            _carts = null;
            _relief = null;
            foreach (Shown shown in _shown)
                Destroy(shown.Root.gameObject);
            _shown.Clear();
        }

        /// <summary>
        /// Place chaque charrette qu'on voit à cette fraction du pas en cours (0 : où elle était au pas d'avant, 1 : au
        /// dernier pas) ; une charrette garée glisse vers sa place d'autant que le permettent ces secondes.
        /// </summary>
        public void Follow(float stepFraction, float deltaSeconds)
        {
            foreach (Shown shown in _shown)
                Place(shown, stepFraction, deltaSeconds);
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void OnCartAdded(Cart cart)
        {
            // Appelé pendant un pas de la simulation : une fiche mal réglée se dit, sans arrêter le monde.
            IReadOnlyList<GameObject> models = cart.Definition.Models;
            GameObject prefab = cart.ModelIndex < models.Count ? models[cart.ModelIndex] : null;
            if (prefab == null)
            {
                Debug.LogError("DICIELA : la charrette n° " + cart.Id + " n'a pas de modèle.", this);
                return;
            }

            var root = new GameObject("charrette n° " + cart.Id).transform;
            root.SetParent(transform, false);
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            GameObject model = Instantiate(prefab, root, false);
            Bounds box = BoxOf(model);
            // L'avant du modèle, son bout vers +z, à qui la tire : au bout du timon.
            float back = cart.Definition.PullMeters - box.max.z;
            model.transform.localPosition = new Vector3(0f, 0f, back);
            _shown.Add(new Shown
            {
                Cart = cart, Root = root, Middle = new Vector3(box.center.x, 0f, box.center.z + back),
                HalfSize = new Vector2(box.extents.x, box.extents.z)
            });
            root.gameObject.SetActive(false);
        }

        // Une charrette garée qui change encore : sa Place a été déplacée ; elle la suit d'un coup.
        private void OnCartChanged(Cart cart)
        {
            foreach (Shown shown in _shown)
            {
                if (ReferenceEquals(shown.Cart, cart) && shown.Parked && cart.State == CartState.Parked)
                    shown.Seen = false;
            }
        }

        // La boîte du modèle, dans le repère de la racine, posée à l'origine sans tourner.
        private static Bounds BoxOf(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);
            Bounds box = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                box.Encapsulate(renderers[i].bounds);
            return box;
        }

        private void Place(Shown shown, float stepFraction, float deltaSeconds)
        {
            Cart cart = shown.Cart;
            bool seen = cart.State != CartState.AtHome;
            if (shown.Root.gameObject.activeSelf != seen)
                shown.Root.gameObject.SetActive(seen);
            if (!seen)
            {
                shown.Seen = false;
                shown.Parked = false;
                return;
            }
            shown.Parked = cart.State == CartState.Parked;

            Vector2 target = Vector2.LerpUnclamped(cart.PreviousPosition, cart.Position, stepFraction);
            float yaw = Mathf.LerpAngle(cart.PreviousHeadingDegrees, cart.HeadingDegrees, stepFraction);
            if (!shown.Seen || cart.State == CartState.Pulled)
            {
                // Tirée, elle suit ; à peine sortie, elle paraît où elle est, sans glisser depuis l'entrepôt.
                shown.At = target;
                shown.Yaw = yaw;
            }
            else
            {
                shown.At = Vector2.MoveTowards(shown.At, target, cart.Definition.SettleMetersPerSecond * deltaSeconds);
                shown.Yaw = Mathf.MoveTowardsAngle(shown.Yaw, yaw, cart.Definition.SettleDegreesPerSecond * deltaSeconds);
            }
            if (shown.Seen && shown.At == shown.PosedAt && shown.Yaw == shown.PosedYaw)
                return;
            shown.Seen = true;
            Pose(shown);
        }

        // Sur le sol, penchée comme lui : sa hauteur sous son avant, son arrière et ses deux flancs donne sa pente.
        private void Pose(Shown shown)
        {
            Quaternion turn = Quaternion.Euler(0f, shown.Yaw, 0f);
            Vector3 forward = turn * Vector3.forward;
            Vector3 right = turn * Vector3.right;
            Vector3 middle = new Vector3(shown.At.x, 0f, shown.At.y) + turn * shown.Middle;
            Vector3 along = forward * (2f * shown.HalfSize.y);
            Vector3 across = right * (2f * shown.HalfSize.x);
            along.y = HeightAt(middle + along * 0.5f) - HeightAt(middle - along * 0.5f);
            across.y = HeightAt(middle + across * 0.5f) - HeightAt(middle - across * 0.5f);
            Quaternion pose = Quaternion.LookRotation(along, Vector3.Cross(along, across));
            middle.y = HeightAt(middle);
            shown.Root.SetPositionAndRotation(middle - pose * shown.Middle, pose);
            shown.PosedAt = shown.At;
            shown.PosedYaw = shown.Yaw;
        }

        private float HeightAt(Vector3 point)
        {
            return _relief.GroundAt(new Vector2(point.x, point.z)).y;
        }
    }
}

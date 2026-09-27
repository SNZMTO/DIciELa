---
paths:
  - "Assets/Scripts/**/*.cs"
---

# Écrire du C# dans DICIELA

- **Lire `Docs/ARCHITECTURE.md`** avant de créer une classe ou d'en déplacer une : chaque système a sa place et ses
  frontières ; ne pas en inventer une nouvelle en silence.
- **Une seule vérité par chose.** Avant d'ajouter un champ, une liste ou un cache, chercher qui tient déjà cette
  information ; un cache garde avec lui la version de ce dont il dépend.
- **La simulation en C# pur** (sans `MonoBehaviour` quand c'est possible), qui lève des événements ; les vues écoutent.
  `Update` reste léger : la simulation avance par pas du temps de jeu, pas image par image.
- **Un fichier = une classe, du même nom, une seule responsabilité.** Viser moins de 200 lignes ; au-delà, découper
  tout de suite, pas plus tard.
- **Conventions** : PascalCase pour les types, méthodes et propriétés publiques ; camelCase pour les variables locales ;
  `_camelCase` pour les champs privés. `[SerializeField] private` plutôt que `public` pour ce qui se règle dans
  l'Inspector. Commentaires en français. `null` géré explicitement.
- **Événements** : `event Action<…>` nommés au passé (`BuildingPlaced`, `DayEnded`) ; tout abonnement est défait dans
  `OnDestroy` (ou `OnDisable`, symétrique d'`OnEnable`).
- **Démarrage rapide du Play actif** : tout champ statique mutable a un `ResetStatics()` appelé depuis
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- **Jamais** : `GameObject.Find`, `SendMessage`, `FindObjectOfType` en jeu (les références passent par l'Inspector ou
  les services) ; un nombre de réglage écrit en dur (il va dans une donnée) ; une chaîne visible du joueur écrite en dur
  (elle passe par une clé de `strings.csv`) ; une allocation (liste, LINQ, lambda) dans une boucle par image ou par tick.
- **Aucune méthode publique « pour les tests »** : un test passe par le même appel que le jeu.
- **Seule la simulation change la simulation** : une méthode qui modifie l'état d'un système est `internal`, ouverte au
  monde par `InternalsVisibleTo` (jamais aux tests) ; au-dessus, on lit, on écoute et on envoie des ordres
  (ARCHITECTURE, section 5).
- **Déplacer et tourner** sont des opérations à part entière, jamais « retirer puis reposer ».
- **Chaque tâche d'un acteur a une issue d'échec** (destination perdue, chemin introuvable, délai dépassé).
- **Toute boucle qui avance par pas a une borne.**
- Les scripts d'éditeur vont dans `Assets/Scripts/Editor/` (namespace `Diciela.EditorTools`) ; un montage se fait par
  un `[MenuItem("DICIELA/…")]` réexécutable.
- **Les dépendances ne vont que dans un sens**, fixé par les assemblages (`.asmdef`) de `ARCHITECTURE.md` : la
  simulation ne connaît ni la vue ni l'interface. Ajouter une référence entre assemblages est un changement
  d'architecture : le proposer, jamais le faire en passant.
- **Généraliser au deuxième cas**, pas avant : on écrit le cas simple, et au deuxième cas semblable on en fait un
  système. Exceptions connues d'avance : les modificateurs et les postes et tâches, génériques dès le départ.

## Signaux d'alerte : s'arrêter et le dire au développeur

- un cas particulier par nom (« si c'est l'Architecte, alors… ») au lieu d'une propriété dans la fiche ;
- deux fonctions aux noms presque pareils, ou deux façons de faire la même chose ;
- un commentaire « gardé pour… », « temporaire », « pour les tests » ;
- un booléen ajouté pour contourner une règle (`ignorerLaRègle`, `modeSpécial`) ;
- une correction « à côté » qui n'est pas dans l'étape ;
- une donnée copiée « pour aller plus vite » au lieu d'être lue chez son propriétaire ;
- un fichier qui dépasse 200 lignes ou qui fait deux métiers ;
- un code difficile à tester par le geste du joueur : c'est qu'il est emmêlé ; on le démêle, on n'ajoute pas de porte
  dérobée.

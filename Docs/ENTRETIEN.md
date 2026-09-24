# DICIELA — l'entretien fondateur

> Les réponses du développeur, confirmées une à une (23 septembre 2026). Elles servent à écrire `GDD.md` en un seul
> passage ; une fois le GDD écrit et validé, ce fichier est supprimé.

## 1. Le cœur du jeu

**Le genre.** Un Satisfactory fantasy, cozy : des chaînes de production tenues par de petits habitants, chacun à la
tâche qu'on lui donne. Un jeu de déco aussi : on pose librement, selon les contraintes du terrain.

**Le pitch (proposé, à relire).** Un village fantastique où chaque habitant a un métier : on bâtit des chaînes de
production qu'ils font tourner, on troque avec les visiteurs, et la Chimère, qui touche à tout, devient peu à peu la
meilleure ouvrière du pays… ou une redoutable capitaliste quand elle part en run.

**Les piliers (proposés, à relire).** (1) Des gens, pas des machines. (2) Le plan est le puzzle. (3) Beau et utile.
(4) On découvre en faisant. (5) Rien n'est perdu pour de bon. (6) Léger, jamais cynique. (7) Lisible avant d'être riche.

**Le ton.** Fantastique léger, décalé, humoristique (Animal Crossing, une pointe de Dungeon Crawler Carl). L'humour
vient des personnages ; même le capitalisme de la Chimère est une blague.

**La population est la boucle.**
- Les **maisons** donnent la capacité de la ville.
- Les **bâtiments de production** ouvrent des postes ; ce sont les **emplois qui attirent** les gens.
- La **nourriture** : chaque ferme produit, en plus de sa récolte et sans rien lui prendre, une réserve cachée de
  nourriture pour les habitants ; elle se lit comme une statistique visible de la ville (« de quoi nourrir 42
  habitants ») et c'est une condition d'arrivée.
- Grandir = logements + emplois + nourriture. Si la nourriture tombe sous la population, **personne ne part** : les
  arrivées s'arrêtent, rien d'irrécupérable.
- **Un bâtiment sans habitant à son poste est à l'arrêt** (une ferme sans fermier ne produit ni récolte ni nourriture).

**La Chimère.** Créée au lancement d'une partie. Le joueur la place à un poste, comme un habitant, et elle y va :
ouvrière, elle construit sur les chantiers ; à la ferme, elle travaille à la ferme. Elle travaille dans tous les
domaines et progresse lentement, mais plus haut que les habitants ordinaires. Selon où elle travaille, elle débloque
des répliques, des tenues et des idées.

**Le début de partie.** La Chimère commence seule, dans une tente de fortune, et construit les premiers bâtiments. Très
vite, le tutoriel fait venir des habitants un peu doués dans chaque discipline, pour lancer le joueur.

**La progression : les idées, façon Animal Crossing.**
- À force de faire des choses et de voir la ville avancer, la Chimère ou des habitants clés ont des **idées** : des
  plans de bâtiments, de recettes, de décos.
- Une idée vient d'une chose faite (« après 50 Planches, le menuisier imagine la table ») ou du métier exercé (la
  Chimère a travaillé à la scierie : elle imagine le moulin à eau).
- Un **carnet d'idées** montre des pistes floues (« le menuisier rêve de quelque chose avec du tissu… ») : le joueur
  sait vers quoi pousser, sans liste de courses.
- Des **grands projets**, rares, donnent un but lointain ; une idée les débloque. Le premier : les habitants
  construisent en secret sa belle maison à la Chimère, une surprise.

## 2. Le troc et l'étal

- **Aucune monnaie en ville.** Tout se paie en marchandises.
- **Les visiteurs de l'extérieur** troquent tout à l'étal : la ville y échange son surplus contre ce qu'elle ne produit
  pas. Chaque marchandise a un **taux de base** ; le joueur peut le bouger un peu ; s'écarter du taux juste donne des
  bonus ou des malus, par la **réputation** de la ville (une seule pour la ville). La réputation bouge peu à peu : rien
  n'est instantané ni irrécupérable ; radin, moins de visiteurs et moins de rares ; généreux, l'inverse.
- Le **marchand**, un habitant au poste d'étal, lit sa statistique d'Échange.
- **Les habitants** viennent se servir à l'étal de ce qui est exposé pour eux (tout sauf la nourriture) : chaque chose
  obtenue les rend plus heureux ; ne rien obtenir n'a pas de conséquence grave, seulement pas de bonus. L'étal est donc
  le lieu où la ville choisit comment partager : entre ses habitants (le bonheur) et les visiteurs (le troc).
- **Pour le lore seulement**, les habitants troquent entre eux, fabriquent de petites choses chez eux et ont des
  possessions tirées au hasard, comme dans Animal Crossing ; aucune économie n'est simulée derrière.
- **Le charme** attire plus de visiteurs et des visiteurs plus rares, et fera sans doute d'autres choses plus tard.

## 3. Les chaînes, les bâtiments, le monde

- **Un bâtiment a un nombre fixe de postes** ; sa cadence dépend des sous-statistiques de ceux qui y travaillent ;
  l'améliorer ajoute des postes.
- **La logistique** est automatique par défaut (un stock commun, les charretiers vont chercher et livrer d'eux-mêmes) ;
  le joueur peut tirer des **liaisons prioritaires** entre deux bâtiments pour optimiser.
- **En ville, rien ne limite la pose, sauf l'eau.** Les ressources de base (bois, pierre, terre à cultiver) ne sont pas
  liées à un gisement local.
- **La carte du monde** : vierge au départ, elle se remplit peu à peu, automatiquement, de ce que racontent les
  visiteurs : des lieux, des gisements. Les ressources étrangères (le métal…) n'existent que sur cette carte ; certaines
  idées en ont besoin.
- **Les expéditions** partent vers un gisement de la carte : une caravane avec des marchandises et des habitants ; plus
  il est loin, plus elle met de temps ; plus il est gros, plus elle rapporte. Rien n'est jamais perdu (au pire un retard
  ou moins de ressources) ; le vrai coût, ce sont les habitants absents de leur poste.

## 4. L'eau

- **Des bâtiments sur l'eau** : un moulin se pose sur une rivière ou un canal, sa roue fait partie de lui (moulin à
  farine, scierie hydraulique, moulin à foulon pour le tissu, martinet pour la forge). Pour construire loin de la
  rivière, on creuse un **canal** (des ouvriers, sous-statistique terrassement).
- **Un réseau de force**, comme l'électricité de Satisfactory : une roue à eau produit de la force (plus sur une rivière
  rapide, moins sur un canal) ; des arbres de transmission, posés comme une route courte, la portent ; les bâtiments
  hydrauliques la consomment, à l'arrêt ou au ralenti sans elle. Le joueur gère un budget de force. **Les roues à eau
  n'ont besoin de personne.**
- **L'eau comme ressource** : seaux et tonneaux pour arroser et brasser ; l'irrigation fait mieux pousser les champs
  proches d'un canal (un rayon d'effet).

## 5. Les chantiers, les routes, le temps

- **Tout se construit par des ouvriers.** Le joueur pose un fantôme ; les charretiers apportent les matériaux ; les
  ouvriers construisent ; un ouvrier qualifié bâtit plus vite. Une **file de chantiers visible**, dont le joueur change
  l'ordre ; annuler rend au stock les matériaux déjà livrés.
- **Les routes** : un chemin de terre est gratuit ; une route pavée demande des **Pavés**, fabriqués par l'atelier de
  pierre. Les piétons marchent partout ; les charrettes vont lentement dans l'herbe, plus vite sur la terre, encore plus
  vite sur les pavés. Une route est un investissement, jamais une obligation.
- **Ranger ou déplacer** un bâtiment : un tout petit chantier, pour la cohérence : il se déconstruit en quelques
  secondes et se reconstruit de même, sans nouveaux matériaux.
- **Les bâtiments uniques se rangent** aussi, et le jeu dit ce qui s'arrête tant qu'ils ne sont pas reposés : une
  pastille sur le bouton de la réserve et une ligne dans la réserve (la Place : plus de visiteurs). Pour la maison de la
  Chimère, une bulle avec son portrait : « Je vais quand même pas dormir dehors… ».
- **Le jour et la nuit** : les habitants travaillent le jour et rentrent dormir chez eux ; les lanternes permettent de
  travailler plus tard le soir.
- **Pas de temps hors du jeu** : la ville ne tourne que quand le jeu est ouvert.
- **Pose libre** : aucune case pour le joueur, on dit des mètres ou rien.

## 6. Les habitants, les maisons, la déco

**Quatre statistiques**, qui regroupent ce que le travail touche ; chacune a une sous-statistique par discipline :

| Statistique | Ce qu'elle regroupe | Sous-statistiques |
|---|---|---|
| **Terre** | ce qui pousse ou se prend dans la nature | culture (fermes), coupe (bûcheron), extraction (carrière), paysage (planter) |
| **Bâtisse** | ce qui se construit sur place | construction (chantiers, routes), terrassement (canaux), aménagement (installer les décos) |
| **Artisanat** | ce qui se fabrique en atelier | une par famille d'atelier : bois, fil, pierre ; plus tard métal, cuisine |
| **Échange** | ce qui circule | transport (charretiers), troc (étal), expédition (caravanes) |

- Un poste lit sa sous-statistique ; c'est elle qui progresse en travaillant ; la statistique globale monte aussi, plus
  lentement. La progression est rapide au début et de plus en plus lente, sur une seule courbe (le tutoriel n'a pas de
  règle à part). Changer de poste ne fait rien perdre.
- La plupart des habitants sont équilibrés, avec un léger point fort : on les regroupe par spécialité. Des profils
  existent (les marchands, meilleurs à l'Échange). Chaque habitant ordinaire a un plafond, plus haut dans son point fort.
- **Les maîtres** : quand la ville s'améliore, des personnages à personnalité arrivent par un tirage régulier dont les
  chances montent avec la qualité de la ville (production, maisons, charme, réputation). Chacun appartient à une
  statistique, a une capacité unique et de meilleures statistiques, progresse aussi, et **ne travaille que dans son
  domaine**. Une poignée au début, énormément plus tard.
- **Les maisons** ont quelques niveaux (cabane, maison, belle maison) : une meilleure maison loge plus de monde, rend
  plus heureux et attire les maîtres ; elle s'améliore sur place, par un petit chantier.
- **Le bonheur** d'un habitant vient de sa maison, de ce qu'il obtient à l'étal et du charme autour de chez lui ; il fait
  mieux travailler et progresser. Il est visible sur sa fiche ; bas, il retire des bonus, rien de plus.
- **La déco a un rôle mécanique** : toutes les décos donnent du charme ; certaines ont un rayon d'effet (la lanterne
  éclaire le travail du soir). Le joueur pose le fantôme d'un banc ou d'une lanterne et un **ouvrier de la commune**
  vient l'installer avec les matériaux ; arbres et buissons se posent une fois la **cabane du paysagiste** construite,
  et les **paysagistes** plantent.

## 7. La run (cadre seulement : elle vient après le prototype de la simulation)

- Elle se joue **sur la place de la ville**, sous un autre angle de caméra (peut-être en zoomant : à voir).
- Au tour par tour, avec une main de cartes, des dés et des contrats, sur 20 à 30 minutes.
- **Le Fric n'existe que dans la run**, pour ses parties et ses gags ; jamais en ville, jamais rapporté.
- **Ce qu'elle rapporte à la ville** : des décos rares, des idées (des plans vus ailleurs), des lieux et des gisements
  révélés sur la carte du monde, parfois un maître rencontré. Rien qui court-circuite la production.
- **Ce que la ville lui donne** : de très légers bonus, trois fois rien.
- **La ville continue de tourner pendant la run** : la run aide à faire passer le temps.
- La Chimère revient de run avec des répliques de magnat (« Le troc ? Quelle barbarie… ») et s'en remet peu à peu en
  retravaillant en ville.

## 8. L'art

- **Tout vient du pack** POLYGON Fantasy Village (Synty), rien de fait maison ; la caméra, la lumière et le rendu
  s'adaptent au pack. Le but : un jeu aussi beau que la démo du pack.
- **Caméra en perspective**, comme la démo (champ de vision autour de 40°). Caméra de gestion à main invisible : se
  déplacer (clavier, bords de l'écran, clic glissé), tourner (Q/E), zoomer à la molette ; en zoomant, elle s'abaisse
  doucement (de loin, la ville en maquette ; de près, les habitants presque de face).
- **Le rendu du pack tel quel** : son Terrain peint, son eau, ses shaders, son profil de filtres, sa lumière.
- **Habitants ordinaires** : les humains du pack. **Maîtres** : les animaux humanoïdes du pack, distingués par leurs
  accessoires, leurs couleurs et leurs noms. À terme, un artiste payé fera de plus jolis personnages.
- **La Chimère** se crée au lancement d'une partie : un corps d'animal et sa couleur, puis des morceaux d'autres animaux
  (queue, oreilles, museau) et des accessoires (chapeau, cape…). Les corps du pack sont d'un seul tenant : pas de tête
  d'une bête sur le corps d'une autre avant l'artiste.
- **Les animations** ne sont pas dans le pack : un pack d'animations est à acheter, par le développeur.

## 9. Le premier jouable

- La Chimère (création, tente, tutoriel) ; les premiers habitants et leurs statistiques.
- Ferme, bûcheron, carrière, scierie, atelier de tissu, atelier de pierre (Pavés et créations en pierre) ; les
  paysagistes et leur cabane.
- Maisons et nourriture ; chantiers, routes de terre et de pavés ; l'étal, le troc et la réputation ; le carnet d'idées ;
  le premier grand projet (la maison de la Chimère).
- L'eau, un morceau de chacun de ses trois usages : le canal ; une roue à eau, des arbres de transmission et la
  scierie hydraulique ; l'irrigation des fermes proches d'un canal.
- Un seul maître, pour essayer.
- **Pas dans le premier jouable** : la run, les expéditions et la carte du monde, les seaux et le brassage.

## 10. La méthode

- **Une seule vérité par chose** ; la simulation en C# pur, à pas fixe, écoutée par la vue ; un système de statistiques
  à modificateurs ; un système de postes et de tâches avec un répartiteur unique ; tout le contenu en fiches ; des tests
  qui passent par les gestes du joueur ; des fichiers courts à une seule responsabilité. Une architecture pensée comme
  par un développeur rigoureux qui la maintiendra des années : logique, lisible, sans « spaghettis ».
- Des documents courts, réécrits et jamais raturés ; remplacer = supprimer le jour même ; aucun agent sans l'accord du
  développeur ; une branche par étape, un commit par étape qui marche, la vérification au CLI.

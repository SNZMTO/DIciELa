# DICIELA — Savoir technique

> Les solutions éprouvées et les pièges connus. Ce document n'est pas chargé à chaque session : on le lit quand on
> conçoit ou qu'on code le système concerné. `ARCHITECTURE.md` dit comment le jeu est bâti ; ce document dit ce qui
> marche et ce qui casse.

## 0. Les leçons du premier prototype

Un premier prototype a été construit en 31 sessions, puis abandonné pour repartir d'ici. Le code n'était pas mauvais ;
c'est la **façon d'avancer** qui l'a emmêlé. Un audit complet a trouvé environ 30 bugs et plus de 200 choses bêtes ou de
dette, presque toutes dues aux mêmes causes. Les voici, avec la parade à tenir.

| Ce qui s'est passé | Pourquoi ça a emmêlé | La parade, ici |
|---|---|---|
| Le design a changé plusieurs fois en route (grille de cases → pose libre → routes au pinceau ; monnaie → troc) et chaque fois la **nouvelle version a été ajoutée à côté de l'ancienne**. | Deux vérités par chose ; chaque système lisait l'une ou l'autre ; les tests suivaient l'ancienne. | Un changement de design **remplace** : on supprime l'ancien le jour même, code, données, tests et documents. |
| « On garde le code pour y revenir » (des fonctions retirées du jeu : une rivière, une mer, des montagnes, quatre entrées de place). | Des milliers de lignes mortes que chaque session devait lire et contourner ; des tests ignorés pendant des semaines. | Ce qui sort du jeu sort du code. Git garde l'historique ; c'est son rôle. |
| Le GDD et le fichier d'instructions ont été **amendés** au lieu d'être réécrits : ratures, « réécrit en partie », décisions qui en contredisaient d'autres sans renvoi ; le document a atteint 270 Ko, l'état du journal 380 lignes. | Plus personne, Claude compris, ne savait ce qui était vrai aujourd'hui. | Un document dit le présent et se réécrit ; l'histoire vit dans `DECISIONS.md` et dans Git ; `ETAT.md` tient en 30 lignes, `CLAUDE.md` sous 200. |
| Des **API gardées « pour les tests »** (poser une route case par case alors que le joueur peint) ; des tests qui fournissaient eux-mêmes le résultat attendu. | Des tests verts pendant que le jeu, lui, avait changé ou cassé. | Un test passe par le geste du joueur ; aucune méthode n'existe pour les tests seuls. |
| Chaque effet (charme, lanterne, bonus) **codé à la main** là où il servait ; des règles de pose, de gomme, de visée et de sauvegarde écrites séparément. | Des règles qui divergeaient entre systèmes (on pouvait poser ce que la gomme ne savait pas retirer). | Un seul système de modificateurs ; une seule fonction par règle, appelée par tous. |
| Des sessions qui enchaînaient une idée nouvelle avant d'avoir fini et nettoyé la précédente. | Des fonctions à moitié faites, empilées. | Une étape à la fois ; une étape finie, c'est aussi ce qu'elle remplace supprimé. |
| Des **workflows d'agents** lancés sur une estimation fausse (plus de 10 millions de jetons au lieu de 1 à 2). | La limite de session du développeur atteinte, deux fois. | Aucun agent sans son accord pour ce lancement, avec un plafond chiffré et tenu. |
| Des fichiers qui grossissaient par couches (un service de grille de 1 700 lignes à sept métiers). | Impossible à lire d'un coup, et chaque retouche en cassait un autre bout. | Un fichier, une responsabilité ; un fichier qui grossit est découpé tout de suite, pas « plus tard ». |

## 1. Règles de conception : chaque bug typique a sa règle

| Bug typique | La règle qui l'empêche |
|---|---|
| Une même chose représentée deux fois (une route en cases **et** en tracé ; un objet retrouvé par sa clé **et** par sa position) : les deux divergent, et chaque système lit l'une ou l'autre. | **Une seule vérité par chose.** Toute autre forme est déduite d'elle, recalculée, jamais tenue à la main. |
| Deux repères de coordonnées décalés d'une demi-unité, et des conversions refaites à la main à plusieurs endroits. | **Un seul repère** (positions en mètres, flottants, sur le plan XZ) et **une seule fonction** par conversion. |
| Tourner ou déplacer un objet en le retirant puis en le reposant : les écouteurs de « retiré » effacent ce qu'il portait (sa porte, ses liens), et un refus laisse l'objet abîmé. | **Déplacer et tourner sont des opérations à part entière**, avec leur propre événement, vérifiées **avant** d'agir, jamais « retirer puis reposer ». |
| Une charrette ou un ouvrier bloqué pour toujours parce que sa destination a disparu ou n'a pas d'accès. | **Chaque tâche a une issue d'échec** : destination perdue, chemin introuvable, délai dépassé → l'acteur rend ce qu'il porte et redevient libre. |
| Une affectation jamais revue quand le réseau change (un dépôt coupé de sa destination). | **Les affectations se revoient à chaque changement de topologie** (pose, retrait, route coupée), par un événement. |
| Un panneau d'interface figé sur les valeurs du moment où on l'a ouvert. | **Une vue écoute les changements** de ce qu'elle montre ; elle ne photographie jamais. |
| Un prix affiché différent du prix payé (un paramètre oublié à l'affichage). | **Ce qui s'affiche et ce qui se calcule passent par la même fonction.** |
| Une partie illisible au chargement, remplacée par une partie vide que l'enregistrement automatique écrit par-dessus. | **On n'écrit jamais par-dessus un fichier qu'on n'a pas su lire** : il est mis de côté, daté, avant toute écriture. |
| Une boucle sur de la géométrie qui ne se termine pas pour une entrée oblique : l'éditeur fige. | **Toute boucle qui avance par pas a une borne** (un nombre maximal de pas), et un test la franchit. |
| Une scène de test branchée par erreur sur la vraie partie. | **La scène de test ne peut pas atteindre la vraie partie par construction** (son fichier de partie est fixé par la scène, pas par un réglage qu'on oublie). |
| Un test qui réécrit les données du projet (il lance un monteur d'assets). | **Un test n'écrit jamais dans `Assets/`.** Il crée ses données en mémoire. |
| Des tests écrits contre des méthodes créées pour eux, qui restent verts pendant que le jeu a changé. | **Un test passe par le chemin du joueur** (le même appel que le geste en jeu) ; aucune méthode publique n'existe « pour les tests ». |
| Une fonction remplacée mais gardée « pour y revenir », qui continue d'être appelée par des coins oubliés. | **Remplacer = supprimer le jour même**, code, données et tests compris. |
| Un effet codé à la main là où il sert (le charme dans l'étal, une lanterne dans la marche, un bonus dans l'atelier). | **Un seul système de modificateurs** : chaque effet est une donnée, chaque système demande son bonus à un seul endroit. |
| Un calcul refait à chaque image ou à chaque tick alors que ses entrées n'ont pas changé. | **On recalcule sur événement**, ou on garde le résultat avec la version de ses entrées. |

## 2. Solutions qui ont fait leurs preuves

### 2.1 La circulation : un maillage invisible

- **Le maillage** : un quadrillage invisible, plus fin que le mètre (4 mailles par mètre), où tout ce qui se pose
  s'inscrit. Une maille porte au plus un objet : c'est la vérité pour « rien ne se chevauche » et pour le passage.
  C# pur. Une maille a un coût d'entrée par façon de se déplacer, en entiers, lu dans les fiches : un piéton paie le
  même prix partout où l'on passe ; une charrette paie plus cher l'herbe que la terre, et la terre que les pavés
  (GDD, section 9). Une maille peut être bloquée. Un objet posé s'y **trame** : son empreinte (un rectangle tourné) marque les mailles qu'elle couvre,
  avec un écart minimal entre deux objets.
- **Le A\*** : huit voisines ; un pas droit coûte 10, un pas en diagonale 14, multipliés par le coût de la maille où
  l'on entre. **Une diagonale ne coupe jamais un coin** : elle n'est permise que si les deux mailles droites qu'elle
  longe sont franchissables (on ne passe pas entre deux bancs qui se touchent par l'angle). Plusieurs départs,
  plusieurs arrivées. **Le test d'arrivée se fait à la sortie de la file, jamais à la découverte** : à coûts pondérés,
  la première arrivée vue n'est pas la moins chère. Heuristique : la distance octile à la boîte des arrivées (jamais
  surestimée, cohérente). Une instance par maillage, tampons gardés d'un appel à l'autre, un tas binaire pour la file
  ouverte ; une entrée périmée (sa maille rejointe moins cher depuis) se saute à la sortie.
- **À total égal, le plus avancé d'abord** (le coût payé le plus grand) : sans ce départage, sur l'herbe libre, tous
  les trajets de même coût forment une large bande (des centaines de milliers de mailles sur la carte) que la
  recherche fouille au lieu de filer droit. Il rend aussi le même trajet à chaque fois.
- **Un numéro de recherche** dit quelles cases des tableaux sont à jour : on ne vide jamais les 4 millions de mailles
  de la carte entre deux recherches (environ 40 Mo de tableaux, faits une fois par monde).
- **Le vérifier** : un test lance le A* sur **400 maillages tirés au hasard** contre un Dijkstra sans heuristique ni
  tas ; les coûts doivent être égaux, et chaque trajet est jugé pas à pas. Les cas écrits à la main se dessinent en
  texte, le nord en haut (`TestGround`), et chaque obstacle y est posé par l'ordre « poser ».
- **Le lissage** : le A* rend un escalier de mailles ; on le tend comme une ficelle. Deux points se relient en ligne
  droite si toutes les mailles que la ligne touche (coins compris) sont franchissables **au même coût** que le bout de
  chemin remplacé : une charrette reste sur sa route.
- **Savoir tout de suite qu'un chemin est introuvable** : une étiquette de composante connexe par façon de se déplacer,
  recalculée à chaque changement de topologie. Sans elle, un A* vers une cible coupée parcourt toute la carte.
- **La marche** : un chemin est une polyligne ; l'avancement se compte en distance. L'allure se relit **à chaque mètre
  parcouru** et à chaque sommet, pas seulement aux sommets : une lanterne au milieu d'un long segment doit compter.
  Sans allocation par tick.
- **Les distances par la route** (qui est le plus proche d'un dépôt) : un Dijkstra gardé tant que le maillage ne change
  pas (un numéro de version du maillage, incrémenté à chaque pose ou retrait), quelques jeux de départs gardés à la
  fois ; sinon chaque charrette relance le calcul à chaque tick.

### 2.2 La simulation et sa vue

- **La simulation avance par pas fixes** (un dixième de seconde), en C# pur, et ne sait pas qu'elle est dessinée.
- **La vue interpole** entre les deux derniers états de la simulation (la fraction du pas en cours) : sans ça, tout ce
  qui bouge saute dix fois par seconde et tourne par à-coups. L'image a un pas de retard, invisible. C'est la même idée
  que l'interpolation d'un jeu en ligne entre deux messages du serveur.
- **Les virages** d'un véhicule : arrondir la polyligne en arcs à la vue seulement ; la simulation garde le chemin exact.

### 2.3 La sauvegarde

- **Du JSON, écrit de façon atomique** : on écrit un fichier temporaire, puis on l'échange contre l'ancien en un seul
  geste (`File.Replace`), l'ancien devenant le cran d'avant (`.bak`). Une coupure en pleine écriture ne perd jamais la
  partie.
- **Rien ne lance d'exception** au chargement : fichier absent, tronqué, écrit par une version future ou modifié à la
  main → « pas de partie à charger », jamais un écran noir.
- **Un numéro de version** dans le fichier ; les champs neufs sont **additifs** (absents d'un vieux fichier = valeur par
  défaut sûre, ou déduite quand le GDD le dit : une sous-statistique ajoutée reçoit la statistique globale calculée sur
  les autres) ; une conversion par version, faite sur le JSON lui-même, testée sur un vrai vieux fichier.
- **Un compte rendu de chargement** : un bâtiment qui ne tient plus à sa place part en réserve avec tout ce qu'il
  portait, un tracé qui ne tient plus s'efface en rendant ses matériaux, rien ne se perd ; ce qu'on ne sait pas relire (un identifiant inconnu) est compté. Si le compte rendu n'est pas parfait, le
  fichier est mis de côté, daté, avant d'être écrasé.
- **Les tests de sauvegarde** écrivent dans un dossier temporaire, choisi **avant** de charger la scène.
- **L'aller-retour** : sauvegarder, recharger, resauvegarder, comparer les deux fichiers sans tenir compte de l'ordre des
  listes. Un script le fait en une commande.

### 2.4 Le hasard déterministe

- **Le hasard en entiers** : un hachage (graine, position ou numéro) → un nombre qui a l'air tiré au hasard mais qui est
  toujours le même. Jamais `UnityEngine.Random`, `System.Random` ni `Mathf.PerlinNoise` : une mise à jour ou une autre
  machine pourrait les faire bouger d'un bit.
- **Ce qu'on tire pour un monde** (les plantes de ses champs sauvages, la pose de chaque élément semé : style,
  rotation, taille, décalage) se tire du hachage, puis **se sauvegarde** : une retouche du tirage ne change pas une
  partie en cours.

### 2.5 Le sol peint

- **Piège : un TerrainData reste binaire, même en Force Text**, et pèse des Mo à chaque version. La règle `*.asset`
  du `.gitattributes` (fins de ligne normalisées) l'abîmerait : `Assets/Terrain/*.asset` a sa propre règle `lfs`,
  placée **après** `*.asset` (la dernière règle l'emporte). Un fichier d'essai a montré que LFS le garde à l'octet
  près. Ses calques (TerrainLayer) sont ceux du pack.
- **Piège : modifier un Terrain en Play modifie l'asset pour de bon** (`SetHeights`, `SetAlphamaps`, arbres), et il
  s'écrit au prochain enregistrement du projet. Pour modifier le sol, le jeu prend une copie en mémoire
  (`Object.Instantiate(terrainData)`), donnée au `Terrain` **et** au `TerrainCollider`, jamais enregistrée ; un test
  PlayMode (deux parties de suite) vérifie que les hauteurs de l'asset n'ont pas bougé.
- **Un Terrain range ses hauteurs par marches de 1 / 32 766 de sa hauteur totale** (3 mm pour 100 m) : une pente lue
  sur une maille de 1 m bouge de quelques centièmes de degré. Un test sur un Terrain tolère cet écart ; la
  simulation lit le relief une fois, en mètres, au lancement.
- **On repeint par lot** : une route ou un coup de pinceau marque un rectangle sale, et le sol ne se repeint qu'à l'image
  suivante, sur ce seul rectangle, une seule fois même si dix changements arrivent dans la même image.
- **Les touffes et petites fleurs** sont des objets combinés en lots statiques ; des milliers d'objets séparés font
  ramer.

### 2.6 Les pinceaux (routes, chemins, peinture du sol)

- **Un coup de pinceau** = une sorte, une demi-largeur, une polyligne. Main levée : un point tous les quelques
  décimètres ; courbe (touche tenue) : une courbe de Bézier.
- **On ne garde jamais le geste brut**, seulement ce qui a été posé, découpé sur ce qui l'arrête (un bâtiment, l'eau) :
  sinon, retirer un bâtiment ferait réapparaître au chargement la route qui ne passait pas dessous.
- **La peinture décorative est une pile ordonnée** : le dernier coup l'emporte ; un coup neuf découpe dans les anciens
  ce qu'il recouvre entièrement ; un coup qui n'ajoute rien ne s'ajoute pas ; une gomme ne se garde que là où elle
  efface encore. Gribouiller au même endroit ne fait pas grossir la sauvegarde.
- **L'index est par segment**, pas par coup : le sol interroge le champ des centaines de milliers de fois.
- **La gomme efface au fil du geste**, morceau par morceau, jamais tout le trait à chaque image.

### 2.7 La visée

- **Un seul rayon** décide de ce que vise le curseur : la chose la plus proche de la caméra, quelle qu'elle soit.
- **D'abord la boîte, puis les triangles du modèle** : la boîte d'une chose haute (un arbre) passe devant ce qui est
  derrière elle. On teste les triangles du maillage (lus une fois, gardés). La boîte de rendu (`Renderer.bounds`) d'un
  objet rond déborde : elle ne sert qu'au tri rapide.
- **Lire les triangles demande « Read/Write » à l'import du maillage** ; sans lui, dans un jeu compilé, la visée retombe
  sur la boîte.
- **Un menu de diagnostic** vise le centre de triangles tirés sur chaque modèle et compte les fautes : c'est la preuve
  que la visée tient.
- **Viser le sol suit le relief** (`Relief.TryRaycast`) : on ne marche que dans la tranche du rayon entre le plus haut
  et le plus bas de la carte, par demi-mailles, puis on affine par dichotomie ; la boucle est bornée. **Tirer le sol
  avec la caméra** prend un plan à la hauteur du point visé : sur une colline, le relief ferait sauter la prise
  pendant le glisser. Deux questions, deux réponses, chacune nommée pour son usage.

### 2.8 Les gestes à la souris

- **Un bouton, un rôle, décidé au relâchement** : un seuil en pixels sépare le clic du glisser, pour les trois boutons.
  Un clic décidé à l'enfoncement fait des erreurs (on rase un bâtiment en voulant glisser).
- **Échap et le clic droit bref reculent d'un cran** : une pile de ce qui est ouvert (panneau, outil, sélection).
- **Un panneau d'interface ouvert rend la ville muette** : aucun clic ne la traverse.
- **Tourner** : une touche brève = un pas ; la touche tenue en bougeant la souris = au degré ; une touche de
  modification aimante.
- **Le curseur est tenu pendant qu'on tourne à la souris** (bouton du milieu, R tenu) : caché et bloqué, on lit son
  déplacement (`<Pointer>/delta`), qui ne bute jamais sur le bord de l'écran, et il revient où il était au
  relâchement (`CursorGrip`). À l'image du relâchement, le curseur rendu n'est pas encore à sa place : on n'y vise pas.
- **La position du pointeur est une action « PassThrough »** : une action « Value » garde l'appareil qui l'a prise
  (un stylet, un écran tactile) et peut y rester collée ; « PassThrough » lit le dernier qui a bougé.
- **La visée passe après la caméra** (`LateUpdate`, ordre d'exécution 100) : sinon l'aperçu se calcule avec la vue
  de l'image d'avant, et il traîne d'une image dès que la vue bouge.
- **Tirer le sol, c'est garder sous le pointeur le point saisi** : un rayon depuis la caméra le trouve sur le relief
  à l'appui, puis le garde sur le plan de sa hauteur (`GroundGrip`) ; à chaque image la vue se déplace pour qu'il y
  revienne. Des pixels convertis en mètres
  glissent : le sol incliné fuit, trois fois plus vite que le pointeur à 20° sous l'horizon. Un rayon qui ne touche
  pas le sol (au-dessus de l'horizon, au-delà de l'affichage) ne saisit rien. Le test compare à la projection
  inverse (`WorldToScreenPoint`).
- **Zoomer vers le pointeur, c'est la même prise** : le sol visé au cran de molette reste sous le pointeur pendant
  tout le lissage du zoom, jusqu'à ce qu'un autre geste (déplacer, tourner) prenne la main ; un cran donné pendant
  qu'on tire le sol suit la prise du glisser.
- **L'inclinaison suit le logarithme de la distance** : un cran multiplie la distance, donc une inclinaison en
  proportion de la distance basculerait dans les derniers crans. Le haut de l'écran reste sous l'horizon (au moins le
  demi-champ de vision plus une marge) : sinon le sol visé près du bord haut est à des centaines de mètres, et tirer
  ou zoomer y fait sauter la vue.
- **Un seul suivi du clic ou glisser** (`DragGesture`) pour tous les boutons : un appui vu pour la première fois part
  de là où on le voit, sinon un bouton enfoncé pendant un chargement fait sauter la vue.
- **Au lancement, le pointeur vaut (0 ; 0) tant qu'il n'est pas entré dans la vue** : un coin, et le défilement au
  bord ferait filer la vue toute seule. Il ne s'arme qu'une fois le pointeur passé loin des bords ; perdre le focus
  le désarme (`EdgeScroll`).

### 2.9 Poser un modèle du pack

- **Une seule routine de pose** pour tout ce qui a un modèle (bâtiments, décos, personnages, charrettes).
- Les prefabs du pack ont leur origine n'importe où (un coin, une porte, un socle enterré) : on **recentre à la pose**
  d'après la boîte de leurs seuls maillages (jamais les particules : la fumée d'une cheminée fausse la boîte), puis on
  applique un décalage fin tenu dans la donnée du modèle.
- **La porte d'un bâtiment** se mesure une fois sur son modèle (par un menu d'éditeur) et se range dans sa donnée ;
  l'entrée du bâtiment est devant elle et tourne avec lui.

### 2.10 L'ambiance

- **Un dégradé de la journée en moments-clés** (aube, matin, midi, après-midi, heure dorée, nuit), chacun avec son
  soleil (couleur, force, hauteur), son ambiance et la chaleur de ses filtres ; on passe de l'un à l'autre par une courbe
  en S, sans saut.
- **Deux profils de filtres mêlés** : un profil de jour et un profil chaud posé par-dessus, dont le poids suit la chaleur
  du moment. Les profils de jeu sont des copies en mémoire : on ne touche jamais les assets en jeu.
- **La distance des ombres suit le zoom** : elle va jusqu'au sol vu au bord haut de l'écran, bornée ; une distance
  fixe est soit trop courte de loin (pas d'ombre), soit trop longue de près (des ombres molles). Le brouillard de la
  carte recule de même quand la caméra s'éloigne. Les deux se règlent sur une copie en mémoire de l'asset URP
  (`Instantiate`, puis `QualitySettings.renderPipeline`) et dans `RenderSettings`, rendus à la sortie : le Play ne
  recharge pas le domaine, et une copie oubliée survivrait d'un Play à l'autre (`CameraAtmosphere`).

## 3. Pièges de Unity et de C#

- **En EditMode, `Awake` et `OnEnable` ne se déclenchent pas** : un composant créé dans un test n'est pas abonné. La
  logique va dans des classes C# pures, testables sans scène.
- **Démarrage rapide du Play** (Enter Play Mode Options, actif dans ce projet : ni le domaine ni la scène ne sont
  rechargés) : les champs statiques, les abonnements et les champs non sérialisés des composants et des fiches
  survivent à la sortie du Play. Chaque statique restant a un `ResetStatics()` appelé depuis
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, tout abonnement est défait dans
  `OnDestroy`, et chaque composant de gestes désactive dans `OnDisable` la carte d'actions qu'il a activée (une classe
  d'actions générée, si l'on en prend une, se libère aussi : `Disable`, `Dispose`). Un bug qui n'apparaît qu'au
  deuxième lancement vient presque toujours de là ; le test des deux parties de suite le cherche.
- **Ne jamais fixer `EditorSceneManager.playModeStartScene` en permanence** : le Test Runner ne peut plus charger sa
  scène, et les tests PlayMode attendent jusqu'au délai. Pour jouer depuis la scène de jeu, un menu l'ouvre puis lance
  le Play.
- **Une scène chargée en additif n'apporte sa lumière que si elle devient la scène active**
  (`SceneManager.SetActiveScene`) : ciel, lumière ambiante, brouillard, reflets.
- **Arrêter une coroutine n'arrête pas le chargement qu'elle attendait** : une scène quittée pendant qu'elle charge
  sa carte la laisserait orpheline. Garder l'`AsyncOperation` et, en partant, décharger la carte dans son
  `completed` (appelé tout de suite si le chargement est déjà fini).
- **Un composant détruit, tenu par une interface, n'est pas `null`** : le `== null` d'Unity ne joue que sur le type
  `Object`. Tester `(cible as Object) == null`.
- **Un `ScriptableObject` modifié dans l'Inspector n'est pas écrit par Ctrl+S** (qui sauve la scène) : `File/Save
  Project`.
- **Changer une valeur par défaut dans le C# ne change pas un asset déjà écrit** : l'asset garde l'ancienne valeur.
- **`ApplyModifiedPropertiesWithoutUndo` ne marque rien si la valeur ne change pas** : faire suivre d'un
  `EditorUtility.SetDirty`, sinon un champ neuf n'atteint jamais le disque.
- **Ouvrir une scène en mode Single décharge les assets que rien ne tient** : un ScriptableObject chargé avant et gardé
  en variable devient un objet détruit, et une référence posée avec lui s'enregistre vide (`{fileID: 0}`), sans
  erreur. Un monteur de scène ouvre donc la scène d'abord, puis lit ses fiches. Après `ImportAsset` ou `CreateAsset`,
  relire par `LoadAssetAtPath`.
- **Un réglage en `float` n'est pas le décimal qu'on croit** : 0,1f vaut 0,100000001 une fois en `double`, et un
  temps de 0,1 tout juste n'atteint pas un pas de 0,1f. Un test choisit ses temps loin des frontières entre deux
  pas, et une vérité qui doit rester exacte se compte en entiers (le temps est un nombre de pas).
- **Ouvrir les `internal` d'un assemblage à un autre** : `[assembly: InternalsVisibleTo("Diciela.Simulation")]` dans un
  `AssemblyInfo.cs` posé à côté du `.asmdef`, avec le nom exact de l'assemblage visé. Une interface ne peut pas avoir
  de membre `internal` : une classe abstraite, si (`internal abstract`), et personne d'autre ne peut alors l'appeler.
- **Un parcours sans allocation** : `foreach` accepte toute structure qui a `GetEnumerator()`, `MoveNext()` et
  `Current` ; une structure qui se rend elle-même ne crée rien sur le tas, même appelée à chaque image (les mailles
  d'un rectangle, `CoveredCells`). Borner sa boucle et rendre un parcours vide hors du maillage.
- **Une énumération sérialisée à trou** (une valeur retirée au milieu) : un script d'éditeur écrit `intValue`, jamais
  `enumValueIndex`. Mieux : ne jamais retirer une valeur du milieu d'une énumération sérialisée.
- **Le Input System lie par position physique, en disposition US** : un raccourci mnémonique (M, I, G…) se lie au
  caractère (`<Keyboard>/#(m)`), un raccourci de position (déplacement) à la position, pour que ZQSD tombe juste sur un
  clavier AZERTY.
- **La molette de l'Input System vaut 1 par cran**, pas 120 : son réglage par défaut (« Scroll Delta Behavior :
  Uniform Across All Platforms ») la ramène à la même échelle partout. Un zoom divisé par 120 ne bougerait presque pas.
- **Ne jamais réécrire un fichier `.inputactions` à la main pendant qu'Unity tourne** : il en garde une copie vivante
  (erreurs d'index, actions muettes). Passer par un script d'éditeur, ou par la fenêtre du Input System.
- **Recompiler pendant le Play** laisse un moniteur du Input System périmé (erreurs à chaque entrée en Play jusqu'à un
  rechargement du domaine).
- **Project Settings → Editor** : Asset Serialization = Force Text, Version Control = Visible Meta Files ; ne jamais
  toucher un `.meta` à la main ; renommer un asset avec son `.meta` (le GUID suit).
- **Deux restes du modèle URP restent, exprès** : `SampleSceneProfile` est le profil de filtres par défaut des
  réglages URP (le supprimer les casse), et le niveau de qualité Mobile, que le CLI ne sait pas retirer.
- **Modifier une scène ou un prefab à la main dans le texte est risqué** : un script de montage (`[MenuItem]`) est
  reproductible et versionné.
- **L'outil de recherche du code saute les dossiers ignorés par Git** (le pack) : une recherche vide là-dedans ne prouve
  rien ; lire un fichier du pack par son chemin exact.
- **UI Toolkit** : une couleur se juge sur le fond où elle s'écrit ; un panneau mis à l'échelle sur la seule largeur
  rapetisse sur une fenêtre étroite ; un bouton dont le texte passe à la ligne double la hauteur d'une barre.
- **Un `PanelSettings` créé par script n'a pas de thème**, et sans thème aucun texte ne s'affiche : un fichier `.tss`
  d'une ligne, `@import url("unity-theme://default");`, lui en donne un.
- **Le pointeur compte depuis le bas de l'écran, UI Toolkit depuis le haut** : retourner y avant
  `RuntimePanelUtils.ScreenToPanel`, mettre la racine du document en `PickingMode.Ignore`, et vérifier que l'élément
  trouvé par `panel.Pick` est bien dans la barre. Le test inverse la conversion d'UI Toolkit pour viser le milieu de
  la barre.
- **Un monteur qui fabrique un prefab** le monte dans une scène d'aperçu (`EditorSceneManager.NewPreviewScene`) : la
  scène ouverte reste propre. **Un matériau URP translucide par script** reprend les réglages de l'Inspector
  (`BaseShaderGUI`) : surface transparente, mélange SrcAlpha / OneMinusSrcAlpha, pas d'écriture de profondeur, passes
  DepthOnly et ShadowCaster coupées, file 3000. Un fantôme qui fait de l'ombre se lit comme une maison bâtie.
- **`FindFirstObjectByType` est obsolète en Unity 6.6** : `FindAnyObjectByType`, et seulement hors du jeu (menus,
  tests).
- **La localisation** : toute chaîne visible passe par des clés (`strings.csv`) ; l'UXML n'écrit aucun texte, le
  présentateur remplit les boutons. Un test vérifie, dans chaque langue, que chaque clé citée par le catalogue
  (familles, constructions) et par les raisons de refus existe ; une nouvelle source de clés s'y ajoute.
- **Un `yield return null` rend la main au test après les `Update` de l'image, avant ses `LateUpdate`** : ce qu'un
  geste joue en `LateUpdate` (la visée, la caméra qui se place) ne se voit qu'une image plus tard. Le banc des tests des
  gestes attend donc deux images (`GestureFixture`). Ce banc est l'`InputTestFixture` de l'Input System (le paquet
  déclaré `testables` dans le manifest) : il coupe les vrais appareils, et dans un `[UnityTest]` un événement simulé
  est lu au début de l'image suivante.
- **Une fiche enregistrée avant l'ajout d'un champ n'a pas ce champ sur disque** : le jeu lit la valeur écrite dans le
  code, et un champ retiré reste orphelin dans le fichier. Le montage réenregistre les fiches de réglages
  (`AssetDatabase.ForceReserializeAssets`).
- **Un module intégré non déclaré est coupé** : d'après la doc d'Unity, ses composants sont ignorés en Play. Le module
  des particules ne l'est pas : la fumée des cheminées du pack ne joue pas (à vérifier quand une maison du pack entre
  en jeu). L'aperçu et le fantôme ne dessinent de toute façon que les maillages.
- **Un rayon vers un sol plat au niveau le plus bas du relief peut le rater d'un arrondi** : la tranche de recherche
  déborde d'un millimètre au-dessus et au-dessous (`Relief.TryRaycast`).
- **Vérifier qu'un appel n'alloue rien** : `Assert.That(() => { … }, Is.Not.AllocatingGCMemory())`, du Test Framework.
  Il faut `using UnityEngine.TestTools.Constraints;` (l'extension) et l'alias
  `using Is = UnityEngine.TestTools.Constraints.Is;` (sinon `Is` est ambigu avec celui de NUnit), et un lambda à
  accolades (sans elles, NUnit le prend pour une valeur à lire). Montrer d'abord que le compteur voit une allocation :
  sinon l'assertion ne prouve rien.

## 4. Les tests qui rapportent le plus

- **Contre un oracle** : un algorithme rapide (le A*) comparé à un algorithme lent et sûr (un Dijkstra, un parcours en
  largeur) sur des centaines d'entrées tirées au hasard.
- **Un fichier d'or** pour tout générateur déterministe.
- **Un aller-retour de sauvegarde** comparé champ à champ, et un fichier hostile (identifiants inconnus, positions hors du
  monde, chevauchements, quantités énormes) qui ne doit lever aucune exception.
- **Un test de scène court** (PlayMode) qui pose, tourne, déplace et retire par les mêmes appels que les gestes.
- **Une campagne ne doit jamais toucher la vraie partie** ni les assets du projet.

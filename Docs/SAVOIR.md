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
| Une charge qui ne trouve plus où rentrer : l'entrepôt s'est rempli pendant la marche du porteur, ou pendant qu'il portait ce qu'il y avait pris. | **La place se retient en prenant la tâche** (`SpaceReserved`, comptée hors de la place libre), celle d'où la charge part comprise, et le reste tant que la charge est en route : elle se rend à la livraison, ou reçoit la charge rendue. |

## 2. Solutions qui ont fait leurs preuves

### 2.1 La circulation : un maillage invisible

- **Le maillage** : un quadrillage invisible, plus fin que le mètre (4 mailles par mètre), où tout ce qui se pose
  s'inscrit. Une maille porte au plus un objet : c'est la vérité pour « rien ne se chevauche » et pour le passage.
  C# pur. Une maille a un coût d'entrée par façon de se déplacer, en entiers, lu dans les fiches : un piéton paie le
  même prix partout où l'on passe ; une charrette paie plus cher l'herbe que la terre, et la terre que les pavés
  (GDD, section 9). Une maille peut être bloquée. Un objet posé s'y **trame** : son empreinte (un rectangle tourné)
  prend toute maille qu'elle touche, même en partie (les axes séparateurs : ceux du monde et ceux du rectangle ; une
  marge d'un dixième de millimètre laisse permis deux bords communs). Une maille libre ne touche donc aucun objet : rien
  ne se chevauche, aucune fente plus étroite qu'une maille ne se traverse, et deux objets se collent à une maille près.
  Ne prendre que les mailles dont le milieu tombe dedans laisse deux objets se recouvrir d'une demi-maille, et un
  piéton passer dans une fente.
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
  tas ; les coûts doivent être égaux, et chaque trajet est jugé pas à pas. Sur les mêmes terrains, les îlots se
  comparent à un remplissage pas à pas en huit directions (la preuve que quatre suffisent), et la ligne tendue à un
  juge qui essaie chaque maille autour de chaque segment, par des produits en croix en entiers (coordonnées doublées :
  un milieu de maille tombe sur des nombres impairs, ses bords sur des pairs). Les cas écrits à la main se dessinent en
  texte, le nord en haut (`TestGround`), et chaque obstacle y est posé par l'ordre « poser ».
- **Le lissage** : le A* rend un escalier de mailles ; on le tend comme une ficelle. Une ligne droite entre deux
  milieux de mailles est libre si toutes les mailles qu'elle touche le sont, **coins compris** : passer par le coin de
  quatre mailles les touche toutes, la règle même du pas en diagonale (une seule fonction du maillage pour les deux).
  Le calcul est exact, en entiers : on compare où la ligne croise sa prochaine frontière de colonne et de rangée, sans
  diviser. On part des bouts de l'escalier et de ses changements de direction (entre deux, l'escalier va droit : la
  ligne est libre), et l'on tire de chaque bout : on file tant que la ligne depuis le dernier pli reste libre ; au
  premier point caché, la ficelle plie sur le segment qui y mène, au plus loin qu'on le voit, là où il passe par un
  milieu de maille. **Tirée d'un seul bout, la ficelle longe un mur au-delà de son coin** : on la tire depuis le
  départ, puis depuis l'arrivée, qui ramène le pli contre le coin. Pour la charrette, « libre » deviendra « au même
  coût que le bout d'escalier remplacé » : elle reste sur sa route.
- **Savoir tout de suite qu'un trajet est introuvable** : les îlots, le sol libre découpé en morceaux d'un seul
  tenant. Comme une diagonale ne coupe jamais un coin, se rejoindre en huit directions revient à se rejoindre en
  quatre : on les relève rangée par rangée, chaque tronçon de mailles libres relié à ceux qu'il touche en dessous par
  un union-find (chaque tronçon pointe vers un plus ancien de son îlot). Ils se refont quand on les lit et que la
  version du maillage a changé : une fois pour tout ce qu'un pas a posé. Le maillage retient, rangée par rangée, la
  version de son dernier changement : les tronçons (`FreeRuns`) ne relisent que les rangées changées et reprennent
  les autres, puis l'union-find repart des tronçons. Sur la carte de 500 m et sa forêt de 5 000 arbres, les îlots
  d'après une pose se refont en 0,5 ms au lieu de 3,9 ms (mesuré à l'étape 30) ; un arbre retiré passe par le même
  chemin. La recherche ne garde que les départs et les arrivées d'un même îlot : un départ seul sur le sien ferait
  fouiller tout l'îlot en vain. Sans les îlots, un A* vers une porte enfermée parcourt toute la carte.
  La charrette aura les siens, si ses passages diffèrent de ceux du piéton.
- **La marche** (`Walk`) : le marcheur suit la ligne tendue, et l'avancement se compte en distance, d'un coin à
  l'autre, sans allocation par pas. Il garde son trajet avec la version du maillage où il l'a vérifié ; quand elle
  change, il revérifie le reste de sa ligne (le segment en cours en entier, puis les suivants et la maille du but) et
  ne cherche un autre trajet, depuis sa maille, que si une construction la coupe : une pose loin de lui ne coûte rien.
  Ce trajet part du milieu de sa maille, qu'il a pu dépasser : il rejoint la ligne au point de son premier segment le
  plus proche de lui, sans sortir de sa maille ; sinon il recule de quelques centimètres avant de repartir.
  Quand une grandeur changera l'allure (une lanterne, une route pour la charrette), elle se relira **à chaque mètre
  parcouru** et à chaque coin, pas seulement aux coins : une lanterne au milieu d'un long segment doit compter.
- **Sortir d'une construction posée sur soi** : tout droit, à travers elle, vers le milieu d'une maille libre qui la
  **borde** (`WayOutFinder`), jamais derrière une autre ; on laisse sa tâche en commençant à sortir, puis on reprend
  sa marche vers son lit ou son travail. Parmi ces mailles, celles
  du plus grand îlot (les îlots comptent leurs mailles), puis la plus proche : la maille libre la plus proche peut être
  une poche que la construction vient de fermer, où l'on resterait pour toujours. La recherche va par anneaux autour
  de sa maille ; la construction est d'un seul tenant, donc un anneau qui n'en a plus aucune maille dit qu'elle est
  finie. Sans maille libre à son bord, on attend que le maillage change, sans chercher de nouveau à chaque pas ; si le
  maillage change pendant qu'on sort, on revérifie sa maille de sortie. Piège connu : la ligne droite vers une maille
  du bord peut effleurer le coin d'une construction collée à celle qu'on quitte, le temps d'un pas ; seule une sortie
  qui suivrait le maillage l'éviterait.
- **Les arbres et les rochers sur le maillage** : chacun y porte l'opposé de son numéro (négatif : jamais une
  construction). Peints serrés, deux pieds se partagent des mailles (le dernier de la carte les garde) : retirer l'un
  ne libère que les mailles qu'il tient encore (`ReleaseOccupant`), jamais tout son rectangle, puis ses voisins debout
  reprennent les mailles libres qu'ils couvrent (`Reclaim`, du dernier de la carte au premier) ; sans l'un ou l'autre,
  le tronc voisin deviendrait traversable. Une forêt de 5 000 arbres à l'échelle du jeu naît, refait ses îlots et se
  traverse en quelques dizaines de millisecondes (`NatureScaleTests`).
- **Chercher la place la plus proche par anneaux carrés** (`SquareRing`, le seul parcours par anneaux) : un anneau ne
  range pas ses places par distance, ses coins sont à √2 fois son rang, plus loin que le milieu de l'anneau suivant.
  Pour la plus proche, on continue tant que le rang de l'anneau reste sous la meilleure distance trouvée, et on écarte
  ce qui dépasse le rayon cherché.
- **Flâner sans dériver** : tirer le point suivant autour de l'endroit où l'on s'est mis à flâner, pas autour de soi ;
  de saut en saut, un tirage autour de soi traverse la carte. Le tirer sur son îlot : il est joignable par
  construction, et un point sans issue n'est qu'une pause de plus. Si l'endroit où l'on flânait est pris, ou coupé de
  soi, se remettre à flâner autour d'où l'on est : sinon chaque tirage rate, pour toujours.
- **Les distances par la route** (qui est le plus proche d'un dépôt) : un Dijkstra gardé tant que le maillage ne change
  pas (un numéro de version du maillage, incrémenté à chaque pose ou retrait), quelques jeux de départs gardés à la
  fois ; sinon chaque charrette relance le calcul à chaque tick.

### 2.2 La simulation et sa vue

- **La simulation avance par pas fixes** (un dixième de seconde), en C# pur, et ne sait pas qu'elle est dessinée.
- **La vue interpole** entre les deux derniers états de la simulation (la fraction du pas en cours) : sans ça, tout ce
  qui bouge saute dix fois par seconde et tourne par à-coups. L'image a un pas de retard, invisible. C'est la même idée
  que l'interpolation d'un jeu en ligne entre deux messages du serveur. Ici, un habitant garde sa position du pas
  d'avant ; l'accumulateur donne la fraction (le temps en attente divisé par la durée d'un pas), et le démarrage la
  passe à la vue des habitants après avoir joué les pas de l'image. Le personnage se tourne vers son déplacement du
  dernier pas, à une vitesse bornée : il ne pivote pas d'un coup à chaque coin.
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
- **SplitMix64** (Sebastiano Vigna) : un compteur qui avance d'un grand nombre impair, brassé à chaque tirage
  (`HashRandom`). Deux entiers donnent son départ (un habitant tire sur son numéro et celui du pas, qui se sauvegarde :
  la même partie après un chargement) ; c'est une valeur, créée là où l'on tire, qui n'alloue rien. Mis à l'échelle
  d'un intervalle par une multiplication, pas par un modulo, qui favoriserait les petites valeurs. Vérifié contre ses
  premières valeurs publiées, recalculées à part (en JavaScript, sur des entiers de 64 bits), jamais sorties de
  mémoire.
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
  (`Object.Instantiate(terrainData)`, dans `MapScene`), donnée au `Terrain` **et** au `TerrainCollider`, jamais
  enregistrée, détruite une fois la carte déchargée ; un test PlayMode (deux parties de suite) vérifie que la partie
  joue sur une copie et que les hauteurs de l'asset n'ont pas bougé.
- **Retirer un arbre peint** : `TerrainData.SetTreeInstance(index, instance)` avec une largeur et une hauteur 0 ; la
  fonction refuse de changer la place ou le modèle, et ne renumérote rien, si bien que le numéro d'instance lu au
  lancement reste bon. Le Terrain dessine des milliers d'arbres à bas coût ; ce qu'on y ajoute (les fanions des arbres
  marqués) se dessine d'un seul appel instancié par image (`Graphics.RenderMeshInstanced`, par lots de 1 023), refait
  seulement quand les marques changent.
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
- **Toucher une construction vise son modèle, jamais le sol** : vu de biais, le haut d'une cabane cache le sol derrière
  elle, et le rayon qui touche son toit retombe hors de son empreinte. Aujourd'hui, la vue (`StructureViews.TryPick`)
  vise la boîte des maillages dans le repère du modèle, qui tourne avec lui (jamais `Renderer.bounds`, droite et plus
  large) ; les triangles viendront avec le premier modèle creux.
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
  de là où on le voit, sinon un bouton enfoncé pendant un chargement fait sauter la vue. Mais un outil pris pendant un
  appui (une touche, le bouton tenu) ne l'adopte pas : tant que l'outil n'est pas en main, son geste garde l'appui en
  cours comme étranger (`DragGesture.HeldElsewhere`), sinon le relâcher ferait cliquer le nouvel outil.
- **UI Toolkit distribue l'appui avant les `LateUpdate`** (`UIElementsUpdatePanels`, dans PreLateUpdate, avant
  `ScriptRunBehaviourLateUpdate`) : un élément qui disparaît à l'appui (la roue qu'on congédie d'un appui à côté) n'est
  déjà plus sous le pointeur quand les gestes demandent si l'appui est à l'interface, et la ville le prenait (la carte
  en main se posait). L'interface retient donc l'image où un de ses éléments a pris un appui (`HudArea`, à l'écoute de
  sa racine en descendant) et dit cet appui à elle, même l'élément disparu. Les gestes lus en `Update` (la caméra)
  passent avant la distribution, et voient encore l'élément.
- **Au lancement, le pointeur vaut (0 ; 0) tant qu'il n'est pas entré dans la vue** : un coin, et le défilement au
  bord ferait filer la vue toute seule. Il ne s'arme qu'une fois le pointeur passé loin des bords ; perdre le focus
  le désarme (`EdgeScroll`).

### 2.9 Poser un modèle du pack

- **Préparer une fois, poser sans calcul.** Le menu `DICIELA/Contenu/Préparer les modèles` fait de chaque modèle du
  pack une copie préparée : tournée porte au nord, centrée sur la boîte de ses seuls maillages (jamais les particules :
  la fumée d'une cheminée fausse la boîte), **l'origine du pack gardée comme sol**. La routine de pose (`ModelPose`), la
  seule pour tout ce qui a un modèle, ne fait plus que poser cette origine au centre de l'empreinte, sur le sol.
- **Le sol d'un bâtiment Synty est son origine**, au niveau de la porte (Shed_02 a sa porte à l'origine même). Ses
  fondations descendent en dessous et doivent s'enfoncer : poser le bas de sa boîte sur l'herbe soulève le bâtiment,
  et la porte flotte.
- **La boîte vraie se prend sur les sommets.** Les coins de la boîte d'une pièce tournée débordent : un accessoire posé
  de biais gonflait l'empreinte de Shed_02 de 5,98 à 6,53 m, et la décentrait de 11 cm. Le test de contenu l'a vu.
- **La porte** : les pièces dont le nom contient « Door » (le mur à porte, le seuil de pierre…). Le côté où elles
  s'écartent le plus du centre donne le quart de tour ; leur boîte donne la place et la largeur de la porte sur la
  façade. Le bas de cette boîte n'est pas le niveau de la porte : le seuil de Shed_02 descend à 28 cm sous l'origine.
- **La couche** : un objet vide nommé « Couche », posé aux pieds de qui dort, son avant (la flèche bleue) vers la tête.
  La préparation le lit dans le repère du modèle préparé, centré : la fiche garde ses pieds (au centimètre) et l'angle
  de sa tête (au degré). Sans ce repère, pas de couche : la Chimère entre dans son logis, et on ne la voit plus.
- **Fusionner.** Un preset du pack est fait de dizaines de pièces (Shed_02 : 62), chacune un objet à créer et au moins
  un dessin par image, plus les ombres. Fusionné en un maillage, avec un sous-maillage par matériau (Shed_02 : 15), il
  se crée d'un objet et se dessine en un appel par matériau. On ne prend que la version la plus détaillée de chaque
  pièce : ses niveaux de loin (LOD) viennent des pièces elles-mêmes, pas du preset (deux maillages dans Shed_02).
  Dans l'éditeur, `Mesh.CombineMeshes` et `GetVertices` lisent des maillages importés sans « Read/Write ».
- **Le maillage préparé pèse des mégaoctets en texte** (Shed_02 : 5,7 Mo) et se refait à chaque préparation : il va
  en LFS, comme le Terrain.
- **Pas d'effet de particules dans un modèle préparé ni dans une composition** (pour l'instant) : avec la flamme du
  pack dans le campement, Unity plantait à chaque fin de campagne PlayMode, en rechargeant la scène du jeu (« Dereferencing
  NULL ImmediatePtr », dans le ramasse-miettes des assets), trois fois sur trois le 01/10/2026, même sur un Unity
  relancé ; les tests eux-mêmes passaient. Un effet reviendra par une autre voie, après une enquête à part.
- **Composer un modèle avec des objets du pack** (le campement) : un prefab dans `Assets/Art/Compositions`, en
  instances des prefabs du pack (chacune garde son nom, le développeur la retouche), son entrée sous un objet nommé
  « Door », sa couche en repère (« Couche », ajouté s'il manque). Mesurer avant de placer : les objets du pack n'ont pas la taille qu'on croit (le cercle de pierres fait
  6,2 m, les tas de rochers 4,8 à 9,9 m, les rochers 2,3 m et plus, la pile de bois 2,2 m, la hache tient debout) :
  les rochers sont à l'échelle du paysage, une réduction en fait un foyer ou un tas de pierres.
- **Sans modèle du pack, une boîte fusionnée de même** (`PlaceholderBuilder`) : le corps laisse devant lui l'épaisseur
  de la porte, qui affleure la façade, pour que la boîte remplisse l'empreinte sans la dépasser (le test de contenu
  la mesure comme un modèle préparé). Une porte posée devant une boîte pleine dépasserait de son épaisseur ; une porte
  à fleur d'une face de même plan clignoterait.
- **Plus tard, si le Profiler le demande** : le GPU Resident Drawer d'Unity 6 (URP en Forward+, déjà le cas du projet,
  plus deux réglages) dessine d'un coup les copies d'un même maillage ; fusionner aussi les niveaux de loin.
- **Vérifier à l'œil** : la planche des modèles (`DICIELA/Album/…`) pose côte à côte le modèle du pack et le modèle
  préparé, vus du côté de la porte. Un fantôme translucide ne montre ni la porte ni le sol. Une mise en scène d'album
  cadre à une distance fixe (`CameraRig.ZoomTo`), jamais par des crans de molette : les crans de deux mises en scène
  s'additionnent, et l'aperçu finit collé à l'objectif (un écran tout vert).
- **Couper un modèle du pack à une hauteur** (le chantier qui monte, décision 166) : ses matériaux sont des Shader
  Graph de Synty (`_Albedo_Map`, `_Normal_Map`, `_Normal_Amount`…), qu'on ne retouche pas. On en fait à l'exécution
  une copie sur un shader du projet (`Diciela/Chantier`, le Lit d'URP repris), propriétés recopiées une à une sous les
  noms du Lit ; une copie par matériau d'origine, partagée par tous les chantiers, détruite avec la vue. La hauteur de
  chaque chantier passe par la valeur propre de chaque rendu (`SetShaderUserValue`, les bits d'un flottant, lus par
  `asfloat(unity_RendererUserValue)`, dans le tampon par objet d'URP) et la vague par des variables globales
  (`Shader.SetGlobalFloat`) : aucun matériau par chantier, aucun `MaterialPropertyBlock` (un bloc sort un rendu du SRP
  Batcher), et aucune propriété hors du tampon `UnityPerMaterial` (la déclarer dans `Properties` sans l'y mettre rend
  tout le shader incompatible). Le test `SiteShadersTests` lit la compatibilité là où l'Inspector la lit
  (`ShaderUtil.GetSRPBatcherCompatibilityCode`, interne). La coupe doit valoir aussi pour les passes
  ShadowCaster, DepthOnly et DepthNormals, sinon la moitié pas bâtie fait de l'ombre : celles d'URP n'ont pas la
  position dans le monde, le shader a donc les siennes. Sans MSAA, un bord adouci se fait par une trame (Bayer 4 × 4)
  sur la vraie matière, et le flou par le calque transparent du fantôme. Une erreur de variante d'un shader n'apparaît
  qu'au rendu : chercher « Shader error in » dans `Logs/Editor.log` après un Play.
- **Les feuillages de Synty ont leur propre shader** (Foliage) : il lit `_Leaf_Texture` là où le bleu de la couleur du
  sommet dépasse 0,5, et `_Trunk_Texture` (les tiges) ailleurs, en UV brut ; ils sont détourés (`_AlphaClip`, au seuil
  `_Alpha_Clip_Threshold`, 0,5) et dessinent leurs deux faces (`_Cull` à 0). Une copie qui l'ignore les montre en carrés
  pleins, ou coupe leurs tiges (73 % du lierre de la cabane) : l'alpha des feuilles est nul sous les UV des tiges. Toute
  copie d'un matériau du pack reprend donc la texture sous son nom, la texture des tiges, le seuil et les faces
  (`OriginalMaterials`). La copie lit les tiges par une branche dynamique (`#pragma dynamic_branch_local _TRUNKMAP` et
  `if (_TRUNKMAP)` : un mot-clé sans variante de plus) ; le bleu va du sommet au fragment de la passe avant d'URP dans
  son interpolateur `viewDirTS`, libre sans parallaxe, demandé avant d'inclure `LitForwardPass` (`ChantierAvant`). Un
  seuil nul ne détoure rien même si le détourage est coché (l'atlas du pack l'est) : on ne détoure qu'au-dessus de 0,
  le détourage coûtant au rendu. Mais un matériau transparent par mélange (`_Surface` à 1 : le bord d'un chaume, dont le
  seuil hérité vaut 0) a une texture pleine ou vide : sa copie, opaque, se détoure au seuil de la fiche de la pose. Un
  shader du pack qu'on ne connaît pas (le triplanaire, les décalcomanies) est dit dans la console, sinon sa copie monte
  blanche en silence. L'aperçu de pose a le même fantôme que le chantier : sans ligne, c'est un fantôme teinté.

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
- **Deux caméras, une seule allumée** : l'atelier de la création a sa caméra, loin de la carte ; celle de la ville
  reste éteinte tant qu'elle ne montre pas la carte, sinon la scène entière se rend deux fois par image. La seconde
  caméra n'a pas d'écouteur de son (deux écouteurs : un avertissement à chaque image), et le menu qui installe la
  caméra de la ville la cherche par son `CameraRig`, jamais « la première caméra de la scène ». Le soleil, l'ambiance
  et les filtres de la carte éclairent aussi l'atelier : son sol et son fond ne portent pas d'ombre.

### 2.11 Habiller un personnage du pack

- **La tête se découpe en morceaux** (`HeadShells`, `MeshShells`) : dans chaque modèle d'animal, les triangles de la
  tête (ceux dont les sommets suivent pour moitié au moins head, eyes et eyebrows) forment des morceaux d'un seul tenant,
  une fois les sommets soudés par leur position (le pack double un sommet à chaque couture et à chaque arête vive : la
  connexité par indice ne sert à rien). Chaque morceau est fermé : on le détache sans trou. Le plus grand est le crâne ;
  une paire de grands morceaux hors du milieu, dont le centre est au-dessus des yeux (leur bas descend plus bas qu'eux),
  les oreilles ; de petits morceaux sur le haut, au milieu, les touffes. Yeux, sourcils, moustaches et museau (celui du
  renard est un morceau à part, il reste aussi) restent au crâne : un museau ne se change pas, et le découper laissait
  des îlots flottants et des plaques (la bouche peinte de la grenouille). Une tête qui sort de ces nombres (0 ou 2
  oreilles, 3 touffes au plus) arrête le menu.
- **Les oreilles du raton laveur sont cousues à son crâne** (`FusedEars`) : dans une tête sans oreilles à part, le
  noyau d'une oreille est ce qui est mince au-dessus des yeux, sur les côtés (un rayon lancé vers l'intérieur ressort à
  moins de 30 cm, le dôme en fait plus de 60 ; un bord qui ne voit rien derrière lui compte aussi), un grand morceau
  de chaque côté. Son dos, épais à la base, échappe aux rayons : il se reconnaît à sa peinture (on ajoute de proche en
  proche les triangles voisins qui ne sont pas de la teinte du crâne, la plus répandue). Un plan de coupe à la base
  mange le front : écarté. Les trous laissés se referment en éventail (`SkullCap`), chaque bord suivi arête par arête
  (un trou peut se toucher lui-même en un sommet), le centre à la hauteur de la moitié basse du bord ; les mesures de
  la tête se prennent sur le crâne refermé, sans quoi un rayon file par le trou. Dans la copie découpée, jamais dans le
  pack, une teinte peut être repeinte (`PartRepaint`) : l'intérieur de l'oreille gauche du raton est peint du blanc de
  sa face ; sur une autre tête, cette lèvre faisait un bandeau clair.
- **Le crâne et le corps** restent des maillages à peau sur le squelette commun (`BodyCutter`), réécrits en place à
  chaque passage (même fichier, mêmes références) : copier champ par champ, jamais par copie sérialisée, qui garde
  l'ancien découpage des sommets (crânes en miettes). **Oreilles et touffes** suivent à 100 % les os de la tête :
  ils deviennent des pièces rigides, maillage et prefab (`HeadPartCutter`, `Assets/Art/Personnages/Pieces`), leur pivot
  à leur base ; A et B ne rangent ni ne cousent leurs sommets pareil : une pièce est la même si elle a autant de
  triangles, la même boîte et le même pivot à 5 mm près.
- **Les seize animaux ont exactement le même squelette** : n'importe quelle tête va sur n'importe quel corps, sans jour
  au cou. **Un seul corps allumé par prefab** : les quinze cachés pointent vers un matériau absent du projet.
- **Habiller** (`Dresser`) : on retire ce que porte le prefab **par structure, jamais par nom** (ses corps, ses pièces
  d'office, ses capes cachées), en gardant les os, l'os racine et la boîte du corps allumé (sans elle, un maillage à
  peau neuf est jugé hors de vue). Le crâne et le corps choisis y sont posés, chacun de sa couleur (`PaletteSwap` :
  chaque matériau par son pendant, personnage ou objet, dans `sharedMaterials`, jamais `.material`).
- **Les pièces se posent sur la tête choisie, à son relief** : chaque tête a ses points (`HeadMeasures`, pris par des
  rayons sur les triangles, jamais sur les sommets, trop clairsemés en low poly ; les rayons du milieu passent à 5 mm de
  la couture, où ils fileraient entre deux triangles) et ses surfaces, cartes de 5 cm (`SurfaceMap`, `SurfaceBaker`) :
  le dessus du crâne, l'avant de la face ; chaque corps, son dos et son ventre. Chaque pièce a la carte de sa face qui
  touche. Une pièce se pose au point du repère de son emplacement plus son décalage, et, sur l'axe de contact, là où sa
  face touche la surface (`Contact`, au jeu, en quelques centaines d'opérations : les modèles du pack ne sont pas
  lisibles en jeu). Ce qui se mesure depuis les pieds ou au cou touche le corps ; le reste, la tête. Un chapeau cache
  les oreilles et les cheveux choisis (touffes, huppe).
- **Des repères stables d'une tête à l'autre** : le haut du crâne et les oreilles se tiennent à mi-profondeur de la tête,
  à hauteur des yeux (le milieu entre l'avant et l'arrière du crâne, sur la ligne du milieu). La place du point le plus
  haut d'un crâne plat varie de 17,5 cm d'une tête à l'autre (raton −9 cm, oiseau +9 cm) : une pièce réglée sur une tête
  glissait d'autant sur une autre. Les oreilles sont deux points, un de chaque côté : là où un rayon lancé à 50° depuis
  le milieu de la tête (à mi-hauteur entre le haut de sa ligne du milieu et son bas) sort du crâne ; la fiche garde
  celui de droite, la gauche en est le miroir (de 31 cm du milieu chez le cochon à 41 cm chez la grenouille). Changer un repère déplace toutes les pièces réglées dessus : on convertit leurs
  décalages pour qu'elles restent à leur place sur leur tête de référence (le chat A pour les pièces du pack ; la leur
  pour une pièce découpée ; l'oiseau pour la huppe).
- **Une paire d'oreilles est en deux moitiés** (`EarHalves`, `PiecePlacement`) : les triangles de chaque côté du
  milieu, enfants « Droite » et « Gauche » d'un même prefab, le pivot au milieu ; une paire du pack est copiée ainsi
  dans `Assets/Art/Personnages/Pieces` (`PackCopy` aplatit le modèle), le pack ne change pas. Chaque moitié s'écarte de
  l'écart entre les oreilles de la tête choisie et celles de sa tête de référence (plus le x de son décalage et de ses
  réglages). Sur une autre tête que sa référence, une paire se pose par sa base (`Contact.TryRest`) : les points de sa
  face à moins de 3 cm du dessus de sa tête de référence, là où elle y est posée, restent au contact ; le plus bas
  touche, les autres s'enfoncent. Posée par son premier point de contact, une paire large flottait sur une tête ronde
  (les oreilles du raton sur la souris). Au bord d'un dôme, sa base déborde du crâne (des oreilles décollées) : elle
  glisse vers le milieu, par pas de 1 cm, jusqu'à porter à plat (3 cm d'écart au plus). Cela ne suffit pas aux oreilles
  larges du raton et du renard : leurs réglages les rapprochent encore et les enfoncent sur les autres têtes ; juger de
  dos, où la base extérieure dépasse de la silhouette du crâne.
- **Une barbe se pose sur la bouche**, pas sur les yeux : les bouches sont peintes d'un trait noir (la case noire de la
  texture, sous les yeux, au milieu), toutes vers 1,37 m, quand les yeux vont de 1,47 à 1,64 m ; son réglage par tête la
  ramène à la bouche, et au besoin lui donne une autre taille, selon l'axe (la grenouille, à la bouche large : plus
  large que haute, ses côtés couvrent les coins). Un bec ou un gros museau n'ont pas trouvé de barbe qui leur aille : ces
  têtes la refusent. Régler une barbe se juge de face et de profil, deux réglages côte à côte (une variante A, une B).
- **Une barbe se teint comme le poil** (`FurCopy`) : le pack la peint dans le masque des cheveux d'une texture d'objet,
  que ses douze couleurs teintent de deux bruns presque pareils (les teintes `_Hair_Color` et `_Skin_Color` des
  matériaux) : sa couleur ne changeait pas. Sa copie prend le matériau de personnage de sa couleur et tous ses sommets
  vont sur une case blanche du masque de la peau, que chaque couleur d'animal teinte de son poil (huit teintes
  différentes). Les douze textures de couleurs sont les mêmes là où il y a peau et cheveux : ce sont les teintes des
  matériaux qui changent.
- **Le dessous d'une pièce découpée se referme** : des oreilles cousues au crâne (le raton) en sortent ouvertes par le
  bas, et sur une tête plus étroite on voyait leur intérieur, en croissant clair. `SkullCap` le referme en éventail,
  dans un sous-maillage à part, que le test de la découpe ne compte pas.
- **Une pièce se tourne sur son pivot** (sa rotation, puis sa taille) ; sa face qui touche se remesure tournée : relancer
  le menu des fiches. Le pack dessine couchés les nœuds : aucun de ses personnages ne les porte, sa scène de démonstration les aligne à 0°. Le casque est un morion : de profil, bord relevé devant et derrière,
  la rosace sur le côté ; tourné d'un quart, il montrait son profil de face.
- **Des réglages par porteur** (`WearerAdjustment`) : un décalage de plus sur des têtes (une pièce de la tête) ou des
  corps (la ceinture, la queue), et au besoin une autre taille, axe par axe. Les variantes A et B d'un animal ont les
  mêmes repères et les mêmes cartes : un réglage les nomme toutes les deux. Un seul enfoncement ne va pas à tous les
  corps : une cape plaquée contre le col évasé du chat entrait de 7 cm dans la tunique évasée du raton. L'enfoncement
  de la face d'une pièce dans un corps se calcule depuis les cartes, comme `Contact`, sans rien photographier.
- **Une pièce tenue par un lien touche par ce lien** (l'anse d'une bourse, les cordons d'une cape) : le contact l'arrête là, et le reste flotte à 10 ou 20 cm du corps. Son décalage sur l'axe de contact la
  plaque, le lien entrant dans le corps, où il ne se voit pas.
- **Les chapeaux low poly ont un fond fermé** que la tête traverse : le contact seul les poserait comme un couvercle.
  Une pièce humaine reprend l'enfoncement de son dessin sur son porteur ; un chapeau animal au fond fermé descend de
  35 % de sa hauteur sous le contact (les chapeaux humains : de 20 à 80 %) ; un anneau ouvert (la couronne) reste posé.
- **Le pack pose lui-même ses pièces** : les humains, cheveux, chapeaux et barbe à l'origine de leur os Head, droits ;
  l'oiseau, sa huppe sous head ; le chat et le renard, leur queue sous pelvis ; tous sans rotation (0°). Les chapeaux
  du pack sont dessinés un peu penchés en arrière : ce n'est pas une pose ratée. Ce sont des copies, reconnues à leur
  maillage (`PackPoses`). Le réglage de départ d'une pièce (`AttachmentFitter`, écrit une fois, quand sa taille est
  nulle) reprend cette pose ; une pièce humaine se ramène de la tête de son porteur à celle du chat, agrandie du rapport
  des crânes à hauteur des sourcils (≈ 3 ; l'os eyebrows des animaux est au niveau des yeux : on mesure les sommets des
  sourcils) ; les cheveux et la barbe, qui enveloppent le crâne, axe par axe (un crâne humain est haut et étroit, un
  crâne d'animal large et plat), la frange aux sourcils ; sinon la règle de l'emplacement (`SlotRule`).
- **Deux squelettes** : les humains ont `Head` (1,57 m ; 1,66 m pour la serveuse et le boucher), les animaux `head`
  (1,21 m, à la base d'une tête énorme) ; le haut du crâne des animaux va de 1,87 à 1,93 m, 1,98 m sur les oreilles du
  raton. **Les petites capes** de l'oiseau, du lapin, de la souris et du cochon ont leur propre squelette : elles ne
  suivront pas l'animation du dos.
- **Vérifier de près** : `DICIELA/Album/Planches des pièces, sur chaque tête (fichiers)` photographie hors scène
  (`Studio`, `PreviewRenderUtility`, qui rend avec l'URP) chaque pièce sur les huit animaux (têtes A), de trois quarts et
  de profil, en planches de 4 × 4 dans `Temp/Planches`, avec `Mesures.csv` (la boîte de chaque pièce rapportée au crâne
  ou au corps : `PieceMeasures`) ; le profil montre ce qui flotte ou s'enfonce, que le trois quarts cache ; une structure en C# se lit par copie : une boîte se construit dans une variable, pas dans une
  propriété (`Bounds` restait réduit à un point). La planche en Play des têtes et des corps reste pour le cou. La
  planche d'essais (`TrialSheet`) photographie des Chimères composées à la main dans
  `UserSettings/Planches/Essais.txt` (tête, corps, pièces ; pas dans `Temp`, qu'Unity vide à sa fermeture), de face,
  de trois quarts et de dos : pour choisir entre deux réglages, on en donne un à chaque variante d'un animal, côte à
  côte. La planche des repères (`LandmarkSheet`) marque sur chaque tête le haut du crâne, les
  oreilles, la face et le cou.

## 3. Pièges de Unity et de C#

- **Restaurer un fichier suivi par Git LFS** (les images, les maillages) : `git show HEAD:chemin` rend le pointeur, pas
  le fichier ; l'écrire à sa place remplace l'image par trois lignes de texte. Passer par `git show HEAD:chemin | git lfs
  smudge chemin`, vers un fichier à part, puis le copier.
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
- **Un `Hide()` appelé pendant qu'une scène se décharge** (le démarrage ferme tout dans son `OnDestroy`) trouve des
  objets Unity déjà détruits, dans un ordre qu'on ne choisit pas : il ne touche que ses champs C#, ou vérifie que
  l'objet vit encore (`if (_camera != null)`, `if (this != null)`). Sinon, une `MissingReferenceException` fait tomber
  le test en cours et laisse la carte de test ouverte pour les suivants.
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
  clavier AZERTY. **Le nom d'une touche liée à sa position** se lit sur le clavier branché : `GetBindingDisplayString`
  rend le caractère qu'elle tape, « & » pour la touche 1 en AZERTY. Pour écrire « 1 » (les cartes du panneau), lire le
  nom de la place : `InputControlPath.ToHumanReadableString(chemin, OmitDevice)`. Les tests ne le voient pas : leur
  clavier virtuel est en disposition US ; seul un Play sur le vrai clavier le montre.
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
- **Mesurer un élément d'UI Toolkit** (étape 20b, la roue des outils) : caché (`display: none`), il mesure 0, pas
  NaN, et ne se mesure qu'à la mise en page suivante une fois montré : un test attend une largeur positive. Ce qui doit
  se mesurer avant de paraître (la roue, ramenée dans l'écran) reste en page, `visibility: hidden` et sans prise
  (`pickingMode` à `Ignore` sur lui et ses enfants). `worldBound` compte `translate`, que le feutre anime en
  s'enfonçant : une place se vérifie sur `layout`. Et un élément qui s'anime par `translate` se centre par ses marges,
  jamais par `translate: -50% -50%`, que son survol écraserait.
- **Une vue du HUD se branche à chaque ouverture et se débranche à sa fermeture** (étape 22) : les éléments d'un
  document vivent plus longtemps qu'une partie. Un rappel posé une seule fois garde ce qu'on lui a donné à la première
  partie (la roue gardait sa première bulle d'aide) ; un rappel posé à chaque ouverture sans être retiré se double. Donc
  `Show` branche, `Hide` débranche, et une infobulle donnée se retire (`Tooltips.Attach` rend de quoi la retirer) ;
  un élément qu'on jette emporte ses rappels.
- **Un clic simulé dans un test** (étape 22) : envoyer à l'élément `PointerDownEvent.GetPooled(Event)` puis
  `PointerUpEvent.GetPooled(Event)`, la cible donnée, un `Event` de souris dont `mousePosition` est le milieu de
  l'élément dans le repère du panneau (origine en haut, sans conversion). Un toucher (`GetPooled(Touch)`) n'a pas
  déclenché le `Clickable` d'un bouton. Un survol : `PointerEnterEvent.GetPooled(Event)`.
- **Une feuille de style qui nomme une icône pas encore générée** garde une référence vide (« Invalid asset path » dans le
  journal) et affiche le triangle jaune d'Unity, même une fois l'icône créée : générer l'icône d'abord, ou réimporter la
  feuille (`set_selection --paths '["…uss"]'`, puis le menu `Assets/Reimport`).
- **UI Toolkit en Unity 6.6** (essai de l'étape 14) : une police `.ttf` se donne telle quelle
  (`-unity-font-definition: url("….ttf")`) et reste nette à toute taille ; chaque graisse est un fichier, et le style
  reste `normal`, sinon Unity épaissit par-dessus. `-unity-slice-scale` règle aussi le pas de la répétition
  (`-unity-slice-type: tiled`) : une image en triple résolution, à l'échelle 1/3, se répète à sa taille. Le tracé
  vectoriel (`generateVisualContent`, `Painter2D`) fait les pointillés (`SetDashPattern`) et le découpage (`PushClip`),
  et s'enregistre en image (`SaveToVectorImage`), mais rangé au ras de son dessin : lui rendre sa boîte
  (`InterfaceIconBuilder`). `PanelSettings.scale` se change à chaud ; en Play dans l'éditeur, c'est l'asset qui
  change, à lui rendre à la sortie. Une variable de style n'est pas sûrement héritée : une face lit les siennes sur
  ses propres classes. Pas de `line-height` en USS.
- **Un texte qui revient à la ligne dans un bloc qui rétrécit** (`flex-shrink` dans une rangée, sous un `max-width`)
  est mesuré à sa largeur d'avant : sa dernière ligne sort du cadre (l'astuce, étape 15). Donner au bloc une largeur
  fixe et au texte `flex-grow: 1; flex-basis: 0`.
- **Une feuille USS importée avant l'asset qu'elle cite** (une icône pas encore générée) affiche le triangle jaune
  d'Unity, et l'éditeur garde cette image invalide en mémoire jusqu'au prochain rechargement du domaine, même après
  avoir réimporté la feuille : générer les images avant d'écrire la feuille, sinon recompiler avant de juger.
- **Un filtre USS sur un grand bloc se rend en basse résolution** : une ombre portée (`filter: drop-shadow(…)`) sur
  un panneau de 416 × 500 rendait flous, en 4K, son texte et ses boutons. Aucun filtre sur ce qui contient du texte :
  une ombre dure se dessine, une ombre floue se génère en image.
- **Voir une interface sans Play ni capture** : un `PanelSettings` dont `targetTexture` est une RenderTexture sRGB
  (le projet est en couleurs linéaires) rend un document hors écran, à la taille voulue, 4K comprise ; lire les
  pixels après `WaitForEndOfFrame` (l'image est à l'endroit). C'est ainsi que `HudLayoutTests` rend le vrai HUD à
  huit tailles d'écran et mesure ses blocs (`worldBound`, en pixels de l'interface) : `InterfaceScale` prend la
  taille de l'image quand le panneau se rend dans une image.
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
- **Le trajet s'appelle `Path`** (le lexique) : dans un fichier qui a `using System.IO;` et `using
  Diciela.Simulation.Land;`, `Path` est ambigu ; y écrire `System.IO.Path`.
- **Un mot-clé de shader allumé par script se déclare en `multi_compile`**, jamais en `shader_feature` : un jeu
  compilé ne garde d'un `shader_feature` que les variantes des matériaux du build. Le chantier allume `_ALPHATEST_ON`,
  `_NORMALMAP` et `_EMISSION` sur des copies faites à l'exécution, alors que `Chantier.mat` n'en a aucun : en
  `shader_feature`, les feuilles redeviendraient des carrés dans le jeu compilé, sans erreur (Unity prend la variante
  la plus proche), alors que l'éditeur, qui compile à la demande, les montre justes.
- **Un `MaterialPropertyBlock` ne se crée pas pendant la construction d'un composant** : dans l'initialiseur d'un champ
  d'un `MonoBehaviour`, Unity lève « CreateImpl is not allowed to be called from a MonoBehaviour constructor ». Le
  créer au premier usage (`??=`), ou dans `Awake`.
- **Une référence perdue n'est pas une référence vide** : un modèle du pack introuvable (le pack absent, ses GUID
  changés) vaut `null` pour l'opérateur `==` d'Unity, comme un champ jamais rempli. Dans l'éditeur, la propriété
  sérialisée les distingue : `objectReferenceValue == null` mais `objectReferenceEntityIdValue.IsValid()` (Unity 6.6 :
  l'identifiant d'objet s'appelle `EntityId`). « Préparer les modèles » refuse ainsi une source perdue au lieu d'en
  faire une boîte.
- **Une capture ne dit pas dans quel sens marche un personnage** : entre deux images, il a pu faire plusieurs sauts et
  tourner. Pour savoir où regarde un modèle, mesurer ses os dans leur pose d'origine : `PrefabUtility.LoadPrefabContents`
  charge un prefab à part, sans toucher la scène ouverte (puis `UnloadPrefabContents`).
- **Vérifier qu'un appel n'alloue rien** : `Assert.That(() => { … }, Is.Not.AllocatingGCMemory())`, du Test Framework.
  Il faut `using UnityEngine.TestTools.Constraints;` (l'extension) et l'alias
  `using Is = UnityEngine.TestTools.Constraints.Is;` (sinon `Is` est ambigu avec celui de NUnit), et un lambda à
  accolades (sans elles, NUnit le prend pour une valeur à lire). Montrer d'abord que le compteur voit une allocation :
  sinon l'assertion ne prouve rien.

## 4. Les tests qui rapportent le plus

- **Contre un oracle** : un algorithme rapide (le A*) comparé à un algorithme lent et sûr (un Dijkstra, un parcours en
  largeur) sur des centaines d'entrées tirées au hasard.
- **Des invariants sur des parties tirées au hasard** : la marche jouée sur 200 terrains où l'on pose des fantômes
  pendant qu'elle marche (jamais dans une maille prise, jamais figée). Le test compte ce qu'il a exercé (des sorties de
  fantôme, des pas de marche) et exige un minimum : vert sans les cas visés ne prouverait rien.
- **Un fichier d'or** pour tout générateur déterministe.
- **Un aller-retour de sauvegarde** comparé champ à champ, et un fichier hostile (identifiants inconnus, positions hors du
  monde, chevauchements, quantités énormes) qui ne doit lever aucune exception.
- **Un test de scène court** (PlayMode) qui pose, tourne, déplace et retire par les mêmes appels que les gestes. Un
  test qui charge la scène de test la quitte par `TestOpening.Leave`, à sa fin et dans son `[UnityTearDown]` : s'il
  échoue en chemin, sa scène et sa carte ne restent pas aux suivants, qui écriraient « carte déjà ouverte » et
  cacheraient la vraie panne.
- **Une campagne ne doit jamais toucher la vraie partie** ni les assets du projet.

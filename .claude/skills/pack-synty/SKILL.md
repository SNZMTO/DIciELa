---
name: pack-synty
description: Ce qu'il faut savoir du pack d'art POLYGON Fantasy Village (Synty) - où sont les choses, personnages et animaux, démo, eau, pièges d'import. À lire avant de brancher un modèle, une caméra ou un rendu.
---

# Le pack POLYGON Fantasy Village (Synty)

**Tout l'art du jeu vient de ce pack, tel quel.** La caméra, la lumière et le rendu s'adaptent à lui. Claude ne compose
jamais un bâtiment ni un décor de sa propre initiative ; les choix d'art sont au développeur. Rien ne s'écrit dans
`Assets/Synty/` : une retouche du développeur (un preset modifié) vit dans `Assets/Art/`, versionné.

## Où sont les choses

- `Assets/Synty/` (hors Git, 1,5 Go, importé par le développeur) : `PolygonFantasyVillage/` (le pack), `PolygonGeneric/`
  et `PNB_Core/` (matériaux et shaders partagés des packs Synty), `SyntyPackageHelper/` (outil d'import).
- `PolygonFantasyVillage/Prefabs/` : `Buildings`, `Characters`, `Environment`, `FX`, `Props`, `Vehicles`.
- `PolygonFantasyVillage/Scenes/` : `Demo.unity` (la référence de beauté), `Demo_BIRP.unity` (la même pour l'ancien
  pipeline, inutile ici), `Overview.unity`, `Buildings.unity`, `Demo_Terrain.asset`, `Volume_01.asset` (profil de
  filtres de la démo).
- **L'outil de recherche du code saute ce dossier** (ignoré par Git) : chercher par son chemin exact, ou par
  PowerShell (`Get-ChildItem -Recurse | Select-String`).
- **Le paquet** est dans le cache de l'Asset Store (`%APPDATA%\Unity\Asset Store-5.x\Synty Studios\3D
  ModelsEnvironmentsFantasy\POLYGON - Fantasy Village - Art by Synty.unitypackage`, 316 Mo, version du 27/09/2026 :
  4 955 entrées). C'est une archive tar gzip : un dossier par GUID, avec `pathname` (le chemin dans le projet),
  `asset` (absent pour un dossier) et `asset.meta`.
- **Le lire sans l'importer** : `gzip -dc "<paquet>" | tar -xf -` dans le dossier scratchpad (le `tar -xzf` direct a
  échoué une fois), puis un petit script Node qui lit chaque `pathname` (une boucle Bash sur 5 000 dossiers est trop
  lente).
- **L'installer sans la fenêtre d'import** : copier chaque `asset` à son `pathname` et `asset.meta` à côté (le GUID
  suit), puis `menu --path "Assets/Refresh"`. Tout doit tomber sous `Assets/Synty/` ; sinon, s'arrêter.

## La démo, la référence

- **Caméra en perspective**, champ de vision 40° (l'Overview : 60°). Le jeu vise ce rendu, pas forcément cet angle.
- Un **Terrain** peint de calques du pack, le **profil de filtres** `Volume_01`, l'**eau** du pack (son shader marche en
  perspective ; il ne rend pas juste en orthographique).
- Pour comparer une vue du jeu à la démo, photographier les deux sous la même caméra.
- **La carte du jeu** (`Assets/Scenes/Carte.unity`) est née d'une copie de `Demo.unity` dont le menu
  `DICIELA/Scènes/Monter les scènes` n'a gardé que le soleil, le volume de filtres (son profil copié dans
  `Assets/Art/Carte_Filtres.asset`) et le Terrain, relié à un relief vierge (`Assets/Terrain/Carte.asset`). Ses calques
  de sol restent ceux du pack : ne pas les retoucher dans le pack ; une retouche passe par une copie dans `Assets/Art`.

## Personnages et animaux

- **Humains** (les habitants ordinaires) : `SM_Chr_Male_01` à `04`, `SM_Chr_Female_01` à `04`, `SM_Chr_Barmaid_01`,
  `SM_Chr_Butcher_01`.
- **Animaux humanoïdes** (les maîtres, et le corps de la Chimère), en mâle et femelle : Oiseau (`Bird`), Lapin
  (`Bunny`), Chat (`Cat`), Renard (`Fox`), Grenouille (`Frog`), Souris (`Mouse`), Cochon (`Pig`), Raton laveur
  (`Racoon`). Couleurs : douze textures (`01` à `04`, A à C), lues par deux familles de matériaux, celle des
  personnages (`Materials/Alts/Animal_Alts/`, corps, têtes, queues) et celle des objets (`Materials/Alts/`, les autres
  pièces), qui ne diffèrent que par leur shader ; toute partie prend n'importe laquelle des douze.
- **Accessoires à attacher** (`Prefabs/Characters/Chr_Attach/`) : queues (`Tail_01` à `05`), oreilles (cochon, raton
  laveur, gobelin), museau (`Snout_01`), cheveux, barbes, chapeaux (`Hat_01` à `08`), capes, sacs, lunettes, monocle,
  couronne, boucles d'oreilles, fleur, écharpe… **Elles ne sont pas modelées à leur place sur le personnage** : debout,
  dans ses axes, leur pivot à leur point d'accroche ; on les pose sur ce point, puis on les confie à un os (SAVOIR 2.11).
  Sont faits pour une tête humaine (environ trois fois plus petite) : les chapeaux 1 à 5, les cheveux 1 à 13, la
  barbe, les boucles d'oreilles, les oreilles de cochon, de raton et de gobelin, le groin (`Snout_01`) ; le reste est à
  la taille des animaux. Les modèles low poly sont fermés : un chapeau a un fond que la tête traverse. Les pièces du pack
  ne sont pas lisibles en jeu (`isReadable: 0`) : toute mesure se fait dans l'éditeur.
- Tous les personnages sont **riggés** (SkinnedMeshRenderer) mais **le pack n'a aucune animation** : un pack
  d'animations compatible est à acheter par le développeur. Posés tels quels, ils restent bras écartés (leur Animator
  n'a pas de contrôleur).
- **Un personnage a son origine aux pieds et regarde vers +z** : dans la pose d'origine, les yeux sont 31 cm devant la
  tête, les orteils devant les talons (mesuré sur les os, test `ChimeraContentTests`). On les pose donc sans correction.
- **Chaque prefab d'animal porte les seize corps d'animaux**, un seul actif (celui de son nom), sur le même squelette,
  mais les quinze cachés pointent vers un matériau absent du projet : changer d'animal, c'est prendre son prefab.
- **Deux squelettes** : les animaux ont `head` à 1,21 m et mesurent 2,1 à 2,5 m, oreilles comprises (des têtes
  énormes) ; les humains ont `Head` à 1,57 m. L'oiseau, le lapin, la souris et le cochon portent d'office une petite
  cape, sous `CapesMini_01`, sur son propre squelette (`CapeRigMini_*`).
- **La tête de chaque animal se sépare de son corps**, sans triangle commun ; elle-même est faite de morceaux fermés :
  crâne, yeux, sourcils, et, selon l'animal, des oreilles (chat, cochon, lapin, renard, souris ; cousues au crâne chez le
  raton laveur, qu'on en découpe et dont on referme les trous), des touffes (cochon, raton laveur, renard, souris), le
  museau (renard seul) : ceux-là se détachent sans trou (SAVOIR 2.11). Le corps est d'un seul tenant
  (bras, jambes, habits). La tête se sépare selon ses os (`head`, `eyes`, `eyebrows`) et va sur le corps de n'importe quel
  autre animal (SAVOIR 2.11). Le chat, le renard et l'oiseau portent d'office une pièce (deux queues, une huppe), copies
  des modèles de `Chr_Attach`.

## Ce que le pack a, et ce qui lui manque

- **Bâtiments tout faits** (`Prefabs/Buildings/Preset/`) : fermes (2), maisons (4), remises (`Shed`, 4), silo, forge
  (`Smithy`), taverne, tour, entrepôt (`Warehouse`), moulin à vent. Le reste se compose avec les pièces modulaires
  (murs, toits, auvent).
- **Accessoires utiles** : roue à eau (`SM_Prop_WaterWheel_01`), ponts (`SM_Bld_Bridge_01` à `03`), lanternes (3),
  bancs, fontaines, puits, enclume, établis, brouette, charrettes (`Vehicles/`, 4), bûches, souches, piles de bois et de
  rochers, planches, haches, pioche, étendoirs, bottes de foin.
- **Pas de modèle tout fait** pour la tente, l'étal, la scierie, le moulin à eau (seulement sa roue), l'atelier de tissu
  (ni métier à tisser ni rouet), la carrière, les échafaudages : le développeur les compose, sinon un remplaçant simple.
- **Cultures** : des plants de citrouilles (`SM_Env_Pumpkin_Leaves_01`, `02`), des feuillages
  (`SM_Env_Plant_Leaves_01`, `02`), des vignes (`SM_Env_Vine_*`), des fleurs sauvages, un buisson, des jardinières
  (`SM_Prop_Planter_*`) et des légumes géants (`Carrot_Giant`, `Watermelon_Giant`, `Zucchini_Giant`, dans
  `Props/SM_Prop_Food_*`) ; le sol labouré est le calque `Village_Dirt_Farm_01`. Pas de blé en modèle (seulement une
  texture `Wheat_01`), ni lin, ni moutons. Beaucoup de fruits et légumes en objets (`Props/SM_Prop_Food_*` : pomme,
  carotte, poireau, laitue, pomme de terre, tomate, citrouille, fraise, cerise, citron, orange, champignon, ail…).
- **Saisons** : aucun modèle d'hiver (ni toits enneigés, ni arbres nus). Seulement de la neige qui tombe
  (`FX_Snow_01`), une texture de sol enneigé (`Snow_01`, sans calque), un rocher enneigé, un arbre (`Tree_04`) en
  quatre couleurs d'automne, et un masque de neige dans les shaders partagés
  (`PolygonGeneric/Shaders/SubGraphs/SnowMask`), peut-être pour enneiger n'importe quel modèle : à vérifier dans
  l'éditeur.
- **Calques de sol** (`Materials/Terrain/`) : deux herbes, fleurs, chemin, terre, terre de ferme, boue, galets de
  rivière, sol planté.

## Pièges

- **Un preset de bâtiment a son origine au niveau de sa porte** (Shed_02 : à la porte même), ses fondations en
  dessous, et des dizaines de pièces (Shed_02 : 62, dont le mur à porte `SM_Bld_Wall_Door_02`, et des accessoires
  autour : tonneaux, caisses, vignes). On ne le pose jamais tel quel : le menu `DICIELA/Contenu/Préparer les modèles` le
  fusionne, le tourne porte au nord, le centre et garde son origine comme sol (SAVOIR 2.9).
- **Les niveaux de détail (LOD) sont dans les pièces**, pas dans le preset : une recherche dans le texte du preset ne les
  voit pas.
- **Viser un modèle par ses triangles demande « Read/Write »** coché à l'import de ses maillages ; sans lui, dans un jeu
  compilé, les triangles ne se lisent pas.
- **L'échelle** : les packs Synty sont faits pour un mètre par unité (un humain mesure environ 1,8 unité), à vérifier
  sur un personnage à l'import. Le jeu utilise le pack à ×1 : on n'écrase jamais le pack pour le faire entrer dans une
  grille (une échelle forcée a obligé à retoucher chaque modèle et chaque effet).
- **La fenêtre d'import** fige l'éditeur si elle est ouverte par le CLI : jamais d'import de `.unitypackage` au CLI ;
  soit le développeur importe, soit on copie les fichiers de l'archive (plus haut).
- **`SyntyPackageHelper` ouvre une fenêtre modale au premier changement du projet** (« requires the package
  com.unity.shadergraph… Install / Skip ») : elle fige le CLI jusqu'à un clic du développeur. Shader Graph vient
  déjà avec URP ; l'outil ne lit que le texte de `manifest.json`. Répondre **Install** (il déclare le paquet).
- **Le pack demande les modules intégrés Terrain et TerrainPhysics** (`com.unity.modules.terrain`, `terrainphysics`) :
  sans eux, `PNB_Core/Scripts/HeightMapAssigner.cs` ne compile pas (`CS1069`), et la démo perd son Terrain.
- **Les autres packs Synty ne vont pas avec celui-ci** : le Sampler Pack (Adventure, City, Knights, Prototype,
  Starter) est dans un vieux format que Unity 6.6 ne lit qu'avec 3 700 avertissements, ses matériaux utilisent le
  shader Standard de l'ancien pipeline (roses en URP), et il s'installe dans `Assets/SyntyStudios/`, que Git
  n'ignore pas. Il a été retiré le 27/09/2026 (il reste dans le cache de l'Asset Store).

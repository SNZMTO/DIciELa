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
- `PolygonFantasyVillage/Scenes/` : `Demo.unity` (la référence de beauté), `Overview.unity`, `Buildings.unity`,
  `Demo_Terrain.asset`, `Volume_01.asset` (profil de filtres de la démo).
- **L'outil de recherche du code saute ce dossier** (ignoré par Git) : chercher par son chemin exact, ou par
  PowerShell (`Get-ChildItem -Recurse | Select-String`).
- **Lister le pack sans l'importer** : il est dans le cache de l'Asset Store (`%APPDATA%\Unity\Asset Store-5.x\Synty
  Studios\…\POLYGON - Fantasy Village - Art by Synty.unitypackage`, 317 Mo). En Git Bash,
  `tar -xzf "<paquet>" --wildcards '*/pathname' --to-command='cat; echo' > liste.txt` écrit le chemin de ses 9 910
  fichiers sans rien extraire.

## La démo, la référence

- **Caméra en perspective**, champ de vision 40° (l'Overview : 60°). Le jeu vise ce rendu, pas forcément cet angle.
- Un **Terrain** peint de calques du pack, le **profil de filtres** `Volume_01`, l'**eau** du pack (son shader marche en
  perspective ; il ne rend pas juste en orthographique).
- Pour comparer une vue du jeu à la démo, photographier les deux sous la même caméra.

## Personnages et animaux

- **Humains** (les habitants ordinaires) : `SM_Chr_Male_01` à `04`, `SM_Chr_Female_01` à `04`, `SM_Chr_Barmaid_01`,
  `SM_Chr_Butcher_01`.
- **Animaux humanoïdes** (les maîtres, et le corps de la Chimère), en mâle et femelle : Oiseau (`Bird`), Lapin
  (`Bunny`), Chat (`Cat`), Renard (`Fox`), Grenouille (`Frog`), Souris (`Mouse`), Cochon (`Pig`), Raton laveur
  (`Racoon`). Variantes de couleur : `Materials/Alts/Animal_Alts/` (A, B, C par animal).
- **Accessoires à attacher** (`Prefabs/Characters/Chr_Attach/`) : queues (`Tail_01` à `05`), oreilles (cochon, raton
  laveur, gobelin), museau (`Snout_01`), cheveux, barbes, chapeaux (`Hat_01` à `08`), capes, sacs, lunettes, monocle,
  couronne, boucles d'oreilles, fleur, écharpe…
- Tous les personnages sont **riggés** (SkinnedMeshRenderer) mais **le pack n'a aucune animation** : un pack
  d'animations compatible est à acheter par le développeur.
- **Les corps sont d'un seul tenant** : pas de tête d'une bête sur le corps d'une autre sans retoucher les modèles.

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

- **Les prefabs ont leur origine n'importe où** (un coin, une porte, un socle enterré) : les recentrer à la pose d'après
  la boîte de leurs seuls maillages (jamais les particules), plus un décalage fin tenu dans la donnée du modèle.
- **Viser un modèle par ses triangles demande « Read/Write »** coché à l'import de ses maillages ; sans lui, dans un jeu
  compilé, les triangles ne se lisent pas.
- **L'échelle** : les packs Synty sont faits pour un mètre par unité (un humain mesure environ 1,8 unité), à vérifier
  sur un personnage à l'import. Le jeu utilise le pack à ×1 : on n'écrase jamais le pack pour le faire entrer dans une
  grille (une échelle forcée a obligé à retoucher chaque modèle et chaque effet).
- **Importer le pack** est une opération du développeur (jamais au CLI : la modale d'import fige l'éditeur).

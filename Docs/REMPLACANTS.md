# DICIELA — les remplaçants

Tout ce qui, dans le jeu, est provisoire : les images que le code dessine faute de mieux, les modèles en boîte, les
textes neutres. Rien ici ne vient du pack. Chaque nouveau remplaçant s'ajoute ici le jour même ; quand tu en remplaces
un, il sort de la liste.

**Pourquoi des dessins.** Le pack POLYGON Fantasy Village n'a que de la 3D : aucune icône, aucun curseur, aucune image
d'interface. Les icônes sont donc dessinées par le code (`Assets/Scripts/Editor/…Icons.cs`) et rangées dans
`Assets/UI/Icones` par le menu `DICIELA/Interface/Générer les images de l'interface`.

**Pour remplacer une icône** : dépose ton image dans `Assets/UI/Icones` et dis-le-moi. Je change son adresse dans
`Assets/UI/Hud.uss` et je retire son dessin du générateur, qui sinon l'écraserait au prochain passage. Une image
vectorielle reste nette à toute taille ; un PNG marche aussi, s'il fait deux à trois fois sa taille à l'écran.

## Les icônes des outils

| Icône (`Assets/UI/Icones/…`) | Ce qu'elle montre | Où tu la vois |
|---|---|---|
| `outils` | un marteau croisé avec une hache | l'icône des outils, en bas à droite ; la case Outils de la roue |
| `defricher` | une hache | Retirer, dans la roue ; la tâche « défricher » d'un poste |
| `deplacer` | quatre flèches | Déplacer, dans la roue |
| `chemins` | un chemin en pointillés | Chemins, dans la roue |
| `place` | la zone de la Place et deux places de charrette | la Place, dans la roue ; la zone, dans l'éventail des sols et sur la carte |
| `pinceau`, `gomme`, `trace`, `forme`, `place-charrette` | les cinq modes des outils de sol | les cases 1 à 5 de la roue de la Place et de Chemins ; la carte |
| `cercle`, `rectangle`, `polygone` | les formes du mode Forme | l'éventail de la case Forme ; la carte |
| `droit`, `courbe` | les styles d'un tracé | l'éventail de la case Tracé ; la carte |
| `sans-symetrie`, `miroir`, `double-miroir`, `etoile` | les symétries | la case Symétrie et son éventail ; la carte |
| `prevision` | une bulle de pensée à l'encre, une maison claire dedans | le bouton lavande de la prévision, au-dessus de l'icône des outils |

## Les curseurs

Un curseur par outil et par mode (décision 224). Son halo prend la couleur de l'outil en jeu ; sa pointe, là où
l'outil agit, est réglée dans `Assets/Scripts/UI/ToolCursorShapes.cs` (à revoir si un dessin change). Pas encore vus en
jeu.

| Icône (`Assets/UI/Icones/…`) | Pour | Dessin |
|---|---|---|
| `curseur-pinceau` | le pinceau | celui de l'icône `pinceau`, cerclé d'un halo |
| `curseur-gomme` | la gomme | celui de `gomme`, cerclé d'un halo |
| `curseur-plume` | le tracé | neuf : une plume |
| `curseur-viseur` | la forme | neuf : un viseur |
| `curseur-place-charrette` | la place de charrette | celui de `place-charrette`, cerclé d'un halo |
| `curseur-marteau` | une construction en main | celui de `batir`, cerclé d'un halo |
| `curseur-hache` | Retirer | celui de `defricher`, cerclé d'un halo |
| `curseur-main` | Déplacer | neuf : une main ouverte |

## Les icônes des métiers et des tâches

| Icône (`Assets/UI/Icones/…`) | Ce qu'elle montre | Où tu la vois |
|---|---|---|
| `batir`, en attendant | un marteau (l'icône de la tâche « bâtir ») | la case des Métiers dans la roue de Construire |
| `conduire` | une charrette | la tâche « conduire » d'un poste |
| `garnir` | un panier | la tâche « garnir » d'un poste |

## L'écran de création

| Icône (`Assets/UI/Icones/…`) | Ce qu'elle montre | Où tu la vois |
|---|---|---|
| `ligne-tete`, `ligne-corps`, `ligne-queue`, `ligne-oreilles`, `ligne-cheveux`, `ligne-chapeau`, `ligne-visage`, `ligne-barbe`, `ligne-ceinture` | une silhouette crème par ligne de choix | les lignes de l'écran de création de la Chimère |

## Les sols

| Quoi | Ce que c'est | Où tu le vois |
|---|---|---|
| La vignette d'un sol | la texture de son calque, recadrée | l'éventail des sols de la roue ; la pastille du curseur ; la carte |

## Dans la ville

| Quoi | Ce que c'est | Où tu le vois | Pour le remplacer |
|---|---|---|---|
| Le fanion d'un arbre marqué | un mât et une flamme, translucides | au pied d'un arbre ou d'un rocher marqué à retirer | `Assets/Scripts/View/MarkMesh.cs` ; le pack a cinq drapeaux, `SM_Prop_Flag_01` à `05` |
| Le cadre d'une place de charrette | un cadre blanc et sa pointe | sur la Place, en mode construction | `Assets/Scripts/View/DrapedMesh.cs` |
| Le cadre de sélection | un rectangle de lin translucide bordé de bois | quand on glisse avec Retirer | `.cadre-selection`, dans `Assets/UI/Hud.uss` |
| La rêverie de la prévision | un voile : une lavande pâle sur tout l'écran, légère au milieu, dense aux bords, en fondu | en prévision (V) | l'animation où la Chimère se projette, avec le pack d'animations ; d'ici là, sa couleur est `--reverie` dans `Assets/UI/Fusion.uss`, ses opacités dans `Assets/Scripts/Editor/ReveriePainter.cs`, puis le menu des images de l'interface |
| L'entrepôt | une boîte à sa taille, sa porte marquée | dans la ville ; sa vignette dans la roue de Construire et sur la carte | le champ « Source Model » de sa fiche (`Assets/Data/Structures/Entrepot.asset`) |
| La loge des bâtisseurs | une boîte à sa taille, sa porte marquée | dans la ville ; sa vignette dans la roue de Construire et sur la carte | le champ « Source Model » de sa fiche (`Assets/Data/Structures/Loge.asset`) |

Pour une construction : le champ « Source Model » de sa fiche reçoit un bâtiment du pack ou ta composition, puis les menus
`DICIELA/Contenu/Préparer les modèles` (le modèle posé dans le jeu) et `DICIELA/Interface/Photographier les
constructions` (sa vignette).

## Les textes provisoires

| Quoi | Le texte d'aujourd'hui | Où tu le vois |
|---|---|---|
| Les noms des pièces de la Chimère | un premier jet neutre : « Chat A », « Renard B »… | l'écran de création |
| Les noms des habitants | leur métier : « Le bâtisseur », « La fermière », « La charretière » | les fiches, les infobulles |
| Les répliques des idées (prévues, pas encore en jeu) | une phrase neutre : « J'ai une idée : un pont, pour passer la rivière. » | le dialogue d'un habitant qui apporte une idée |
| La voix de la Chimère | rien encore : elle viendra avec son histoire | — |

Les textes sont dans `Assets/Localization/strings.csv`.

## Dessiné par le code, mais pas provisoire

Le style de l'interface, d'après la maquette que tu as validée : les cadres de bois, les boutons, les rubans
(`Assets/UI/Images`, générés), les feutres, les creux et les fanions du stock (dessinés à l'écran), et les icônes de la
maquette (`bois`, `pierre`, `maison`, `lit`, `soleil`, `croix`, `precedent`, `suivant`, `porter`, `batir`). Les polices
Lilita One et Nunito sont libres (licence OFL). Tu peux tout changer ; rien de tout cela n'attend un remplaçant.

## Ce qui vient du pack, tel quel

La cabane (`Bld_Preset_Shed_02`), le campement (des objets du pack composés à ta demande, décision 170), les arbres, les
rochers, les personnages, et les sols (la terre est une copie du calque du pack, répétée tous les 5 m : décision 222).

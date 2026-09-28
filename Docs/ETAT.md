# DICIELA — état actuel

**Branche** : `etape-12-creation`, fusionnée dans `main` et poussée.

**Fait** : l'étape 12, l'écran de création de la Chimère (décisions 98 à 102).
- Une partie s'ouvre sur un panneau à droite, la Chimère à venir au point d'arrivée, face à la caméra : corps (16),
  couleur (12), puis queue, oreilles, museau, tête, visage, cou, dos, ceinture, « Aucun » en tête.
- Le temps attend « C'est parti » (ou Entrée) : elle naît là où était l'aperçu, la barre paraît, la vue recule.
- L'apparence est à l'habitant ; ses fiches, dans `Assets/Data/Apparence`, viennent du pack. Chaque pièce se pose sur
  son point (« Anchor », en mètres depuis les pieds), puis s'accroche à l'os de son emplacement. La planche
  `DICIELA/Album/Planche de la Chimère…` (après « C'est parti ») montre les 39 pièces.
**EditMode 137/137, PlayMode 83/83**, Play court sans erreur (414 pas après « C'est parti »). Album : 3 captures.

**À vérifier par toi** (Play) : l'écran s'ouvre, le temps ne part pas. Flèches du corps : l'animal change et reprend
sa couleur. Une ligne de pièce : « Aucun », chaque pièce, « Aucun ». A et E tournent autour d'elle. « C'est parti » :
elle naît, la barre montre « Tente ». Régler une pièce : son « Anchor », puis un aller-retour de flèche sur sa ligne.

**À toi** :
1. La place des pièces. Les chapeaux sont redescendus par une règle mesurée, à juger. Pas vus de près : cou, dos,
   ceinture (une bourse au bassin est sans doute dans le ventre), les trois `Bow` (nœuds ou arcs ?).
2. Les noms (un premier jet neutre dans `strings.csv`) et l'offre (les chapeaux 1 à 5 sont à la taille des humains).
3. La caméra de l'écran reste à 15 m au plus près ; ma reco : un cran plus près pour cet écran seul. Et le modèle de
   la tente (GDD 16), une boîte en attendant.

**Prochaine étape** : l'étape 13 ; ma reco : le tas de départ et son poste « à tout faire » (GDD 6), premier travail de
la Chimère, qui touche aux tâches : arrêt pour `/code-review ultra` avant sa fusion (décision 77). Ménage vers la 15.
**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Montre-moi le plan court de l'étape suivante : le tas de départ et son poste « à tout
> faire ».

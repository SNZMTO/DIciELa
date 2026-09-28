# DICIELA — état actuel

**Branche** : `etape-11-tente`, poussée, pas fusionnée : elle touche la journée de la Chimère (sa marche vers son lit),
et la décision 77 fait attendre une étape qui touche la marche. La fusion est ton choix (plus bas).

**Fait** : l'étape 11, la tente (décisions 92 à 97).
- La barre ne propose que la tente tant que la Chimère n'a pas de logis. Plantée d'un coup, sans fantôme, la tente
  quitte la barre, qui propose alors la cabane ; une seconde tente est refusée (« Déjà posée »).
- La tente se voit en boîte de 3 × 3 m, porte marquée (Défauts). `Préparer les modèles` fait cette boîte pour toute
  fiche sans modèle du pack.
- Toucher la tente (clic gauche, rien en main) envoie la Chimère s'y coucher : elle marche jusqu'à sa porte et
  disparaît. La toucher encore la réveille, même en chemin. Porte hors d'atteinte : elle ne part pas, ou renonce.
- L'outil « choisir » vise le modèle, pas le sol. L'écran de création devient l'étape 12.
**EditMode 132/132, PlayMode 75/75**, Play court sans erreur (833 pas, le cycle complet dans le journal). Album : 4 captures.

**À vérifier par toi** (Play) : la barre ne montre que « Tente ». Pose-la : elle se dresse d'un coup, opaque, et la
barre montre « Cabane ». Clique sur la tente : la Chimère y va et disparaît devant la porte ; reclique : elle ressort.
Reclique pendant qu'elle y va : elle s'arrête et flâne. Un clic sur le sol ou sur une cabane ne fait rien.

**À toi** :
1. Fusionner sans `/code-review ultra`, ou lancer la revue sur la branche. Ma reco : fusionner. `Walk` n'a pas changé,
   seule la journée de l'habitant gagne une branche, gardée par neuf tests. Garde les deux revues gratuites pour les
   tâches et la sauvegarde. Sur ton mot : `git fetch . etape-11-tente:main`, puis `git push origin main`.
2. Le modèle de la tente (GDD 16) : une composition du pack, à toi ; la boîte en attendant.

**Prochaine étape** : l'étape 12, l'écran de création de la Chimère (corps, couleur, morceaux, accessoires du pack).
**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Montre-moi le plan court de l'étape suivante : l'écran de création de la Chimère.

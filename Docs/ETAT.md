# DICIELA — état actuel

**Branche** : `etape-8b-pose-visible`, fusionnée dans `main`. **Étapes 6 à 8b finies** : la carte, la caméra de
gestion, et la pose des constructions, visible (décisions 60 à 64), après des ménages du code (57 à 59).

**Ce qui marche** : EditMode 49/49, PlayMode 30/30, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout, sans jamais toucher une carte existante. En jeu : la barre en bas (Logement, Cabane) ; l'aperçu suit la
souris, vert ou rouge avec sa raison ; R tourne ; le clic gauche pose un fantôme ; un clic droit bref ou Échap range
l'outil. Les captures de l'album se mettent en scène par **DICIELA > Album > …**, en Play.

**À vérifier par lui** (**DICIELA > Jouer**) : poser, tourner, refuser, ranger l'outil ; un clic sur la barre ne pose
rien. Le sens de la rotation au bouton du milieu (fiche `CameraSettings`) et les couleurs de l'aperçu et du fantôme
(fiche `PlacementSettings`) se règlent en jouant.

**Questions pour toi** :
1. Le modèle de la cabane (et de la tente, que le pack n'a pas) : ton choix d'art. En attendant, une boîte.
2. La taille de la carte, avant de la peindre (« À toi », GDD 2).
3. La suite. Ma reco : étape 9, les chemins : l'A* sur le maillage (SAVOIR 2.1), vérifié contre un Dijkstra, sans
   rien de visible encore ; puis la Chimère qui marche jusqu'à son poste.

**À savoir** : 35 « À toi » dans le GDD, chacun avec ma reco. À surveiller : la file d'événements « en fin de couche »
(33), avant le premier système qui en écoute un autre ; `CameraRig` (194 lignes) se découpe au prochain changement
de la caméra (la prise du sol à part) ; la caméra qui monte quand une colline cache le point visé ; suivre un habitant.

**Prochaine étape** : 9, les chemins sur le maillage, si tu valides la question 3.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis la prochaine étape : montre-moi
> le plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

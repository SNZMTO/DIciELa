# DICIELA — état actuel

**Branche** : `main`, à jour. **Étapes 6 à 9 finies** ; la 9 (les trajets) ne change rien à l'écran.
EditMode 65/65, PlayMode 56/56, Play court sans erreur. **DICIELA > Scènes > Monter les scènes** monte tout.

**En jeu** : la barre en bas ; l'aperçu suit la souris, vert ou rouge avec sa raison ; R tourne (bref : un cran ;
tenu : au degré) ; le clic gauche pose un fantôme ; clic droit bref ou Échap range l'outil. La caméra part à 57 m, le
bouton du milieu tourne et incline, les ombres et le brouillard suivent le zoom (fiche `CameraSettings`).

**Les trajets** (décisions 73 et 74) : un fantôme bloque le passage dès sa pose. `World.PathFinder` cherche le trajet
le moins cher d'un piéton sur le maillage (A*, huit voisines, sans couper les coins, à égalité le plus avancé
d'abord), sans allocation ; vérifié contre un Dijkstra sur 400 terrains tirés au hasard et sur une carte vide de
2 000 × 2 000 mailles. Personne ne marche encore : c'est l'étape 9b.

**Réponses du développeur** : la cabane prend `Bld_Preset_Shed_02` du pack (décision 75), House_03 est pressentie
pour la maison ; la carte garde 500 m pour l'instant. Le menu temporaire de la planche est supprimé.

**Prochaine étape : 8c, le modèle de la cabane** (son plan court est à valider) : un menu mesure sur le modèle son
empreinte et sa porte (SAVOIR 2.9) et les range dans la fiche, avec son orientation ; la boîte de remplacement et son
monteur disparaissent (la décision 62 est remplacée) ; captures pour l'album. Ensuite, 9b : l'injoignable su tout de
suite (composantes), le lissage, la marche, puis la Chimère.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis la prochaine étape : montre-moi
> le plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

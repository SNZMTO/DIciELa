# DICIELA — état actuel

**Branche** : `main`, à jour. **Étapes 6 à 8b finies**, puis la revue du code et de la caméra (décisions 65 à 72).
EditMode 53/53, PlayMode 56/56, Play court sans erreur. **DICIELA > Scènes > Monter les scènes** monte tout.

**En jeu** : la barre en bas ; l'aperçu suit la souris, vert ou rouge avec sa raison ; R tourne (bref : un cran ;
tenu : au degré) ; le clic gauche pose un fantôme ; clic droit bref ou Échap range l'outil. La caméra part à 57 m, le
bouton du milieu tourne et incline, les ombres et le brouillard suivent le zoom (fiche `CameraSettings`).

**Réponses du développeur** : la carte garde 500 m pour l'instant (tranchée plus tard, en posant une ville sur un
Terrain nu) ; la suite est l'étape 9, les chemins.

**Questions pour toi** (ma reco entre parenthèses) :
1. Le modèle de la cabane : Shed_01, Shed_02, Shed_03, Shed_04 ou House_03 du pack (Shed_02 : la plus petite fermée,
   6,5 × 8,5 m). La fiche prendra sa taille et sa porte. La planche se remet en scène en Play par **DICIELA > Album >
   Planche des cabanes candidates** : ce menu est temporaire (`ModelBoardMenu.cs`, non suivi par Git), à supprimer
   une fois le choix fait.
2. Un fantôme (posé, pas bâti) bloque-t-il le passage ? (Oui, dès qu'il est posé : une maille, un objet, une seule
   vérité pour la pose et le passage.)
3. Le plan de l'étape 9 ci-dessous.

**Étape 9, les chemins** (rien de visible, SAVOIR 2.1) : une maille se traverse si elle est libre ou devant une porte,
au même prix partout pour un piéton ; un A* dans `Simulation.Land` (huit voisines, jamais de coin coupé, plusieurs
départs et arrivées, test d'arrivée à la sortie de la file, heuristique octile, tas binaire, tampons gardés, sans
allocation par appel) ; des tests EditMode contre un Dijkstra sur 400 maillages tirés au hasard, et des cas écrits à
la main. Ensuite, en 9b : l'injoignable su tout de suite (composantes), le lissage, la marche, puis la Chimère.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis la prochaine étape : montre-moi
> le plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

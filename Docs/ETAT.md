# DICIELA — état actuel

**Branche** : `menage-4-revue`, fusionnée dans `main`. **Étapes 6 à 8b finies**, puis une revue complète du code et de
la caméra (décisions 65 à 72) : les bugs de la pose corrigés, la caméra retouchée, les gestes testés.

**Ce qui marche** : EditMode 53/53, PlayMode 56/56, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout et réenregistre les fiches, sans toucher une carte existante. En jeu : la barre en bas ; l'aperçu suit la
souris, vert ou rouge avec sa raison ; R tourne (bref : un cran ; tenu : au degré) ; le clic gauche pose un fantôme ;
clic droit bref ou Échap range l'outil. La caméra part à 57 m, le bouton du milieu tourne et incline, les ombres et
le brouillard suivent le zoom ; un seul soleil, celui de la carte.

**À vérifier par lui** (**DICIELA > Jouer**) : la lumière, plus sombre (celle du pack) ; incliner au bouton du milieu ;
le brouillard qui recule en dézoomant ; la vue de départ. Tout se règle en jouant, dans la fiche `CameraSettings`.

**Questions pour toi** :
1. Le modèle de la cabane (et de la tente, que le pack n'a pas) : ton choix d'art. En attendant, une boîte. Une maison
   du pack avec cheminée demandera le module des particules pour sa fumée : je l'ajoute sur ton accord.
2. La taille de la carte, avant de la peindre (« À toi », GDD 2).
3. La suite. Ma reco : étape 9, les chemins : l'A* sur le maillage (SAVOIR 2.1), vérifié contre un Dijkstra, sans
   rien de visible encore ; puis la Chimère qui marche jusqu'à son poste.

**À savoir** : 35 « À toi » au GDD, avec ma reco sauf ce qui est à toi seul (histoire, Chimère, modèles, quartiers).
À surveiller : la file « en fin de couche » (33) ; la caméra quand une colline cache le point visé ; suivre un habitant.

**Prochaine étape** : 9, les chemins sur le maillage, si tu valides la question 3.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis la prochaine étape : montre-moi
> le plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

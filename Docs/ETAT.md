# DICIELA — état actuel

**Branche** : `menage-2-spaghetti`, fusionnée dans `main`. **Étapes 6, 7 et 8a finies** (la carte, la caméra, la pose
dans la simulation, sans image encore), puis un ménage du code : seule la simulation change la simulation (57).

**Ce qui marche** : EditMode 39/39, PlayMode 7/7, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout, sans jamais toucher une carte existante. `Jeu` charge `Carte` (relief vierge de 500 m), que le monde lit et
que la caméra montre. La pose suit le GDD (dans la carte, 15° au plus, sans chevaucher, rien devant une porte).

**À vérifier par lui** (**DICIELA > Jouer**) : ZQSD ou les flèches, A et E, la molette, le clic droit glissé, la souris
au bord ; on ne sort pas de la carte. **Avant de peindre la carte** : sa taille (« À toi », GDD 2).

**Quatre questions avant l'étape 8b** (la pose visible) :
1. La touche qui tourne une construction (GDD 9 : brève = 15°, tenue = au degré). Ma reco : R.
2. Comment choisir quoi construire (le plan de l'interface est un « À toi »). Ma reco : une barre en bas, par famille.
3. Le modèle de la cabane (et de la tente, que le pack n'a pas) : ton choix d'art. En attendant, une boîte simple.
4. Deux retouches de forme avant que 8b s'appuie dessus (ma reco : oui) : la place et l'angle d'une construction en
   une seule valeur (`Placement`, comme Timberborn) ; les règles de pose à part de la liste des constructions.

**À savoir** : 34 « À toi » dans le GDD, chacun avec ma reco. Restent du modèle, par choix : le niveau de qualité
Mobile et `SampleSceneProfile`. À surveiller : les événements partent tout de suite ; la file « en fin de couche »
(décision 33) se code avant le premier système qui en écoute un autre.

**Prochaine étape** : 8b. Poser un bâtiment, visible : la visée sur le sol, le fantôme vert ou rouge (la même fonction
que l'ordre), tourner, poser au clic, le modèle posé (une seule routine de pose, SAVOIR 2.9), une première fiche.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux quatre questions d'ETAT avec moi, puis étape 8b : montre-moi le
> plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

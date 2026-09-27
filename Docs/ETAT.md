# DICIELA — état actuel

**Branche** : `menage-3b-camera`, fusionnée dans `main`. **Étapes 6, 7 et 8a finies** (la carte, la caméra, la pose
dans la simulation, sans image encore), puis des ménages : seule la simulation change la simulation (57), la place et
les règles de pose (58), la caméra qui zoome vers le pointeur et tourne au bouton du milieu (59), trois bugs corrigés.

**Ce qui marche** : EditMode 40/40, PlayMode 14/14, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout, sans jamais toucher une carte existante. `Jeu` charge `Carte` (relief vierge de 500 m), que le monde lit et
que la caméra montre. La pose suit le GDD (dans la carte, 15° au plus, sans chevaucher, rien devant une porte).

**À vérifier par lui** (**DICIELA > Jouer**) : ZQSD ou les flèches, A et E, la molette (vers le pointeur), le clic
droit glissé (le sol suit la souris), le bouton du milieu glissé (la vue tourne), la souris au bord.

**Questions pour toi** (les trois premières avant l'étape 8b, la pose visible) :
1. La touche qui tourne une construction (GDD 9 : brève = 15°, tenue = au degré). Ma reco : R.
2. Comment choisir quoi construire (le plan de l'interface est un « À toi »). Ma reco : une barre en bas, par famille.
3. Le modèle de la cabane (et de la tente, que le pack n'a pas) : ton choix d'art. En attendant, une boîte simple.
4. **Avant de peindre la carte** : sa taille (« À toi », GDD 2).

**À savoir** : 34 « À toi » dans le GDD, chacun avec ma reco. À surveiller : la file d'événements « en fin de couche »
(33), à coder avant le premier système qui en écoute un autre ; la caméra qui monte quand une colline cache le point
visé, à coder quand la carte aura ses collines ; suivre un habitant, quand il y en aura.

**Prochaine étape** : 8b. Poser un bâtiment, visible : la visée sur le sol, l'aperçu vert ou rouge (les règles de
pose, comme l'ordre), tourner, poser au clic, le modèle posé (une seule routine de pose, SAVOIR 2.9), une fiche.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis étape 8b : montre-moi le plan
> court, attends mon accord, code-la, vérifie-la, captures pour l'album.

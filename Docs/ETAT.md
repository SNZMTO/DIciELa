# DICIELA — état actuel

**Branche** : `etape-8a-pose`, fusionnée dans `main`. **Étapes 6, 7 et 8a finies** : la carte, la caméra de gestion,
et la pose des constructions dans la simulation (sans image encore). Première capture dans l'album.

**Ce qui marche** : EditMode 39/39, PlayMode 7/7, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout (réexécutable ; ne touche jamais une carte existante). `Jeu` charge `Carte` (née de la démo du pack, relief
vierge de 500 m) ; le monde reçoit son relief ; la caméra la montre, filtres actifs. La pose suit le GDD : dans la
carte, 15° au plus, sans chevaucher, rien devant une porte (1,5 m, Défaut), par l'ordre « poser ».

**À vérifier par lui** (**DICIELA > Jouer**) : ZQSD ou les flèches déplacent, A et E tournent, la molette zoome en
abaissant la vue, le clic droit glissé tire le sol, la souris au bord fait défiler ; on ne sort pas de la carte.
Réglages : `Assets/Data/Settings/CameraSettings.asset`. **Avant de peindre la carte** : sa taille (« À toi », GDD 2).

**Trois questions avant l'étape 8b** (la pose visible) :
1. La touche qui tourne une construction (GDD 9 : brève = 15°, tenue = au degré). Ma reco : R.
2. Comment choisir quoi construire (le plan de l'interface est un « À toi »). Ma reco : une barre en bas, par famille.
3. Le modèle de la cabane (et de la tente, que le pack n'a pas) : ton choix d'art. En attendant, une boîte simple.

**À savoir**
- 34 « À toi » dans le GDD, chacun avec ma reco (« je te laisse faire » applique les recos).
- Restent du modèle, par choix : le niveau de qualité Mobile et `SampleSceneProfile` (les filtres des réglages URP).

**Prochaine étape** : 8b. Poser un bâtiment, visible : la visée sur le sol, le fantôme vert ou rouge (la même fonction
que l'ordre), tourner, poser au clic, le modèle posé (une seule routine de pose, SAVOIR 2.9), une première fiche.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux trois questions d'ETAT avec moi, puis étape 8b : montre-moi le
> plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

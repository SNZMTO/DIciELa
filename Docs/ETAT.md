# DICIELA — état actuel

**Branche** : `etape-7-camera`, fusionnée dans `main`. **Étapes 6 et 7 finies** : la carte existe, le monde la reçoit,
et la caméra de gestion la montre. Première capture dans l'album.

**Ce qui marche** : EditMode 29/29, PlayMode 7/7, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout (réexécutable ; ne touche jamais une carte existante). `Carte` : née de la démo du pack (soleil, filtres,
Terrain), relief vierge de 500 m. `Jeu` la charge ; le monde reçoit son relief ; la caméra la montre, filtres actifs.
`JeuTest` charge `CarteTest` (64 m) : les tests ne touchent jamais la vraie carte.

**À vérifier par lui** (**DICIELA > Jouer**) : ZQSD ou les flèches déplacent, A et E tournent, la molette zoome en
abaissant la vue, le clic droit glissé tire le sol, la souris au bord de l'écran fait défiler ; on ne sort jamais de la
carte. Les réglages sont dans `Assets/Data/Settings/CameraSettings.asset`. **Avant de peindre la carte** : sa taille
(« À toi », GDD 2) ; pour en changer, supprimer `Carte.unity` et `Assets/Terrain/Carte.asset`, régler
`CarteNeuve.asset`, relancer le menu.

**À savoir**
- L'asset d'actions du modèle est retiré ; celui du jeu est `Assets/Input/Diciela.inputactions`.
- Restent du modèle, par choix : le niveau de qualité Mobile et `SampleSceneProfile` (les filtres des réglages URP).
- 34 « À toi » dans le GDD, chacun avec ma reco (« je te laisse faire » applique les recos) ; nouveau : les touches de
  la caméra et le bouton du glisser.

**Prochaine étape** : 8. Poser un bâtiment : le maillage (SAVOIR 2.1), les constructions et leur pose (pente, eau,
chevauchement), l'ordre « poser » et son aperçu (fantôme), la visée (SAVOIR 2.7) ; une première fiche de bâtiment.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 8 : poser un bâtiment. Montre-moi le plan court, attends mon accord, puis
> code-le et vérifie-le ; des captures pour l'album.

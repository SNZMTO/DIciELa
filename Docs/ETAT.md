# DICIELA — état actuel

**Branche** : `etape-6-carte`, fusionnée dans `main`. **Étape 6 finie** : la carte existe et le monde la reçoit.

**Ce qui marche** : EditMode 29/29, PlayMode 3/3, Play court sans erreur. **DICIELA > Scènes > Monter les scènes**
monte tout (réexécutable, ne touche jamais une carte existante). La scène `Carte` est née de la démo du pack (soleil,
filtres, Terrain) avec un relief vierge : 500 m, 1025 points, sol à 20 m, calques du pack. `Jeu` la charge et en fait
la scène active ; le monde reçoit son relief (hauteur et pente). `JeuTest` charge `CarteTest` (plate, 64 m) : les tests
ne touchent jamais la vraie carte.

**À vérifier par lui** : ouvrir `Assets/Scenes/Carte.unity` : un grand terrain d'herbe, la lumière et les filtres de
la démo. **Avant de peindre** : la taille (« À toi », GDD section 2) ; pour en changer, supprimer `Carte.unity` et
`Assets/Terrain/Carte.asset`, régler `Assets/Data/Settings/CarteNeuve.asset`, relancer le menu. Puis peindre : relief,
calques du pack. **DICIELA > Jouer** : la console dit « carte de 500 × 500 m ».

**À savoir**
- Le Terrain (19 Mo) passe par LFS. Le profil de filtres de la carte est une copie dans `Assets/Art` (le pack ne
  s'écrit jamais).
- Restent du modèle, par choix : l'asset d'actions (retiré à l'étape des gestes), le niveau de qualité Mobile, et
  `SampleSceneProfile` (le profil de filtres des réglages de rendu URP).
- 33 « À toi » dans le GDD, chacun avec ma reco (« je te laisse faire » applique les recos).

**Prochaine étape** : 7. La caméra de gestion (GDD, section 16) : perspective à 40°, déplacer (clavier, bords, clic
glissé), tourner (Q/E), zoomer à la molette en s'abaissant ; l'asset d'actions du jeu remplace celui du modèle ; les
premières captures de l'album.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 7 : la caméra de gestion. Montre-moi le plan court, attends mon accord, puis
> code-la et vérifie-la ; fais les premières captures de l'album.

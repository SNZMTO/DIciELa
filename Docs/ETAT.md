# DICIELA — état actuel

**Branche** : `menage-1-gdd`, fusionnée dans `main`. **Session de ménage finie** : le GDD resserré sans changer une
règle ; l'architecture relue contre le code.

**Ce qui marche** : EditMode 23/23, PlayMode 1/1 (inchangés : le ménage n'a touché aucun code). La boucle de jeu tourne
dans la scène `Jeu` (**DICIELA > Jouer**) ; le journal raconte le monde dans la console.

**Le ménage** : GDD de 498 à 491 lignes (8 paires de règles réunies, redites retirées ; mêmes Défauts, mêmes chiffres,
contrôle mécanique à l'appui) ; deux trous trouvés, ajoutés en « À toi » (qui récolte les champs sauvages ; qui retire
les arbres entre la fin du tas de départ et la cabane du paysagiste). `ARCHITECTURE` : la fiche de réglages, le tas de
départ comme lieu de travail, le test de scène. `SAVOIR` 2.5 au présent (la règle LFS du Terrain est en place).

**À vérifier par lui** : rien à la souris. **33 « À toi »** dans le GDD, chacun avec ma reco (« je te laisse faire »
applique les recos) ; le plus pressé : la taille de la carte et la résolution du Terrain, avant de la peindre.

**À savoir**
- Restent du modèle, par choix : l'asset d'actions (retiré à l'étape des gestes), le niveau de qualité Mobile, et
  `SampleSceneProfile` (le profil de filtres des réglages de rendu URP).
- Le pack ouvre une fenêtre modale (`SyntyPackageHelper`) qui fige le CLI jusqu'à un clic (procédure `pack-synty`).
- Prochaine session de ménage : dans cinq étapes environ.

**Prochaine étape** : 6. La carte : sa taille et la résolution du Terrain (ses « À toi »), la scène `Carte` (à lui, dans
`Assets/Terrain/` pour le TerrainData), et sa description pure lue par `Game` au lancement (ARCHITECTURE, section 10).

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 6 : la carte. Commence par les « À toi » de la taille de la carte et de la
> résolution du Terrain, puis montre-moi le plan court, attends mon accord, puis code-la et vérifie-la.

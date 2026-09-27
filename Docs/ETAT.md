# DICIELA — état actuel

**Branche** : `etape-3-menage` (pas encore commitée). **Étapes 3 et 4 écrites, en attente de vérification** : Unity
est figé par une fenêtre du pack (« Synty requires the package com.unity.shadergraph… »).

**À faire par lui d'abord** : cliquer **Install** dans cette fenêtre (Shader Graph vient déjà avec URP ; l'outil de
Synty ne lit que le texte du manifest). Unity importe alors le pack et applique les paquets.

**Étape 3, le ménage (écrit)** : Sampler Pack retiré (corbeille) ; Fantasy Village à jour, installé en copiant
l'archive (`Assets/Synty/`) ; `TutorialInfo` et `Readme` retirés ; paquets du manifest (décision 47) ; produit
`DIciELa`, société `SNZMTO` ; règle LFS de `Assets/Terrain/` essayée ; procédures `pack-synty` et `unity-cli`.
Restent, par choix : `SampleScene` (retirée quand la scène `Jeu` la remplacera), l'asset d'actions du modèle (retiré à
l'étape des gestes), le niveau de qualité Mobile (le CLI ne sait pas retirer un niveau).

**Étape 4, le socle du temps (écrit, compilé hors Unity)** : assemblages Data, Simulation.Core, Simulation,
EditorTools, Tests.EditMode ; fiche `SimulationSettings` et menu `DICIELA/Données/Créer les fiches manquantes` ;
`Clock` (le temps = un nombre entier de pas) et `World.Step` ; tests de l'horloge et deux garde-fous (références des
assemblages, code de la simulation). La boucle de jeu et la scène `Jeu` viennent à l'étape 5.

**Après le clic, dans l'ordre** : vérifier (compilation, console, tests EditMode), créer la fiche par le menu,
committer l'étape 3 puis l'étape 4 (branche `etape-4-socle`), fusionner.

**À savoir** : 31 « À toi » dans le GDD (le plus pressé : la taille de la carte et la résolution du Terrain) ; le GDD
dépasse 490 lignes, à resserrer à la session de ménage.

**Prochaine étape** : 5. La boucle de jeu (`Game`, pas fixe borné, journal console) et la scène `Jeu`, montée par un
menu, qui remplace `SampleScene`.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Si les étapes 3 et 4 ne sont pas commitées, finis-les (vérification, commits). Puis
> étape 5 : montre-moi le plan court de la boucle de jeu et de la scène Jeu, attends mon accord, puis code-la.

# DICIELA — état actuel

**Branche** : `etape-4-socle`, fusionnée dans `main`. **Étapes 3 et 4 finies** : le projet est nettoyé, et le socle du
temps existe, testé.

**Ce qui marche** : EditMode 17/17 ; console vide. Le pack Fantasy Village à jour est dans `Assets/Synty/` (hors Git).
Assemblages `Diciela.Data`, `Simulation.Core`, `Simulation`, `EditorTools`, `Tests.EditMode`. La fiche
`Assets/Data/Settings/SimulationSettings.asset` (menu `DICIELA/Données/Créer les fiches manquantes`, réexécutable).
`Clock` : le temps est un nombre entier de pas (décision 49) ; `World.Step`. Deux garde-fous : les références des
assemblages suivent `ARCHITECTURE.md` ; le code de la simulation n'utilise rien d'interdit.

**À vérifier par lui** : rien à la souris ; rien ne se voit encore. Produit `DIciELa`, société `SNZMTO` : le dossier
des sauvegardes sera `%USERPROFILE%\AppData\LocalLow\SNZMTO\DIciELa`.

**Restent du modèle, par choix** : `SampleScene` (retirée quand `Jeu` la remplace, étape 5), l'asset d'actions du modèle
(retiré à l'étape des gestes), le niveau de qualité Mobile (le CLI ne sait pas retirer un niveau).

**À savoir**
- Le pack ouvre une fenêtre modale (`SyntyPackageHelper`) qui fige le CLI jusqu'à un clic (procédure `pack-synty`).
- 31 « À toi » dans le GDD ; le plus pressé : la taille de la carte et la résolution du Terrain, avant de la peindre.
- Le GDD dépasse 490 lignes : à resserrer à la session de ménage (dans une étape environ).

**Prochaine étape** : 5. La boucle de jeu dans `Diciela.Game` (pas fixe borné par la fiche, journal des événements
dans la console) et la scène `Jeu`, montée par un menu, qui remplace `SampleScene` ; un test PlayMode court.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 5 : la boucle de jeu et la scène Jeu. Montre-moi d'abord le plan court, attends
> mon accord, puis code-la et vérifie-la (procédure verifier).

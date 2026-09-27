# DICIELA — état actuel

**Branche** : `etape-5-boucle`, fusionnée dans `main`. **Étape 5 finie** : la boucle de jeu tourne dans la scène `Jeu`.

**Ce qui marche** : EditMode 23/23, PlayMode 1/1, deux Play courts sans erreur (250 pas en 25 s, soit 50 minutes de
jeu ; un second Play n'abonne rien en double). Assemblages `Data`, `Simulation.Core`, `Simulation`, `Game`,
`EditorTools` et les deux de tests. `GameRunner` (scène `Jeu`, objet « Démarrage ») crée le monde à chaque Play, joue
au plus 10 pas par image et abandonne le retard au-delà ; le journal écrit dans la console « monde créé », les jours,
les phases et « monde arrêté après N pas ».

**À vérifier par lui** : **DICIELA > Jouer**, d'où qu'il soit : Play dans `Jeu`, console « monde créé, jour 1… », puis
en quittant « monde arrêté après N pas » (dix pas par seconde). L'écran reste gris : rien à voir encore.

**À savoir**
- Restent du modèle, par choix : l'asset d'actions (retiré à l'étape des gestes), le niveau de qualité Mobile, et
  `SampleSceneProfile` (le profil de filtres des réglages de rendu URP).
- Le pack ouvre une fenêtre modale (`SyntyPackageHelper`) qui fige le CLI jusqu'à un clic (procédure `pack-synty`).
- 31 « À toi » dans le GDD ; le plus pressé : la taille de la carte et la résolution du Terrain, avant de la peindre.
- **La prochaine étape est la session de ménage** (une toutes les cinq étapes) : relire un système en entier ; le GDD
  (plus de 490 lignes) est le candidat désigné.

**Prochaines étapes, dans l'ordre**
1. Session de ménage : resserrer le GDD sans rien changer à ses règles ; relire l'architecture contre le code.
2. La carte : sa taille et sa résolution (ses « À toi »), la scène `Carte`, sa description pure lue par `Game`.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Session de ménage : propose-moi comment resserrer le GDD sans changer une règle
> (plan court, attends mon accord), puis fais-le.

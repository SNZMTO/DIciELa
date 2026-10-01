# DICIELA — état actuel

**Branche** : `etape-21b-camera` (avec l'étape 21), commitée, **pas fusionnée** : la caméra n'a pas encore été vue en
Play (il faut Unity au premier plan) ; puis les captures de l'album, ton avis, la fusion. `main` : 20b, poussée.

**Fait** : les étapes 15 à 20b (le tas, le HUD, le ménage, le défrichage, l'écran de création, le chantier qui monte,
la roue des outils ; décisions 147 à 169), puis :
- 21 : la tente devient un campement allégé d'objets du pack (un foyer en pierres, une couche, un coffre, un sac, une
  marmite, deux pierres d'entrée ; 4,61 × 3,50 m) ; à côté, le tas de départ est une pile de bûches et un tas de pierres
  (2,70 × 1,06 m). `DICIELA/Contenu/Composer le campement et le tas` (Assets/Art/Compositions, à toi). « Campement » à
  l'écran (décision 170). La flamme est retirée : ses particules faisaient planter Unity après chaque campagne.
- 21b : la caméra plus proche et plus basse : champ de vision 35°, départ à 25 m (vue à 32°), de 22,5° à 50° (171).
**EditMode 174/174, PlayMode 156/156**, Unity ne plante plus.

**À vérifier par toi** : en partie, la caméra te va-t-elle mieux (`Assets/Data/Settings/CameraSettings.asset`) ? Le
campement et le tas te plaisent-ils ? Le foyer éteint te suffit-il, ou faut-il chercher comment rallumer la flamme ?

**À toi** : peindre la carte et ranger tes modèles dans `Assets/Data/Nature` (la carte n'a encore aucun arbre) ; le
fanion des arbres marqués (le pack a `SM_Prop_Flag_01` à `05`) ; l'icône des outils et le plateau de la roue
(remplaçants) ; les pastels et le bandeau de brique (recos au GDD 16) ; la voix de la Chimère ; les noms de
`strings.csv`.

**Dette** : `Fusion.uss` répète les couleurs de `:root` ; à trois cartes, le panneau chevauchera l'astuce en 720 p.

**Prochaine étape** : voir la caméra et le campement en Play, l'album, fusionner 21 et 21b ; puis les touches 1 à 9 du
panneau ouvert ; puis GDD 17. Après, une session de ménage.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : …

# DICIELA — état actuel

**Branche** : `etape-21-campement`, commitée, **pas fusionnée** : Unity plante (dans son ramasse-miettes d'assets)
quand il recharge la scène après une campagne PlayMode, deux fois depuis que le campement a sa flamme. Cause à trouver.

**Fait** : les étapes 15 à 20b (le tas, le HUD, le ménage, le défrichage, l'écran de création, le chantier qui monte,
la roue des outils ; décisions 147 à 169). `main` est à l'étape 20b, poussée.
- 21 : la tente devient un campement allégé d'objets du pack (un feu, une couche, un coffre, un sac, une marmite, deux
  pierres d'entrée ; 4,61 × 3,50 m) ; à côté, le tas de départ est une pile de bûches et un tas de pierres (2,70 ×
  1,06 m). `DICIELA/Contenu/Composer le campement et le tas` (Assets/Art/Compositions, à toi) ; la préparation garde
  les effets ; « Campement » à l'écran (décision 170). **EditMode 175/175, PlayMode 156/156.**

**À faire en premier** : relancer Unity ; une campagne PlayMode : si Unity replante, préparer le campement sans sa
flamme et relancer ; si le crash disparaît, attacher la flamme autrement (une instance du prefab du pack plutôt
qu'une copie), puis fusionner.

**À vérifier par toi** : le campement allégé et le tas te plaisent-ils ? La flamme fait-elle feu ou bougie géante ?

**À toi** : peindre la carte et ranger tes modèles dans `Assets/Data/Nature` (la carte n'a encore aucun arbre) ; le
fanion des arbres marqués (le pack a `SM_Prop_Flag_01` à `05`) ; l'icône des outils et le plateau de la roue
(remplaçants) ; les pastels et le bandeau de brique (recos au GDD 16) ; la voix de la Chimère ; les noms de
`strings.csv`.

**Dette** : `Fusion.uss` répète les couleurs de `:root` ; à trois cartes, le panneau chevauchera l'astuce en 720 p.

**Prochaine étape** : finir l'étape 21 (le crash, l'album, la fusion) ; puis les touches 1 à 9 du panneau ouvert ;
puis GDD 17. Après, une session de ménage.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : … Continue avec tes recos.

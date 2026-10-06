# DICIELA — état actuel

**Branches** : `main` : l'étape 51. `etape-53-outils-de-la-place` (sur `etape-52-zone-de-la-place`) : 53 testée et
commitée, **pas encore vue rendue ni fusionnée** (Unity n'était pas devant) ; puis `git fetch . etape-53-…:main`, push.

**Fait** : 23 à 52 ; 53 (206) : les outils de la Place. La carte « Place », première des Métiers (l'onglet paraît dès
le campement), met l'outil en main ; une barre étroite en bas : Pinceau, Gomme, Cercle, Rectangle, Place de charrette
(touches 1 à 5, panneau fermé), la superficie, les places, la taille du pinceau (Ctrl + molette). Une forme part au
relâchement ; son aperçu, vert ou rouge, dit la raison. Un clic pose une place de charrette, un clic sur elle la
retire ; R la tourne. Zone et marques se voient en mode construction : un voile, son contour, des cadres blancs.
**EditMode 256/256, PlayMode 211/211.**

**D'abord, Unity devant** : le Play ; `DICIELA/Album/Installer le campement…`, `Tracer la Place…`, `Tirer un disque
trop grand…` ; captures, album, commit, fusion de 52 et 53.

**Prochaine étape, la session de ménage** (due) : découper `StallOffers`, `Villager`, `Villagers`, `WorkSlots` ; la
visée du sol écrite quatre fois (`PlacementTool`, `MoveTool`, `ClearingTool`, `PlaceCanvas`) ; puis paver, puis les
décorations.
**À toi** : la forme guidée ; combien de zones à quel niveau ; les six icônes de la Place (remplaçants) ; mes choix de
la 206 (Ctrl + molette, qui coupe le zoom même sans l'outil ; retirer une place d'un clic ; le mode construction ;
l'usage des modes à l'infobulle) ; l'astuce, qui ne parle pas encore de tracer la Place ; la fiche de la Place (les
consignes) ; « Run In Background » ; peindre la carte ; les modèles de la loge et de l'entrepôt ; les icônes des
Métiers, de « conduire », « garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la voix de la Chimère ; les noms.
**À vérifier par toi, à la souris** : 1 à 5 changent de mode panneau fermé ; un disque trop grand devient rouge et ne
part pas ; un clic droit abandonne le geste, un second lâche l'outil ; B montre le voile ; R tourne la place ; le survol
d'un mode dit son usage ; la Chimère réveillée flâne devant sa porte ; un Tab bref ne choisit rien.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
`EQUILIBRAGE.md` sans menu ; la charrette au pas de l'herbe partout.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Unity est devant : fais le Play et les captures de l'étape 53, puis la fusion. Ensuite le ménage.

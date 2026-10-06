# DICIELA — état actuel

**Branches** : `main` : l'étape 51. Empilées, commitées, **pas encore fusionnées** : `etape-52-zone-de-la-place`, puis
`etape-53-outils-de-la-place` (**pas encore vue rendue** : Unity n'était jamais au premier plan), puis
`etape-54-menage`. Une fois 53 vue : `git fetch . etape-54-menage:main` (porte 52, 53 et 54), `git push origin main`.

**Fait** : 23 à 52 ; 53 (206) : les outils de la Place. La carte « Place », première des Métiers, met l'outil en main ;
une barre étroite en bas : Pinceau, Gomme, Cercle, Rectangle, Place de charrette (touches 1 à 5, panneau fermé), la
superficie, les places, la taille du pinceau (Ctrl + molette). Une forme part au relâchement ; son aperçu, vert ou
rouge, dit la raison. Un clic pose une place de charrette, un clic sur elle la retire ; R la tourne. Zone et marques se
voient en mode construction. 54 (207) : le ménage des gestes : un seul fantôme pour la pose et Déplacer, une seule
visée du sol. **EditMode 256/256, PlayMode 211/211.**

**D'abord, Unity devant** : le Play ; `DICIELA/Album/Installer le campement…`, `Tracer la Place…`, `Tirer un disque
trop grand…` ; captures, album, puis la fusion des trois branches.

**Prochaine étape, le ménage de la société et de l'étal** : relire le système, découper `StallOffers` (trouver le
travail / porter la charge), `Villager`, `Villagers`, `WorkSlots` ; puis paver, puis les décorations.
**À toi** : la forme guidée ; combien de zones à quel niveau ; les six icônes de la Place (remplaçants) ; mes choix de
la 206 (Ctrl + molette, qui coupe le zoom même sans l'outil ; retirer une place d'un clic ; le mode construction ;
l'usage des modes à l'infobulle) ; l'astuce, qui ne parle pas encore de tracer la Place ; la fiche de la Place (les
consignes) ; « Run In Background » ; peindre la carte ; les modèles de la loge et de l'entrepôt ; les icônes des
Métiers, de « conduire », « garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la voix de la Chimère ; les noms.
**À vérifier par toi, à la souris** : 1 à 5 changent de mode panneau fermé ; un disque trop grand devient rouge et ne
part pas ; un clic droit abandonne le geste, un second lâche l'outil ; B montre le voile ; R tourne la place ; le survol
d'un mode dit son usage ; poser et déplacer une construction se font comme avant.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
`EQUILIBRAGE.md` sans menu ; la charrette au pas de l'herbe partout.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Unity est devant : fais le Play et les captures de l'étape 53, puis la fusion. Ensuite le ménage.

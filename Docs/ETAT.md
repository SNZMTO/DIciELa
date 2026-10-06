# DICIELA — état actuel

**Branches** : `main` : l'étape 51. Empilées, commitées, **pas encore fusionnées** : `etape-52-zone-de-la-place`,
`etape-53-outils-de-la-place` (**pas encore vue rendue** : Unity n'était jamais au premier plan), `etape-54-menage`,
puis `etape-55-menage-societe`. Une fois 53 vue : `git fetch . etape-55-menage-societe:main` (porte 52 à 55),
`git push origin main`.

**Fait** : 23 à 53 ; 53 (206) : les outils de la Place, la carte « Place », la barre en bas et ses cinq modes (touches 1
à 5), une forme qui part au relâchement, les places de charrette posées, tournées, retirées d'un clic. 54 (207) : le
ménage des gestes. 55 (208) : le ménage de la société et de l'étal : la porte du logis et la place devant une charrette
lues chacune en un seul endroit, le monde assemble ce que consulte chaque habitant, une charrette qu'on vient chercher
garde sa place (testé) ; les quatre gros fichiers ne sont pas découpés (chacun le sera à la couture que sa prochaine
étape apporte). **EditMode 257/257, PlayMode 211/211.**

**D'abord, Unity devant** : le Play ; `DICIELA/Album/Installer le campement…`, `Tracer la Place…`, `Tirer un disque
trop grand…` ; captures, album, puis la fusion des quatre branches.

**Prochaine étape : paver la Place** (GDD, sections 9, 11 et 12 ; poser les questions avant de coder), puis les
décorations. Ménage suivant après trois ou quatre étapes.
**À toi** : le découpage que j'ai retourné (208) ; le pavé de la Place (la planche des pavés existe) ; la forme guidée ;
combien de zones à quel niveau ; les six icônes de la Place (remplaçants) ; mes choix de la 206 (Ctrl + molette, qui
coupe le zoom même sans l'outil ; retirer une place d'un clic ; le mode construction ; l'usage des modes à
l'infobulle) ; l'astuce, qui ne parle pas encore de tracer la Place ; la fiche de la Place (les consignes) ; « Run In
Background » ; peindre la carte ; les modèles de la loge et de l'entrepôt ; les icônes des Métiers, de « conduire »,
« garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la voix de la Chimère ; les noms.
**À vérifier par toi, à la souris** : 1 à 5 changent de mode panneau fermé ; un disque trop grand devient rouge et ne
part pas ; un clic droit abandonne le geste, un second lâche l'outil ; B montre le voile ; R tourne la place ; le survol
d'un mode dit son usage ; poser et déplacer une construction se font comme avant ; « Aller dormir » couche la Chimère.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
`EQUILIBRAGE.md` sans menu ; la charrette au pas de l'herbe partout.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Unity est devant : fais le Play et les captures de l'étape 53, puis la fusion. Ensuite paver la Place.

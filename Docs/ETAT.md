# DICIELA — état actuel

**Branches** : `main` : l'étape 55, fusionnée et poussée. `etape-56-chemins` : la décision 209 et le GDD réécrit
(sections 9, 11, 16), pas encore de code ; l'étape 56 s'y code, puis se fusionne (`git fetch . etape-56-chemins:main`).

**Fait** : 23 à 55 ; 52 et 53 (205, 206) : la Place, une zone qu'on trace à son outil ; vue rendue le 06/10, album à
jour ; la bulle des raisons passe à la ligne au lieu de déborder (un test le tient). 54 (207) : le ménage des gestes.
55 (208) : le ménage de la société et de l'étal. **EditMode 257/257, PlayMode 211/211.**

**Prochaine étape, 56 : les chemins dans la simulation** (décision 209 ; lire GDD 9, ARCHITECTURE 8, SAVOIR 2.5 et
2.6). Le plan court à proposer avant de coder, avec ses trois points durs : les tracés au terrain (une sorte, des
mailles, un état ; peints et gommés par les formes de la Place, `GroundShape`) ; un chantier qui n'est pas une
construction (`ConstructionSite` ne sert qu'une `Structure` : le généraliser, c'est le deuxième cas) ; le pas des
charrettes selon le sol (le maillage n'a pas encore de coût par façon de se déplacer). Puis 57 : l'outil, la vue, la
barre commune avec la Place. Au prochain Play, Unity devant : reprendre la capture du disque trop grand.
**À toi** : mes choix de la 209 (la touche C et la gauche de la roue ; une seule barre pour la Place et les Chemins ;
0,01 h par m² ; un bâtiment refusé sur un chemin ; les calques `Village_Dirt_01` et `Path_01`) ; le découpage retourné
et la charrette qu'on vient chercher (208) ; le contour en escalier de la zone ; la forme guidée ; combien de zones à
quel niveau ; les icônes de la Place ; mes choix de la 206 (Ctrl + molette ; retirer une place d'un clic ; le mode
construction ; l'usage des modes à l'infobulle) ; l'astuce, qui ne parle pas de tracer la Place ; la fiche de la Place
(les consignes) ; « Run In Background » ; peindre la carte ; les modèles de la loge et de l'entrepôt ; les icônes des
Métiers, de « conduire », « garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la voix de la Chimère ; les noms.
**À vérifier par toi, à la souris** : 1 à 5 changent de mode panneau fermé ; un clic droit abandonne le geste, un
second lâche l'outil ; B montre le voile ; R tourne la place ; poser et déplacer une construction se font comme avant ;
« Aller dormir » couche la Chimère ; une raison longue tient dans sa bulle.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
`EQUILIBRAGE.md` sans menu ; « C'est parti » sans menu d'album (un clic à la souris) ; le script de recompilation hors
de `Tools/`.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Étape 56 : les chemins dans la simulation ; propose-moi le plan court avant de coder.

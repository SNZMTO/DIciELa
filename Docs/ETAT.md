# DICIELA — état actuel

**Branches** : `main` : l'étape 55 ; `etape-56-chemins` : les 209 à 211 et le GDD, sans code ; l'étape 56 s'y code.

**Fait** : 23 à 55 ; 52 et 53 (205, 206) : la Place, une zone qu'on trace à son outil, vue rendue, album à jour ; 54 et
55 (207, 208) : le ménage des gestes, puis de la société et de l'étal. **EditMode 257/257, PlayMode 211/211.**

**Prochaine étape, 56 : les sols dans la simulation** (décisions 209 et 210 ; lire GDD 9, ARCHITECTURE 8, SAVOIR 2.1,
2.5 et 2.6). Le plan court à proposer avant de coder, avec ses trois points durs : les sols au terrain (une sorte et sa
fiche, des mailles, un état ; posés et effacés par les formes de la Place, `GroundShape` ; un sol déjà posé fait un
trou) ; un chantier qui n'est pas une construction (`ConstructionSite` ne sert qu'une `Structure` : le généraliser,
c'est le deuxième cas) ; le pas des charrettes selon le sol (le maillage n'a pas encore de coût par façon de se
déplacer). Puis 57 : les outils (Chemins, le sol de la Place), la vue, les calques faits des images du pack ; puis le
pont, première idée qui se montre (211). Au prochain Play, Unity devant : reprendre la capture du disque trop grand.
**À toi** : le déclencheur de l'idée de chaque sol après les pavés (GDD 9) ; la phrase neutre des idées, un remplaçant ;
mes choix des 206 et 209 à 211, à retourner (dont Ctrl + molette, qui coupe le zoom même sans l'outil) ; le découpage
retourné et la charrette qu'on vient chercher (208) ; le contour en escalier de la zone ; la forme guidée ; combien de
zones à quel niveau ; les icônes de la Place ; l'astuce, qui ne parle pas de tracer la Place ; la fiche de la Place (les
consignes) ; « Run In Background » ; peindre la carte ; les modèles de la loge et de l'entrepôt ; les icônes des
Métiers, de « conduire », « garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la voix de la Chimère ; les
noms.
**À vérifier par toi, à la souris** : 1 à 5 changent de mode panneau fermé ; un clic droit abandonne le geste, un second
lâche l'outil ; B montre le voile ; R tourne la place ; le survol d'un mode dit son usage ; poser et déplacer une
construction se font comme avant ; « Aller dormir » couche la Chimère ; une raison longue tient dans sa bulle.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
`EQUILIBRAGE.md` sans menu ; « C'est parti » sans menu d'album ; le script de recompilation hors de `Tools/`.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Étape 56 : les sols dans la simulation ; propose-moi le plan court avant de coder.

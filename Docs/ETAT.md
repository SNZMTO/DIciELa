# DICIELA — état actuel

**Branches** : `main` : l'étape 56 ; la 57 s'ouvre de `main`.

**Fait** : 23 à 56 ; 52 à 55 (205 à 208) : la Place tracée et vue, puis le ménage ; 56 (209 à 213) : les sols dans la
simulation (une couche à côté du maillage, un chantier pour tracer ou effacer, un bâtiment refusé sur un chemin), le
pas des charrettes selon le sol, et une estimation qui sait où sont les sols (100 m en 4,5 ms au lieu de 143).
**EditMode 275/275, PlayMode 211/211.** Rien ne se voit encore : la vue et les outils sont la 57.

**Prochaine étape, 57 : les outils des sols et leur vue** (209, 210, 213 ; GDD 9, 11, 16 ; ARCHITECTURE 8, 11 ; SAVOIR
2.5, 2.6, 2.8), plan court d'abord : l'outil Chemins (touche C) ; la barre de la Place partagée, ses sols, leur gomme ;
l'aperçu et ses raisons (`FloorResult`) ; le fantôme en voile ; les sols peints par lots (`FloorsChanged`) ; les
calques faits des images du pack. Puis le pont (211). Au prochain Play, Unity devant : la capture du disque trop grand.
**À toi** : le prix au m² des sols qui se paient (GDD 9 ; ma reco : 1 Pavé pour 2 m²) ; mes choix des 206, 209 à 213, à
retourner (dont Ctrl + molette, qui coupe le zoom même sans l'outil ; l'ouvrier d'un sol debout sur sa forme ; la gomme
du sol bornée à la zone de la Place ; le pas de la charrette lu sous qui la tire) ; la phrase neutre des idées, un
remplaçant ; le découpage retourné et la charrette qu'on vient chercher (208) ; le contour en escalier de la zone ; la
forme guidée ; combien de zones à quel niveau ; les icônes de la Place ; l'astuce, qui ne parle pas de tracer la Place ;
la fiche de la Place (les consignes) ; « Run In Background » ; peindre la carte ; les modèles de la loge et de
l'entrepôt ; les icônes des Métiers, de « conduire », « garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la
voix de la Chimère ; les noms.
**À vérifier par toi, à la souris** : 1 à 5 changent de mode panneau fermé ; un clic droit abandonne le geste, un second
lâche l'outil ; B montre le voile ; R tourne la place ; le survol d'un mode dit son usage ; poser et déplacer une
construction se font comme avant ; « Aller dormir » couche la Chimère ; une raison longue tient dans sa bulle ; une
charrette se gare toujours à sa place (56 change son trajet ; pas de Play court, Unity n'était pas devant).
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
`EQUILIBRAGE.md` sans menu ; « C'est parti » sans menu d'album ; le script de recompilation hors de `Tools/`.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Étape 57 : les outils des sols et leur vue ; propose-moi le plan court avant de coder.

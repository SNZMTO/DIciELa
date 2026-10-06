# DICIELA — état actuel

**Branches** : `main` : l'étape 51 (37 à 51 fusionnées le 06/10). `etape-52-zone-de-la-place` : les décisions 204 et
205 et l'étape 52, pas encore fusionnée : elle retire la Place du jeu tant que l'étape 53 n'a pas ses outils.

**Fait** : 23 à 51 ; 52 (204, 205) : la Place devient une zone que le joueur trace, dans la simulation : peindre et
gommer par des formes (trait, disque, rectangle), 450 m² au plus, d'un seul tenant, rien ne s'y bâtit ; ses places de
charrette sont des marques qu'il pose ; les charrettes et l'étal passent sur elles ; la Place construction est
supprimée (fiche, composition, bordure, planches, sol qu'on traverse, cercle des places).
**EditMode 253/253, PlayMode 197/197.**

**Dans le jeu, jusqu'à l'étape 53** : aucun outil pour tracer la Place ; les charrettes restent chez elles.
**Prochaine étape, 53 : les outils de la Place**, ma proposition, à valider ou corriger par toi d'abord :
- la carte « Place » (la première des Métiers) met en main l'outil de la Place ; une petite barre s'ouvre en bas :
  Pinceau, Gomme, Cercle, Rectangle, Place de charrette (touches 1 à 5 ; la taille du pinceau à la molette) ; la
  superficie s'affiche (« 312 / 450 m² ») ; l'aperçu vert ou rouge dit la raison d'un refus, comme à la pose ;
- la zone et les marques ne se voient qu'outil de construction en main : un voile léger sur le sol, son contour, et
  les marques en rectangles blancs, comme les fantômes ; R tourne la marque ;
- un test de bout en bout par les gestes, une capture pour l'album, puis la fusion de 52 et 53.
Après 53, la session de ménage, due (six étapes depuis) : découper `StallOffers`, `Villager`, `Villagers`, `WorkSlots`
(53 découpe ce qu'elle touche : `PlacementTool`, `PlaceZone`, `NavGrid`, `Carts`) ; puis paver, puis les décorations.
**À toi** : la barre d'outils (plus haut) ; la forme guidée ; combien de zones à quel niveau ; la fiche de la Place
(les consignes) ; « Run In Background » ; peindre la carte ; les modèles de la loge et de l'entrepôt ; les icônes des
Métiers, de « conduire », « garnir », « déplacer » ; le fanion des arbres ; le bandeau ; la voix de la Chimère ; les noms.
**À vérifier par toi, à la souris** : la Chimère réveillée en chemin flâne devant sa porte ; un Tab bref ne choisit
rien ; un clic à côté de la roue la ferme ; la roue montre ses trois icônes.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes sur l'astuce en 720 p ; des réglages d'interface dans le code ;
« Monter les scènes » à repasser ; `EQUILIBRAGE.md` sans menu ; la charrette au pas de l'herbe partout.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. La barre d'outils de la Place : …

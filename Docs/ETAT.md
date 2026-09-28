# DICIELA — état actuel

**Branche** : `main`, à jour. **Étapes 6 à 9 et 8c finies.** EditMode 66/66, PlayMode 56/56, Play court sans erreur.
**DICIELA > Scènes > Monter les scènes** monte tout.

**En jeu** : la barre en bas ; l'aperçu de la cabane (la silhouette de son modèle) suit la souris, vert ou rouge avec
sa raison ; R tourne (bref : un cran ; tenu : au degré) ; le clic gauche pose un fantôme ; clic droit bref ou Échap
range l'outil. La caméra part à 57 m, le bouton du milieu tourne et incline, les ombres et le brouillard suivent le zoom.

**La cabane** (8c, décision 76) : `Bld_Preset_Shed_02`, préparé par **DICIELA > Contenu > Préparer les modèles** : ses
62 pièces fusionnées en un maillage (15 matériaux, en LFS), porte au nord et au niveau du sol, fondations enfoncées ;
empreinte 5,99 × 8,47 m, accessoires compris. **DICIELA > Album > Planche des modèles** (en Play) pose côte à côte le
modèle du pack et le préparé, vus du côté des portes.

**Les trajets** (9, décisions 73 et 74) : un fantôme bloque le passage dès sa pose ; `World.PathFinder` cherche le
trajet le moins cher d'un piéton (A* sur le maillage), sans allocation, vérifié contre un Dijkstra sur 400 terrains.

**Questions pour toi** (ma reco entre parenthèses) :
1. Sur la planche (album du 28/09), ta « petite marque » est-elle au ras de l'herbe ? (Oui d'après la capture : la
   porte est au sol ; sinon, dis de combien enfoncer, et la fiche recevra ce décalage.)

**Prochaine étape : 9b** (son plan court est à valider) : savoir tout de suite qu'un trajet est introuvable
(composantes connexes, recalculées à chaque pose), le lissage du trajet en ligne tendue, la marche le long du trajet ;
puis la Chimère, qui marchera la première. Plus tard, si le Profiler le demande : le GPU Resident Drawer, les LOD.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis la prochaine étape : montre-moi
> le plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

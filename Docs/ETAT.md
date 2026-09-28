# DICIELA — état actuel

**Branche** : `main`, à jour. **Étapes 6 à 9, 8c et 9b finies.** EditMode 77/77, PlayMode 56/56, Play court sans
erreur. **DICIELA > Scènes > Monter les scènes** monte tout.

**En jeu** : l'aperçu de la cabane (décision 76) suit la souris, vert ou rouge avec sa raison ; R tourne ; clic gauche :
un fantôme ; clic droit bref ou Échap : l'outil rangé. La caméra part à 57 m ; le bouton du milieu tourne et incline.

**Les trajets** (9 et 9b, décisions 73, 74, 79, 80) : un fantôme bloque le passage dès sa pose. `World.PathFinder`
rend le trajet le moins cher d'un piéton en escalier de mailles et en ligne tendue, tirée des deux bouts ;
`World.Islands` dit sans rien fouiller qui rejoint qui (une quinzaine de ms sur la carte de 500 m après une pose).

**À vérifier par toi** : en Play, pose deux cabanes ou plus, puis **DICIELA > Album > Planche des trajets, de la
première porte à la dernière** : la vue se penche ; un trait blanc en escalier et un orange tendu vont d'une porte à
l'autre en contournant les cabanes. Enferme la dernière porte entre des cabanes, relance : « injoignable ».

**Questions pour toi**, pour la Chimère : 1. Où apparaît-elle au lancement ? Ma reco : au point d'arrivée de la
carte, que tu poseras dans `Carte` ; en attendant, au centre. 2. Que fait-elle sans poste (GDD 7 : « il flâne ») ? Ma
reco : elle va à un point libre tiré à quelques mètres, s'y arrête un moment, recommence (les colons de RimWorld).

**Prochaine étape : la Chimère marche** (plan court à valider) : la société naît avec elle (une habitante, sa
position, sa sorte et sa vitesse dans une fiche) ; la marche le long d'un trajet tendu (refait quand un fantôme le
coupe, sortie d'un fantôme posé sur elle) ; la vue la montre (un personnage du pack, lissé entre deux pas). Elle
s'arrêtera avant sa fusion pour ton `/code-review ultra` ; le ménage qui suit sera la revue « ultracode », si tu
l'écris (décisions 77 et 78). Ensuite sa création, la tente ; le GPU Resident Drawer et les LOD si le Profiler le dit.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Réponds d'abord aux questions d'ETAT avec moi, puis la prochaine étape : montre-moi
> le plan court, attends mon accord, code-la, vérifie-la, captures pour l'album.

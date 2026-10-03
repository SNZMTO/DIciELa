# DICIELA — l'architecture

> Comment le jeu est bâti : les couches, qui possède quoi, les règles de passage. À lire avant de créer ou de déplacer
> du code. Le pourquoi des choix est dans `DECISIONS.md` (30 et suivantes) ; le « comment faire » éprouvé est dans
> `SAVOIR.md`, auquel ce document renvoie au lieu de le recopier.

## 1. Les principes

- **La simulation ne sait pas qu'elle est dessinée.** C'est du C# pur, qui avance par pas fixes, reçoit des ordres et
  lève des événements. La vue, les gestes et l'interface sont au-dessus d'elle ; elle ne les connaît pas.
- **Une seule vérité par chose.** Chaque donnée a un propriétaire (section 4). Tout le reste en est déduit, recalculé,
  jamais sauvegardé.
- **Les dépendances ne vont que vers le bas**, et le compilateur l'impose : chaque couche est un assemblage (un fichier
  `.asmdef` : un paquet de scripts compilé à part, qui ne voit que les paquets qu'il déclare).
- **Le joueur n'a qu'une porte** : les ordres. Les tests et la ville témoin passent par la même, et le compilateur la
  garde (section 5).
- **Le contenu est donnée** : des fiches (ScriptableObjects : des fichiers de données réglés dans l'Inspector), lues par
  la simulation, jamais écrites par elle.

## 2. Les couches et les assemblages

Du bas vers le haut. Le nom de l'assemblage est aussi son namespace.

| Couche | Assemblage | Dossier | Ce qu'elle contient |
|---|---|---|---|
| les fiches | `Diciela.Data` | `Assets/Scripts/Data` | les classes de fiches, sans logique ; la table des textes |
| le socle | `Diciela.Simulation.Core` | `Assets/Scripts/Simulation/Core` | le temps ; le hasard par hachage ; la formule et le registre des effets ; la géométrie en mètres et la seule conversion mètre ↔ maille ; les contrats du travail (lieu de travail, tâche à prendre) ; le savoir de la ville |
| le terrain | `Diciela.Simulation.Land` | `Assets/Scripts/Simulation/Land` | la carte et son état ; les emprises peintes (tracés et plantations) ; les constructions et la réserve ; le maillage, ses chemins et ses distances ; l'eau et la force |
| l'économie | `Diciela.Simulation.Economy` | `Assets/Scripts/Simulation/Economy` | les stocks et leurs consignes ; les réservations ; les ateliers ; la culture ; les chantiers ; la valeur des marchandises ; les tâches à prendre |
| la société | `Diciela.Simulation.Society` | `Assets/Scripts/Simulation/Society` | les habitants et leur journée ; les postes ; les arrivées et les logements ; la nourriture ; l'étal et les visiteurs ; le charme ; la raison d'arrêt d'un lieu |
| le monde | `Diciela.Simulation` | `Assets/Scripts/Simulation` | le monde (il assemble les systèmes et les fait tourner) ; les ordres ; les déclencheurs ; le déroulé du départ ; la capture et la restauration |
| la sauvegarde | `Diciela.Persistence` | `Assets/Scripts/Persistence` | instantanés ↔ JSON, versions, liste des mondes, fichiers mis de côté |
| la vue | `Diciela.View` | `Assets/Scripts/View` | modèles, copie de travail du Terrain, sol, personnages et leur habillage, caméra, ambiance, calques, pastilles |
| les gestes | `Diciela.Controls` | `Assets/Scripts/Controls` | Input System, visée, outils (poser, tracer, peindre, effacer, ranger, déplacer, choisir), valider, pile d'Échap |
| l'interface | `Diciela.UI` | `Assets/Scripts/UI` | UI Toolkit : panneaux, présentateurs, écran de création, menus, bulles, carnet |
| le démarrage | `Diciela.Game` | `Assets/Scripts/Game` | racine de composition et sa scène (vues, gestes, ouverture de partie), lecture de la carte, boucle à pas fixe, pause, enregistrement, journal |

Les annexes : `Diciela.EditorTools` (`Assets/Scripts/Editor`, éditeur seulement : menus `DICIELA/…`, monteurs,
diagnostics, rapport d'équilibrage) ; `Diciela.Scenarios` (`Assets/Scripts/Scenarios`, éditeur seulement : la ville
témoin) ; `Diciela.Tests.EditMode` et `Diciela.Tests.PlayMode` (`Assets/Tests/EditMode`, `Assets/Tests/PlayMode`),
avec leurs fichiers témoins dans `Assets/Tests/Fixtures`. Les fiches vivent dans `Assets/Data/<sorte>`, les textes dans
`Assets/Localization/strings.csv`.

**Les références.** Elles ne sont pas transitives : un assemblage liste tous ceux dont il nomme un type. Un test
compare les `.asmdef` à ce tableau ; ajouter une flèche est un changement d'architecture, jamais un geste en passant.
Un assemblage naît avec son premier code ; tous sont en références explicites (`autoReferenced` faux,
`overrideReferences` vrai : aucune DLL d'office).

| Assemblage | Référence |
|---|---|
| Data | (rien) |
| Simulation.Core | Data |
| Simulation.Land | Data, Core |
| Simulation.Economy | Data, Core, Land |
| Simulation.Society | Data, Core, Land, Economy |
| Simulation | Data, Core, Land, Economy, Society |
| Persistence | Data et les cinq de la simulation ; Newtonsoft (seul à l'avoir) |
| View | Data et les cinq de la simulation ; URP (les ombres et le brouillard qui suivent le zoom) |
| Controls | View, Data, la simulation ; Input System |
| UI | Controls, View, Persistence, Data, la simulation |
| Game | tout ce qui précède |
| Scenarios | Data, la simulation |
| EditorTools | Data, la simulation, Persistence, View, Controls, UI, Game, Scenarios ; Input System, URP (réglages de la caméra) |
| Tests.EditMode | Data, la simulation, Persistence, Scenarios ; NUnit, Test Runner |
| Tests.PlayMode | tout l'exécutable ; NUnit, Test Runner, Input System et son banc d'essai, URP |

**La règle de passage.** Le haut appelle le bas ; le bas lève des événements que le haut écoute ; un calcul qui a
besoin de deux couches se fait dans la plus haute. Un système ne modifie que ce qu'il possède ; pour changer l'état
d'un autre, il l'appelle. Si une couche a besoin d'une réponse du haut, la donnée est mal rangée : on la descend.

Exemple, l'atelier : il ne sait pas qui travaille chez lui. L'habitant (société) y va et lui apporte son travail par un
appel. Sans habitant, rien n'arrive et l'atelier est à l'arrêt. La pastille « personne au poste » se calcule donc dans
la société, qui voit tout ce qui est en dessous.

## 3. Les systèmes de la simulation

- **Le socle.** Le temps (heures et jours de jeu, phases jour, soir, nuit), et l'accumulateur qui transforme le temps
  réel en nombre de pas à jouer. Le hasard par hachage (SAVOIR 2.4). Les
  effets : leur type et leur formule ; le registre est un objet créé par le monde et passé à chaque système. La
  géométrie (mètres, plan XZ, rectangles tournés, polylignes). Les contrats du travail : un lieu de travail ouvre des
  postes ; une tâche à prendre dit où, quoi et quelle sous-statistique elle lit. Le savoir de la ville (ce qu'elle
  sait bâtir : au départ, ce que dit le déroulé du départ ; puis les idées), au socle parce que les règles de pose le
  lisent comme le panneau Construire, et plus tard les recettes et les décos (décision 185).
- **Le terrain.** La carte reçue au lancement (section 10) et son état : arbres et rochers retirés, champs sauvages
  (désigné, récolté, minuterie de repousse). Les emprises peintes (section 8). Les constructions : bâtiments, décos,
  la Place, la réserve ; les devants de porte, déduits des constructions, que lisent les règles de pose ; le sol que
  tient chaque construction (`StructureGround`), pris à la pose, rendu et repris quand elle change de place
  (`StructureMoves`, l'ordre « déplacer », vérifié par les mêmes règles, la construction déplacée ne comptant pas). Le maillage
  (SAVOIR 2.1), avec ses îlots, son A*, la ligne tendue qu'on tire d'un trajet, la sortie d'une construction posée sur
  un piéton et ses distances par la route, écrits pour lui ; il dit seul où l'on marche (sur un sol qu'on traverse,
  comme la Place, aussi). L'eau :
  canaux, relief creusé, zones irriguées. La force : réseaux et budget, un moulin bâti consommant qu'il travaille ou
  non.
- **L'économie.** Les stocks : portes des bâtiments, entrepôts, charrettes de l'étal, charge d'un porteur, panier d'un
  visiteur, tas au sol ; chacun a son contenu et sa consigne. Une seule fonction crée ou détruit une marchandise, avec
  sa cause ; une seule fonction « rend à la ville », toujours à une place retenue (une charge en route garde la sienne
  à son départ, décision 178). Les réservations. Les ateliers (recette, avancement). La culture
  des plantations (semis, pousse, graines gardées, récolte). Les chantiers (rang dans la file, livraisons, avancement),
  et le petit chantier d'un bâtiment qu'on déplace : son premier travail le défait (`StructureState.Moving` : il ne sert
  plus), défait il saute à la place qu'il retenait, remonté il sert de nouveau, sans nouvelle mise en service ; les
  charrettes suivent une Place déplacée (`Carts` écoute `StructureMoved`, pour ses places de charrette).
  La production comptée par marchandise. La valeur des marchandises. La publication des tâches à prendre (porter,
  bâtir, retirer un arbre, conduire une charrette). Les charrettes de l'étal et les places de charrette de la Place
  (`Carts`, la seule fonction qui change une charrette ; `CartOffers`, les charrettes à conduire).
- **La société.** Les habitants (ordinaires, maîtres, la Chimère : trois sortes décrites par leurs fiches, jamais un
  cas particulier par nom) et leur journée (travail, soirée, nuit). Leur marche, le long de la ligne tendue d'un
  trajet, et la sortie d'une construction qu'on ne traverse pas, posée sur eux ; la flânerie de qui n'a pas de poste ;
  le sommeil de la Chimère, devant la porte de son logis, sur ordre. La table des postes, qui ouvre aussi le poste « à
  tout faire » d'un premier habitant et le ferme quand son lieu de travail est bâti.
  Les arrivées : une seule fonction (`Villagers.Arrive`), la Chimère comprise ; c'est leur source qui en dit les
  conditions (le déroulé du départ : un lit libre ; plus tard, les arrivées ordinaires : un lit, un poste, de quoi
  manger ; les maîtres : un lit, de quoi manger). Les logements (`Housing` : qui a son lit où ; les lits libres, déduits
  des maisons bâties) et le déménagement de nuit. Le coucher et le lever (`Bedtimes`) : la Chimère sur ordre, les
  habitants d'eux-mêmes, au début de la nuit, et debout au matin. La nourriture.
  Le tirage des maîtres. L'étal (curseurs, souhaits, visiteurs, réputation). Le charme. La raison d'arrêt d'un lieu,
  une seule fonction lue par la vue et l'interface.
- **Le monde.** `World` assemble les systèmes et les appelle dans l'ordre du pas, rien d'autre. À côté, chacune dans
  sa classe : le traitement des ordres ; les déclencheurs (une condition mène à une récompense : idée, réplique,
  tenue) ;
  le déroulé du départ (le stock de départ à la mise en service du campement ; les premières arrivées, à la fin du
  pas, chacune avec son idée ; leur installation à leur lieu de travail, quand il est mis en service) ; les opérations qui touchent plusieurs couches, faites par des appels vers le bas dans un ordre
  écrit (poser : le terrain, puis l'économie ouvre un chantier, ou la construction, dressée dès la pose, est mise en
  service ; mettre en service, une seule opération, à la pose ou à la fin du pas où un chantier finit : l'économie
  ouvre un stock, la société des postes, l'économie des charrettes ou des places de charrette (décision 197), puis le
  déroulé du départ la regarde, décisions 179 et 191 : le stock de
  départ pour le premier logis de la Chimère, et le premier habitant dont c'est le lieu de travail y prend un poste ;
  plus tard, la maison de la Chimère qui remplace le campement) ; la capture et la restauration. Le monde reçoit les réglages, la carte et le
  catalogue, d'où il tire le déroulé du départ et les statistiques.

## 4. Qui possède quoi

Ce qui est déduit se recalcule sur événement, ou se garde avec la version de ce dont il dépend ; il n'est jamais
sauvegardé.

| La chose | Son propriétaire | Ce qu'on en déduit |
|---|---|---|
| l'heure, le jour | le temps | la phase |
| la carte peinte | reçue au lancement ; seule son empreinte est sauvegardée | le relief creusé, l'eau |
| arbres et rochers : marqués (dans l'ordre des marques), retirés ; champ sauvage : désigné, récolté, minuterie | le terrain | le maillage |
| le travail déjà fait sur un arbre ou un rocher marqué ; qui l'a pris | l'économie | — |
| une charrette de l'étal : son entrepôt, la place qui l'appelle, garée ou non ; les places de charrette d'une Place, tirées à sa mise en service | l'économie (`Carts`) | — |
| tracés et plantations : forme, sorte, état (en chantier, bâti) | le terrain | les coûts du maillage ; les champs, leur surface, leurs postes |
| une construction : sorte, place, angle, niveau, état (fantôme, bâtie, en déplacement avec sa place visée, rangée) | le terrain | le maillage et ses îlots ; les devants de porte ; la réserve ; le logis de la Chimère (la construction bâtie que sa fiche dit être son logis) |
| un chantier : rang dans la file, livraisons déposées, avancement | l'économie | — |
| contenu et consigne de chaque stock | l'économie | le stock de la ville |
| les réservations (quantité promise, place retenue) | l'économie, jamais sauvegardées | — |
| la culture de chaque plantation : semis, pousse, graines gardées | l'économie | la nourriture (dans la société) |
| la recette et l'avancement d'un atelier ; la production par marchandise | l'économie | la valeur des marchandises |
| un habitant : sorte, statistiques, plafonds, apparence, tenue portée, position, effet d'étal en cours | la société | la statistique globale, la valeur pour une tâche, le bonheur |
| qui tient quel poste ; les postes ouverts pour un premier habitant ; les postes fermés | la société (une seule table) | qui est présent sur un lieu |
| qui a son lit où ; qui dort (couché ou en chemin) | la société | les lits libres (des maisons bâties) ; où dort chacun (la Chimère : son logis) |
| les curseurs, les souhaits, les visiteurs, la réputation | la société | les taux de l'étal ; le charme |
| ce que la ville sait faire (ses constructions ; plus tard ses recettes, ses décos) | le socle (`Knowledge`) : au départ, le déroulé du départ ; puis les idées (celles des premiers habitants, apprises par le déroulé du départ ; plus tard, les déclencheurs) | le carnet ; ce que le panneau Construire propose, et ses onglets |
| l'avancement des déclencheurs, la part de chaque habitant | le monde | — |
| l'étape du départ : le prochain premier habitant, l'heure d'où il peut arriver ; chaque premier habitant arrivé, son lieu de travail et le poste ouvert pour lui | le monde | le stock de départ donné (le bilan des marchandises le dit) |
| les effets actifs | ce qui les porte (une lanterne, un maître, une plantation irriguée…) | le registre des effets |
| une tâche en cours, une marche, une flânerie | personne : elles ne sont pas sauvegardées | — |
| le dossier des sauvegardes | la fiche de session | — |

## 5. Le pas, les ordres et les événements

- **Le pas fixe.** La simulation avance par pas de 0,1 s de temps réel (Défaut, à confirmer en jouant) et compte en
  heures de jeu ; seule l'horloge convertit, des heures en pas et d'un pas en heures, à partir de la durée d'une
  journée. La vérité du temps est le nombre de pas joués, et une journée compte un nombre entier de pas. `Game` donne
  le temps réel de chaque image à l'accumulateur du socle et joue au plus un nombre borné de pas par image ; quand la
  borne est atteinte, le retard est abandonné (une image lente ralentit le jeu au lieu de le faire courir après
  lui-même). Une vitesse accélérée, si elle vient, fait plus de pas, jamais un pas plus long. La vue lisse le mouvement
  entre les deux derniers pas : ce qui bouge garde sa position du pas précédent.
- **L'ordre du pas**, écrit dans `World` et nulle part ailleurs : les ordres en attente, puis le temps, le terrain,
  l'économie, la société, le monde.
- **Les ordres** (le patron Commande) : chaque geste qui change la ville est un petit objet envoyé au monde : créer
  la Chimère (à la validation de l'écran de création), la coucher, la réveiller, poser, tracer, peindre, effacer, ranger,
  reposer, déplacer (angle compris), affecter, fermer un poste, régler. Chacun est vérifié en entier avant d'agir, par
  la même fonction que l'aperçu, et lève son propre événement. Déplacer est une opération à part entière, jamais
  « retirer puis reposer ». Les ordres attendent le début du pas suivant.
- **Seule la simulation change la simulation.** Ce qui modifie l'état d'un système (poser, faire avancer le temps,
  tenir le maillage) est `internal`, ouvert par `InternalsVisibleTo` aux seules couches de la simulation au-dessus de
  lui ; un ordre (`Command`) ne s'exécute que par le monde. Au-dessus, on lit, on écoute et on envoie des ordres.
- **Les événements** : des `event Action<…>` nommés au passé (`StructurePlaced`, `DayEnded`). Ceux levés pendant un pas
  sont mis en file et livrés à la fin de la couche, jamais au milieu d'une boucle. Dans la simulation, un système
  n'écoute que pour tenir à jour ce qu'il possède ou ce qu'il déduit.
- **La topologie** : le maillage porte un numéro de version, changé à chaque pose, retrait ou tracé, et chaque rangée
  la version de son dernier changement. Ce qui s'en déduit se garde avec la version d'où il vient et se refait quand on
  le lit et qu'elle a changé : les îlots (seules les rangées changées se relisent), ce qu'un habitant rejoint, les
  distances gardées, le trajet d'un marcheur (qui le lit à chaque pas : il revérifie le reste de sa ligne, et n'en
  cherche un autre que si elle est coupée). Ce qui doit agir quand elle change sans la lire à chaque pas écoute
  « topologie modifiée », levé avec ce numéro : la logistique, les logements, la pastille « injoignable ». Toute
  affectation gardée l'est avec cette version.
- **Le déterminisme** : les mêmes ordres, aux mêmes pas, donnent la même partie, quelle que soit la cadence d'images.
  Le hasard vient du hachage ; aucun ordre de traitement ne dépend de l'ordre d'un dictionnaire.

## 6. Les effets

- Un seul système. Chaque effet est une donnée portée par une fiche : la grandeur qu'il vise (une fiche : cadence,
  récolte, heures du soir, vitesse de progression, bonheur…), son étage (ajout, pourcentage, facteur), sa valeur, sa
  portée (la ville, une zone d'un rayon, un habitant, un lieu).
- Chaque système demande la grandeur qu'il lit, avec son contexte (habitant, lieu, position), à une seule fonction.
  L'habitant apporte au lieu un travail déjà multiplié par les effets qui visent la cadence ; l'irrigation agit sur la
  récolte, calculée par la culture ; la lanterne sur les heures du soir, lues par la journée des habitants.
- Le calcul (GDD, section 3) : les effets de zone d'une même sorte ne comptent qu'une fois par cible (le plus fort) ;
  puis les ajouts ; puis les pourcentages, additionnés ; puis les facteurs, dont la force (de 0 à 1). Aucun
  pourcentage n'est négatif aujourd'hui ; un malus demandera un plancher.
- Le registre est déduit : chaque porteur y inscrit ses effets sur événement (pose, rangement, arrivée), et la
  restauration le reconstruit. Ce qui s'affiche au joueur passe par la même fonction.

## 7. Les postes et les tâches

- Un seul système pour tous les métiers : un lieu de travail (bâtiment, campement, champ, étal) ouvre des postes ;
  la fiche du poste liste les sortes de tâches permises, avec leur priorité ; la fiche d'une sorte de tâche dit la
  sous-statistique qu'elle lit.
- L'économie publie les tâches à prendre (transporter, bâtir, semer, récolter, retirer un arbre…) ; l'étal publie le
  service des visiteurs. Un habitant ne prend qu'une tâche permise par son poste, au plus près ou dans l'ordre de la
  file des chantiers, et qu'il peut rejoindre à pied (`Reach`, un seul par monde : les îlots que touchent les abords de
  chaque lieu se gardent avec la version du maillage, un habitant sans tâche cherche à chaque pas sans les refaire).
- **Prendre est un appel unique vers le bas qui réserve** ce que la tâche promet (les marchandises, la place à
  l'arrivée) : deux habitants ne prennent jamais la même. La réservation appartient à l'économie ; elle n'est pas
  sauvegardée et repart vide au chargement.
- Une tâche est une suite de pas (aller, travailler, prendre, déposer), chacun avec une issue d'échec : destination
  perdue, chemin introuvable, délai dépassé. L'habitant rend alors ce qu'il porte, relâche ses réservations et redevient
  disponible ; il garde son poste.
- La journée d'un habitant (travail, soirée, nuit) est une petite machine à états au-dessus de ses tâches : son travail
  (`Job` : le poste tenu, le choix de la tâche dans l'ordre du poste, la flânerie près du poste) et sa tâche en cours
  (`Chore` : marcher jusqu'au but de son geste, y faire ce qu'il dit, travailler sur place ; chaque marche a son issue
  d'échec ; une construction servie qui change de place, il la suit : le but d'une tâche se lit là où elle est). La sorte de tâche dit, par une valeur fixe de sa fiche, quel geste le code joue (`TaskWork`) ; chaque geste
  (`Gesture` : porter, bâtir, défricher, conduire) est le seul à parler à l'économie ; il peut ralentir la marche (une
  charrette tirée) et suit chacun de ses pas. Ce qu'une tâche réserve vit dans l'économie (`Delivery`, `BuildShift`,
  `Clearing`, `Drive`), jusqu'à la place retenue dans un entrepôt pour ce qu'on y
  apportera.
- La cadence d'un lieu est la somme de ce qu'apportent les présents. Qui est présent se déduit des tâches en cours ; le
  plafond d'ouvriers d'un chantier et l'effet d'un maître lisent la même donnée.
- Les compteurs restent chez leur source : l'économie compte la production par marchandise ; la société lève
  `WorkContributed` (habitant, lieu, sorte de lieu, heures) ; les déclencheurs gardent la part de chacun.

## 8. Les emprises peintes : tracés et plantations

- Tout ce qui se peint au sol est une emprise du terrain : chemins, routes, ponts, canaux, arbres de transmission,
  plantations. Un seul système de pinceau (SAVOIR 2.6) : on garde le trait découpé, jamais le geste.
- Une emprise a une sorte (une fiche), une forme et un état (en chantier, bâti) ; une route pas encore bâtie
  n'accélère personne. On ne pose ni ne trace rien sur une plantation : on l'efface d'abord.
- Les champs se déduisent des plantations qui se touchent ; un champ prend l'identité de sa plus ancienne plantation.
  Les places des plants se déduisent sur le maillage : il n'y a pas de second quadrillage.
- La culture (semis, pousse, graines gardées, récolte) appartient à l'économie, par plantation. Le tas de récolte est
  un stock posé au sol.
- Le maillage a un coût par façon de se déplacer (piéton, charrette), lu dans les fiches, en entiers.

## 9. Les fiches

- Une fiche par chose : marchandise, recette, bâtiment, déco, plante, sorte d'emprise, sorte de tâche, poste,
  statistique et sous-statistique, grandeur d'effet, sorte d'habitant, tête, corps, couleur, emplacement et pièce,
  maître, idée et déclencheur, réglages. Les listes ouvertes sont des fiches plutôt que des énumérations : ajouter une sous-statistique, c'est
  ajouter une fiche.
- Chaque fiche a un identifiant texte stable, que la sauvegarde cite ; jamais le nom du fichier. Un catalogue racine les
  rassemble ; le jeu ne charge que lui.
- La fiche d'un bâtiment garde son modèle du pack (la source) et le modèle préparé depuis lui par le menu
  `DICIELA/Contenu/Préparer les modèles` (une construction neuve, ses fiches nées, se prépare seule) : fusionné, porte au nord, centré sur l'empreinte, son origine au sol. Le menu
  y range aussi l'empreinte et la porte, mesurées sur le modèle (SAVOIR 2.9). Le modèle préparé se refait depuis la
  source ; sans source, le même menu fait une boîte à la taille de la fiche, sa porte marquée, et une source qui ne se
  charge plus (le pack absent) est refusée ; un test vérifie que le modèle suit sa fiche. La fiche dit aussi si le bâtiment est unique, s'il est le logis de la Chimère, s'il se
  dresse dès la pose (le campement) et les lits qu'il offre : la simulation lit ces propriétés, jamais un nom. La fiche d'une sorte d'habitant
  dit sa marche, sa flânerie, les bornes où se tirent ses statistiques de départ et s'il rentre dormir la nuit ; son
  allure est l'apparence de chaque habitant : un humain du pack, tel que le pack le fait (sa fiche de personnage,
  `CharacterDefinition`), ou, pour la Chimère, une tête et un corps (chacun séparé
  d'un personnage du pack par le menu des fiches, sur le squelette commun), chacun de sa couleur (une couleur : les
  deux matériaux du pack de même teinte, pour les personnages et pour les objets), au plus une pièce par emplacement, de
  sa couleur. La tête porte ses points et ses surfaces mesurés, les pièces qui se sont détachées d'elle (et ce que le
  pack pose sur elle) et les emplacements qu'elle ne porte pas ; le corps garde son personnage, son dos et son ventre
  mesurés, et ce que ce personnage porte ailleurs que sur la tête. L'emplacement dit l'os, le repère (les pieds ou un
  point de la tête), le contact, si ses pièces vont par paires (qui se posent par leur base) et si l'on ne peut pas s'en
  passer ; la pièce, son modèle (du pack, détaché d'une tête, ou une copie du pack : une paire en deux moitiés, une barbe
  teinte comme le poil), sa tête de référence, son décalage, sa rotation, sa taille par axe, ses réglages sur les têtes
  ou les corps qui ne ressemblent pas aux autres, la face qui touche et ce qu'elle cache. Ce qui se déduit du pack
  (maillages, points, surfaces, pièces d'office, os, repère, contact, paires et emplacements obligatoires, copies, tête
  de référence) est réécrit à chaque passage du menu ; le décalage et la taille d'une pièce ne sont écrits qu'une fois
  (taille nulle : pas encore réglée), puis ils sont au développeur, comme sa rotation et ses réglages. La vue habille
  (SAVOIR 2.11) ; la simulation garde l'apparence sans la lire. Le catalogue mène à la sorte de la Chimère
  et à ce que l'écran de création propose, dans son ordre (les pièces détachées des têtes en tête de liste).
- Une fiche de réglages de la simulation tient le pas, la borne de pas par image, la durée de la journée et de ses
  phases, les mailles par mètre et les bornes de boucle. Une fiche de session dit quelle carte charger et où ranger
  les sauvegardes.
- La simulation lit les fiches sans jamais les écrire. Dans la simulation : jamais `.name`, jamais `== null` sur une
  fiche (`ReferenceEquals`), ni `AnimationCurve` ni `Gradient`, et aucun cache rangé dans une fiche.

**Le lexique.** Le code est en anglais, les commentaires et les documents en français ; un mot du jeu a un seul nom de
code.

| Jeu | Code | Jeu | Code | Jeu | Code |
|---|---|---|---|---|---|
| habitant | `Villager` | stock | `Inventory` | emprise peinte | `Stroke` |
| Chimère | `Chimera` | entrepôt | `Warehouse` | plantation | `Planting` |
| maître | `Master` | tas au sol | `GroundPile` | champ (sauvage) | `Field` (`WildField`) |
| lieu de travail | `Workplace` | étal ; charrette ; place de charrette | `Stall` ; `Cart` ; `CartSpot` | construction | `Structure` |
| poste | `WorkSlot` | visiteur | `Visitor` | chantier, file | `ConstructionSite`, `ConstructionQueue` |
| tâche, à prendre | `Task`, `TaskOffer` | marchandise | `Good` | réserve | `Reserve` |
| sorte de tâche | `TaskKind` | recette, atelier | `Recipe`, `Workshop` | maillage | `NavGrid` |
| effet, grandeur | `Modifier`, `ModifierTarget` | fiche | `…Definition` | force, réseau | `Power`, `PowerNetwork` |
| déclencheur, récompense | `Trigger`, `Reward` | ordre | `Command` | roue, arbre de transmission | `Waterwheel`, `Shaft` |
| savoir de la ville | `Knowledge` | monde | `World` | pastille | `StatusBadge` |
| déroulé du départ | `Opening` | instantané | `Snapshot` | description de la carte | `MapDescription` |
| place et angle | `Placement` | règles de pose | `PlacementRules` | aperçu ; fantôme (posé, pas bâti) | `Preview` ; `Planned` |
| outil de pose | `PlacementTool` | panneau Construire | `BuildBar` | famille ; catalogue | `StructureFamilyDefinition` ; `Catalog` |
| table des textes | `TextTable` | zone de l'interface | `IInterfaceArea` | routine de pose | `ModelPose` |
| trajet ; sa recherche | `Path` ; `PathFinder` | modèle du pack ; préparé | `SourceModel` ; `Model` | îlot ; les îlots | `Island` ; `Islands` |
| ligne tendue ; la tendre | `Path.Corners` ; `PathTightener` | ligne droite (libre ou non) | `StraightLine` | marche | `Walk` |
| flâner | `Wander` | sorte d'habitant | `VillagerKindDefinition` | point d'arrivée | `ArrivalPoint` |
| hasard par hachage | `HashRandom` | bâtie (une construction) | `Built` | unique | `IsUnique` |
| logis de la Chimère | `ChimeraHome` | aller dormir ; se réveiller | `Bedtime` ; `Wake` | choisir ; toucher | `ChooseTool` ; `Touch` |
| corps ; couleur (d'un corps) | `Body` ; `Palette` | emplacement ; pièce | `AttachmentSlot` ; `Attachment` | apparence ; habiller | `Appearance` ; `Dresser` |
| écran de création | `CreationScreen` | aperçu de la Chimère | `AppearancePreview` | valider | `ConfirmControls` |
| scène du démarrage | `GameStage` | décalage ; taille (d'une pièce) | `Offset` ; `Scale` | tête ; pièce portée | `Head` ; `WornAttachment` |
| choix de l'écran de création | `CreationChoices` | découpe (crâne, corps) | `BodyCutter` | morceau soudé (d'un maillage) | `Shell` |
| repère (d'un emplacement) | `AttachmentFrame` | contact ; carte de surface | `ContactAxis` ; `SurfaceMap` | oreilles cousues ; couvercle | `FusedEars` ; `SkullCap` |
| atelier ; ses gestes | `Atelier` ; `PreviewControls` | tourner (la Chimère) | `Turn` | reculer (la caméra) | `PullBack` |
| pose (d'une pièce) ; paire, moitié ; obligatoire | `PiecePlacement` ; `Pairs`, `EarHalves` ; `Required` | rotation ; réglage (par tête, par corps) | `Rotation` ; `WearerAdjustment` | tête de référence ; se poser par sa base | `Reference` ; `Contact.TryRest` |
| abords (d'une construction) | `Surroundings` | livraison ; place d'ouvrier | `Delivery` ; `BuildShift` | bilan des marchandises ; cause | `GoodsLedger` ; `GoodsCause` |
| entrepôts (ce qui tient les marchandises de la ville) | `Warehouses` | poser (l'opération du monde) | `Placing` | travail ; tâche en cours | `Job` ; `Chore` |
| fiche (d'un lieu) | `PlaceSheet` | Échap ; ce qu'il referme | `EscapeControls` ; `IEscapable` | là où est le HUD | `HudArea` |
| statistique, sous-statistique | `StatisticDefinition`, `SubStatisticDefinition` | table des statistiques ; celles d'un habitant | `StatisticTable` ; `VillagerStatistics` | ce que consulte un habitant | `VillagerServices` |
| arbre ou rocher (de la carte) ; sa sorte ; ceux de la carte | `NatureItem` ; `NatureDefinition` ; `Nature` | marquer, démarquer (à retirer) ; défricher | `Mark`, `Unmark` ; `Clear`, `Clearing` | geste (d'une sorte de tâche) ; but d'une tâche | `Gesture` ; `ChoreGoal` |
| place retenue (dans un stock) | `SpaceReserved` | ce qu'on peut rejoindre à pied | `Reach` | anneau carré (de mailles) | `SquareRing` |
| outil « Retirer » ; ses gestes | `ClearingTool` ; `ClearingControls` | outil en main | `IHeldTool` | cadre tiré à la souris | `ISelectionFrame`, `SelectionFrame` |
| arbres vus ; leurs boîtes | `NatureViews` ; `NatureBoxes` | fanion (d'un arbre marqué) | `Mark`, `MarkMesh` | copie du sol | `MapScene` |
| onglet, carte (du panneau Construire) | `Tab`, `Card` (`BuildBarElements`) | roue des outils ; son anneau | `ToolWheel` ; `WheelRing` | infobulle | `Tooltips` |
| gestes des outils ; outil choisi | `ToolControls` ; `ToolChoice` | touches des outils | `IToolKeys` | ouvrir, fermer (un panneau) | `Open`, `Close` (`IBuildMenu`) |
| la main (un seul outil) | `Hand` | outil de la roue (ses gestes ; son allure) | `ToolAction` ; `WheelTool` | boutons de la fiche | `PlaceSheetActions`, `PlaceSheetAction` |
| couche (où la Chimère dort) | `HasCouch`, `CouchFeet` | stock de départ | `StartingGoods` ; `GoodsCause.StartingStock` | classes de style posées par le code | `StyleClasses` |
| mettre en service (l'opération du monde) | `Commissioning` | chantiers finis (à mettre en service) | `ConstructionSites.Finished` | reprendre ses mailles (un pied debout) | `NavGrid.Reclaim` |
| tronçons (d'une rangée du maillage) | `FreeRuns` | ligne du chantier (côté rendu) | `SiteLine` | stock de la ville changé | `CityStockChanged` |
| lits (qu'offre une maison) | `Beds` | ce que le panneau propose | `BuildBarOffer` | logements | `Housing` |
| arriver (la seule arrivée) | `Arrive` | point fort (d'un arrivant) | `Strength` | coucher et lever | `Bedtimes` |
| premier habitant | `FirstVillagerDefinition` | personnage du pack (tel quel) | `CharacterDefinition` | sorte des habitants ordinaires | `Catalog.VillagerKind` |
| le nom d'un habitant (sa clé) | `NameKey` | ouvrir un poste pour lui | `WorkSlots.OpenFor` | un poste sur la fiche d'un lieu | `PlaceSheetSlot` |
| apprendre (une idée) ; appris | `Knowledge.Learn` ; `Learned` | lieu de travail (d'un premier habitant) | `Workplace` | les onglets du panneau | `BuildBarTabs` |
| fermer un poste | `WorkSlots.Close` | s'installer à son lieu de travail | `Opening.Settle` | un sol qu'on traverse (la Place) | `IsWalkable` |
| devants de porte | `Doorways` | épouser le relief (un sol) | `FloorDrape` | dallage (d'une composition) | `FloorMesh`, `PackComposition.Floor` |
| bordure de la Place | `PlaceBorder` | poser par le milieu de sa boîte | `Piece.Centered` | encombrement ; places libres ; ce qui tient encore | `GoodDefinition.Space` ; `FreeSpace` ; `RoomFor` |
| charrettes de l'étal ; à conduire ; vues | `Carts` ; `CartOffers` ; `CartViews` | conduire ; atteler ; tirer ; garer | `Drive` ; `Hitch` ; `Pull` ; `Park` | cap (au sol) ; où se garent les charrettes | `Heading` ; `CartSpotLayout` |
| déplacer (une construction) ; son sol | `MoveStructureCommand`, `StructureMoves` ; `StructureGround` | déplacée ; déplacement refusé | `StructureMoved` ; `MoveRefused` |

## 10. La carte, les scènes et la sauvegarde

- **Les scènes.** `Carte` est au développeur : le Terrain, l'eau, la lumière, les filtres, et ce qu'il y pose (arbres,
  rochers, champs sauvages, sortes d'eau, point d'arrivée). Elle naît d'une copie de la démo du pack dont le menu de
  montage ne garde que le soleil, les filtres et le Terrain (relief vierge) ; il ne la touche plus ensuite. `Jeu` est
  montée par le même menu : démarrage, caméra, interface, l'atelier de la création (un prefab généré, posé loin de la
  carte, avec sa caméra), sans soleil à elle ; elle charge `Carte` (refusée si elle est déjà ouverte dans l'éditeur) et
  en fait la scène active, sinon la lumière du pack est perdue. `JeuTest`, montée par le même menu, a les mêmes objets
  et charge `CarteTest` (plate, éditeur seulement). Chaque scène racine a sa fiche de session, lue avant
  tout chargement : les tests ne peuvent pas atteindre la vraie partie. On ne fixe jamais la scène de départ du Play ;
  un menu `DICIELA/Jouer` ouvre `Jeu`, seule, et lance le Play.
- **La carte entre dans la simulation en données pures.** Au lancement d'un monde, `Game` convertit la scène `Carte` en
  une description : hauteurs en mètres, eaux et leur sorte, arbres, rochers, champs sauvages, point d'arrivée, avec des
  identifiants tirés de la position arrondie. Le relief entre le premier, lu sur le Terrain désigné par la racine de la
  carte (`MapRoot`), posé à l'origine ; hauteur et pente se lisent sur une même surface, par interpolation bilinéaire.
  Le point d'arrivée est l'objet de la carte qui porte `ArrivalPoint` ; sans lui, le centre. Les arbres et les rochers
  sont les instances du pinceau à arbres du Terrain dont le modèle est rangé dans une sorte (`NatureDefinition`) ; un
  modèle rangé nulle part est dit dans la console et reste un décor. Chacun a un numéro dans l'ordre de la description,
  et la lecture garde le numéro de son instance sur le Terrain : la vue le retrouve ainsi ; son identifiant de
  sauvegarde se tirera de sa position arrondie. Le reste entre avec les systèmes qui s'en servent. `World` reçoit la
  description ; la ville témoin en fabrique une. La partie joue sur une copie en mémoire du sol (`MapScene`), donnée au
  Terrain et à son collider, détruite au départ : la carte du développeur n'est jamais modifiée (SAVOIR 2.5).
  L'empreinte de la description est sauvegardée : si la carte a été repeinte, les poses sont revérifiées par la fonction
  des ordres.
- **La copie de travail du Terrain.** Ce qui modifie le sol en jeu travaille sur une copie en mémoire (`Instantiate`
  du TerrainData, donnée au Terrain et au TerrainCollider), jamais enregistrée : modifier le Terrain en Play le
  modifierait pour de bon. Le rangement du Terrain dans Git est dans SAVOIR 2.5.
- **La sauvegarde.** Chaque système donne et reprend son état sous forme d'instantanés, des données simples distinctes
  de ses classes vivantes et définies dans sa couche. `Persistence` en fait du JSON (Newtonsoft), l'écrit de façon
  atomique et convertit les vieux fichiers sur le JSON lui-même ; elle reçoit son dossier, elle ne le cherche pas. Le
  reste suit SAVOIR 2.3.
- **La restauration** remplit les couches du bas vers le haut sans lever d'événement de jeu, recalcule les déductions
  dans le même ordre, puis lève un seul `WorldRestored`. Le monde fait le compte rendu des réparations ; la sauvegarde
  ne compte que ce qu'elle n'a pas su lire, et met le fichier de côté avant de l'écraser.
- **L'enregistrement automatique** se fait entre deux pas, jamais pendant.

## 11. La vue, les gestes et l'interface

- **La vue** écoute les événements et ne photographie jamais : elle se construit à partir de l'état au chargement,
  puis suit les événements, par la même routine de pose (SAVOIR 2.9). Chaque abonnement est défait dans `OnDestroy`.
  Ce qui bouge se lit à chaque image : le démarrage donne aux habitants et aux charrettes vus la fraction du pas en
  cours, et chacun se montre entre sa position du pas d'avant et celle du dernier (SAVOIR 2.2) ; une charrette garée
  glisse en douceur jusqu'à sa place. Les vues du monde (constructions, habitants, nature, charrettes) se montrent,
  se suivent et se cachent ensemble (`WorldViews`) ; celle des charrettes (`CartViews`) naît au démarrage, hors de la
  scène. Endormie, la Chimère reste pour la
  simulation devant la porte de son logis ; la vue la montre couchée sur sa couche, si la fiche du logis en a une (le
  campement), sinon elle la cache (elle est entrée). Un point du modèle posé se calcule par la même routine de pose
  (`ModelPose`).
- **La caméra de gestion** : la vue la tient (`CameraRig` : un point visé sur la carte, qui suit le relief de la
  simulation, et une distance qui règle l'inclinaison ; `CameraAtmosphere` : les ombres et le brouillard qui suivent
  le zoom, sur une copie en mémoire des réglages du rendu, rendue à la sortie) ; les gestes la commandent
  (`CameraControls`) ; ses chiffres sont dans une fiche. La visée de la pose passe après elle (SAVOIR 2.8).
- **Les gestes** : un seul asset d'actions, monté par un script d'éditeur. Les raccourcis se lient au caractère, le
  déplacement à la position (SAVOIR 3). Un geste appartient à la ville ou à l'interface dès l'enfoncement, même si
  ce qui l'a reçu disparaît aussitôt (la roue qu'on congédie), et à l'outil qui était en main à l'enfoncement : l'outil
  pris pendant l'appui n'en fait rien ; clic ou glisser se décide au relâchement (SAVOIR 2.8). Sans outil en main, un clic gauche touche la construction dont le
  modèle est sous le pointeur : la vue dit laquelle (SAVOIR 2.7), l'outil « choisir » en fait un ordre. Un outil en
  main (`IHeldTool` : poser, retirer) prend le clic gauche ; un seul à la fois : la main (`Hand`) lâche les autres
  quand on en prend un, par n'importe quel chemin, et le geste « toucher » y lit si un outil est en main. La roue des
  outils les prend : Retirer en main, ou Construire, qui ouvre le panneau Construire, dont une carte met la
  construction en main ; ses gestes (`ToolControls` : la touche de la roue tenue, celle de chaque outil) ne la
  connaissent que par `IToolWheel`, et elle lit leurs touches par `IToolKeys`. Ses outils sont décrits par deux
  tables, au rang de leur choix (`ToolChoice`) : `ToolAction` pour les gestes (l'action, sa touche d'origine, que lit
  aussi le monteur des actions), `WheelTool` pour l'interface (l'élément, le nom, l'usage) ; ce que fait chaque outil
  n'est écrit que dans le présentateur de la roue. L'outil « Retirer »
  vise un arbre par la boîte de son modèle et marque ce que contient le cadre qu'on tire, que l'interface dessine
  (`ISelectionFrame`). Un panneau ouvert rend la ville muette ; les conteneurs plein écran de l'interface ne captent pas le pointeur. Les gestes ne
  connaissent pas l'interface : elle leur dit seulement où elle est (`IInterfaceArea`), et `Game` les relie.
- **L'ouverture d'une partie** : la scène du démarrage (`GameStage`) montre le monde (constructions, habitants) et ouvre
  l'écran de création dans l'atelier (`Atelier` : un décor généré loin de la carte, sa lumière, sa caméra) ; l'aperçu
  (`AppearancePreview`) s'y tient sur le socle, et les gestes de l'atelier (`PreviewControls`, leur propre carte
  d'actions) le font tourner. Une seule caméra rend à la fois : celle de la ville reste éteinte tant qu'elle ne montre
  pas la carte. La validation envoie l'ordre « créer la Chimère », cache l'atelier, ouvre la ville (caméra, barre, gestes
  de la pose et du toucher ; la caméra part au plus près de la Chimère et recule) et fait partir le temps, dont le
  premier pas se joue aussitôt : la Chimère naît au point d'arrivée dans la même image.
- **L'interface** suit le patron MVP : un présentateur écoute la simulation et remplit son panneau. Pour les écrans
  chargés (l'étal, la liste des habitants), la liaison de données de UI Toolkit reste possible.
- **Le dessin de l'interface** (décisions 144 et 145) : une seule feuille, `Assets/UI/Fusion.uss`, importée par le
  thème, tient les couleurs, les tailles et les polices ; les vues ne posent que ses classes, et les générateurs
  d'images lisent ses couleurs. Une façon de dessiner par matière : le cadre de bois (`WoodFrame`) pose sous son
  contenu deux images générées, son ombre et sa face tirée de l'atlas du pack, découpées en neuf (`Cadre.uss`,
  générée avec elles) ; les faces plates (`Facet` : applique, feutre, creux, bulle ; `FeltButton` pour celles qui se
  cliquent) se dessinent en vectoriel d'après leurs propriétés `--…`, que les états (`:hover`, `:active`, `.ouvert`)
  changent, les classes que les vues posent étant écrites une fois (`StyleClasses`) ; les icônes sont des images
  vectorielles générées. Aucun filtre USS. Le panneau garde ses pixels :
  `InterfaceScale` lui donne son échelle d'après l'écran (ou l'image où il se rend) et le réglage. Les images,
  l'ombre et les icônes se refont par `DICIELA/Interface/Générer les images de l'interface`, les vignettes des
  constructions par `DICIELA/Interface/Photographier les constructions`.
- **Le HUD** est un seul document (`Hud.uxml`, sa mise en page dans `Hud.uss`), sur l'objet « Interface » : chaque
  bloc a sa vue, qui cherche son sous-arbre par son nom, et son présentateur (le panneau Construire, `BuildBar`,
  ouvert à la demande par `IBuildMenu`, dont les onglets et les cartes se font dans `BuildBarElements` ; la roue des
  outils, `ToolWheel`, ses éléments `ToolWheelElements`, son anneau `WheelRing` et ses mesures `WheelGeometry` ;
  la journée, `DayBoard` ; la fiche du lieu touché, `PlaceSheet` ; le stock en fanions, `StockPennants` ; l'astuce,
  `TipBoard`, dont le portrait est pris à l'atelier par la vue, `Portrait`) ; une seule bulle sert d'infobulle à tous
  (`Tooltips`), chaque vue dit ce qu'elle montre au survol. Une vue se branche quand la ville s'ouvre (`Show`) et se
  débranche quand elle se ferme (`Hide`) : ses boutons, ses rappels, ses infobulles (une infobulle donnée se retire). Le démarrage donne à la journée, à chaque image, la
  fraction du pas en cours. Tout le HUD dit aux gestes où il est (`HudArea`) ; l'outil « choisir » dit ce qu'on a
  touché, et le démarrage le relie à la fiche. Échap est une pile (`EscapeControls`) : la fiche, la roue, le panneau Construire, puis l'outil
  en main.
- **Aucun état statique modifiable** dans la simulation : le registre, le catalogue et les textes sont des objets
  créés par `Game`. Le Play de ce projet ne recharge ni le code ni la scène : `Game` crée le monde, la table des textes
  et l'outil de pose, et les libère à la sortie (les outils de l'éditeur lisent le monde en cours par
  `GameRunner.World`) ; chaque composant de gestes active sa carte de l'asset d'actions dans
  `OnEnable` et la désactive dans `OnDisable`.

## 12. Les tests et les garde-fous

- **EditMode** pour toute la simulation, par les ordres. Les fiches des tests sont créées en mémoire et remplies par
  une seule fabrique de test (par `SerializedObject`, le chemin de l'Inspector), puis détruites.
- **PlayMode** pour ce qui a besoin d'une scène : la scène `JeuTest` crée le monde sur sa carte plate et le fait
  avancer (lu dans le journal) ; deux parties de suite y posent chacune une seule fois, et l'asset du Terrain en sort
  inchangé ; poser, tourner, déplacer, ranger par les mêmes appels que les gestes ; les gestes eux-mêmes, joués par une
  souris et un clavier simulés (le banc d'essai de l'Input System).
- **Les tests qui rapportent le plus** sont dans SAVOIR 4. Les oracles des trajets sont écrits dans les tests, pas
  dans le jeu : un Dijkstra naïf pour le A*, un remplissage pas à pas pour les îlots, un juge qui essaie chaque maille
  pour la ligne tendue. Les fichiers témoins (or, vieilles sauvegardes) sont dans `Assets/Tests/Fixtures`, en lecture
  seule, régénérés par un menu seulement.
- **Les garde-fous** : un test lit le code de la simulation et refuse `MonoBehaviour`, `GameObject`, `Time`,
  `UnityEngine.Random`, `System.Random`, `Mathf.PerlinNoise`, `Debug`, `Resources`, `UnityEditor`, les statiques
  modifiables et Newtonsoft ; un test compare les `.asmdef` au tableau de la section 2 (dossier, références,
  plateformes, symboles ; aucun `.asmref`) ; deux tests gardent la section 5 (ce qui change l'état reste `internal`,
  aucune ouverture hors de la simulation, lue sur les assemblages compilés) ; un test lit dans l'Inspector caché
  d'Unity que les shaders du chantier restent regroupés par le SRP Batcher ; un test vérifie que les
  identifiants de fiches sont uniques et que les clés de textes existent.
- **Le journal** : `Game` écrit dans la console les événements clés (jour fini, arrivée, chantier fini), pour que le
  Play court de la procédure `verifier` prouve que la simulation travaille ; dans l'éditeur et un build de
  développement seulement (`Debug.isDebugBuild`), jamais dans le jeu du joueur.

## 13. La ville témoin, le rapport d'équilibrage et la vue témoin

- **La ville témoin** (`Diciela.Scenarios`) : un plan de ville écrit comme une liste d'ordres datés (heure de partie)
  qui citent les fiches par identifiant. Elle n'utilise que les ordres, la capture de sauvegarde et les événements
  publics : aucune méthode n'existe pour elle. Un test la joue plusieurs jours sur une carte plate, sans image, et
  échoue sur les invariants : le bilan des marchandises égale la somme des causes ; personne n'est bloqué (un échec de
  tâche qui revient, une position figée) ; aucune exception.
- **Le rapport d'équilibrage** : un menu `DICIELA/…` écrit `Docs/EQUILIBRAGE.md`, généré, jamais retouché à la main :
  valeur de chaque marchandise et taux de l'étal, cadences des recettes, et le rythme de la ville témoin jouée sur la
  vraie carte, face aux cibles du GDD, lues dans une fiche. On le régénère en fin d'étape quand les chiffres bougent.
- **La vue témoin de l'album** : un menu charge une partie témoin (qui porte son heure), place la caméra au point de
  vue d'une fiche et capture en JPG pour `Docs/ALBUM.md`.

## 14. Les références

- [Factorio, FFF-30](https://www.factorio.com/blog/post/fff-30) : les gestes du joueur envoyés à la simulation.
- [Timberborn, étude de cas Unity](https://unity.com/resources/timberborn) : un pas découpé en étapes nommées.
- [Boss Room, architecture](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/manual/samples/bossroom/architecture.html) :
  assemblages par domaine, racine de composition, messages typés plutôt qu'un état de jeu en ScriptableObjects.
- [RimWorld, tâches et réservations](https://rimworldwiki.com/wiki/Modding_Tutorials/Code_MendingJob) ;
  [Oxygen Not Included, priorités](https://oxygennotincluded.wiki.gg/wiki/Errand).
- [Factorio, modules](https://wiki.factorio.com/Module) et [Path of Exile](https://pathofexile.fandom.com/wiki/Stat) :
  des bonus qui s'additionnent, des étages séparés, un plancher.
- Unity, [Level up your code with design patterns and SOLID](https://unity.com/resources/design-patterns-solid-ebook)
  (Commande, Observateur, État, MVP, drapeau sale, poids mouche) et « Create modular game architecture with
  ScriptableObjects » (les fiches comme données partagées) ; la
  [liaison de données de UI Toolkit](https://docs.unity3d.com/6000.5/Documentation/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/data-binding.html).

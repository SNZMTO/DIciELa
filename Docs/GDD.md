# DICIELA — le document de jeu

> Le jeu tel qu'il doit être, au présent. Chaque ligne est une **Règle** (décidée), un **Défaut** (un départ qu'on
> ajuste en jouant ; quand il entre dans une fiche de données, la fiche fait foi et le GDD garde l'intention) ou un
> **À toi** (à trancher par le développeur, avec la recommandation de Claude). Le pourquoi des choix est dans
> `DECISIONS.md`.

## 1. Le jeu

- **Règle.** DICIELA est un Satisfactory fantasy et cozy : un village fantastique où chaque habitant a un métier. On
  bâtit des chaînes de production que les habitants font tourner, chacun à la tâche qu'on lui donne ; on troque à
  l'étal avec les visiteurs ; on décore en pose libre. La Chimère, qui touche à tout, devient peu à peu la meilleure
  ouvrière du pays… ou une redoutable capitaliste quand elle part en run.
- **Règle.** Les sept piliers :
  1. **Des gens, pas des machines.** Tout ce qui travaille est un habitant qui a un métier et qui progresse ; seules
     les roues à eau tournent seules.
  2. **Le plan est le puzzle.** Où l'on pose compte : le trajet des charretiers, la place des entrepôts, l'eau, les
     routes.
  3. **Beau et utile.** Ce qui embellit sert aussi : les décos donnent du charme, les routes font gagner du temps.
  4. **On découvre en faisant.** Les plans viennent en travaillant, par des idées, pas d'un arbre qu'on achète.
  5. **Rien n'est perdu pour de bon.** Personne ne part, rien ne se démolit, tout retard se rattrape.
  6. **Léger, jamais cynique.** L'humour vient des personnages ; même le capitalisme de la Chimère est une blague.
  7. **Lisible avant d'être riche.** Chaque chiffre qui compte se voit, chaque panne se voit sur place.
- **Règle.** Le ton : fantastique léger, décalé, humoristique (Animal Crossing, une pointe de Dungeon Crawler Carl).
- **Règle.** On joue sur PC, au clavier et à la souris.

## 2. La carte et les mondes

- **Règle.** Une seule carte, peinte à la main par le développeur sur le Terrain du pack ; la même pour chaque monde.
  Elle a des collines avec des replats où bâtir ; un lac rejoint par une rivière, avec un passage rapide (voir 10, la
  force) ; un bord où arrive l'esquisse d'une route, par où viennent les visiteurs et les nouveaux habitants ; des
  arbres, des rochers, et des champs sauvages près des replats où l'on bâtit ; assez de place pour une grande ville et
  ses canaux.
- **Règle.** Le développeur pose dans la carte son point d'arrivée : la Chimère y apparaît au lancement d'une partie,
  et la vue part de là. Sans lui, c'est le centre de la carte.
- **Défaut.** Environ 500 m de côté.
- **À toi.** La taille exacte, avant de peindre. Ma reco : la trouver en posant sur un Terrain nu les bâtiments d'une
  ville de fin de partie.
- **Règle.** En ville, rien ne limite la pose, sauf l'eau, les pentes trop raides, et les arbres et rochers de la carte
  tant qu'on ne les a pas retirés.
- **Défaut.** Pente maximale pour poser un bâtiment : 15°.
- **Règle.** Les paysagistes retirent les arbres et les rochers de la carte : un arbre rend un peu de Bois, un rocher un
  peu de Pierre. Au début, la Chimère s'en charge, à son poste « à tout faire ».
- **Règle.** Les ressources de base (bois, pierre, terre à cultiver) ne dépendent d'aucun gisement : un bûcheron ou une
  carrière produit où qu'on le pose.
- **Règle.** Chaque monde tire au hasard ce qui pousse sur ses champs sauvages : il garantit les plantes des premières
  chaînes et en tire deux ou trois autres ; celles qui manquent arrivent dans les paniers des visiteurs.
- **Règle.** Récolter un champ sauvage donne des graines de sa plante, une marchandise. Le joueur désigne le champ une
  fois : on le récolte chaque fois qu'il a repoussé, la Chimère au début, les fermiers ensuite (sous-statistique
  culture).
- **Règle.** Des graines se sèment dans un champ (voir 8), ou se troquent. Vendues au lieu d'être semées, elles
  repoussent d'elles-mêmes là où on les avait trouvées, et le champ sauvage se récolte de nouveau.
- **Défaut.** Cinq champs sauvages. Le lin est garanti (le tissu en a besoin) ; les autres se tirent parmi le blé, la
  carotte, le poireau, la laitue, la pomme de terre, la tomate, la citrouille, la fraise et les fleurs. Un champ
  sauvage donne de quoi semer 400 m² ; ses graines vendues repoussent en trois jours.
- **À toi.** Un champ sauvage récolté ne repousse-t-il qu'après une vente de ses graines ? Ma reco : oui, dès qu'on a
  vendu ce qu'une de ses récoltes donne ; s'il y en a deux de la même plante, le premier récolté repousse.
- **À toi.** Quels fermiers récoltent les champs sauvages, qui n'ouvrent pas de poste ? Ma reco : ceux de n'importe
  quel champ, au plus près, comme une tâche de plus de leur poste.
- **Règle.** Le menu liste les mondes ; chacun a sa sauvegarde, enregistrée toute seule à intervalle régulier et en
  quittant.
- **Défaut.** Un enregistrement toutes les cinq minutes.

## 3. Le temps et les calculs

- **Règle.** Tout se compte en heures et en jours de jeu ; la durée d'une journée règle le rythme de tout le jeu.
- **Défaut.** Une journée dure environ douze minutes : deux tiers de travail, une courte soirée, un quart de nuit.
- **Règle.** Les habitants travaillent le jour, passent la soirée en ville (c'est là qu'ils se servent à l'étal) et
  rentrent dormir chez eux. Les lanternes prolongent le travail du soir (voir 12).
- **Règle.** La nuit, la ville dort et rien ne se produit ; le joueur prépare la journée (poser, réaffecter, décorer).
- **Défaut.** À l'écran, les habitants marchent à leur pas naturel, environ 1,5 m/s : traverser 250 m prend un tiers
  d'une journée de travail. Les distances comptent ; une ville ramassée va plus vite.
- **Règle.** La ville ne tourne que quand le jeu est ouvert. Ouvrir le menu met le jeu en pause.
- **Défaut.** Le rythme visé :
  - les premiers habitants arrivent le premier jour ;
  - une dizaine d'habitants vers la fin de la première heure, une vingtaine vers la fin de la deuxième ;
  - la maison de la Chimère vers la troisième heure ;
  - un habitant ordinaire atteint les trois quarts de son plafond en une vingtaine de jours de travail.
- **Règle.** Le joueur ne reste jamais plus de quelques minutes sans rien à décider.
- **Règle.** Tous les effets (bonheur, lanternes, charme, maîtres, irrigation, force…) passent par un seul système.
  Chacun vise une grandeur (cadence, récolte, heures du soir, vitesse de progression…), et tous se cumulent de la même
  façon. Un effet de zone ne se cumule pas avec lui-même : deux lanternes éclairent un poste comme une seule (la plus
  forte compte). Puis viennent les ajouts, puis les pourcentages, qui s'additionnent, et enfin la force, qui multiplie
  (de 0 à 1). Exemple : un maître (+100 %) et le bonheur (+20 %) avec une force à moitié : × 2,2 × 0,5 = × 1,1.
- **Règle.** La valeur d'une marchandise est le temps de travail qu'elle demande, ingrédients compris, compté sur sa
  recette de base, au lieu de travail le plus simple : ni les progrès des habitants, ni la force, ni le bonheur ne la
  changent. Si un Bois demande une demi-heure, et une Planche deux Bois plus une heure, la Planche vaut deux heures,
  soit quatre Bois. Les taux de l'étal s'en déduisent (voir 11).
- **Règle.** Un seul réglage à la main : la rareté de chaque marchandise, qui multiplie sa valeur.
- **Défaut.** Rareté 1 pour tout ce que la ville sait faire ; plus haut pour les ressources étrangères.

## 4. La population

- **Règle.** La population est la boucle : grandir, c'est avoir des logements, des emplois et de la nourriture.
- **Règle.** Les maisons donnent la capacité de la ville. Les bâtiments ouvrent des postes, et ce sont les emplois
  libres qui attirent les gens.
- **Règle.** La nourriture est une capacité (« de quoi nourrir 42 habitants »), lue comme une statistique de la ville.
  Chaque champ semé en donne sa part selon ses postes pourvus, en plus de sa récolte et sans rien lui prendre, même
  quand il s'arrête faute de place.
- **Défaut.** Chaque poste pourvu d'un champ nourrit six habitants.
- **Règle.** Un habitant arrive quand il y a un lit libre, un poste libre et de quoi nourrir un habitant de plus.
- **Défaut.** Au plus une arrivée toutes les deux heures de jeu.
- **Règle.** Si la nourriture tombe sous la population, personne ne part : les arrivées s'arrêtent, rien de plus. La
  ville le montre (« les arrivées attendent de quoi manger ») ; pour les relancer, on met quelqu'un de plus aux champs.
- **Règle.** Personne ne part, ne vieillit ni ne meurt ; la ville ne grandit que par les arrivées.
- **Règle.** La Chimère compte dans la nourriture comme une habitante ; elle dort chez elle (sa tente, puis sa maison).
- **Règle.** Le jeu loge chaque habitant au plus près de son poste ; les maîtres passent d'abord, dans les meilleures
  maisons. Un habitant qui change de poste déménage la nuit, si un lit se libère plus près.
- **Règle.** Les maisons ont trois niveaux : cabane, maison, belle maison. Une meilleure maison loge plus de monde, rend
  plus heureux et attire les maîtres ; elle s'améliore sur place, par un petit chantier.
- **Défaut.** Cabane : 2 lits ; maison : 4 ; belle maison : 6.

## 5. Les habitants

- **Règle.** Quatre statistiques, chacune avec une sous-statistique par discipline :

| Statistique | Ce qu'elle regroupe | Sous-statistiques |
|---|---|---|
| **Terre** | ce qui pousse ou se prend dans la nature | culture (champs, champs sauvages), coupe (bûcheron), extraction (carrière), paysage (planter, retirer arbres et rochers) |
| **Bâtisse** | ce qui se construit sur place | construction (chantiers, routes), terrassement (canaux), aménagement (installer les décos) |
| **Artisanat** | ce qui se fabrique en atelier | une par famille d'atelier : bois, fil, pierre ; plus tard métal, cuisine |
| **Échange** | ce qui circule | transport (charretiers), troc (étal), expédition (caravanes) |

- **Règle.** C'est la tâche qui dit quelle sous-statistique elle lit : l'ouvrier lit construction sur un chantier,
  terrassement sur un canal, aménagement sur une déco.
- **Règle.** La statistique globale est la moyenne de ses sous-statistiques : quand l'une monte, la globale monte aussi,
  plus lentement. Une sous-statistique ajoutée plus tard (métal, cuisine) commence à la valeur de la globale.
- **Règle.** La valeur d'un habitant pour une tâche mêle sa sous-statistique et sa statistique globale : travailler aux
  champs aide un peu à la carrière.
- **Défaut.** Les statistiques vont de 0 à 100 ; la valeur pour une tâche est faite pour quatre cinquièmes de la
  sous-statistique et pour un cinquième de la globale.
- **Règle.** Travailler fait progresser la sous-statistique lue. Chaque heure de travail la rapproche de son plafond
  d'une petite part de l'écart restant : vite au début, puis de plus en plus lentement, sans jamais dépasser le
  plafond. Une seule courbe pour tous, tutoriel compris.
- **Règle.** Chaque personnage a deux réglages : sa vitesse de progression et ses plafonds ; le bonheur augmente la
  vitesse.
- **Règle.** Changer de poste ne fait rien perdre : on déplace un habitant pour qu'il apprenne ailleurs.
- **Règle.** La plupart des habitants sont équilibrés, avec un léger point fort ; des profils existent (les marchands,
  meilleurs à l'Échange). Chaque habitant ordinaire a ses plafonds, plus hauts dans son point fort.
- **Défaut.** Un arrivant commence entre 5 et 20 ; ses plafonds vont de 40 à 60, jusqu'à 80 dans son point fort.
- **Règle.** La liste des habitants les montre tous, avec des filtres (statistique, poste, maîtres…) et des tris (par
  n'importe quelle statistique), pour repérer qui déplacer et qui former. La fiche d'un habitant montre ses
  statistiques, ses plafonds (un trait sur chaque barre), son poste et son bonheur.
- **Règle.** Le bonheur d'un habitant vient de sa maison, de ce qu'il obtient à l'étal et du charme de la ville rapporté
  au nombre d'habitants : une ville qui grandit doit s'embellir d'autant. Il ne donne que des bonus : heureux, on
  travaille et on progresse mieux ; malheureux, on perd ces bonus, rien de plus.
- **Défaut.** Le bonheur va de 0 à 100, à parts égales entre la maison, l'étal et le charme ; à 100, 20 % de cadence
  et de vitesse de progression en plus.
- **Règle.** Les maîtres : quand la ville s'améliore, des personnages à forte personnalité arrivent par un tirage
  régulier, dont les chances montent avec la qualité de la ville (production, maisons, charme, réputation).
- **Règle.** Un maître appartient à une statistique et ne travaille que dans son domaine. Il a une capacité unique (un
  effet du système d'effets), de meilleures statistiques, et progresse aussi. Comme tout arrivant, il lui faut un lit
  libre et de quoi manger, mais pas de poste libre. Une poignée au début, énormément plus tard.
- **Défaut.** Un tirage par jour. Un maître commence à 40 dans son domaine et y plafonne à 100.
- **À toi.** Un maître arrive alors qu'aucun poste de son domaine n'est libre. Ma reco : il prend le poste de son
  domaine tenu par le moins bon, qui reprend le meilleur poste libre.
- **À toi.** Énormément de maîtres, c'est autant de capacités à inventer et à équilibrer. Ma reco : des familles de
  capacités (un même effet, des forces différentes), comme les villageois d'Animal Crossing ont des caractères types.
- **Règle.** Pour le décor seulement, les habitants troquent entre eux, fabriquent de petites choses chez eux et ont des
  possessions tirées au hasard, comme dans Animal Crossing ; aucune économie n'est simulée derrière.
- **À toi.** Cette vie du village fait-elle partie du premier jouable ? Ma reco : non, elle vient après.

## 6. La Chimère

- **Règle.** On la crée au lancement d'une partie, sur l'écran de création : une tête d'animal et un corps d'animal,
  pas forcément le même, puis des morceaux d'autres animaux (oreilles, touffe, queue) et des accessoires, au plus un
  par emplacement ; chaque partie a sa couleur. Elle apparaît au point d'arrivée de la carte (voir 2).
- **Règle.** L'écran de création est un panneau sur le côté, dans le langage du HUD (voir 16), dans un atelier hors de la carte, en low poly (un
  fond en alcôve, un sol, un socle) ; la Chimère à venir s'y tient de face sur le socle. On la fait tourner, avec A et
  E ou en la glissant au clic gauche ; elle garde son angle quand on change une pièce. Une ligne par partie, sa pièce
  et sa couleur, chacune entre ses flèches : la tête (seize), le corps et ses habits (seize), puis la queue, les
  oreilles, les cheveux, le chapeau, le visage, la barbe et la ceinture (pas encore d'accessoires de cou ni de dos). La tête, le corps, la queue et
  les oreilles ont toujours une pièce ; les autres lignes ont « aucun » en tête. Les douze couleurs du pack vont sur
  n'importe quelle partie ; changer de partie lui rend sa couleur d'origine. On part du chat, dans ses couleurs, avec
  ses oreilles et sa queue. « C'est parti » (ou Entrée) la crée, et la ville s'ouvre sur elle, au point d'arrivée.
- **Règle.** Une tête est un crâne, ses yeux, ses sourcils et son museau : chaque tête garde le sien, il ne se change
  pas. Ce qui s'en détache (les oreilles du chat, du cochon, du lapin, du raton laveur, du renard et de la souris ; les
  touffes du cochon, du raton laveur, du renard et de la souris) sont des pièces qu'elle porte d'office, qu'on peut
  changer. Changer de tête change ses pièces : chaque ligne qui montrait une pièce de l'ancienne tête (ou rien, faute
  d'en avoir) prend celle de la nouvelle ; une pièce choisie par le joueur reste, et une ligne qui ne peut pas être
  vide garde sa pièce si la nouvelle tête n'en porte pas. Toutes les têtes sont ainsi sans oreilles, et chaque paire
  d'oreilles va sur chacune, sauf la grenouille : elle ne porte pas d'oreilles, sa ligne reste vide avec elle.
- **Règle.** Chaque pièce se pose sur la tête choisie, à son relief : un chapeau descend jusqu'à toucher le crâne, des
  lunettes reculent contre la face, une bourse pend à la hanche, accrochée à la ceinture. Des oreilles se posent aux
  oreilles de la tête choisie, chacune de son côté, par leur base : aucune ne flotte, et sur une tête plus ronde elles
  se rapprochent du milieu, sans déborder du crâne. Les nœuds se tiennent debout (le grand à l'arrière de la tête, ses
  rubans dans le dos). La barbe ne va qu'au chat, à la grenouille, au lapin et à la souris (les autres têtes ne la portent pas,
  leur ligne reste vide) ; elle prend la couleur du poil de sa couleur. Sur le chat, elle encadre la bouche ; sur la
  grenouille, élargie, sous les narines, ses côtés sur les coins de la bouche et le sourire dans son trou ; sur la souris,
  petite, autour de la bouche, comme sur le lapin.
  Une tête qui ne ressemble pas aux autres a ses réglages, pièce par pièce : sur la grenouille, la couronne se pose
  entre les dômes des yeux ; les oreilles du raton et du renard se rapprochent et s'enfoncent sur les autres
  têtes, celles du raton aussi bas que sur le chat. Un chapeau cache les oreilles et les cheveux choisis, touffes et
  huppe comprises (ils restent choisis) ; la couronne, la fleur et les nœuds non. Les cheveux sont ceux des animaux
  (les touffes, la crête de l'oiseau) : les coiffures humaines du pack ne sont pas proposées. Les pièces faites pour
  les humains (chapeaux 1 à 5, barbe, oreilles du pack) sont agrandies pour une tête d'animal, la barbe axe par axe
  pour épouser un crâne large et plat.
- **Règle.** Le temps attend « C'est parti » : la journée commence quand elle arrive.
- **Règle.** Elle n'a pas de nom : c'est la Chimère.
- **À toi.** Les noms des têtes, des corps, des couleurs et des pièces : un premier jet neutre (« Chat A »,
  « Couleur 4 », « Chapeau 6 », « Oreilles de chat »), à réécrire dans la table des textes.
- **Règle.** Le joueur la place à un poste, comme un habitant, et elle y va ; on ne la dirige jamais directement, et
  elle ne prend jamais de poste d'elle-même. Sans poste, elle flâne (voir 7).
- **Règle.** Elle travaille dans tous les domaines. Elle progresse lentement, mais plus haut que les habitants
  ordinaires : sa vitesse est faible, ses plafonds hauts.
- **Défaut.** Elle commence à 10 partout ; tous ses plafonds sont à 100.
- **Règle.** Selon où elle travaille, elle débloque des répliques, des tenues et des idées.
- **Règle.** Personne ne la connaît. Elle arrive dans un lieu vide, avec des trous de mémoire et quelques idées.
- **À toi.** Son histoire, que tu écriras.
- **Règle.** La tente vient en premier, la Place en deuxième. La tente est son logis de départ, et seulement cela :
  elle n'a ni stock ni poste. Elle se plante d'un coup, sans chantier.
- **Règle.** Tant que la Chimère n'a pas de logis, la barre de construction ne propose que la tente ; plantée, la tente
  quitte la barre (elle est unique) et le reste s'ouvre. C'est une consigne de la barre : un ordre qui poserait autre
  chose d'abord n'est pas refusé.
- **Règle.** La Chimère dort quand le joueur le veut : il touche son logis (la tente, puis sa maison), et elle va s'y
  coucher : elle marche jusqu'à sa porte, entre, et on ne la voit plus. Le joueur la réveille en touchant de nouveau
  son logis, même quand elle y va encore : elle flâne alors devant sa porte. Endormie, elle quitte son poste jusqu'à ce
  que le joueur la réveille ; son sommeil n'a pas d'autre effet.
- **Règle.** Si elle ne peut pas rejoindre sa porte, elle ne part pas se coucher ; si sa porte est coupée d'elle en
  chemin, elle y renonce et flâne.
- **Règle.** Le stock de départ est un tas posé au sol près de la tente, qui sert d'entrepôt jusqu'au premier
  entrepôt. Il se pose de lui-même quand la tente est plantée, à droite de sa porte, sinon au plus près. Il porte un
  poste « à tout faire » (porter, bâtir, récolter, défricher, dans cet ordre), que tient la Chimère ; le poste
  disparaît avec le tas, quand le tas est vide et que la loge, un entrepôt et la cabane du paysagiste sont bâtis
  (sinon, plus personne ne retirerait les arbres et les rochers).
- **Règle.** Le joueur y met la Chimère en touchant le tas : sa fiche s'ouvre sur le côté, avec « Y mettre la
  Chimère » ou « Retirer la Chimère ». Tant que la carte n'a ni arbres ni champs sauvages à traiter, le poste ne
  fait que porter et bâtir.
- **Défaut.** Le tas de départ fait 2 × 2 m et tient 50 marchandises : 30 Bois et 20 Pierre. La Place, la cabane et
  les premiers bâtiments ne demandent que du Bois et de la Pierre ; le stock de départ suffit à la Place et à deux
  cabanes. Une cabane coûte 10 Bois et 5 Pierre et demande 2 h de travail à un débutant ; à pied, on porte 5
  marchandises par voyage.
- **Règle.** Les premiers habitants arrivent vite, un par un, à mesure que les lits le permettent. Chacun est fort dans
  une discipline, apporte l'idée de son lieu de travail et le bâtit avec la Chimère, avant même la loge. Ils restent en
  ville comme habitants ordinaires ; ce ne sont pas des maîtres.
- **Règle.** Ils n'attendent ni poste libre (ils apportent l'idée de ce qui l'ouvrira) ni de quoi manger (au début,
  aucun champ ne tourne) : il leur faut seulement un lit. La règle de la nourriture vaut pour les arrivées suivantes.
- **Défaut.** Ils commencent à 40 dans la sous-statistique de leur métier. Le déroulé :
  1. La Chimère plante sa tente, d'un coup ; le joueur choisit l'endroit. Elle connaît déjà la tente, la Place, la
     cabane, le chemin de terre, le pont, le bûcheron et la carrière.
  2. Elle bâtit la Place et deux cabanes, et récolte un champ sauvage de lin.
  3. Un premier habitant par statistique arrive, chacun avec son idée : le bâtisseur (la loge des bâtisseurs), la
     fermière (les champs : elle sème le premier, en lin), la charretière (l'entrepôt), le menuisier (la scierie).
  4. Quand la nourriture retient les arrivées, le tutoriel fait récolter un deuxième champ sauvage, agrandir le champ
     avec ses graines et y mettre un deuxième fermier.

## 7. Les postes et le travail

- **Règle.** Un seul système de postes et de tâches pour tous les métiers. Le poste dit quelles tâches on y fait ; la
  tâche dit la sous-statistique qu'elle lit (voir 5).
- **Règle.** Un bâtiment a un nombre fixe de postes ; l'améliorer en ajoute, par un petit chantier. Un champ, lui, en
  ouvre selon sa surface (voir 8).
- **Défaut.** Trois niveaux ; deux postes au premier, un de plus par niveau.
- **Règle.** Un lieu de travail (bâtiment ou champ) sans habitant à son poste est à l'arrêt : un champ sans fermier ne
  produit ni récolte ni nourriture. Seul un bâtiment sans poste tourne seul : la roue à eau.
- **Règle.** Un lieu de travail qui ne tourne pas le dit sur place, par une pastille : personne au poste, pas de
  graines, pas d'ingrédients, stock plein, pas de force, injoignable.
- **Règle.** La cadence d'un bâtiment est la somme de ce qu'apporte chaque habitant présent à son poste ; chacun apporte
  selon sa valeur pour la tâche.
- **Défaut.** Un expert apporte environ trois fois ce qu'apporte un débutant : l'apport croît en ligne droite avec la
  valeur pour la tâche, ×1 à 0, ×3 à 100.
- **Règle.** Un habitant sans poste (un arrivant, ou celui dont le bâtiment est rangé) prend tout seul le poste libre
  qui lui va le mieux ; s'il n'y en a aucun, il flâne et ne travaille pas. Le joueur réaffecte qui il veut.
- **Règle.** Flâner, c'est aller à un point libre tiré autour de l'endroit où l'on s'est mis à flâner, s'y arrêter un
  moment, puis recommencer, comme les colons de RimWorld. Un point devenu injoignable en chemin n'est qu'une pause de
  plus. Si l'endroit même où l'on flânait est pris, ou coupé de soi, on se remet à flâner autour d'où l'on est.
- **Défaut.** Un point à 6 m au plus de l'endroit où l'on s'est mis à flâner, à 2 m au moins de soi ; une pause de 0,1
  à 0,3 h de jeu (3 à 9 s).
- **Règle.** Le joueur peut fermer un poste : personne ne le prend, et celui qui l'occupait redevient libre.
- **Règle.** Des bâtiments de service ouvrent les postes des métiers qui ne produisent rien : la loge des bâtisseurs
  (les ouvriers : chantiers, canaux, décos), l'entrepôt (les charretiers), la cabane du paysagiste (les paysagistes).
- **À toi.** Un habitant en pleine tâche est-il interrompu par une tâche plus urgente ? Ma reco : non, il va au bout,
  comme dans Oxygen Not Included.
- **Règle.** Le poste « à tout faire » choisit ses tâches dans cet ordre : porter, bâtir, récolter, défricher.
- **Règle.** On ne travaille que pendant les heures de travail. Si la journée finit pendant une tâche, l'habitant
  finit l'étape en cours, dépose ce qu'il porte, puis part en soirée ; la Chimère, elle, flâne près de son poste.
- **À toi.** Une pastille ou plusieurs quand un lieu cumule les pannes ? Ma reco : une seule, la plus urgente :
  injoignable, pas de force, personne au poste, pas de graines ou d'ingrédients, stock plein.
- **À toi.** Quel poste tient un premier habitant avant que son lieu de travail existe (voir 6) ? Ma reco : un poste
  « à tout faire » ouvert pour lui, qui se ferme quand son lieu est bâti.

## 8. La production et la logistique

- **Règle.** Un atelier suit une recette : ce qu'il consomme, ce qu'il produit, en combien de temps. Le joueur choisit
  la recette de chaque atelier.
- **Règle.** Les champs se peignent librement, comme les potagers de Stardew Valley en plus grand : chaque coup de
  pinceau est une plantation d'une seule plante. Le sol devient de la terre labourée et les plantes poussent à vue.
- **Règle.** Des plantations qui se touchent forment un champ, qui peut mêler plusieurs plantes ; un chemin entre deux
  plantations les sépare en deux champs.
- **Règle.** Il n'y a pas de bâtiment « ferme » : le champ est le lieu de travail. Il ouvre un poste de fermier par
  tranche entière de sa surface (plus petit qu'une tranche, il n'en ouvre aucun, et le jeu dit de l'agrandir ou de le
  rejoindre à un autre) ; ses fermiers s'occupent de toutes ses plantations (sous-statistique culture).
- **Défaut.** Une tranche fait 400 m² ; une plantation fait au moins 4 m de côté.
- **Règle.** Les fermiers sèment, la plante pousse seule, puis ils récoltent et ressèment aussitôt, si le champ a ses
  graines. Une plante mûre attend qu'on la récolte : rien ne pourrit.
- **Règle.** Chaque semis consomme des graines, chaque récolte en rend un peu plus. Le champ garde de quoi se
  ressemer ; le surplus part avec la récolte.
- **Défaut.** Une récolte rend les graines semées plus un quart. Une plante pousse en deux à six jours, selon sa sorte.
- **Règle.** La récolte s'entasse au bord du champ le plus proche d'un entrepôt : c'est là que passent les charretiers.
- **Règle.** Peindre ou effacer une plantation est immédiat : ce sont les fermiers qui labourent, sèment et récoltent.
  Un champ ne se range pas ; effacé, il rend au stock ses graines semées.
- **Règle.** On ne pose rien et on ne trace rien sur une plantation : on l'efface d'abord.
- **À toi.** La forme d'une plantation. Ma reco : le même pinceau que les routes, d'au moins 4 m de large.
- **À toi.** Le tas de récolte quand l'entrepôt le plus proche change. Ma reco : il reste où il est jusqu'à ce qu'on
  le vide ; les récoltes suivantes vont au nouveau bord.
- **À toi.** La fertilité du sol et la rotation des cultures, comme dans Farthest Frontier ou Manor Lords ? Ma reco :
  pas au premier jouable, où la lisibilité passe avant ; à reconsidérer ensuite, pour les joueurs qui aiment optimiser.
- **Règle.** Chaque bâtiment a deux petits stocks à sa porte : ses ingrédients et sa production. Sans ingrédients, il
  attend ; quand sa production est pleine, il s'arrête.
- **Règle.** Les charretiers vident ces stocks dans les entrepôts, où la ville conserve en gros, et y prennent ce dont
  les ateliers et les chantiers ont besoin. Tout passe par un entrepôt ; ils vont au plus près.
- **Règle.** Le stock de la ville est ce que tiennent ses entrepôts (le tas de départ au début) : un chiffre par
  marchandise.
- **Règle.** Une liaison prioritaire, tirée par le joueur entre deux bâtiments, livre directement de l'un à l'autre sans
  passer par l'entrepôt : c'est l'outil pour optimiser.
- **Défaut.** Chacun des deux petits stocks d'un bâtiment tient 10 marchandises ; un entrepôt en tient 200.
- **À toi.** Où vont les marchandises rendues au stock quand les entrepôts sont pleins ? Ma reco : en tas au sol, sur
  place, avec une pastille ; les charretiers les reprennent dès qu'il y a de la place.
- **À toi.** Un entrepôt rangé compte-t-il dans le stock de la ville ? Ma reco : non ; il garde ses marchandises.
- **À toi.** Que fait un charretier chargé quand on recharge la partie ? Ma reco : il dépose à l'entrepôt joignable le
  plus proche.
- **Règle.** Les récoltes de fruits et de légumes sont des marchandises : elles se troquent, et serviront plus tard à la
  cuisine.

## 9. Les chantiers et les routes

- **Règle.** Tout se construit par des ouvriers, sauf la tente, que la Chimère plante d'un coup ; au tout début, avant
  la loge, la Chimère et les premiers habitants bâtissent eux-mêmes (voir 6). Le joueur pose un fantôme ; les
  charretiers apportent les matériaux ; les ouvriers bâtissent ; un ouvrier qualifié bâtit plus vite.
- **Règle.** La file des chantiers est visible et le joueur en change l'ordre. Un chantier qui attend ses matériaux
  laisse passer les suivants. Annuler rend au stock les matériaux déjà livrés.
- **Défaut.** Trois ouvriers au plus sur un chantier ; les chantiers se servent dans l'ordre de la file.
- **Règle.** Pose libre : aucune case pour le joueur, on parle en mètres. La porte d'un bâtiment reste dégagée : rien ne
  se pose devant. Un bâtiment qu'on ne peut plus atteindre le montre.
- **Règle.** Rien ne se chevauche, même de quelques centimètres, et on ne se glisse pas entre deux constructions plus
  serrées que 25 cm (une maille du maillage) : deux constructions se collent à 25 cm près.
- **Défaut.** Devant une porte, 1,5 m restent libres, sur la largeur de la porte.
- **Règle.** Poser : la construction choisie suit le pointeur ; son aperçu est vert si elle se pose là, rouge sinon,
  et la raison s'écrit. **R** la tourne : une touche brève d'un cran, tenue en bougeant la souris au degré près. Le
  clic gauche pose ; l'outil reste en main pour la suivante, sauf après une construction unique ; un clic droit bref ou
  Échap le pose de côté.
- **Défaut.** Le cran de rotation : 15°.
- **Règle.** Une construction posée attend ses bâtisseurs en fantôme translucide ; bâtie, elle se voit avec ses
  matériaux. Dès sa pose, elle prend sa place : on ne traverse pas plus un fantôme qu'un bâtiment. Qui marche et voit
  son trajet coupé en cherche un autre ; qui se trouve dessous en sort à pied, tout droit, par le sol libre qui la
  borde, jamais celui qu'une autre construction cache : du côté où il est le plus vaste (jamais une poche qu'elle vient
  de fermer), au plus près.
- **À toi.** Retirer un arbre ou en planter un passe-t-il par la file des chantiers ? Ma reco : non, ce sont des tâches
  des paysagistes, dans l'ordre où le joueur les a désignées.
- **Règle.** Ranger ou déplacer un bâtiment est un tout petit chantier : il se déconstruit en quelques secondes et se
  reconstruit de même, sans nouveaux matériaux. Pendant un déplacement, ses travailleurs gardent leur poste et
  attendent.
- **À toi.** Les petits stocks d'un bâtiment déplacé voyagent-ils avec lui ? Ma reco : oui.
- **À toi.** Peut-on ranger le dernier lieu qui ouvre des postes de bâtisseurs ? Ma reco : non, le jeu dit pourquoi ;
  sinon plus personne ne pourrait le reposer.
- **Règle.** On ne démolit pas, on range. La réserve n'a pas de limite. Ranger un bâtiment rend au stock ce que
  gardaient ses petits stocks ; un entrepôt rangé garde ses marchandises jusqu'à ce qu'on le repose.
- **Règle.** Les bâtiments uniques sont la tente puis la maison de la Chimère, la Place et les grands projets : un seul
  de chacun dans la ville. Posé, un bâtiment unique quitte la barre, et une seconde pose est refusée (« Déjà posée »).
  Ils se rangent aussi, et le jeu dit ce qui s'arrête tant qu'ils ne sont pas reposés : une pastille sur le bouton de la
  réserve et une ligne dans la réserve (la Place : plus de visiteurs). Pour la maison de la Chimère, une bulle avec son
  portrait : « Je vais quand même pas dormir dehors… ».
- **Règle.** Un chemin de terre ne coûte aucun matériau (c'est un chantier quand même) ; une route pavée demande des
  Pavés, faits par l'atelier de pierre. Un pont est un bout de route posé sur l'eau. Routes et ponts ne se rangent pas :
  ils s'effacent, par un petit chantier qui rend leurs matériaux.
- **Défaut.** Un pont coûte du Bois.
- **Règle.** Les piétons marchent partout où rien n'est posé, par le trajet le plus court ; les charrettes vont
  lentement dans l'herbe, plus vite sur la terre, encore plus vite sur les pavés. Une route est un investissement,
  jamais une obligation.
- **Défaut.** Charrettes : moitié moins vite dans l'herbe, vitesse normale sur la terre, une fois et demie plus vite sur
  les pavés. Les piétons vont à la même vitesse partout.

## 10. L'eau

- **Règle.** On ne pose rien sur l'eau, sauf ce qui est fait pour elle : les roues à eau, les bâtiments hydrauliques
  posés avec leur roue, et les ponts.
- **Règle.** Les bâtiments hydrauliques sont les moulins : moulin à farine, scierie hydraulique, moulin à foulon pour le
  tissu, martinet pour la forge. Chacun se pose soit sur une rivière ou un canal, avec sa propre roue, soit sur la
  terre, branché au réseau de force.
- **Règle.** Un canal se trace comme une route, part d'une rivière, du lac ou d'un autre canal, et ne coûte que du
  travail : les ouvriers le creusent (sous-statistique terrassement). Il reste au niveau de l'eau d'où il part : il
  entaille le terrain, sans monter. Il se rebouche par un petit chantier.
- **Défaut.** Un canal entaille le terrain de 3 m au plus.
- **Règle.** La force : toute roue à eau verse de la force dans son réseau, tout bâtiment hydraulique en puise. Un
  réseau, ce sont des roues et des bâtiments reliés par des arbres de transmission, posés comme une route courte ; ce
  qui n'est relié à rien ne partage rien. Le joueur gère un budget de force.
- **Règle.** Une roue donne plus de force sur une rivière rapide, moins sur un canal ; elle n'a besoin de personne.
- **Règle.** Un bâtiment hydraulique garde ses postes ; la force multiplie leur cadence. Sans force, il est à l'arrêt ;
  avec trop peu, tout son réseau ralentit en proportion. Un bâtiment hydraulique bâti consomme sa force, qu'il
  travaille ou non.
- **Défaut.** Une roue donne 10 de force sur le passage rapide, 6 sur la rivière, 3 sur un canal. La scierie hydraulique
  consomme 5 et va deux fois plus vite qu'une scierie.
- **Règle.** L'irrigation : une plantation proche de l'eau (lac, rivière ou canal) donne plus de récolte, pas plus de
  nourriture.
- **Défaut.** À moins de 20 m de l'eau : 25 % de récolte en plus.
- **Règle.** L'eau sert aussi de ressource : seaux et tonneaux pour arroser et brasser.
- **À toi.** Arroser à la main et irriguer feraient la même chose. Ma reco : l'arrosage sert les champs loin de l'eau,
  au prix du travail d'un porteur d'eau.

## 11. L'étal, le troc et la réputation

- **Règle.** Aucune monnaie en ville : tout se paie en marchandises.
- **Règle.** La Place est un sol de pierre beige bordé de cailloux, posé comme un bâtiment. C'est là qu'arrivent les
  visiteurs et que se tient l'étal, et là que se jouera la run. Elle a des niveaux qui la rendent plus cozy, chacun par
  un petit chantier.
- **Défaut.** Trois niveaux ; chacun ajoute un poste de marchand, du charme et un visiteur par jour.
- **Règle.** Les postes de l'étal sont ceux des marchands, dont la sous-statistique troc règle la cadence (combien de
  visiteurs ils servent).
- **Règle.** L'étal a deux étagères, « habitants » et « troc », que le joueur garnit : une quantité, ou « garder au
  moins N » au stock. Les charretiers les remplissent depuis les entrepôts.
- **Règle.** Les visiteurs viennent le jour, par la route du bord de la carte, avec un panier, parfois un objet rare. Le
  marchand échange tout seul ce qui est sur l'étagère du troc contre ce qui est sur la liste de souhaits du joueur ; un
  visiteur qui n'a rien de souhaité repart sans échanger, sans effet sur la réputation.
- **Défaut.** La liste de souhaits commence avec tout ce que la ville ne sait pas faire. Le marchand paie avec ce que
  l'étagère du troc a en plus grande quantité.
- **Règle.** Chaque marchandise a un taux de base, déduit de sa valeur (voir 3). Pour chaque marchandise, le joueur
  règle ce qu'il en demande par rapport à ce taux : plus, la ville est radine ; moins, elle est généreuse.
- **Défaut.** De 20 % en dessous à 20 % au-dessus du taux de base.
- **À toi.** La valeur de ce que la ville ne fabrique pas (ressources étrangères, décos rares) et celle d'une récolte.
  Ma reco : une valeur écrite dans la fiche pour ce qui n'a pas de recette en ville ; pour une récolte, le seul
  travail des fermiers, sans le temps de pousse.
- **À toi.** « Ce que la ville ne sait pas faire » (souhaits, paniers) : sans l'idée, ou sans atelier qui tourne ? Ma
  reco : ce qu'aucune recette connue ne produit.
- **Règle.** La réputation, une seule pour la ville, suit peu à peu la justesse des échanges : rien n'est instantané ni
  irrécupérable. Radine, moins de visiteurs et moins de rares ; généreuse, l'inverse.
- **Défaut.** Elle met plusieurs jours à changer franchement. Deux à quatre visiteurs par jour au début.
- **Règle.** Le charme de la ville attire aussi plus de visiteurs, et de plus rares (voir 12).
- **Règle.** Les visiteurs apportent un peu de ressources étrangères, de quoi essayer une idée, et racontent d'où elles
  viennent ; les expéditions en rapportent en quantité (voir 14). Au premier jouable, leurs paniers portent ce que la
  ville ne fait pas encore : fruits, légumes, graines, décos.
- **Règle.** Les habitants se servent sur l'étagère « habitants », qui ne porte jamais ce qui se mange (fruits,
  légumes, blé) : ils mangent la nourriture de la ville. Chaque objet obtenu les rend plus heureux, d'autant plus qu'il
  a de la valeur ; ne rien obtenir n'a pas de conséquence grave, seulement pas de bonus. L'étal est le lieu où la ville
  choisit comment partager, entre ses habitants et les visiteurs.
- **Règle.** Un habitant revient prendre un objet le soir, quand l'effet du précédent s'est estompé. Sans marchand, les
  habitants se servent encore : leur étagère est en libre-service ; seul le troc s'arrête.
- **Défaut.** L'effet d'un objet dure trois jours.

## 12. La déco et le charme

- **Règle.** Toutes les décos donnent du charme. Le charme de la ville est la somme de ses décos ; il n'y a pas de
  charme par maison. Chaque déco de plus d'une même sorte en donne un peu moins : la variété paie.
- **Défaut.** Chaque exemplaire de plus d'une même déco donne 10 % de charme de moins que le précédent.
- **Règle.** Certaines décos ont en plus un effet à rayon : la lanterne prolonge le travail du soir des postes proches.
- **Défaut.** Une lanterne donne deux heures de plus, à moins de 10 m.
- **À toi.** La lanterne compte-t-elle la place du poste ou celle où l'on travaille (un ouvrier de la loge sur un
  chantier) ? Ma reco : là où l'on travaille.
- **À toi.** La Place donne du charme (voir 11) alors que le charme est « la somme des décos ». Ma reco : toute
  construction dont la fiche porte du charme compte, la Place comprise.
- **Règle.** Le joueur pose le fantôme d'un banc ou d'une lanterne ; un ouvrier de la loge vient l'installer avec les
  matériaux (sous-statistique aménagement).
- **Règle.** Arbres et buissons se posent une fois la cabane du paysagiste bâtie ; les paysagistes les plantent. Ils ne
  coûtent que leur travail.
- **Règle.** Les décos se rangent et se déplacent comme les bâtiments. Une déco rare, reçue d'un visiteur ou de la run,
  arrive dans la réserve ; un ouvrier la pose sans matériaux.
- **Règle.** Les « créations en pierre » de l'atelier de pierre sont des marchandises, pas des décos.
- **À toi.** Des quartiers, plus tard, chacun avec son charme, et peut-être son étal quand un seul ne suffira plus à une
  grande ville ? C'est ton idée, gardée pour après le premier jouable.

## 13. Les idées et les grands projets

- **Règle.** À force de faire des choses et de voir la ville avancer, la Chimère ou des habitants ont des idées : des
  plans de bâtiments, de recettes, de décos.
- **Règle.** Une idée a un déclencheur : une chose faite (« après 50 Planches, le menuisier imagine la table ») ou un
  métier exercé (la Chimère a travaillé à la scierie : elle imagine le moulin à eau). Pour l'histoire, elle revient à
  l'habitant qui y a le plus contribué, ou à la Chimère.
- **Règle.** Une idée sert tout de suite, sans coût, comme un plan d'Animal Crossing.
- **Règle.** Le carnet d'idées montre des pistes floues (« le menuisier rêve de quelque chose avec du tissu… ») : le
  joueur sait vers quoi pousser, sans liste de courses.
- **Règle.** Les grands projets, rares, donnent un but lointain ; une idée les débloque.
- **Règle.** Le premier est la maison de la Chimère, bâtie en secret. Le carnet dit que les habitants préparent quelque
  chose ; le joueur pose où il veut un chantier bâché ; le chantier dit ce qu'il consomme, pas ce qu'il bâtit ; à la
  fin, la surprise. La maison remplace la tente comme logis de la Chimère. Elle la rend aussi heureuse qu'une belle
  maison.
- **Défaut.** Les idées du premier jouable, en plus de celles du départ (voir 6) :

| Idée | Déclencheur |
|---|---|
| la maison | la ville compte huit habitants |
| l'atelier de pierre | 50 Pierres extraites |
| la route pavée | 20 Pavés taillés |
| l'atelier de tissu | 20 Lin récoltés |
| la cabane du paysagiste | trois champs sauvages récoltés |
| le banc | 30 Planches sciées |
| la lanterne | cinq jours passés |
| la roue à eau, les arbres de transmission, la scierie hydraulique | la Chimère a travaillé deux jours à la scierie |
| le canal | la première roue à eau posée |
| la maison de la Chimère | la ville compte vingt habitants |
| la belle maison | la ville compte vingt-cinq habitants |

## 14. La carte du monde et les expéditions

- **Règle.** La carte du monde est vierge au départ ; elle se remplit peu à peu, toute seule, de ce que racontent les
  visiteurs : des lieux, des gisements.
- **Règle.** Les ressources étrangères (le métal…) n'ont leurs gisements que là ; certaines idées en ont besoin.
- **Règle.** Une expédition part vers un gisement : une caravane avec des marchandises et des habitants. Plus il est
  loin, plus elle met de temps ; plus il est gros, plus elle rapporte. Rien n'est jamais perdu, au pire un retard ou
  moins de ressources ; le vrai coût, ce sont les habitants absents de leur poste.
- **À toi.** Ce qu'emporte une caravane, et si un gisement s'épuise. Ma reco : la caravane emporte des vivres en
  proportion de la distance, et les marchandises que le lieu demande, comme les expéditions d'Anno 1800 ; un gisement
  ne s'épuise pas, puisque la règle met le vrai coût dans les habitants absents, et il n'y a pas de stock lointain à
  compter.

## 15. La run

- **Règle.** Elle se joue sur la Place, sous un autre angle de caméra.
- **À toi.** Cet angle : en zoomant sur la Place, ou une autre vue ? Ma reco : zoomer sur la Place et y figer la
  caméra en vue de table, la ville autour : on voit qu'elle continue de tourner, et il n'y a qu'une scène.
- **Règle.** Au tour par tour, avec une main de cartes, des dés et des contrats, en 20 à 30 minutes.
- **Règle.** Le Fric n'existe que dans la run, pour ses parties et ses gags ; jamais en ville, jamais rapporté.
- **Règle.** Elle rapporte à la ville des décos rares, des idées (des plans vus ailleurs), des lieux et des gisements
  sur la carte du monde, parfois un maître ; rien qui court-circuite la production.
- **Règle.** La ville lui donne de très légers bonus, trois fois rien.
- **Règle.** La ville continue de tourner pendant la run : la run aide à faire passer le temps.
- **Règle.** La Chimère revient de run avec des répliques de magnat (« Le troc ? Quelle barbarie… ») et s'en remet peu
  à peu en retravaillant en ville.

## 16. La présentation

- **Règle.** Tout vient du pack POLYGON Fantasy Village (Synty), tel quel ; la caméra, la lumière et le rendu
  s'adaptent à lui. On garde son rendu : son sol peint, son eau, ses shaders, son profil de filtres, sa lumière.
- **Règle.** Le but : une vraie partie, ville bâtie, aussi belle que la démo du pack. Pour y arriver, chaque bâtiment
  arrive habillé d'accessoires du pack, composés par le développeur ; le sol s'adapte sous et autour de ce qu'on pose
  (de la terre sous un bâtiment, l'herbe écartée) ; les routes se peignent dans le sol. À regarder : Manor Lords et
  Foundation, deux jeux dont les villes bâties par les joueurs ressemblent à des maquettes.
- **Règle.** Caméra en perspective, comme la démo (champ de vision autour de 40°). Caméra de gestion à main invisible :
  se déplacer (clavier, bords de l'écran, clic droit glissé qui tire le sol), tourner (A et E en AZERTY, ou le bouton
  du milieu glissé de gauche à droite), zoomer à la molette, vers l'endroit que vise la souris. En zoomant, elle
  s'abaisse doucement, d'autant à chaque cran : de loin, la ville en maquette ; de près, les habitants presque de face.
  On part à mi-chemin des crans.
- **Règle.** Le bouton du milieu glissé de haut en bas incline la vue, en plus de ce que donne le zoom, et le zoom
  garde ce choix. Le haut de l'écran reste au moins 5° sous l'horizon, car la carte n'a pas de décor au-delà, et la
  vue ne dépasse pas 80°, une vue d'oiseau (Défauts).
- **Règle.** Les ombres vont jusqu'au sol vu en haut de l'écran, de près comme de loin, dans une borne (50 à 300 m,
  Défauts) ; le brouillard ne voile que le lointain et recule quand la caméra s'éloigne.
- **À toi.** Les touches de la caméra et son glisser. Ma reco (c'est ce qui est codé) : des touches liées à leur place
  sur le clavier, pour que tout clavier ait la même main (en AZERTY : ZQSD pour se déplacer, A et E pour tourner) ; le
  glisser au clic droit, le clic droit bref restant « reculer d'un cran ».
- **Règle.** Quand on pose ou qu'on peint, le jeu montre ce qui ne se voit pas : l'eau qui irrigue, la portée des
  lanternes, le réseau de force.
- **Règle.** Les habitants ordinaires sont les humains du pack ; les maîtres, ses animaux humanoïdes, distingués par
  leurs accessoires, leurs couleurs et leurs noms. À terme, un artiste payé fera de plus jolis personnages.
- **Règle.** La tête de chaque animal du pack est une pièce à part de son modèle : elle se pose sur le corps de
  n'importe quel autre, tous les animaux ayant le même squelette. Le corps reste d'un seul tenant, bras, jambes et
  habits ensemble. Ses oreilles et ses touffes sont des pièces à part, qu'on change (voir 6) ; son museau reste à elle.
- **Règle.** Ce qu'un animal porte dans le pack (la queue du chat et du renard, la huppe de l'oiseau) est un choix de
  l'écran comme un autre ; la petite cape de l'oiseau, du lapin, de la souris et du cochon reste avec son corps, comme
  un habit.
- **Règle.** Chaque pièce a sa place sur le personnage, la même pour tous les animaux, qui ont un seul squelette.
- **À toi.** La place de chaque pièce, mesurée au départ (la base d'un chapeau un peu sous le haut du crâne, les
  lunettes contre la face, les pièces faites pour les humains là où le pack pose les cheveux de l'oiseau), à affiner
  dans sa fiche en regardant l'écran de création ou les planches de la Chimère.
- **Défaut.** Au plus près, la caméra est à 8 m : la ville s'ouvre de là sur la Chimère qui naît, puis la caméra recule
  à sa distance de départ.
- **Règle.** Le pack n'a pas d'animations : un pack d'animations est à acheter, par le développeur. En attendant, les
  personnages glissent, bras écartés, et se tournent vers où ils vont.
- **Défaut.** Un personnage se tourne à 540° par seconde.
- **Règle.** Le jeu n'a pas de saisons.
- **À toi.** Des saisons, plus tard ? Le pack n'en a que des bouts, et aucun bâtiment ni arbre d'hiver (détail dans la
  procédure `pack-synty`). Ma reco : non ; si le rythme en demande un jour, un événement du temps plutôt que quatre
  décors, comme les sécheresses de Timberborn.
- **Règle.** L'interface parle une seule langue, la « fusion » : le bois porte (les cadres), le lin porte le texte, le
  rouge brique décore sans jamais informer, le laiton fixe (les clous du grand panneau). Tout ce qui se clique est un
  feutre pastel en relief : un reflet en haut, une lèvre en bas, une ombre dessous. Un pastel posé à plat ne se clique
  pas ; un creux de lin contient une valeur qu'on lit. Les coins sont coupés, jamais arrondis ; ni noir ni blanc pur.
- **Règle.** Chaque pastel a son rôle : menthe pour l'action principale, sable pour l'action neutre, rose pour la
  famille Logement (rose pâle pour ses cartes), beurre pour les marchandises, ciel pour la journée, lavande pour la
  Chimère. Seule exception, la jauge de la journée : menthe pour le travail, pêche pour la soirée, lavande hachurée
  pour la nuit.
- **Règle.** Deux polices : Lilita One pour les titres, Nunito pour le reste. Aucun texte sous 18 px à la taille de
  référence.
- **Règle.** La journée en haut à gauche ; le stock en fanions sous une tringle, en haut au centre ; la fiche du lieu
  touché à droite ; la barre de construction en bas au centre ; l'astuce en bas à gauche. Un bouton n'apparaît
  qu'avec ce qu'il fait : le bouton du menu arrive avec le menu (la pause, les réglages).
- **Règle.** On choisit quoi construire dans la barre du bas : les familles en onglets, chacune de son pastel avec son
  icône, et au-dessus de l'onglet ouvert, ses constructions en cartes : la vignette de la construction (une photo de
  son modèle) et son nom ; ce qu'elle coûte et ce qu'elle offre (des lits) s'y ajouteront avec eux. L'onglet ouvert
  et la carte en main restent enfoncés.
- **Règle.** La journée montre le jour, sa phase et une jauge : le travail, la soirée et la nuit hachurée, chacun à
  sa part de la journée ; ce qui est passé est plus sombre ; un repère marque l'heure.
- **Règle.** L'interface se lit à toutes les tailles d'écran. Elle grossit avec la hauteur de l'écran, à partir d'une
  référence de 900 px, et selon le réglage « Taille de l'interface » ; elle ne rétrécit jamais sous sa taille de
  référence et se garde au moins 1280 × 720 de place. Chaque bloc tient à son coin ou à son bord : sur un écran large,
  le milieu reste libre. Le plus petit écran visé est 1280 × 720 ; ce qui ne tient pas défile.
- **Défaut.** « Taille de l'interface » : 100 %, de 80 à 150 %. Les blocs à 24 px du bord de l'écran, 20 px en haut.
- **Règle.** Sans rien en main, un clic gauche touche la construction qu'on voit sous le pointeur : on vise son modèle,
  pas le sol derrière lui. Pour l'instant, toucher le logis de la Chimère la couche ou la réveille (voir 6).
- **Règle.** Tant que le modèle d'une construction n'est pas choisi, elle se voit en boîte à sa taille, sa porte
  marquée.
- **Règle.** La cabane a le modèle `Bld_Preset_Shed_02` du pack ; `House_03` est pressentie pour la maison.
- **Règle.** Une construction se pose la porte au niveau du sol : ses fondations s'enfoncent, et cachent le vide sous
  elle sur une pente. Son empreinte est la boîte de son modèle, accessoires compris : rien ne se chevauche à l'écran.
- **À toi.** Le modèle de la tente, que le pack n'a pas. En attendant, une boîte.
- **Défaut.** La tente mesure 3 × 3 m, sa porte 1,2 m de large ; sa boîte fait 2,5 m de haut.
- **À toi.** Le reste du plan de l'interface ; le son et la musique. Ma reco : chaque panneau naît avec son système
  (la liste des habitants avec eux, l'étal avec le troc), sur le côté, la ville visible derrière, comme dans
  Timberborn ; le son et la musique après le premier jouable, avec un pack acheté, comme l'art.
- **À toi.** Les questions ouvertes du HUD. Mes recos : une seule fiche ouverte à la fois, la nouvelle remplace
  l'ancienne ; Échap ferme d'abord la fiche, puis lâche l'outil en main, puis ouvre le menu ; les touches 1 à 9
  prennent les cartes de la famille ouverte ; l'interface ne prend pas le focus du clavier ; les fanions montrent les
  marchandises de l'étape en cours, et un fanion à zéro reste, en douce ; la jauge ne compte pas les heures, son
  infobulle dit ce qui suit ; chaque famille reçoit un pastel libre, dans l'ordre ; le bandeau de brique montre les
  animaux du pack. L'astuce montre le portrait de la Chimère : qui parle, le jeu ou elle, est à toi (sa voix aussi).

## 17. Le premier jouable

- **Règle.** Il contient :
  - la Chimère (création, tente, tutoriel), les premiers habitants et leurs statistiques, la liste des habitants ;
  - la carte peinte, ses arbres et rochers à retirer, ses champs sauvages et leurs graines ;
  - la Place et ses niveaux, l'étal, le troc et la réputation ;
  - la cabane, la maison, la belle maison, la nourriture ;
  - la loge des bâtisseurs et l'entrepôt ;
  - les champs, le bûcheron, la carrière, la scierie, l'atelier de tissu, l'atelier de pierre (Pavés et créations en
    pierre), la cabane du paysagiste ;
  - les chantiers et la réserve, les chemins de terre, les routes pavées, les ponts ;
  - l'eau : le canal ; une roue à eau, des arbres de transmission et la scierie hydraulique ; l'irrigation ;
  - le carnet d'idées et le premier grand projet, la maison de la Chimère ;
  - les décos : bancs, lanternes, arbres, buissons ;
  - un seul maître, pour essayer.
- **Règle.** Il ne contient pas : la run ; la carte du monde et les expéditions ; les seaux et le brassage ; les
  liaisons prioritaires.
- **Défaut.** Les recettes :
  - bûcheron → Bois ; carrière → Pierre ; champ → ce qu'on y sème, et des graines (le lin d'abord) ;
  - scierie : Bois → Planches ; atelier de tissu : Lin → Tissu ;
  - atelier de pierre : Pierre → Pavés, ou Pierre → créations en pierre.
- **Règle.** Le maître du premier jouable est un bâtisseur : les chantiers où il travaille vont deux fois plus vite.
- **À toi.** Son animal, son nom et sa personnalité.

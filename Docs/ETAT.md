# DICIELA — état actuel

**Branches** : `main` : l'étape 36. De `etape-37-travail` à `etape-48-deplacer`, chacune partie de la précédente,
pas encore vues rendues : elles se fusionnent dans l'ordre après un Play et l'album (un Play n'avance que si Unity a le
focus ; je ne le prends jamais : laisse-le au premier plan, ou dis-moi quand tu cliques dedans).

**Fait** : 23 à 36, vus en Play le 02/10 ; 37 à 39, les premiers habitants au travail (189 à 191) ; 40 à 43, la Place
(192, 193 ; album : `DICIELA/Album/Poser la Place près du campement`) ; 44, l'encombrement (194) ; 45, les animaux à
83 % (195) ; 46, la Place ronde (196) ; 47, les charrettes qui accourent (197, 198) : chaque entrepôt, le campement
compris, a sa charrette, l'une des quatre du pack au hasard ; un charretier la tire jusqu'à la Place bâtie et la gare le
long d'un cercle, où elle glisse en douceur ; 48a, déplacer (199) : l'ordre « déplacer », vérifié par les règles de pose
sans compter la construction elle-même ; un chantier saute aussitôt, qui le sert le suit (un bâtiment bâti saute encore
d'un coup : son petit chantier vient en 48b). **EditMode 233/233, PlayMode 194/194.**

**À vérifier par toi, à la souris** : la Chimère réveillée en chemin flâne devant sa porte ; un Tab bref ne choisit
rien ; un clic à côté de la roue la ferme ; la Place devant une porte, sur une pente.
**Prochaine étape** : 48b, le petit chantier d'un bâtiment déplacé (le défaire, le remonter) ; 49, l'outil Déplacer
dans la roue (M) ; puis remplir les charrettes, les niveaux de la Place (découper avant `GameStage`, `Villager`, `Villagers`,
`WorkSlots`, tous près de 200 lignes).
**À toi** : peindre la carte, ranger tes modèles (`Assets/Data/Nature`) ; retoucher la composition de la
Place (`Assets/Art/Compositions/Place.prefab`, puis « Préparer les modèles ») ; les modèles de la loge et de l'entrepôt
(boîtes) ; les icônes des Métiers et de « conduire » ; le fanion des arbres marqués ; l'icône des outils, le plateau
de la roue ; le bandeau de brique ; la voix de la Chimère ; les noms ; GDD 8.
**Dette** : `Fusion.uss` répète `:root` ; trois cartes chevaucheront l'astuce en 720 p ; des réglages de l'interface
restent dans le code ; « Monter les scènes » à repasser ; `EQUILIBRAGE.md` sans menu ; la charrette va partout au pas
de l'herbe tant qu'il n'y a pas de routes ; les places de charrette se tirent selon leur nombre du moment : les
niveaux devront les tirer pour le plus grand nombre, sinon les charrettes déjà garées bougent.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : … Continue avec tes recos.

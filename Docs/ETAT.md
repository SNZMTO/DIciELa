# DICIELA — état actuel

**Branches** : tout est fusionné ; `main` : l'étape 34. La prochaine étape s'ouvre sur sa branche, depuis `main`.

**Fait** : 23 à 25, la couche, le campement qui fait tout, les feuilles du chantier ; 26 à 33, la revue « max »
corrigée, l'efficacité, le ménage, les touches 1 à 9 des cartes (vus en Play le 02/10) ; 34, le savoir de la ville
(185) : la ville ne bâtit que ce que dit le déroulé du départ (le campement et la cabane, pour l'instant) ; le panneau
ne propose que cela, une pose inconnue est refusée. Rien ne change à l'écran ; son Play court reste à faire (Unity
n'était pas devant). **EditMode 186/186, PlayMode 185/185.**

**À vérifier par toi, à la souris** (ce qu'une capture ne montre pas) : réveillée en chemin, la Chimère flâne devant
sa porte ; un Tab bref près du bas de l'écran ne choisit rien ; un clic à côté de la roue la ferme sans rien poser ;
panneau ouvert, 1 met la cabane en main ; l'étiquette au coin des cartes te va (sinon, dis-le : elle part).

**Le chemin vers le premier jouable** (GDD 6 et 17), dans l'ordre du déroulé : 35, les lits (GDD 4 : la cabane en a
deux ; la carte du panneau dit ce qu'une construction offre) ; 36, les premiers habitants, un par statistique, chacun
avec son idée (la loge, les champs, l'entrepôt, la scierie), qu'ils bâtissent avec la Chimère ; 37, la loge et
l'entrepôt. Il me faudra tes modèles du pack pour les habitants et ces bâtiments (des boîtes en attendant).

**À toi** : peindre la carte et ranger tes modèles (`Assets/Data/Nature`) ; le fanion des arbres marqués ; l'icône des
outils et le plateau de la roue ; les pastels des familles et le bandeau de brique (GDD 16) ; la voix de la Chimère ;
les noms ; GDD 8 (les marchandises rendues, entrepôts pleins).
**Dette** : `Fusion.uss` répète `:root` ; à trois cartes, le panneau chevauchera l'astuce en 720 p ; des réglages
restent dans le code de l'interface (zone morte de la roue, écart des infobulles, portrait : reco, leur fiche) ; les
monteurs de contenu réécrits (étape 31) n'ont pas encore tourné : au prochain « Monter les scènes », `git status` ne
doit montrer aucun asset changé. Une construction que la Chimère sait en arrivant s'ajoute à `Depart.asset` avec sa
fiche (SAVOIR 3).

**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : …

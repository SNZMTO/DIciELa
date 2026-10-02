# DICIELA — état actuel

**Branches** : de `etape-23-couche` à `etape-33-touches-cartes`, l'une sur l'autre, poussées, **pas fusionnées** (pas
encore vues rendues) ; `main` : l'étape 22. Une fois vues : `git fetch . etape-33-touches-cartes:main`, puis
`git push origin main`.

**Fait** : 23 à 25, la couche, le campement qui fait tout, les feuilles du chantier ; 26 à 29, la revue « max »
corrigée (15 sur 15) ; 30, l'efficacité : îlots relus par rangées (0,5 ms au lieu de 3,9), « joignable » gardé avec la
version du maillage, chantier au SRP Batcher, fanions et fiche mis à jour sur place (181) ; 31, le reste de la revue :
ville qui ne s'ouvre pas, monteurs qui ne remettent rien, garde-fous (182) ; 32, le ménage (183) ; 33, les touches 1
à 9 du panneau Construire, écrites sur les cartes (184). **EditMode 181/181, PlayMode 185/185.**

**À faire en premier** : Unity au premier plan quelques minutes pour le Play court et les captures de 23 à 33 (une
autre application était devant à chaque fois) ; juger, l'album (21 à 33), puis fusionner.

**À vérifier par toi, en Play** : « Aller dormir » lavande ; réveillée en chemin, la Chimère flâne devant sa porte ;
elle dort sur le tapis, tête au coussin ; le tas de pierres se voit ; sur un chantier de cabane, la ligne monte comme
avant (passée par la valeur propre de chaque rendu), le lierre a ses tiges, la frange du chaume est découpée ;
l'aperçu a la forme des feuilles ; un Tab bref en bas de l'écran ne choisit rien ; un clic à côté de la roue la
ferme ; panneau ouvert, 1 prend la cabane, et chaque carte porte sa touche au coin (sa place est à juger).

**Prochaine étape, à toi** : le premier jouable (GDD 17). Ma reco : la loge des bâtisseurs et l'entrepôt d'abord
(les postes et le stock de la ville sans la Chimère), puis les premiers habitants ; il me faut leurs modèles du pack.

**À toi** : peindre la carte et ranger tes modèles (`Assets/Data/Nature`) ; le fanion des arbres marqués ; l'icône des
outils et le plateau de la roue ; les pastels des familles et le bandeau de brique (GDD 16) ; la voix de la Chimère ;
les noms ; GDD 8, où vont les marchandises rendues quand les entrepôts sont pleins ; quand la maison de la Chimère
existera, son logis devient-il la maison (le code garde aujourd'hui le premier logis bâti) ?
**Dette** : `Fusion.uss` répète `:root` et garde des classes inemployées ; à trois cartes, le panneau chevauchera
l'astuce en 720 p ; des nombres de réglage restent dans le code de l'interface (zone morte de la roue, écart des
infobulles, taille et cadrage du portrait ; ma reco : la fiche de l'interface, à la prochaine retouche de ces blocs).

**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : …

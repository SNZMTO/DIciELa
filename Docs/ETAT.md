# DICIELA — état actuel

**Branche** : `main`, à jour. **Étapes 6 à 10 finies**, l'étape 10 relue par `/code-review ultra` (deux remarques
mineures, corrigées). EditMode 106/106, PlayMode 65/65, Play court sans erreur.

**En jeu** : la Chimère naît au point d'arrivée de la carte (au centre tant que tu n'en poses pas), et la vue part sur
elle (décisions 82 à 86). Sans poste, elle flâne : un point libre à 6 m au plus, une pause de 3 à 9 s. Un fantôme qui
coupe son trajet la fait passer autour ; posé sur elle, elle en sort à pied. Le pack n'a pas d'animations : elle
glisse, bras écartés. Son personnage attend l'écran de création : le chat du pack (`SM_Chr_Cat_Female_01`).

**À vérifier par toi** (Play) : elle est sous la caméra au départ et se promène. Pose une cabane devant elle quand elle
marche : elle la contourne. Pose-en une sur elle : elle en sort tout droit, vers le côté le plus proche.
**DICIELA > Album > Se rapprocher de la Chimère** : la vue plonge vers elle. **Le point d'arrivée** : ouvre `Carte`,
*GameObject > Create Empty*, *Add Component* > **Arrival Point**, place l'objet (un cercle et un mât orange le
montrent), Ctrl+S, ferme `Carte`, **DICIELA > Jouer** : elle et la vue partent de là.

**À toi** (ma reco est appliquée) : son personnage en attendant sa création ; tu le changes dans
`Assets/Data/Habitants/Chimere.asset` (tout animal du pack convient : ils regardent tous vers +z).

**Prochaine étape : le ménage « ultracode »** (décision 77) : une revue de tout le projet, un agent par angle (bugs de
la simulation, écarts entre documents et code, règles d'architecture, tests faibles), chaque trouvaille revérifiée par
un autre, plafond de 1,5 M jetons ; les corrections codées ensuite ici, chacune avec son test. Rien ne part sans que tu
écrives « ultracode » et acceptes son plan. Trois points connus de la marche l'attendent : le petit recul après un
trajet refait ; la sortie d'un fantôme qui peut viser une poche fermée ; un second ordre « créer la Chimère » muet.
Ensuite : sa création et la tente.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. ultracode : montre-moi le plan du ménage (angles, agents, plafond), attends mon
> accord, lance-le, puis corrige ses trouvailles avec moi.

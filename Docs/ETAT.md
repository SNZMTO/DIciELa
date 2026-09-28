# DICIELA — état actuel

**Branche** : `etape-10-chimere-marche`, poussée, **pas fusionnée** : elle attend ton `/code-review ultra` (décision 77).
Étapes 6 à 10 finies. EditMode 105/105, PlayMode 65/65, Play court sans erreur. `main` est à la fin de 9b.

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

**Prochaine étape** : ta revue `/code-review ultra` sur `etape-10-chimere-marche` (facturée à part, lancée par toi).
Je trie ses trouvailles avec toi, je corrige (chaque correction avec son test), je vérifie, puis je fusionne dans
`main`. Ensuite, le ménage « ultracode » (décision 77) : un agent par angle, chaque trouvaille revérifiée, plafond de
1,5 M jetons ; rien ne part sans que tu écrives « ultracode » et acceptes son plan. Puis sa création et la tente.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Voici les trouvailles de `/code-review ultra` sur `etape-10-chimere-marche` :
> [colle-les]. Trie-les avec moi, corrige, vérifie, fusionne ; puis montre-moi le plan du ménage « ultracode ».

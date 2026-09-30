# DICIELA — état actuel

**Branche** : `etape-17b-arbres-vus`, fusionnée dans `main` et poussée ; 17c (l'outil « Retirer ») suit.

**Fait** : les étapes 15 et 16 (le tas, son poste, le HUD, le ménage et la revue ; décisions 147 à 156), puis 17a
et 17b, le défrichage (décisions 157 à 160).
- La tente plantée, le tas se pose (30 Bois, 20 Pierre ; il tient 100) ; la Chimère au poste porte, bâtit une cabane,
  flâne le soir ; endormie ou retirée, elle rend sa charge. Le HUD : fiche du tas, fanions, infobulles, astuce, Échap.
- 17a : les arbres et rochers prennent leurs mailles (« Un arbre ou un rocher est dans le chemin ») ; marqués, la
  Chimère les retire dans l'ordre des marques, en dernier au poste, et porte ce qu'ils rendent au tas (place retenue).
  Les tâches passent par des gestes (porter, bâtir, défricher) ; une forêt de 5 000 arbres reste rapide.
- 17b : la partie joue sur une copie du sol ; les arbres peints au modèle rangé sont lus ; retiré, un arbre quitte le
  Terrain ; marqué, un fanion (remplaçant) se dresse à son pied ; menu « Marquer les arbres près de la Chimère ».
**EditMode 173/173, PlayMode 130/130**, Plays sans erreur.

**À vérifier par toi** : peins quelques arbres (Terrain › Paint Trees, un modèle du pack), glisse ce modèle dans
`Assets/Data/Nature/Arbre` (liste Modèles) ; Play, la tente, la Chimère au tas, le menu : fanions, elle abat, le Bois
monte.

**À toi** : peindre la carte (vierge) ; ranger tes modèles d'arbres et de rochers dans `Assets/Data/Nature` ; l'icône
« défricher » (une hache en attendant) ; le modèle du tas ; la voix de la Chimère ; les touches 1 à 9, les pastels
des familles suivantes, le bandeau de brique (GDD 16) ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : `Fusion.uss` répète les couleurs de `:root` (variable dans une variable pas essayée).

**Prochaine étape** : 17c, l'outil « Retirer » (un clic, un rectangle) ; puis les chantiers qui se remplissent.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Continue l'étape 17 (17c), en suivant tes recos.

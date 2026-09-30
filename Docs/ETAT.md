# DICIELA — état actuel

**Branche** : `etape-17a-defricher`, fusionnée dans `main` et poussée ; 17b (la carte et la vue) suit.

**Fait** : les étapes 15 et 16 (le tas, son poste, le HUD, le ménage et la revue ; décisions 147 à 156) et 17a, la
simulation du défrichage (décisions 157 à 160), sans rien encore à l'écran.
- La tente plantée, le tas se pose (30 Bois, 20 Pierre ; il tient 100) ; la Chimère au poste porte, 5 par voyage, puis
  bâtit une cabane (1 h 40) ; le soir, elle flâne près du tas ; endormie ou retirée, elle rend sa charge au tas.
- Le HUD : la fiche du tas, les fanions, les infobulles, le coût des cartes, l'astuce ; Échap ferme la fiche d'abord.
- 17a : les arbres et rochers de la carte prennent leurs mailles (pose refusée : « Un arbre ou un rocher est dans le
  chemin ») ; marqués, la Chimère les retire dans l'ordre des marques, en dernier au poste, et porte ce qu'ils rendent
  au tas, dont la place est retenue ; une forêt de 5 000 arbres reste rapide. Les tâches passent par des gestes
  (porter, bâtir, défricher).
**EditMode 173/173, PlayMode 128/128**, Plays sans erreur.

**À vérifier par toi** (Play) : rien de neuf à l'écran avant 17b ; le reste comme avant (la tente, le tas, une
cabane, la fiche, Échap).

**À toi** : peindre la carte (vierge) ; ranger tes modèles d'arbres et de rochers dans `Assets/Data/Nature` ; l'icône
« défricher » (une hache en attendant) ; le modèle du tas ; la voix de la Chimère ; les touches 1 à 9, les pastels
des familles suivantes, le bandeau de brique (GDD 16) ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : `Fusion.uss` répète les couleurs de `:root` (variable dans une variable pas essayée).

**Prochaine étape** : 17b, lire les arbres peints sur le Terrain et les montrer (marqués, retirés) ; puis 17c, l'outil
« Retirer » à la souris (un clic, un rectangle).

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Continue l'étape 17 (17b puis 17c), en suivant tes recos.

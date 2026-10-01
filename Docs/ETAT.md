# DICIELA — état actuel

**Branche** : `etape-18b-matiere`, fusionnée dans `main` et poussée ; 18c (les détails du HUD) suit.

**Fait** : les étapes 15 à 17 (le tas, le HUD, le ménage, le défrichage ; décisions 147 à 161), puis 18a et 18b
(décisions 162 et 163) : l'écran de création retrouve son bois brun dans l'atelier ; le HUD prend de la matière.
- La tente plantée, le tas se pose (30 Bois, 20 Pierre ; il tient 100) ; la Chimère au poste porte, bâtit une cabane,
  flâne le soir ; endormie ou retirée, elle rend sa charge. Le HUD : fiche du tas, fanions, infobulles, astuce, Échap.
- 17a : les arbres et rochers prennent leurs mailles (« Un arbre ou un rocher est dans le chemin ») ; marqués, la
  Chimère les retire dans l'ordre des marques, en dernier au poste, et porte ce qu'ils rendent au tas (place retenue).
  Les tâches passent par des gestes (porter, bâtir, défricher) ; une forêt de 5 000 arbres reste rapide.
- 17b : la partie joue sur une copie du sol ; les arbres peints au modèle rangé sont lus ; retiré, un arbre quitte le
  Terrain ; marqué, un fanion (remplaçant) se dresse à son pied. 17c : l'outil « Retirer » (clic, cadre, Échap).
**EditMode 174/174, PlayMode 137/137**, Plays sans erreur.

**À vérifier par toi** : laisse 20 m nus au centre de la carte (la Chimère y naît, la tente s'y plante), peins autour
quelques arbres (Terrain › Paint Trees, un modèle du pack), glisse ce modèle dans
`Assets/Data/Nature/Arbre` (liste Modèles) ; Play, la tente, la Chimère au tas, « DICIELA/Album/Prendre l'outil
Retirer », un clic ou un cadre sur les arbres : fanions, elle abat, le Bois monte ; Échap lâche l'outil.

**À toi** : peindre la carte et ranger tes modèles dans `Assets/Data/Nature` ; le bouton « Retirer », son icône,
le fanion ; les chantiers qui se remplissent (trois questions) ; le modèle du tas ; la voix de la Chimère ; les touches
1 à 9, les pastels, le bandeau de brique ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : `Fusion.uss` répète les couleurs de `:root` (variable dans une variable pas essayée).
**Prochaine étape** : le bouton « Retirer » et les chantiers qui se remplissent, selon tes réponses ; puis GDD 17.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : … Continue avec tes recos.

# DICIELA — état actuel

**Branche** : `etape-18d-hud-d-avant`, fusionnée dans `main` et poussée.

**Fait** : les étapes 15 à 17 (le tas, le HUD, le ménage, le défrichage ; décisions 147 à 161), puis 18 : l'écran de
création retrouve son panneau de bois brun dans l'atelier (162) ; le HUD en jeu reste la fusion d'avant (165).
- La tente plantée, le tas se pose (30 Bois, 20 Pierre ; il tient 100) ; la Chimère au poste porte, bâtit une cabane,
  flâne le soir ; endormie ou retirée, elle rend sa charge. Le HUD : fiche du tas, fanions, infobulles, astuce, Échap.
- 17 : les arbres et rochers prennent leurs mailles ; marqués (outil « Retirer » : clic, cadre, Échap), la Chimère les
  retire dans l'ordre des marques et porte ce qu'ils rendent au tas ; la partie joue sur une copie du sol.
- 18 : la création, son bois brun, ses coins dorés, son ruban, une icône par ligne ; la Chimère se balance au repos.
**EditMode 174/174, PlayMode 137/137**, Plays sans erreur.

**À vérifier par toi** : l'écran de création ; le HUD en jeu, revenu comme avant. Puis laisse 20 m nus au centre de la
carte, peins autour quelques arbres (Paint Trees, un modèle du pack), glisse ce modèle dans `Assets/Data/Nature/Arbre` ;
Play, la tente, la Chimère au tas, « DICIELA/Album/Prendre l'outil Retirer », un clic ou un cadre : elle abat.

**À toi** : peindre la carte et ranger tes modèles dans `Assets/Data/Nature` ; le bouton « Retirer », son icône,
le fanion ; les chantiers qui se remplissent (trois questions) ; le modèle du tas ; la voix de la Chimère ; les touches
1 à 9, les pastels, le bandeau de brique ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : `Fusion.uss` répète les couleurs de `:root` (variable dans une variable pas essayée).

**Prochaine étape** : le bouton « Retirer » et les chantiers qui se remplissent, selon tes réponses ; puis GDD 17.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : … Continue avec tes recos.

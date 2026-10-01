# DICIELA — état actuel

**Branche** : `etape-19-chantier-qui-monte`, fusionnée dans `main` et poussée.

**Fait** : les étapes 15 à 18 (le tas, le HUD, le ménage, le défrichage, l'écran de création ; décisions 147 à 165),
puis 19 : le chantier qui monte (166).
- La tente plantée, le tas se pose (30 Bois, 20 Pierre ; il tient 100) ; la Chimère au poste porte, bâtit une cabane,
  flâne le soir ; endormie ou retirée, elle rend sa charge. Le HUD : fiche du tas, fanions, infobulles, astuce, Échap.
- 17 : les arbres et rochers marqués (outil « Retirer ») sont abattus dans l'ordre des marques.
- 19 : pendant qu'on la bâtit, une cabane montre sa vraie matière sous une ligne qui suit le travail fait, le fantôme
  au-dessus ; la ligne ondule, son bord est flou, un liseré doré et des étincelles la soulignent.
**EditMode 174/174, PlayMode 140/140**, Plays sans erreur.

**À vérifier par toi** : Play, la tente, la Chimère au tas, pose une cabane, rapproche-toi : quand elle a tout livré,
la cabane monte. Les couleurs du liseré, des étincelles et du fantôme se règlent dans `Assets/Art/Vue/ChantierFantome.mat`,
la vague et le flou dans la fiche `PlacementSettings` (Défauts de la 166).

**À toi** : peindre la carte et ranger tes modèles dans `Assets/Data/Nature` ; le bouton « Retirer », son icône,
le fanion ; le modèle du tas ; la voix de la Chimère ; les touches 1 à 9, les pastels, le bandeau de brique ; les noms
de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : `Fusion.uss` répète les couleurs de `:root` (variable dans une variable pas essayée).

**Prochaine étape** : le bouton « Retirer », selon tes réponses ; puis GDD 17.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : …

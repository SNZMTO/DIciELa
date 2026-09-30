# DICIELA — état actuel

**Branche** : `etape-15-tas`, 15a à 15c commités, pas encore fusionnés (l'astuce vient).

**Fait** : 15a à 15c, le tas de départ, son poste « à tout faire » et leurs pièces du HUD (décisions 147 à 153).
- L'économie entre dans le code : stocks, entrepôts (le tas), chantiers, tâches à prendre, bilan des marchandises.
- La tente plantée, le tas se pose à droite de sa porte vue de face (30 Bois, 20 Pierre). Une cabane posée devient un
  chantier : la Chimère au poste y porte 5 par voyage, puis la bâtit (1 h 40). Le soir, elle finit sa marche, dépose
  et flâne près du tas ; endormie ou retirée, elle rend sa charge au tas.
- La fiche du tas, en haut à droite : son contenu, son poste, « Y mettre la Chimère » / « Retirer la Chimère ». Toucher
  le sol ou la croix la ferme ; Échap la ferme avant de lâcher l'outil.
- Le stock en fanions sous une tringle, en haut au centre ; une infobulle au survol (fanions, journée) ; le coût sur
  les cartes de la barre. Menus d'album : `Toucher le tas`, `Mettre la Chimère au tas`.
**EditMode 153/153, PlayMode 126/126**, Play sans erreur (livraisons et cabane bâtie dans le journal).

**À vérifier par toi** (Play) : planter la tente (les fanions paraissent), toucher le tas, « Y mettre la Chimère »,
poser une cabane : elle porte trois fois, les fanions et la fiche baissent, puis elle bâtit ; survoler un fanion et la
journée (infobulles) ; Échap ferme la fiche, puis lâche la carte en main.

**À toi** : le modèle du tas (une boîte de 2,5 m en attendant) ; qui parle dans l'astuce ; les autres questions du HUD
(GDD 16) ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : dans `Fusion.uss`, les faces répètent les couleurs de `:root` (variable dans une variable pas essayée).
Le menu `DICIELA/Album/Poser trois cabanes` n'en pose plus qu'une : le tas prend la place des deux autres.

**Prochaine étape** : 15d, l'astuce en bas à gauche (le portrait de la Chimère, ce qu'il faut faire ensuite) ; puis
fusion de l'étape 15 dans `main`, et l'étape 16, le ménage.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Finis l'étape 15 : 15d, l'astuce, puis la fusion dans main.

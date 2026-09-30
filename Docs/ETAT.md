# DICIELA — état actuel

**Branche** : `etape-15-tas`, 15a commité, pas encore fusionné (15b et 15c viennent).

**Fait** : l'étape 15a, la simulation du tas de départ et de son poste « à tout faire » (décisions 147 à 151).
- L'économie entre dans le code (`Diciela.Simulation.Economy`) : stocks, entrepôts (le tas), chantiers et leur file,
  tâches à prendre, bilan des marchandises. Le monde reçoit le catalogue.
- La tente plantée, le tas se pose à droite de sa porte vue de face (30 Bois, 20 Pierre) et ouvre son poste. Ordres
  « affecter » et « retirer ». Une cabane posée devient un chantier : la Chimère y porte 5 par voyage, puis la bâtit
  (1 h 40 à 10 partout). Le soir, elle finit sa marche, dépose, flâne près du tas. Endormie ou retirée, elle rend sa
  charge au tas. Menu `DICIELA/Données/Créer les fiches manquantes` pour les fiches.
**EditMode 153/153, PlayMode 119/119.**

**À vérifier par toi** (Play) : la tente plantée, le tas apparaît à droite de sa porte, vu de face. On ne peut pas
encore y mettre la Chimère au clic : la fiche du tas vient avec 15b.

**À toi** : le modèle du tas (le pack a des tas de bûches et de pierres ; une boîte de 2,5 m en attendant) ; qui parle
dans l'astuce ; les questions ouvertes du HUD (GDD 16) ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : dans `Fusion.uss`, les faces répètent les couleurs de `:root` (variable dans une variable pas essayée).

**Prochaine étape** : 15b, la fiche du tas (toucher le tas, « Y mettre la Chimère » / « Retirer la Chimère », une seule
fiche à la fois, Échap la ferme) ; puis 15c, les fanions du stock, les infobulles, l'astuce, le coût sur les cartes.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Continue l'étape 15 : 15b, la fiche du tas, puis 15c (fanions, infobulles, astuce).

# DICIELA — état actuel

**Branche** : `etape-15-tas`, étape 15 finie (15a à 15d), pas encore fusionnée : une campagne PlayMode à repasser.

**Fait** : l'étape 15, le tas de départ, son poste « à tout faire » et leurs pièces du HUD (décisions 147 à 154).
- L'économie entre dans le code : stocks, entrepôts (le tas), chantiers, tâches à prendre, bilan des marchandises.
- La tente plantée, le tas se pose à droite de sa porte vue de face (30 Bois, 20 Pierre). Une cabane posée devient un
  chantier : la Chimère au poste y porte 5 par voyage, puis la bâtit (1 h 40). Le soir, elle finit sa marche, dépose
  et flâne près du tas ; endormie ou retirée, elle rend sa charge au tas.
- Le HUD : la fiche du tas (en haut à droite), les fanions du stock (en haut au centre), les infobulles, le coût des
  cartes, l'astuce (en bas à gauche, le portrait de la Chimère et la phrase du jeu). Échap ferme la fiche d'abord.
**EditMode 153/153, PlayMode 128/128** avant la largeur fixe de l'astuce ; depuis, le lanceur PlayMode rend 0 test
(132 vus) : **redémarrer Unity**, puis relancer la campagne avant de fusionner.

**À vérifier par toi** (Play) : l'astuce après « C'est parti » ; planter la tente (le tas, les fanions) ; toucher le
tas, « Y mettre la Chimère » ; poser une cabane : trois voyages, les fanions baissent, elle bâtit ; survoler un fanion
et la journée ; Échap ferme la fiche, puis lâche la carte.

**À toi** : le modèle du tas (une boîte de 2,5 m en attendant) ; la voix de la Chimère ; les touches 1 à 9, les pastels
des familles suivantes, le bandeau de brique (GDD 16) ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : `Fusion.uss` répète les couleurs de `:root` ; `Album/Poser trois cabanes` n'en pose plus qu'une (le tas).

**Prochaine étape** : l'étape 16, le ménage du travail (économie, tâches, HUD du tas) : relire, corriger ce qui a dérivé.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Redémarre la campagne PlayMode, fusionne l'étape 15, puis fais le ménage (étape 16).

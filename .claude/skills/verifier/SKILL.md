---
name: verifier
description: La passe de vérification d'une étape (recompiler, tests EditMode puis PlayMode, Play court, console relue) et la façon d'en rendre compte. À lancer avant tout commit de code.
---

# Vérifier une étape

Une seule passe par étape, dans cet ordre, en s'arrêtant à la première panne. La procédure `unity-cli` donne le détail
de chaque commande. Les sorties vont dans le dossier scratchpad de la session, jamais dans le projet.

1. **État** : `editor_status` et `list_open_scenes`. En Play → attendre, ne rien lancer. Scène sale → demander.
2. **Recompiler** : `bash Tools/compile.sh` (`recompile`, `recompile_status` jusqu'à la fin, `editor_status` jusqu'à prêt,
   puis les `error CS` du journal). Une erreur de
   compilation → corriger d'abord ; **aucune campagne PlayMode tant qu'elle existe**.
3. **EditMode** : `run_tests --timeout 300 --mode EditMode` vers un fichier, `node Tools/testreport.js <fichier>`.
4. **PlayMode** : `bash Tools/playtests.sh <dossier déjà créé>`. Zéro test lancé alors que `list_tests` en voit →
   demander au développeur de redémarrer Unity.
5. **Play court** (si l'étape touche le jeu) : la console avant, `editor_play`, les menus `DICIELA/…` de l'étape,
   20 à 40 s, les captures de l'album si l'étape change ce qu'on voit (`Docs/ALBUM.md`), la console après, `editor_stop`, puis `node Tools/console_after.js apres.txt avant.txt` : zéro erreur neuve,
   et la preuve que la simulation a travaillé.
6. **`git status`** : ne committer que ce qui est à l'étape ; dire ce qui est au développeur (il travaille dans
   l'éditeur en même temps).

**Compte rendu** : les chiffres (« EditMode 42/42, PlayMode 6/6, Play de 30 s sans erreur ») et ce que seul le
développeur peut vérifier à la souris, geste par geste, chacun avec ce qu'il vérifie en termes de jeu.

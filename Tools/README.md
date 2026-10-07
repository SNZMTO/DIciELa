# Tools/ — les scripts de vérification

Ils se lancent depuis la racine du projet, l'éditeur ouvert sur une scène propre. La procédure `verifier`
(`.claude/skills/verifier/`) dit quand et dans quel ordre.

| Script | Ce qu'il fait |
|---|---|
| `compile.sh` | `bash Tools/compile.sh` : recompile dans l'éditeur ouvert, attend la fin et le rechargement, cherche les `error CS` écrites au journal depuis le départ. Sortie 0 si la compilation a réussi, 1 sur une erreur, 2 si l'éditeur est en Play. |
| `testreport.js` | `node Tools/testreport.js <fichier>` : lit la sortie d'un `run_tests` redirigé vers un fichier, affiche les tests rouges et le compte. Sortie 0 si tout est vert. |
| `playtests.sh` | `bash Tools/playtests.sh <dossier déjà créé>` : lance la campagne PlayMode en asynchrone, attend la fin (cinq minutes au plus), affiche le compte. Refuse de partir si l'éditeur est en Play. |
| `console_after.js` | `node Tools/console_after.js apres.txt avant.txt [regex]` : ne garde que les entrées de console postérieures au début du Play. Sortie 1 si une erreur ou une exception est neuve. |

Les sorties vont dans le dossier scratchpad de la session, jamais dans le projet.

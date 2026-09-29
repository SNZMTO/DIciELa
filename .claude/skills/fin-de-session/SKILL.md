---
name: fin-de-session
description: Clore une session de travail sur DICIELA - état à jour, décisions numérotées, documents réécrits, commit, push, prompt de la session suivante.
disable-model-invocation: true
---

# Clore une session

0. **Les quatre questions anti-spaghetti**, à répondre honnêtement dans le compte rendu :
   - Qu'est-ce que j'ai **supprimé** (ce que l'étape remplace) ?
   - Y a-t-il maintenant **deux façons de faire la même chose**, ou deux copies d'une même donnée ?
   - Pour chaque donnée neuve : **qui en est le propriétaire** (la seule vérité) ?
   - Quelle **flèche de dépendance** entre assemblages l'étape a-t-elle ajoutée (aucune, idéalement) ?
   Et : un fichier a-t-il dépassé 200 lignes ? Si oui, il est découpé avant le commit.
   **Toutes les cinq étapes environ**, la prochaine étape est une **session de ménage** sur un seul système : le relire
   en entier, corriger ce qui a dérivé, sans agent. `ETAT.md` le rappelle. Exception (décisions 77 et 90) : avant le
   premier jouable, une revue « ultracode » des seuls bugs de la simulation (deux chercheurs Opus, chaque trouvaille
   revérifiée par un vérificateur Sonnet, plafond 1 M jetons), lancée seulement si le développeur écrit « ultracode » et
   accepte le plan. Ses chercheurs rangent parfois une vraie trouvaille parmi leurs « écartées » : les relire toutes.
1. **Les décisions** prises pendant la session ont leur numéro dans `Docs/DECISIONS.md` (datées, avec leur raison), et
   le GDD ou l'architecture sont **réécrits** là où elles changent la vérité (pas de rature, pas de note datée).
2. **Ce qui a été remplacé est supprimé** : code, données, tests, lignes de documents.
3. **Une leçon apprise** (un piège, une solution) va dans `Docs/SAVOIR.md` ou dans la procédure `unity-cli`.
4. **`Docs/ETAT.md` est réécrit en entier**, 30 lignes au plus : branche et étape ; ce qui marche (avec les chiffres des
   tests) ; ce que le développeur doit vérifier à la souris ; les questions ouvertes ; la prochaine étape ; le prompt de
   la session suivante, en un bloc prêt à coller.
5. **Commit** (message court en français, sans fichier du développeur), puis `git push`. Si l'étape est finie et
   vérifiée : fusion en avance rapide `git fetch . <branche>:main`, `git push origin main`. Jamais de rappel ni d'arrêt
   pour `/code-review ultra` (décision 136) : toute étape finie et testée se fusionne.
6. **Dire au développeur** en quelques lignes : ce qui est fait, ce qui reste à vérifier par lui, et le prompt de la
   session suivante.

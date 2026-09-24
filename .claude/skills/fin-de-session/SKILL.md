---
name: fin-de-session
description: Clore une session de travail sur DICIELA - état à jour, décisions numérotées, documents réécrits, commit, push, prompt de la session suivante.
disable-model-invocation: true
---

# Clore une session

1. **Les décisions** prises pendant la session ont leur numéro dans `Docs/DECISIONS.md` (datées, avec leur raison), et
   le GDD ou l'architecture sont **réécrits** là où elles changent la vérité (pas de rature, pas de note datée).
2. **Ce qui a été remplacé est supprimé** : code, données, tests, lignes de documents.
3. **Une leçon apprise** (un piège, une solution) va dans `Docs/SAVOIR.md` ou dans la procédure `unity-cli`.
4. **`Docs/ETAT.md` est réécrit en entier**, 30 lignes au plus : branche et étape ; ce qui marche (avec les chiffres des
   tests) ; ce que le développeur doit vérifier à la souris ; les questions ouvertes ; la prochaine étape ; le prompt de
   la session suivante, en un bloc prêt à coller.
5. **Commit** (message court en français, sans fichier du développeur), puis `git push`. Si l'étape est finie et
   vérifiée : fusion en avance rapide `git fetch . <branche>:main`, `git push origin main`.
6. **Dire au développeur** en quelques lignes : ce qui est fait, ce qui reste à vérifier par lui, et le prompt de la
   session suivante.

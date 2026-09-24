# DICIELA — état actuel

**Branche** : `main`. **Étape** : les fondations écrites, aucun code de jeu.

**Ce qui existe** : un projet Unity 6.6 neuf (modèle Universal 3D, contenu d'exemple du modèle encore là) ;
`CLAUDE.md` ; les règles par dossier (`.claude/rules/`) ; les procédures `unity-cli`, `verifier`, `fin-de-session`,
`pack-synty` ; le garde-fou `.claude/hooks/garde-unity.js` (essayé sur neuf commandes) ; les scripts `Tools/` ;
`Docs/ENTRETIEN.md` (le design confirmé), `Docs/SAVOIR.md` (leçons et solutions), `Docs/DECISIONS.md` (1 à 3).

**Pas encore** : `Docs/GDD.md`, `Docs/ARCHITECTURE.md`, le pack Synty dans ce projet, un pack d'animations, le moindre
test.

**À savoir** : le nom du produit dans Unity est encore `V2-DIciELa` et la société `DefaultCompany` : ils décident du
dossier des sauvegardes (`LocalLow/<société>/<produit>`). Les changer avant la première sauvegarde (proposition :
société à son nom, produit `DICIELA`), avec son accord (ce sont des réglages du projet).

**Prochaines étapes, dans l'ordre**
1. Écrire `Docs/GDD.md` depuis `ENTRETIEN.md`, en un passage, puis le faire valider section par section ; numéroter les
   choix dans `DECISIONS.md` ; supprimer `ENTRETIEN.md`.
2. Écrire `Docs/ARCHITECTURE.md` (systèmes, frontières, assemblages et namespaces, simulation à pas fixe, événements,
   modificateurs, postes et tâches, données, sauvegarde, tests), validé par lui.
3. Nettoyer le modèle Unity, régler nom et société, décider du démarrage rapide du Play ; il importe le pack ; puis la
   première étape de code.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 1 : écris Docs/GDD.md à partir de Docs/ENTRETIEN.md, en un seul passage, clair,
> au présent, avec pour chaque règle Règle / Défaut / À toi. Montre-moi d'abord le plan des sections et les points où
> l'entretien est muet ou flou, attends mon accord, puis écris. Ensuite on le relit section par section.

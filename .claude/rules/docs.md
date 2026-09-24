---
paths:
  - "Docs/**"
  - "CLAUDE.md"
  - ".claude/**"
---

# Écrire un document de DICIELA

- **Un document dit le présent.** Il se réécrit quand la vérité change : jamais de rature, jamais de « réécrit en
  partie », jamais de date d'incident dans le corps. L'histoire vit dans `Docs/DECISIONS.md` et dans Git.
- **`GDD.md`** : le jeu tel qu'il doit être, court. Chaque règle dit si c'est une **Règle** (décidée), un **Défaut**
  (un chiffre de départ qu'on ajustera) ou un **À toi** (à trancher par le développeur ; ne jamais le trancher seul).
- **`DECISIONS.md`** : une décision par numéro, datée, avec sa raison en une ou deux phrases. Une décision qui en
  remplace une autre le dit (« remplace la 12 ») et l'ancienne est marquée « remplacée par la N ».
- **`ETAT.md`** : 30 lignes au plus : branche, étape, ce qui marche, ce qui est à vérifier par le développeur, la
  prochaine étape, le prompt de la session suivante.
- **`CLAUDE.md`** : moins de 200 lignes, seulement des règles qui s'appliquent à chaque session. Une procédure va dans
  une procédure (`.claude/skills/`), une règle qui ne concerne qu'un dossier va dans `.claude/rules/`.
- Une nouvelle leçon (un piège, une solution) va dans `SAVOIR.md`, à sa section ; un piège du CLI va dans la procédure
  `unity-cli`.
- Écrire clair : des phrases courtes, les mots du jeu plutôt que ceux du code, en français.

---
paths:
  - "Assets/Data/**"
  - "Assets/Scripts/Data/**"
  - "Assets/Localization/**"
---

# Les données de DICIELA

- **Tout contenu est une fiche** (ScriptableObject) : bâtiments, recettes, marchandises, idées, décos, habitants,
  maîtres, visiteurs… Ajouter du contenu, c'est ajouter une fiche, pas du code.
- Chaque classe de fiche porte `[CreateAssetMenu]` ; les fiches vivent dans `Assets/Data/`, rangées par sorte.
- **Tous les nombres de réglage** sont dans une fiche (la fiche qu'ils décrivent, ou une fiche de réglages généraux),
  jamais dans le code.
- **Toute chaîne visible** est une clé de `Assets/Localization/strings.csv` (français et anglais).
- Les fiches sont créées ou retouchées par un script d'éditeur réexécutable (`DICIELA/…`) plutôt qu'à la main ; après
  un script, `File/Save Project` pour écrire sur le disque.
- **Un monteur ne réécrase jamais un réglage fait à la main** par le développeur : il crée ce qui manque, il ne remet
  pas à zéro ce qui existe.
- Ne jamais toucher un `.meta` à la main ; renommer un asset avec son `.meta`.
- Une énumération sérialisée ne perd jamais une valeur du milieu (les fiches garderaient un nombre faux).

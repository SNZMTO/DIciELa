---
paths:
  - "Assets/Tests/**"
  - "Assets/Scripts/**/Tests/**"
---

# Écrire un test dans DICIELA

- **Un test passe par le chemin du joueur** : le même appel que le geste en jeu. Si le jeu n'a pas cet appel, le test ne
  l'invente pas : c'est le code qui manque d'une bonne entrée.
- **EditMode** pour le C# pur (rapide, sans scène) ; **PlayMode** seulement pour ce qui a besoin d'une scène.
- **Un test n'écrit jamais dans `Assets/`** et ne lance jamais un monteur d'assets : il crée ses données en mémoire.
- **Un test ne touche jamais la vraie partie** : le dossier de sauvegarde des tests est fixé avant de charger la scène.
- **Le résultat attendu vient d'ailleurs que du code testé** : une valeur écrite à la main dans le test, un oracle lent
  et sûr (Dijkstra, parcours en largeur), ou un fichier d'or. Un test qui recalcule l'attendu avec la même formule ne
  teste rien.
- **Pas de nombre recopié des vraies données** : un test qui dépend du contenu du jeu lit la donnée, ou crée la sienne.
- **Toute boucle d'un test a une borne** ; `LogAssert.ignoreFailingMessages` se remet dans un `finally`.
- **Aucun `[Ignore]` qui dure** : un test ignoré plus d'une étape est réparé ou supprimé.
- Les campagnes se lancent par la procédure `verifier` (une seule à la fois).

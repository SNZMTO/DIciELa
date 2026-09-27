# DICIELA — état actuel

**Branche** : `etape-2-architecture`, fusionnée dans `main`. **Étape 2 finie** : `Docs/ARCHITECTURE.md` est écrit.

**Ce qui existe** : un projet Unity 6.6 neuf (modèle Universal 3D, contenu d'exemple encore là), sans code ni test ;
`Docs/GDD.md`, `Docs/ARCHITECTURE.md` (couches, qui possède quoi, règles de passage), `Docs/DECISIONS.md` (1 à 46),
`Docs/SAVOIR.md`, `Docs/ALBUM.md` (vide), `.claude/`. La tente n'est que le logis de la Chimère (décisions 45 et 46).

**À vérifier par lui** : rien à la souris. 31 « À toi » dans le GDD, chacun avec ma reco ; le plus pressé reste la
taille de la carte (et la résolution du Terrain), à fixer avant de la peindre.

**À savoir**
- Le `.gitattributes` abîmerait un TerrainData (règle `*.asset`) : ne rien committer du Terrain avant la règle de son
  dossier (décision 41, SAVOIR 2.5).
- LFS n'a pas agi au premier commit (`git lfs ls-files` vide, un PNG du modèle stocké en clair).
- Le GDD dépasse 490 lignes : à resserrer à la session de ménage.

**Prochaines étapes, dans l'ordre**
1. **Ménage du projet, avec lui** : retirer le contenu d'exemple et l'asset d'actions du modèle ; paquets (déclarer
   Newtonsoft, retirer AI Navigation et Collab après avoir vérifié la démo du pack) ; essai de LFS et règle du Terrain ;
   nom du produit et société (ils décident du dossier des sauvegardes). Puis il importe le pack.
2. La première étape de code : les assemblages de `ARCHITECTURE.md` et leurs garde-fous (test des références, test du
   code de la simulation), le temps et le pas fixe, la fiche de réglages.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 3 : le ménage du projet (liste d'ETAT). Montre-moi d'abord la liste de ce que tu retires, déclares ou règles, attends mon accord, puis fais-le.

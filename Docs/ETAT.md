# DICIELA — état actuel

**Branche** : `etape-1-gdd`, fusionnée dans `main`. **Étape 1 finie** : le GDD est écrit et validé.

**Ce qui existe** : un projet Unity 6.6 neuf (modèle Universal 3D, contenu d'exemple encore là), sans code ni test ;
`Docs/GDD.md` (17 sections) ; `Docs/DECISIONS.md` (1 à 29) ; `Docs/ALBUM.md` (vide) ; `Docs/SAVOIR.md` ; `.claude/`.

**À vérifier par lui** : rien à la souris. Onze « À toi » restent ouverts dans le GDD ; le plus pressé est la taille de
la carte, à fixer avant de la peindre.

**À savoir**
- Nom du produit `V2-DIciELa` et société `DefaultCompany` : ils décident du dossier des sauvegardes ; les changer avant
  la première sauvegarde, avec son accord.
- `SAVOIR.md` §2.5 : la carte peinte à la main demande de choisir comment ranger le Terrain (étape 2).
- Le GDD dépasse 450 lignes, à resserrer lors d'une session de ménage (une toutes les cinq étapes environ).

**Prochaines étapes, dans l'ordre**
1. `Docs/ARCHITECTURE.md`, validé par lui : systèmes et frontières ; assemblages (`.asmdef`) à dépendances à sens
   unique ; « qui possède quoi » ; simulation à pas fixe et événements ; effets ; postes et tâches ; champs peints ;
   données et fiches ; sauvegarde et rangement de la carte ; tests (dont une ville qui vit seule pour vérifier le
   rythme) ; rapport d'équilibrage ; vue témoin de l'album. Références : les livres gratuits de Unity « Create modular
   game architecture with ScriptableObjects » (Unity 6) et « Level up your code with design patterns and SOLID ».
2. Nettoyer le modèle Unity, régler nom et société ; il importe le pack ; puis la première étape de code.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Étape 2 : écris Docs/ARCHITECTURE.md à partir du GDD (lu par sections) et des
> références d'ETAT. Montre-moi d'abord le plan (systèmes, assemblages, qui possède quoi) et tes questions, attends mon
> accord, puis écris.

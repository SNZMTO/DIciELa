# DICIELA — état actuel

**Branche** : `etape-14-hud`, fusionnée dans `main` et poussée.

**Fait** : l'étape 14, le HUD « fusion » dans Unity (décisions 137 à 146).
- Une seule feuille de style (`Assets/UI/Fusion.uss`) ; polices Lilita One et Nunito ; faces vectorielles (feutre,
  applique, creux, bulle) ; cadre de bois tiré de l'atlas du pack, son ombre générée ; icônes vectorielles. Menus
  `DICIELA/Interface/Générer les images de l'interface` et `Photographier les constructions`.
- L'interface grossit avec la hauteur de l'écran (référence 900 px, jamais sous 1, au moins 1280 × 720 de place) ;
  réglage « Taille de l'interface » dans la fiche des réglages de l'interface (80 à 150 %). Nette jusqu'à la 4K.
- La journée en haut à gauche, la barre de construction en bas (onglets, cartes avec vignette, raison en bulle),
  l'écran de création dans la même langue. L'ancien panneau doré et son générateur sont supprimés.
**EditMode 139/139, PlayMode 119/119** (dont le HUD rendu à huit tailles d'écran).

**À vérifier par toi** (Play) : survol et appui des boutons de feutre (ils montent, s'enfoncent) ; l'onglet ouvert et
la carte en main restent enfoncés ; le repère de la journée avance, la phase change le soir ; les flèches de l'écran
de création ; la vue Game à plusieurs tailles (1280 × 720, 1920 × 1080, 4K) : le HUD garde ses coins.

**À toi** : les questions ouvertes du HUD (GDD 16 : une fiche à la fois, Échap, touches 1 à 9, fanions, couleur des
familles suivantes, qui parle dans l'astuce) ; les noms de `strings.csv` ; les accessoires de cou et de dos.

**Dette** : dans `Fusion.uss`, les faces répètent les couleurs de `:root` (variable dans une variable pas essayée).

**Prochaine étape** : l'étape 15, le tas de départ et son poste « à tout faire » (GDD 6 et 7, décisions 137 à 141),
avec ses pièces du HUD : les fanions du stock, la fiche du tas à droite, les infobulles, l'astuce. Ménage à la 16.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Montre-moi le plan court de l'étape 15 : le tas de départ et son poste « à tout
> faire », avec ses pièces du HUD (fanions, fiche du tas, infobulles, astuce).

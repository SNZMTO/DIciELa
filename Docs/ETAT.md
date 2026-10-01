# DICIELA — état actuel

**Branche** : `main`, à jour et poussée (étapes 21 et 21b fusionnées). La suite attend ton accord sur le plan.

**Fait** : les étapes 15 à 20b (le tas, le HUD, le ménage, le défrichage, l'écran de création, le chantier qui monte,
la roue des outils ; décisions 147 à 169) ; 21, le campement d'objets du pack et, à côté, le tas de bûches et de
pierres, sans flamme (ses particules faisaient planter Unity ; décision 170) ; 21b, la caméra plus proche et plus basse
(171). **EditMode 174/174, PlayMode 156/156.**

**Tes réponses du 01/10** (décision 172 et GDD à écrire) : plus de tas, le campement fait tout : logis, stock de départ
(100 places, 30 Bois, 20 Pierre), poste « à tout faire ». Un clic ouvre sa fiche (stock, poste, « Y mettre la Chimère »,
« Aller dormir » / « Réveiller ») ; endormie, elle est couchée sur sa couche ; bûches et pierres entrent dans le
campement ; la loge, un entrepôt et la cabane du paysagiste bâtis et le stock vide, stock et poste ferment.

**Revue du code (20 à 21b)** : pas de spaghetti grave ; à corriger : les deux outils de la roue écrits à la main à huit
endroits ; « un seul outil en main » tenu par GameStage (190 lignes) ; la roue et le panneau branchés de deux façons ;
des recopies (la hache des icônes, « bloc-cache » ×8) ; une allocation par image ; deux fichiers de plus de 200 lignes.

**Plan proposé** : 22, le ménage de cette revue (rien ne change à l'écran) ; 23, le campement fait tout (la tente
renommée campement, le tas supprimé, la fiche avec le sommeil, l'astuce, la composition refaite, l'album de 21 à 23) ;
23b, la Chimère couchée sur sa couche (un repère « Couche » dans la composition, lu comme la porte).

**À toi** : peindre la carte et ranger tes modèles (`Assets/Data/Nature`) ; le fanion des arbres marqués ; l'icône des
outils et le plateau de la roue ; les pastels et le bandeau de brique (GDD 16) ; la voix de la Chimère ; les noms.

**Dette** : `Fusion.uss` répète les couleurs de `:root` ; à trois cartes, le panneau chevauchera l'astuce en 720 p.

**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Mes réponses : …

# DICIELA — état actuel

**Branches** : 60 à 74 sont validées (14/10) ; `readme-vitrine` les porte, avec la vitrine publique (README, outils).
`main` est encore à l'étape 59 : `git fetch . readme-vitrine:main` fusionne le tout.
**Fait** : 23 à 59 (jusqu'à la 215) ; 60 à 64 (216 à 220), les outils de sol revus ; 65 (222), la terre à l'échelle de
l'herbe ; 66 (221, 223), la roue à étages ; 67 (224), le curseur ; 68 (225), les mouvements ; 69 (226), Construire dans
la roue ; 70 (227), les sols arrondis et cernés de leur liant ; 71 (228 à 230), la prévision : V ou le bouton lavande
montrent la ville une fois ses chantiers finis, sous un voile de rêverie ; on y construit ; Échap l'éteint ; 72 (231,
232), le voile allégé, et les sols qui se fondent comme dans la démo : la terre sur 1 m dans l'herbe, les pavés dans
leur terre, elle-même fondue dans l'herbe, chaque largeur sur la fiche du sol ; 73 (233), les sols peints deux fois
plus fin (25 cm le texel), leurs bords plus ronds ; 74, du ménage : les couleurs de l'interface nommées une fois, les
actions des chiffres renommées « Slot1 » à « Slot9 ». Les pavés attendent leur idée.
**EditMode 288/288, PlayMode 252/252.** L'album des étapes 70 à 74 est pris en jeu (09/10/2026), par deux menus
neufs : `DICIELA/Album/Tracer un chemin de terre devant le campement`, puis `… Regarder le chemin de terre de près`.
**Prochaine étape : 75**, selon tes retours.
**À toi** : les remplaçants, tous dans `Docs/REMPLACANTS.md` ; le voile et les fondus des sols, à l'œil (la fiche d'un
sol : Fondu, Fondu du liant, Effilochage) ; la roue, la carte, le curseur et les mouvements, à l'usage ; Maj qui
inverse le geste ; mes choix des 216 à 233 ; le prix au m² des sols qui se paient (ma reco : 1 Pavé pour 2 m²) ; mes
choix des 206, 209 à 214 ; le découpage retourné et la charrette qu'on vient chercher (208) ; combien de zones à quel
niveau ; l'astuce, qui ne parle pas de tracer la Place ; « Run In Background » ; peindre la carte ; le bandeau.
**À vérifier par toi, à la souris** (sur la vue Game ; l'album montre le reste) : V encore, tout revient ; le bouton
lavande fait comme V ; Échap éteint la prévision, après l'outil en main ; un arbre marqué s'y efface (carte peinte).
**Dette** : des réglages d'interface dans le code (l'écart de l'infobulle, le défilement des nombres, la zone morte de
la roue) ; `EQUILIBRAGE.md` sans menu (il attend la ville témoin) ; la mise en scène de la Place ne prend plus son
outil (à reprendre comme celle des chemins, sur `AlbumMenu.Around`) ; « Poser trois cabanes » n'en pose que deux ;
trois scènes `Assets/InitTestScene…` laissées par le lanceur de tests (hors Git), à supprimer.
**Prompt de la session suivante**
> Lis CLAUDE.md et Docs/ETAT.md. J'ai regardé les sols et la prévision : voici mes retours.

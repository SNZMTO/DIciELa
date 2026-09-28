# DICIELA — état actuel

**Branche** : `menage-5-ultracode`, poussée, **pas encore fusionnée** : elle touche la marche, elle attend ton
`/code-review ultra` (décision 77). `main` est à l'étape 10.

**Fait** : le ménage « ultracode » (8 agents, 1,10 M jetons sur 1,5 M). 8 trouvailles retenues, toutes confirmées par
leur vérificateur, plus 3 vraies parmi les 40 écartées. Chaque correction a son test, vu rouge avant elle :
- la Chimère ne recule plus quand un fantôme coupe son trajet : elle rejoint sa nouvelle ligne sans quitter sa maille ;
- posée sous un fantôme, elle sort par une maille libre qui le borde, du côté le plus vaste : jamais dans une poche
  qu'il ferme, jamais à travers une autre construction ; un second fantôme sur sa sortie lui en fait choisir une autre ;
- si l'endroit où elle flânait est couvert, ou coupé d'elle, elle flâne autour d'où elle est ;
- une construction prend toute maille qu'elle touche : rien ne se chevauche, plus de fente de 13 cm à traverser ;
- un second ordre « créer la Chimère » est refusé et le dit ; les événements partent à la fin de leur couche ;
- le recul minimal de la flânerie (2 m) est dans la fiche de sa sorte ; la garde d'architecture refuse `AnimationCurve`,
  `Gradient` et `.name` dans la simulation ; les tests PlayMode attendent une condition, jamais des secondes.
**EditMode 117/117, PlayMode 65/65**, Play court sans erreur (296 pas).

**À vérifier par toi** (Play) : pose une cabane en travers de sa marche : elle la contourne, sans hoquet en arrière.
Pose-en une sur elle, collée à une autre : elle sort par un bord libre, jamais à travers l'autre. Approche une cabane
d'une autre : à moins de 25 cm, l'aperçu peut rougir (« recouvre ») selon où tombent leurs bords.

**À toi** (mes recos appliquées, l'accord du développeur) : décisions 87 (deux constructions se collent à 25 cm près), 88 (la
sortie d'un fantôme), 89 (flâner ailleurs quand son coin est perdu), 90 (la prochaine revue ultracode : avant le premier
jouable, les seuls bugs, plafond 1 M). Son personnage reste le chat du pack, dans `Assets/Data/Habitants/Chimere.asset`.

**Prochaine étape** : ton `/code-review ultra`, ses remarques, la fusion ; puis la création de la Chimère et la tente.
**Prompt de la session suivante**

> Lis CLAUDE.md et Docs/ETAT.md. Corrige avec moi les remarques du `/code-review ultra` de `menage-5-ultracode`,
> fusionne, puis montre-moi le plan court de l'étape suivante : la création de la Chimère et la tente.

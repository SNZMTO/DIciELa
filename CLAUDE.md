# DICIELA — instructions pour Claude Code

## Le projet

- **DICIELA** : un Satisfactory fantasy, cozy. On bâtit des chaînes de production tenues par de petits habitants qui ont
  un métier et progressent en travaillant ; on troque à l'étal ; on décore en pose libre ; la Chimère touche à tout. Une
  run roguelike viendra après le prototype de la simulation.
- **Unity 6.6 (6000.6.0f1), C#, URP** ; tout l'art vient du pack POLYGON Fantasy Village (Synty), dans `Assets/Synty/`,
  hors Git, importé par le développeur.
- **Développeur solo, débutant sur Unity et C#** (il vient de GameMaker). Explique chaque concept Unity ou C# la
  première fois qu'il apparaît, en comparant avec GameMaker quand ça aide. **Réponds en français.**

## Où lire quoi

- **Au début de chaque session : `Docs/ETAT.md`** (30 lignes : où on en est, la prochaine étape). Rien d'autre d'office.
- `Docs/GDD.md` : le jeu, au présent. Lire la section du système concerné avant de le coder.
- `Docs/ARCHITECTURE.md` : les systèmes, leurs frontières, leurs règles. Le lire avant de créer ou déplacer du code.
- `Docs/DECISIONS.md` : les décisions numérotées, avec leur date et leur raison.
- `Docs/SAVOIR.md` : les solutions éprouvées et les pièges connus, à lire quand on conçoit le système concerné.
- `Docs/ENTRETIEN.md` : les réponses de l'entretien fondateur ; il disparaît une fois le GDD écrit.
- Les procédures (`.claude/skills/`) : `unity-cli`, `verifier`, `fin-de-session`, `pack-synty`. Les règles par dossier
  (`.claude/rules/`) se chargent seules quand on touche ces fichiers.

## Règles de travail

- **Une chose à la fois.** Avant un changement structurant : un plan court, et j'attends son accord.
- **Reformuler avant d'écrire.** Sur une idée du développeur : ce que j'ai compris et les questions, en quelques lignes,
  dans la minute ; rien n'est noté avant sa validation.
- **La meilleure option, pas la moins chère.** Recommander une option, dire son coût en une ligne, pas d'échelle de prix.
  « Je te laisse faire » = appliquer la recommandation annoncée.
- **Un avis sincère, toujours.** Donner mon avis réel, même s'il contredit le développeur, avec sa raison et ma
  confiance. Je ne change d'avis que devant un argument nouveau, et je dis lequel ; s'il insiste sans argument neuf, je
  maintiens mon avis et je lui laisse la décision (« c'est ton choix ; mon avis reste X, parce que Y »). Jamais un « tu
  as raison » par réflexe. Quand je suis d'accord, je dis pourquoi, et ce qui pourrait me faire changer d'avis. Si une
  de ses idées me semble faible, je le dis, avec une meilleure proposition.
- **Ne jamais deviner une règle de jeu.** Si le GDD est muet, flou ou contradictoire : poser la question. Toute décision
  prise en chemin reçoit un numéro dans `Docs/DECISIONS.md` le jour même, et le GDD est réécrit en conséquence.
- **Remplacer = supprimer, le jour même** : code, données, tests et documents de ce qui est remplacé.
- **Une seule vérité par chose** ; tout le reste en est déduit (détail dans `ARCHITECTURE.md`).
- **Contre le spaghetti** : les dépendances ne vont que dans un sens (les assemblages l'imposent) ; on généralise au
  deuxième cas, pas avant ; un fichier qui dépasse 200 lignes est découpé dans l'étape ; chaque fin d'étape répond aux
  quatre questions de la procédure `fin-de-session` (supprimé ? deux façons ? propriétaire ? flèche ajoutée ?) ; les
  signaux d'alerte sont dans `.claude/rules/csharp.md` : devant l'un d'eux, s'arrêter et le dire.
- **Les documents se réécrivent, ils ne se raturent pas** : au présent, sans historique ; l'histoire est dans
  `DECISIONS.md` et dans Git.
- **Automatiser le montage** plutôt que le dicter : un script d'éditeur `[MenuItem("DICIELA/…")]`, sinon une commande du
  CLI, en dernier recours une liste de clics précis.
- **Fin d'étape** : « ce que j'ai fait », « ce que tu fais dans Unity », puis comment tester : chaque geste dit ce qu'il
  vérifie en termes de jeu. Rien d'autre.
- **Agents et workflows : jamais sans son accord explicite pour ce lancement**, avec un plafond de jetons annoncé et tenu
  (compter ~400 k jetons par agent Opus qui lit du code, ~80 k par agent Sonnet en effort moyen à budget serré). Ce qui
  tient en quelques lectures se fait soi-même. Le code s'écrit et se compile dans la session principale.
- **Économie de contexte** : une étape par session (deux au plus), retouches par Edit plutôt que réécritures, une seule
  passe de vérification par étape, une capture d'écran seulement si c'est le seul moyen de vérifier.

## Unity CLI : l'essentiel (tout le reste dans la procédure `unity-cli`)

- Binaire, hors du PATH : `C:/Users/utilisateur/AppData/Local/Unity/bin/unity.exe`. Appel type :
  `C:/Users/utilisateur/AppData/Local/Unity/bin/unity.exe --no-banner --non-interactive command <outil> --param valeur --project-path "C:\Users\utilisateur\Desktop\lebondiciela\V2-DIciELa"`.
- Le CLI est postérieur à mes connaissances : ne jamais inventer une commande ; vérifier dans la procédure ou la doc.
- **Interdits** (des hooks en bloquent une partie, `.claude/hooks/garde-unity.js`) :
  1. une seule campagne de tests à la fois : `run_tests --timeout 300` redirigé vers un fichier ; sur « timed out »,
     `test_status`, jamais relancer ;
  2. jamais de `recompile`, de campagne ni d'`editor_play` quand `editor_status` répond `playing` (le développeur est
     souvent en Play) ; jamais d'`editor_stop` sur son Play ;
  3. jamais de `git checkout`, `switch` (sauf `-c`), `stash` ni `reset --hard` : ils rechargent une scène ouverte ;
     fusionner par `git fetch . <branche>:main` ;
  4. jamais d'import de `.unitypackage` par le CLI (une modale fige l'éditeur) ; l'`eval` reste fermé sans sa demande.
- Une commande destructrice sur ce qui n'est pas dans Git (le pack, les sauvegardes, les réglages) : je propose, il
  exécute. Un fichier suivi par Git et couvert par un plan accepté : je le supprime moi-même et je le dis.

## Architecture (le détail dans `Docs/ARCHITECTURE.md`)

- La simulation est du C# pur, à pas fixe, qui ne sait pas qu'elle est dessinée ; elle lève des événements, la vue les
  écoute.
- Tout le contenu est donnée (ScriptableObjects dans `Assets/Data/`) ; jamais de nombre magique, jamais de chaîne visible
  en dur (clés de `Assets/Localization/strings.csv`).
- Un seul système de modificateurs pour tous les effets ; un seul système de postes et de tâches pour tous les métiers.
- Input System (le nouveau), jamais l'ancien `Input`. Les namespaces et les assemblages sont fixés dans
  `ARCHITECTURE.md`, tous préfixés `Diciela.` ; les scripts d'éditeur vont dans `Diciela.EditorTools`, jamais
  `Diciela.Editor` (en conflit avec `UnityEditor.Editor`).

## Art

- **Tout vient du pack, tel quel.** La caméra, la lumière et le rendu s'adaptent au pack. Je ne compose jamais un
  bâtiment ni un décor de ma propre initiative ; s'il manque une pièce, je le dis et je pose un remplaçant simple. Les
  choix d'art sont au développeur. Rien ne s'écrit jamais dans `Assets/Synty/`.
- Je n'ajoute aucun package et n'importe aucun asset sans son accord. Je ne modifie pas `ProjectSettings/` sans le dire.

## Git

- Une branche par étape (`etape-N-sujet`), ouverte par moi à partir de `main` (`git switch -c`, qui ne touche aucun
  fichier). Un commit par étape qui marche, message court en français, **jamais sans que les tests aient tourné**.
- Fusion en avance rapide sans changer de branche : `git fetch . <branche>:main`, puis `git push origin main`.
- Versionner `Assets/` (hors `Synty/`), `Packages/`, `ProjectSettings/`, `Docs/`, `Tools/`, `.claude/`, `CLAUDE.md`.
  Ne jamais toucher un `.meta` à la main. Le développeur travaille dans l'éditeur pendant les sessions : `git status`
  avant un commit, et dire ce qui est à lui.

## Fin de session

La procédure `fin-de-session` : `Docs/ETAT.md` à jour (30 lignes), décisions numérotées, commit, push, et le prompt de la
session suivante.

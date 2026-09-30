---
name: unity-cli
description: Piloter l'éditeur Unity au CLI (état, recompilation, tests, Play, console, menus, captures) et ses pièges. À lire avant toute commande unity.exe.
---

# Le Unity CLI dans DICIELA

Le CLI de Unity est sorti après les connaissances de Claude : **ne jamais inventer une commande ni un paramètre**.
En cas de doute, `unity list` (les outils du paquet `com.unity.pipeline`) ou la doc (`docs.unity.com/en-us/hub/unity-cli`).

## Appel

- Binaire, hors du PATH, écrit en toutes lettres (les permissions de `.claude/settings.json` ne reconnaissent que cette
  forme) :
  `C:/Users/utilisateur/AppData/Local/Unity/bin/unity.exe --no-banner --non-interactive command <outil> --param valeur --project-path "C:\Users\utilisateur\Desktop\lebondiciela\V2-DIciELa"`
- Paramètres en `--nom valeur`, jamais `nom=valeur`. `--timeout N` se place **après** le nom de la commande.
- **Git Bash mange les chemins** : préfixer de `MSYS_NO_PATHCONV=1` toute commande avec `--path`, `--asset` ou
  `--target /Hiérarchie`.
- Chaque commande a 30 s ; un `HTTP 500` isolé est passager, réessayer une fois.

## L'état de l'éditeur

- **Seul `editor_status` fait foi** (`unity status` du Hub répond « ready » même quand le pipeline est figé). En Play :
  `"playMode":"playing"`. Prêt : `"status":"ready"`, `"compiling":false`, `"domainReloadInProgress":false`.
- **Le développeur est souvent en Play** entre deux messages : `editor_status` avant chaque recompilation, campagne ou
  Play, dans la même commande. Jamais d'`editor_stop` sur son Play.
- **Le pipeline peut se figer** (toute commande expire, `editor_status` compris) : seul un redémarrage d'Unity, par le
  développeur, débloque. Le lui dire, ne pas insister.
- `list_open_scenes` dit quelle scène est ouverte et si elle est sale, avant tout `open_scene` ou `menu --path`.
- **Une fenêtre modale fige aussi toutes les commandes** (« Main thread operation timed out »), alors que le journal
  d'Unity continue de s'écrire (« Scanning for USB devices ») sans plus rien importer. Exemple : `SyntyPackageHelper`
  (procédure `pack-synty`). Seul un clic du développeur débloque ; ne pas boucler sur `editor_status` en attendant :
  chaque appel écrit une erreur dans la console.

## Recompiler

- Juste après un échec, `recompile_status` répond encore `"compilationFailed":true` pendant qu'il recompile (le
  drapeau de la fois d'avant) : n'attendre que `"status":"completed"` ou `"up_to_date"`, jamais ce drapeau.
- `recompile`, puis `recompile_status` en boucle bornée (30 × 3 s) jusqu'à `completed` ou `up_to_date`, puis
  `editor_status` jusqu'à prêt : la commande qui suit une recompilation échoue sinon (`Network error`).
- **Jamais pendant le Play** (le garde-fou le bloque) : un moniteur du Input System resterait périmé.
- Un `up_to_date` immédiat veut souvent dire que l'éditeur n'a pas vu les fichiers : `menu --path "Assets/Refresh"`.
- **Un fichier écrit pendant une compilation peut être importé sans être compilé** : `recompile` répond `up_to_date`,
  et ni `Assets/Refresh` ni une retouche du fichier n'y changent rien. Le sortir du dossier, `Refresh`, le remettre,
  `Refresh`. Vérifier qu'un type neuf est compilé : `grep -c <Type> Library/ScriptAssemblies/<Assemblage>.dll`.
- Un fichier de données retouché hors de l'éditeur (la table des textes) n'est relu qu'après `Assets/Refresh`.
- `get_console_logs` ne remonte pas les erreurs de compilation : `grep "error CS" Logs/Editor.log` en ne lisant que les
  lignes ajoutées depuis la dernière compilation. `editor_play` refuse d'entrer s'il y a une erreur de compilation.

## Les tests

- **Une seule campagne à la fois.** `run_tests` rend la main à 30 s avec « timed out » alors que la campagne continue ;
  en relancer une fige le pipeline (redémarrage obligatoire). Donc :
  `run_tests --timeout 300 --mode EditMode > <scratchpad>/editmode.txt`, puis `node Tools/testreport.js <fichier>`.
  Sur « timed out » : `test_status` (ou `Temp/pipeline_test_status.json`), jamais relancer.
- **PlayMode** : le CLI refuse le mode synchrone ; `bash Tools/playtests.sh <dossier déjà créé>` (asynchrone, attend
  `completed`, refuse de partir en Play).
- **Jamais de campagne PlayMode après une erreur de compilation** : la campagne resterait en file, puis toutes les
  suivantes rendraient 0 test jusqu'au redémarrage d'Unity. Si une campagne PlayMode rend 0 test alors que `list_tests`
  en voit, c'est ce blocage : essayer une fois une vraie recompilation (attendre `editor_status` prêt), puis une
  campagne ; encore 0 test, demander le redémarrage. Un `recompile` qui répond `up_to_date` ne lève rien : retoucher
  un fichier du code (un commentaire suffit) pour qu'il recompile vraiment (étape 17c). Il est aussi arrivé sans erreur
  de compilation, après un Play et une retouche d'USS (étape 15) : une recompilation l'a levé, la cause reste inconnue.
- **Un script qui enchaîne recompilation et campagne s'arrête sur une erreur** : `recompile_status` rend `completed`
  même quand la compilation échoue (`"failed":true`) ; chercher `error CS` dans le journal et ne rien lancer s'il y en a.
  Une campagne partie sur une erreur reste « running », puis rend l'ancien assemblage des tests une fois l'erreur
  corrigée : ses chiffres ne comptent pas.
- Pas de `run_tests --filter`. Enchaîner des campagnes, chacune ayant rendu, ne fige rien.
- Une campagne peut rouvrir les scènes et réenregistrer un prefab que le développeur avait ouvert : `git status` avant,
  et prévenir.

## Le Play, la console, les captures

- **Play au CLI** : `editor_play`, attendre, `console --tail 400` vers un fichier, `editor_stop` (le sien). `simulate_key`
  ne prend qu'après 5 s de Play ; une touche chiffre s'écrit `Digit3` (`--key 3` est lu comme un index).
- `simulate_key` n'atteint que les actions branchées par `action.performed +=`, jamais celles sondées par
  `WasPressedThisFrame` : pour qu'un geste soit jouable au CLI, lui donner un menu `DICIELA/…`.
- **La console garde un tampon qui survit aux compilations** : ne lire que les entrées dont le `seq` dépasse le dernier vu
  avant le Play (`node Tools/console_after.js apres.txt avant.txt`). `clear_console` ne vide pas ce tampon.
- `console --tail N` rend un tableau dont la colonne Résultat est un JSON suivi d'autres colonnes : le couper au premier
  objet complet (c'est ce que fait `console_after.js`).
- **Ne jamais conclure d'un Play muet** : vérifier que la simulation a vraiment travaillé (des lignes de journal).
- **Le Play ne tourne que si Unity a le focus** (`runInBackground: 0` dans les réglages du projet) : ailleurs, il se fige
  et une capture montre une image figée. `editor_focus` avant `editor_play`, et lire le nombre de pas dans la ligne
  « monde arrêté après N pas » à l'`editor_stop` : un seul pas veut dire que rien n'a tourné.
- **Captures** : `capture_game_view --source screen --width 1600 --height 900 --save_path Assets/Temp/x.png` (en Play ;
  2400 × 1350 marche encore, 3840 × 2160 rend une image noire ou périmée ; l'image est la vue Game rééchantillonnée à
  la taille demandée : si la vue n'a pas ce rapport, l'image est étirée (juger les proportions avec prudence) ;
  la source `camera` manque l'UI Toolkit), puis supprimer `Assets/Temp` et son `.meta`. Pour vérifier, une capture seulement si
  c'est le seul moyen ; pour l'album, voir `Docs/ALBUM.md`. Le JPG de l'album se fait par PowerShell (`System.Drawing`,
  encodeur JPEG, qualité 88 : environ 100 Ko) ; Python n'est pas installé.

## Menus, scènes, assets

- Les menus `DICIELA/…` se lancent par `menu --path "DICIELA/…"` sur une scène propre, en Play comme hors Play : la
  validation « en Play seulement » n'est pas appliquée par le CLI, donc un script qui en dépend vérifie `playing` avant.
- **Ce qui n'existe qu'à la souris** devient atteignable au CLI par un `[MenuItem("DICIELA/…")]`.
- `set_serialized_field` prend `--target` et **n'écrit pas sur disque** : `menu --path "File/Save Project"` ensuite.
- `delete_asset --asset <chemin> --confirm true` (le paramètre `--path` n'existe pas) ; les commandes destructrices
  demandent `--confirm true`, et `dry_run` existe.
- **Jamais d'import de `.unitypackage`** (une modale fige l'éditeur) ; toute modale (« sauver la scène ? ») est un clic
  du développeur. L'`eval` reste fermé sans sa demande.
- Ne jamais réécrire un `.inputactions` à la main pendant qu'Unity tourne : passer par un script d'éditeur.

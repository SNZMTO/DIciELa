// Garde-fou lancé par Claude Code avant chaque commande Bash ou PowerShell (hook PreToolUse, .claude/settings.json).
// Il bloque les gestes qui figent l'éditeur Unity ou abîment la partie. Sortie 2 = commande refusée, le message
// (stderr) est montré à Claude ; sortie 0 = la commande suit son cours normal. En cas de doute (entrée illisible,
// éditeur injoignable), il laisse passer : il ne doit jamais bloquer le travail par sa propre panne.
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const UNITY = 'C:/Users/utilisateur/AppData/Local/Unity/bin/unity.exe';

function refuse(message) {
  process.stderr.write('[garde-unity] ' + message + '\n');
  process.exit(2);
}

let input = '';
try { input = fs.readFileSync(0, 'utf8'); } catch (e) { process.exit(0); }
let data;
try { data = JSON.parse(input); } catch (e) { process.exit(0); }
const cmd = String((data.tool_input && data.tool_input.command) || '');
if (!cmd) process.exit(0);

const projet = process.env.CLAUDE_PROJECT_DIR || data.cwd || process.cwd();
const unity = /unity(\.exe)?["']?\s[^|;&]*\bcommand\s+(\w+)/i.exec(cmd);
const outil = unity ? unity[2] : '';

// 1. Git : les commandes qui changent les fichiers sous l'éditeur ouvert (et rechargent ses scènes), ou en effacent.
//    Un texte entre guillemets (un message de commit) ou dans un heredoc n'est pas lu, sauf derrière « bash -c » ;
//    les options globales de git se sautent : « git -C . checkout » est un checkout.
const MASQUE = 'Q';
function lisible(commande) {
  const sansHeredoc = commande.replace(/<<-?\s*(['"]?)(\w+)\1[^\n]*\n[\s\S]*?\n\s*\2(?=\s|$)/g, ' ');
  const imbriquees = [];
  const aPlat = sansHeredoc.replace(/"(?:\\.|[^"\\])*"|'[^']*'/g, (cite, decalage, tout) => {
    if (/\b(?:bash|sh|zsh|pwsh|powershell)(?:\.exe)?\s+(?:-c|-Command)\s*$/i.test(tout.slice(0, decalage))) {
      imbriquees.push(cite.slice(1, -1));
    }
    return MASQUE;
  });
  return [aPlat, ...imbriquees.map(lisible)].join(' ; ');
}
const OPTION_GLOBALE = String.raw`(?:-C|-c|--git-dir|--work-tree|--namespace)(?:=|\s+)\S+|--no-pager|-P|--bare|--literal-pathspecs|--no-optional-locks`;
const GIT = new RegExp(String.raw`(?:^|[\s;&|(])git((?:\s+(?:${OPTION_GLOBALE}))*)\s+([a-z][a-z-]*)([^|;&\n]*)`, 'g');
const CHANGE_LES_FICHIERS = ['pull', 'merge', 'rebase', 'cherry-pick', 'revert'];
for (const [, , sous, reste] of lisible(cmd).matchAll(GIT)) {
  // Les redirections (2>&1, > fichier) ne sont pas des arguments de git.
  const args = reste.replace(/\s*\d*>>?&?\s*\S*/g, '').replace(/\s*<\s*\S+/g, '').trim();
  if (sous === 'stash') refuse('git stash recharge les scènes ouvertes : committer plutôt, ou demander au développeur.');
  if (sous === 'reset' && /(^|\s)--hard\b/.test(args)) refuse('git reset --hard est interdit : il écrase le travail en cours et recharge les scènes.');
  if (sous === 'checkout' && !/^-b\s+\S+$/.test(args)) {
    refuse('git checkout change les fichiers sous l\'éditeur ouvert. Nouvelle branche : git switch -c <nom>. Fusion : git fetch . <branche>:main.');
  }
  if (sous === 'switch' && !/^(-c|--create)\s+\S+$/.test(args)) {
    refuse('git switch vers une branche existante, ou depuis un autre point de départ, change les fichiers sous l\'éditeur. Nouvelle branche depuis ici : git switch -c <nom>. Fusion sans changer de branche : git fetch . <branche>:main.');
  }
  if (sous === 'restore' && !(/(^|\s)(--staged|-S)\b/.test(args) && !/(^|\s)(--worktree|-W)\b/.test(args))) {
    refuse('git restore réécrit des fichiers sous l\'éditeur ouvert : demander au développeur.');
  }
  if (CHANGE_LES_FICHIERS.includes(sous)) {
    refuse('git ' + sous + ' change les fichiers sous l\'éditeur ouvert. Fusion sans changer de branche : git fetch . <branche>:main ; le reste, au développeur.');
  }
  if (sous === 'clean' && /(^|\s)(-[a-zA-Z]*f[a-zA-Z]*|--force)\b/.test(args)) {
    refuse('git clean efface des fichiers que Git ne suit pas : ceux du développeur, le pack, les réglages. Jamais.');
  }
}

// 2. Unity CLI : les outils interdits.
if (outil === 'eval') refuse('l\'eval du CLI (C# arbitraire) reste fermé sans la demande du développeur.');
if (/\.unitypackage/i.test(cmd) && unity) refuse('jamais d\'import de .unitypackage par le CLI : une modale fige l\'éditeur. C\'est une opération du développeur.');

// 3. Une seule campagne de tests à la fois.
const lanceTests = outil === 'run_tests' || /playtests\.sh/.test(cmd);
if (outil === 'run_tests') {
  if (/--filter\b/.test(cmd)) refuse('pas de run_tests --filter : lancer la campagne entière du mode voulu.');
  if (!/--timeout\s+\d+/.test(cmd) && !/--async_tests\s+true/.test(cmd)) refuse('run_tests demande --timeout 300 (après le nom de la commande) et une sortie redirigée vers un fichier.');
}
if (lanceTests) {
  try {
    const statut = fs.readFileSync(path.join(projet, 'Temp', 'pipeline_test_status.json'), 'utf8');
    if (/"status"\s*:\s*"(running|pending|started)"/.test(statut)) {
      refuse('une campagne de tests tourne déjà : lire test_status, ne jamais en relancer une (le pipeline se fige).');
    }
  } catch (e) { /* pas de fichier : aucune campagne connue */ }
}

// 4. Rien qui recompile, teste ou lance le Play pendant que l'éditeur est en Play.
if (outil === 'recompile' || outil === 'editor_play' || lanceTests) {
  let statut = '';
  try {
    statut = execFileSync(UNITY, ['--no-banner', '--non-interactive', 'command', 'editor_status', '--project-path', projet],
      { encoding: 'utf8', timeout: 30000, stdio: ['ignore', 'pipe', 'ignore'] });
  } catch (e) { process.exit(0); }
  if (/"playMode"\s*:\s*"playing"/.test(statut) || /"status"\s*:\s*"playing"/.test(statut)) {
    refuse('l\'éditeur est en Play (souvent le développeur qui joue) : pas de ' + (outil || 'campagne') + ' maintenant. Attendre qu\'il sorte du Play ; jamais d\'editor_stop sur son Play.');
  }
}

process.exit(0);

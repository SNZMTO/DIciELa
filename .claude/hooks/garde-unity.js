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

// 1. Git : les commandes qui rechargent une scène ouverte dans l'éditeur.
if (/\bgit\s+stash\b/.test(cmd)) refuse('git stash recharge les scènes ouvertes : committer plutôt, ou demander au développeur.');
if (/\bgit\s+reset\s+[^|;&]*--hard\b/.test(cmd)) refuse('git reset --hard est interdit : il écrase le travail en cours et recharge les scènes.');
if (/\bgit\s+checkout\b/.test(cmd) && !/\bgit\s+checkout\s+-b\b/.test(cmd)) {
  refuse('git checkout change les fichiers sous l\'éditeur ouvert. Nouvelle branche : git switch -c <nom>. Fusion : git fetch . <branche>:main.');
}
if (/\bgit\s+switch\b/.test(cmd) && !/\bgit\s+switch\s+(-c|--create)\b/.test(cmd)) {
  refuse('git switch vers une branche existante change les fichiers sous l\'éditeur. Fusion sans changer de branche : git fetch . <branche>:main.');
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

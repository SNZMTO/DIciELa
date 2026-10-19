// Les cas du garde-fou .claude/hooks/garde-unity.js, surtout sa partie Git : ce qu'il refuse (sortie 2) et ce qu'il laisse
// passer (sortie 0), et trois cas du CLI d'Unity. Hors de Windows, l'éditeur est injoignable : le garde-fou laisse passer.
//   node Tools/garde-unity.test.js        sortie 0 si tous les cas tiennent, 1 sinon
const { spawnSync } = require('child_process');
const os = require('os');
const path = require('path');

const GARDE = path.join(__dirname, '..', '.claude', 'hooks', 'garde-unity.js');

const REFUSEES = [
  'git checkout main',
  'git -C . checkout main',
  'git -C "C:\\Users\\moi\\Projets\\DIciELa" checkout -- Assets/Scenes/Jeu.unity',
  'git --no-pager checkout etape-74-menage',
  'git -c core.autocrlf=false checkout main',
  'git checkout -B etape-75',
  'git checkout -b etape-75 origin/main',
  'git switch main',
  'git switch -c etape-75 origin/main',
  'git stash',
  'git stash list',
  'git reset --hard HEAD~1',
  'git pull',
  'git pull --ff-only origin main',
  'git merge etape-74-menage',
  'git rebase main',
  'git cherry-pick 1234abc',
  'git revert 1234abc',
  'git restore Assets/Scripts/Game/Boot.cs',
  'git restore --staged --worktree Assets/Scripts/Game/Boot.cs',
  'git clean -fd',
  'git clean -xdf',
  'git clean --force',
  'cd projet && git checkout main',
  'git status; git checkout main',
  'git status | head -3 && git merge etape-74',
  'bash -c "git checkout main"',
  'cmd /c "git checkout main"',
  '"git" checkout main',
  'git.exe checkout main',
  '/usr/bin/git checkout main',
  'echo `git checkout main`',
  '"C:/Program Files/Git/cmd/git.exe" checkout main',
  'C:/Users/moi/AppData/Local/Unity/bin/unity.exe --no-banner --non-interactive command eval --code "1"',
  'C:/Users/moi/AppData/Local/Unity/bin/unity.exe --no-banner command run_tests --filter Chimera --timeout 300',
  'unity --no-banner --non-interactive command eval --code "1"',
];

const PERMISES = [
  'git switch -c etape-75-sols',
  'git switch -c etape-75-sols 2>&1',
  'git switch --create etape-75-sols',
  'git checkout -b etape-75-sols',
  'git fetch . etape-74-menage:main',
  'git push origin main',
  'git status',
  'git log --oneline -5',
  'git diff --stat',
  'git show HEAD:Docs/ETAT.md',
  'git merge-base main etape-74-menage',
  'git reset HEAD Assets/Scripts/Game/Boot.cs',
  'git restore --staged Assets/Scripts/Game/Boot.cs',
  'git clean -n',
  'git branch -d etape-60-fantome-sols',
  'git commit -m "Étape 75 : jamais de git checkout ni de git stash, la fusion par git fetch"',
  "git commit -m 'git pull ne sert pas ici'",
  "git commit -F - <<'EOF'\nÉtape 75 : on ne fait pas git checkout\n\ngit merge non plus.\nEOF",
  'echo "git reset --hard" > note.txt',
  'grep -rn "git" Docs/SAVOIR.md',
  'C:/Users/moi/AppData/Local/Unity/bin/unity.exe --no-banner command run_tests --timeout 300 > sortie.txt',
  'unity --no-banner --non-interactive command editor_status --project-path .',
];

function sortie(commande) {
  const r = spawnSync(process.execPath, [GARDE], {
    input: JSON.stringify({ tool_input: { command: commande } }),
    env: { ...process.env, CLAUDE_PROJECT_DIR: os.tmpdir() },
    encoding: 'utf8',
  });
  return r.status;
}

let fautes = 0;
for (const c of REFUSEES) if (sortie(c) !== 2) { fautes++; console.log('laisse passer : ' + c); }
for (const c of PERMISES) if (sortie(c) !== 0) { fautes++; console.log('refuse à tort : ' + c.replace(/\n/g, '⏎')); }
console.log(`${REFUSEES.length + PERMISES.length - fautes}/${REFUSEES.length + PERMISES.length} cas tiennent.`);
process.exit(fautes === 0 ? 0 : 1);

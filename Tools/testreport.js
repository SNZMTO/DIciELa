// node Tools/testreport.js <sortie_run_tests.txt>
// Lit la sortie d'un `run_tests` du CLI de Unity (un tableau dont la colonne Résultat est un JSON) et affiche les tests
// rouges, puis le compte. Sortie 0 si tout est vert et qu'au moins un test a tourné, 1 sinon.
const fs = require('fs');

// Le premier objet JSON complet du texte, en tenant compte des chaînes (une accolade dans un message ne compte pas).
function firstJson(text) {
  const start = text.indexOf('{');
  if (start < 0) return null;
  let depth = 0, inString = false, escaped = false;
  for (let i = start; i < text.length; i++) {
    const c = text[i];
    if (inString) {
      if (escaped) escaped = false;
      else if (c === '\\') escaped = true;
      else if (c === '"') inString = false;
      continue;
    }
    if (c === '"') inString = true;
    else if (c === '{') depth++;
    else if (c === '}' && --depth === 0) {
      try { return JSON.parse(text.slice(start, i + 1)); } catch (e) { return null; }
    }
  }
  return null;
}

const file = process.argv[2];
if (!file) { console.log('Usage : node Tools/testreport.js <sortie_run_tests.txt>'); process.exit(1); }
const json = firstJson(fs.readFileSync(file, 'utf8'));
if (!json) { console.log('Aucun résultat lisible dans ' + file + ' (campagne encore en cours ? lire test_status).'); process.exit(1); }

const summary = json.Summary || json.summary || {};
const results = json.Results || json.results || [];
const total = summary.Total ?? summary.total ?? results.length;
const passed = summary.Passed ?? summary.passed ?? 0;
const failed = summary.Failed ?? summary.failed ?? 0;
const skipped = summary.Skipped ?? summary.skipped ?? 0;

for (const r of results) {
  const status = String(r.Status || r.status || '');
  if (/fail|error/i.test(status)) {
    const name = String(r.FullName || r.fullName || r.Name || '');
    const message = String(r.Message || r.message || '').trim().split('\n').join(' ');
    console.log(`ROUGE  ${name}\n       ${message.slice(0, 400)}`);
  }
}
console.log(`${passed}/${total} verts, ${failed} rouges${skipped ? `, ${skipped} ignorés` : ''}.`);
process.exit(failed === 0 && total > 0 ? 0 : 1);

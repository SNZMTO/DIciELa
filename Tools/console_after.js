// node Tools/console_after.js <console_apres.txt> <console_avant.txt> [regex]
// Les deux fichiers sont des sorties de `console --tail 400` du CLI de Unity. La console garde un tampon qui survit aux
// compilations : on ne garde que les entrées dont le numéro (seq) dépasse le dernier vu avant le Play. Affiche ces
// entrées (celles qui correspondent à la regex si elle est donnée). Sortie 1 si l'une est une erreur ou une exception.
const fs = require('fs');

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

function entries(file) {
  const json = firstJson(fs.readFileSync(file, 'utf8'));
  return (json && (json.entries || json.Entries)) || [];
}

const [after, before, pattern] = process.argv.slice(2);
if (!after || !before) { console.log('Usage : node Tools/console_after.js <apres.txt> <avant.txt> [regex]'); process.exit(1); }
const lastSeen = entries(before).reduce((max, e) => Math.max(max, e.seq || 0), 0);
const filter = pattern ? new RegExp(pattern) : null;
let bad = 0;
for (const e of entries(after)) {
  if ((e.seq || 0) <= lastSeen) continue;
  const level = String(e.level || e.type || '');
  const message = String(e.message || '');
  const isBad = /error|exception|assert/i.test(level);
  if (isBad) bad++;
  if (!filter || filter.test(message) || isBad) console.log(`#${e.seq} ${level.padEnd(9)} ${message.split('\n')[0].slice(0, 300)}`);
}
console.log(bad ? `${bad} erreur(s) ou exception(s) neuve(s).` : 'Aucune erreur neuve.');
process.exit(bad ? 1 : 0);

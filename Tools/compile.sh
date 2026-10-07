#!/usr/bin/env bash
# bash Tools/compile.sh
# Recompile le code dans l'éditeur ouvert, attend la fin (recompile_status, puis editor_status prêt), et cherche les
# erreurs de compilation écrites dans le journal de l'éditeur depuis le départ : recompile_status rend « completed »
# même quand la compilation échoue, et get_console_logs ne les remonte pas. Rend 0 si la compilation a réussi, 1 sur
# une erreur (elles s'affichent), 2 si l'éditeur est en Play (le Play du développeur ne se touche pas). À enchaîner par
# && avant une campagne, jamais par ; ni par un tube.
set -u

UNITY="C:/Users/utilisateur/AppData/Local/Unity/bin/unity.exe"
PROJECT="${CLAUDE_PROJECT_DIR:-C:\Users\utilisateur\Desktop\lebondiciela\V2-DIciELa}"
LOG="$(cd "$(dirname "$0")/.." && pwd)/Logs/Editor.log"

run() { MSYS_NO_PATHCONV=1 "$UNITY" --no-banner --non-interactive command "$@" --project-path "$PROJECT" 2>&1; }

run editor_status | grep -q '"playMode":"stopped"' || { echo "L'éditeur est en Play (ou ne répond pas) : pas de recompilation."; exit 2; }
start=$(wc -c < "$LOG")
run recompile | tail -1 | cut -c1-200

# Au plus deux minutes chacune : la compilation, puis le rechargement du domaine.
for _ in $(seq 1 40); do
  out=$(run recompile_status | tail -1)
  if echo "$out" | grep -q '"status":"completed"\|"status":"up_to_date"'; then echo "$out" | cut -c1-200; break; fi
  sleep 3
done
for _ in $(seq 1 40); do
  run editor_status | tail -1 | grep -q '"status":"ready","compiling":false,"domainReloadInProgress":false' && break
  sleep 3
done

errors=$(tail -c +$((start + 1)) "$LOG" | grep "error CS" | sort -u)
if [ -n "$errors" ]; then echo "$errors" | head -40; exit 1; fi
echo "COMPILATION OK"

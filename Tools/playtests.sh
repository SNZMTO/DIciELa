#!/usr/bin/env bash
# bash Tools/playtests.sh <dossier déjà créé>
# Lance la campagne de tests PlayMode et attend son résultat. Le CLI refuse le mode synchrone en PlayMode (entrer en Play
# recharge le domaine et coupe la requête) : on lance en asynchrone et on lit test_status jusqu'à « completed », en
# bornant l'attente à cinq minutes. Rend 0 si tout est vert. Ne part jamais si l'éditeur est en Play.
set -u

UNITY="C:/Users/utilisateur/AppData/Local/Unity/bin/unity.exe"
PROJECT="${CLAUDE_PROJECT_DIR:-C:\Users\utilisateur\Desktop\lebondiciela\V2-DIciELa}"
OUT="${1:?donner un dossier de sortie déjà créé}"
[ -d "$OUT" ] || { echo "Le dossier $OUT n'existe pas : le créer d'abord."; exit 2; }

run() { MSYS_NO_PATHCONV=1 "$UNITY" --no-banner --non-interactive command "$@" --project-path "$PROJECT" 2>&1; }

if run editor_status | grep -q '"playMode":"playing"'; then
  echo "L'éditeur est en Play : pas de campagne."; exit 4
fi

run run_tests --mode PlayMode --async_tests true > "$OUT/playtests_start.txt"
grep -q '"success":true' "$OUT/playtests_start.txt" || { echo "Lancement refusé :"; tail -c 300 "$OUT/playtests_start.txt"; exit 2; }

F="$OUT/playtests.txt"
done=0
for _ in $(seq 1 60); do
  sleep 5
  run test_status > "$F"
  grep -q '"status": *"completed"' "$F" && { done=1; break; }
done
[ $done -eq 1 ] || { echo "La campagne n'a pas fini en cinq minutes : lire test_status, ne pas relancer."; exit 3; }

node "$(dirname "$0")/testreport.js" "$F"

echo "Start pre provision tasks..."
  cd ./AI.Agentic.Frondend || exit 1
  cp .env.prod .env
  npm run build

echo "Pre Provision tasks completed."
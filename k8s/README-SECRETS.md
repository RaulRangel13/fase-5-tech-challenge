# Como criar o Secret no Kubernetes (NUNCA commitar o .env)
#
# 1. Na raiz do projeto, crie/edite o arquivo .env (copie de .env.example e preencha).
# 2. Crie o secret a partir do .env:
#
#    kubectl create secret generic agro-secrets --from-env-file=.env -n default
#
# 3. Se o secret já existir e você quiser atualizar:
#    kubectl delete secret agro-secrets -n default
#    kubectl create secret generic agro-secrets --from-env-file=.env -n default
#
# O .env deve conter as chaves esperadas pelos deployments, por exemplo:
#   POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB
#   CONNECTION_STRING_IDENTITY, CONNECTION_STRING_MANAGEMENT, CONNECTION_STRING_ALERT
#   JWT_SECRET_KEY, RABBITMQ_DEFAULT_USER, RABBITMQ_DEFAULT_PASS
#   GF_SECURITY_ADMIN_USER, GF_SECURITY_ADMIN_PASSWORD
#
# Veja .env.example e docs/SECRETS.md.

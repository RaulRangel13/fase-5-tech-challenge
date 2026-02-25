# Guia de Configuração (SETUP) - AgroSolutions

Este guia descreve como rodar a plataforma AgroSolutions localmente. Você tem duas opções: **Docker Compose** (recomendado para testes rápidos) ou **Kubernetes / Minikube** (recomendado para o requisito oficial).

## Pré-requisitos
Independente da forma escolhida, você deve ter instalado na sua máquina:
1. Docker Desktop ou Engine
2. .NET 8 SDK (se quiser rodar os testes unitários via CLI)

Se for rodar via Kubernetes:
3. Minikube ou Kind
4. `kubectl` instalado e configurado

---

## Opção 1: Rodando via Kubernetes (Recomendado)

O projeto já contém todos os manifestos necessários na pasta `k8s/`. Esta opção demonstra o uso de orquestração de contêineres e obedece integralmente ao requisito técnico.

### Passo 1: Iniciar o cluster Kubernetes
Se estiver usando o Minikube (certifique-se de alocar bons recursos, pois subiremos RabbitMQ, Postgres e 4 APIs C#):
```bash
minikube start --cpus=4 --memory=8192
```

### Passo 2: Construir as Imagens Docker (Local Registry)
Aponte seu terminal local para o daemon docker do Minikube para que as imagens fiquem disponíveis no cluster sem precisar publicá-las no DockerHub:
```bash
# Windows (PowerShell)
minikube docker-env | Invoke-Expression

# Linux / Mac
eval $(minikube docker-env)
```
Em seguida, compile as imagens:
```bash
docker build -t agro-identity:latest -f src/AGRO.Identity.Service/Dockerfile .
docker build -t agro-management:latest -f src/AGRO.Management.Service/Dockerfile .
docker build -t agro-ingestion:latest -f src/AGRO.Ingestion.Service/Dockerfile .
docker build -t agro-alert:latest -f src/AGRO.Alert.Service/Dockerfile .
```

### Passo 3: Aplicar Infraestrutura (Postgres, RabbitMQ, Prometheus, Grafana)
```bash
cd k8s/infra
kubectl apply -f postgres.yaml
kubectl apply -f rabbitmq.yaml
kubectl apply -f prometheus.yaml
```
**Grafana (ConfigMaps + Deploy):**

Antes do `grafana.yaml`, gere os ConfigMaps de provisioning/dashboards:

```bash
./generate-grafana-cm.sh
kubectl apply -f grafana-configmaps.yaml
kubectl apply -f grafana.yaml
```

Os DataSources e Dashboards do Grafana já vão ser provisionados via ConfigMap.

### Passo 4: Aplicar os Microsserviços
```bash
cd ../apps
kubectl apply -f identity.yaml
kubectl apply -f management.yaml
kubectl apply -f ingestion.yaml
kubectl apply -f alert.yaml
```

Aguarde os pods ficarem com status `Running`:
```bash
kubectl get pods -w
```

### Passo 5: Acesso aos Serviços (NodePort)
Dependendo do Minikube, você precisará executar:
```bash
minikube service agro-identity --url
minikube service agro-management --url
minikube service agro-ingestion --url
minikube service agro-alert --url
minikube service grafana --url
```
Geralmente, o Grafana ficará disponível na porta mapeada e você poderá visualizá-lo com login `admin` / `admin`.

---

## Opção 2: Rodando via Docker Compose

Se o intuito for subir rápido:
```bash
docker-compose up --build -d
```
Isso vai criar toda a infraestrutura com as rotas fixas:
- Identity API: `http://localhost:5001/swagger`
- Management API: `http://localhost:5002/swagger`
- Ingestion API: `http://localhost:5003/swagger`
- Alert API: `http://localhost:5004/swagger`
- RabbitMQ UI: `http://localhost:15672` (guest / guest)
- Grafana: `http://localhost:3000` (admin / admin)
- Prometheus: `http://localhost:9090`

**Primeira vez ou após `docker-compose down -v`:** o script em `init-db/01-create-databases.sql` cria os bancos `agro_identity_db`, `agro_management_db` e `agro_alert_db` na inicialização do PostgreSQL.

### Dashboard de Monitoramento (dados do produtor)
Além do dashboard técnico (métricas Prometheus), existe um dashboard de **telemetria/alertas** via PostgreSQL:
- Grafana: `http://localhost:3000` → pasta **Tech Challenge** → **AgroSolutions - Monitoramento por Talhão**

---

## Rodando os Testes Unitários

Para atender ao requisito de **testes unitários obrigatórios quando o deploy é local**, o projeto inclui testes em `tests/AGRO.Tests/`:

* **FarmServiceTests:** cadastro de fazenda e talhão, listagem, talhão em fazenda inexistente.
* **AuthServiceTests:** registro, login, email duplicado, credenciais inválidas.
* **AlertServiceTests:** listagem de alertas (com e sem filtro), status do talhão (Normal quando sem dados), histórico de telemetria.

Na raiz do repositório:

```bash
dotnet test
```

O pipeline de CI (`.github/workflows/ci.yml`) executa esses testes antes do build das imagens Docker; todos devem passar para o pipeline concluir com sucesso.

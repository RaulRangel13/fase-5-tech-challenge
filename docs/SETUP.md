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
- RabbitMQ UI: `http://localhost:15672` (guest / guest)
- Grafana: `http://localhost:3000` (admin / admin)
- Prometheus: `http://localhost:9090`

---

## Rodando os Testes Unitários

Para garantir que o CI/CD (GitHub Actions) vai passar caso comite as alterações, rode os testes de unidade na pasta raiz do projeto:
```bash
dotnet test
```

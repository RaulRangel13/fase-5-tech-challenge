# Arquitetura da Solução - AgroSolutions MVP

## 1. Visão Geral da Arquitetura
A plataforma AgroSolutions foi desenhada baseada no padrão de **Microsserviços**. Cada domínio possui seu próprio serviço, banco de dados (ou schema isolado) e responsabilidades delimitadas para aumentar a coesão e reduzir o acoplamento. 

### Diagrama de Contexto & Container (C4 Model)

```mermaid
graph TD
    User([Produtor Rural]) -->|Acessa Dashboard| Grafana[Grafana Dashboard]
    User -->|Acessa APIs| API_GW(Gateway / Ingress Controller*)
    
    API_GW -->|Autenticação| ID_SVC[Identity Service]
    API_GW -->|Gestão| MGMT_SVC[Management Service]
    API_GW -->|Telemetria| INGEST_SVC[Ingestion Service]
    
    INGEST_SVC -->|Publica Dados| RabbitMQ[(RabbitMQ Broker)]
    RabbitMQ -->|Consome Dados| ALERT_SVC[Alert Service]
    
    ID_SVC -->|Persiste Usuários| DB_POSTGRES[(PostgreSQL - Identity DB)]
    MGMT_SVC -->|Persiste Propriedades| DB_POSTGRES[(PostgreSQL - Management DB)]
    ALERT_SVC -->|Persiste Alertas e Leituras| DB_POSTGRES[(PostgreSQL - Alert DB)]
    
    Prometheus[Prometheus] -->|Scrape /metrics| ID_SVC
    Prometheus -->|Scrape /metrics| MGMT_SVC
    Prometheus -->|Scrape /metrics| INGEST_SVC
    Prometheus -->|Scrape /metrics| ALERT_SVC
    Grafana -->|Consulta Métricas| Prometheus
```

## 2. Componentes e Responsabilidades

* **Identity Service (.NET 8):** Responsável por registro de produtores e emissão de tokens JWT.
* **Management Service (.NET 8):** Responsável pelo "Core Domain" da fazenda (gestão de propriedades rurais e seus talhões/áreas de plantio). Acesso seguro via JWT.
* **Ingestion Service (.NET 8):** Ponto de entrada (High Throughput) para telemetria de sensores de sensores de IoT. Não persiste dados, apenas enfileira as requisições no RabbitMQ para processamento em background.
* **Alert Service (.NET 8 Web API + Worker):** Serviço híbrido: consome a fila RabbitMQ (regras de alerta) e expõe API REST para listar alertas, histórico de telemetria e status por talhão. Persiste leituras de sensores e alertas no PostgreSQL. Regras: Alerta de Seca (umidade < 30% por mais de 24h), Risco de Praga (temperatura elevada).
* **Message Broker (RabbitMQ):** Desacopla a Ingestão da Análise. Garante resiliência: se o Alert Service cair, as medições ficam retidas.
* **PostgreSQL:** Banco de dados relacional (dividido logicamente em diferentes schemas/bases por serviço).
* **Observabilidade:**
  * **Prometheus:** Coleta as métricas HTTP e latência dos serviços .NET consumindo o endpoint `/metrics` exposto pela lib `prometheus-net`.
  * **Grafana:** Exibe (1) métricas técnicas via Prometheus (RPS/latência) e (2) dashboard de negócio com histórico de telemetria e alertas via PostgreSQL (para cumprir o requisito de "gráfico histórico" e "alertas no dashboard").

## 3. Justificativa Técnica das Decisões Arquiteturais

* **Microsserviços por domínio:** Separação Identity / Management / Ingestion / Alert permite evolução e deploy independentes e atende ao requisito de arquitetura baseada em microsserviços.
* **Mensageria (RabbitMQ):** Desacopla a ingestão do processamento; garante resiliência e escalabilidade da ingestão sem sobrecarregar o banco.
* **Persistência de leituras no Alert Service:** Permite histórico para gráficos e aplicação da regra de 24h (umidade &lt; 30% por mais de 24 horas) sem depender apenas de uma medição pontual.
* **Um banco por serviço (PostgreSQL):** `agro_identity_db`, `agro_management_db`, `agro_alert_db` — isolamento de dados e responsabilidade por serviço.
* **Stack Prometheus/Grafana:** Atende ao requisito de observabilidade (alternativa a Zabbix); métricas de aplicação e dashboard de negócio no mesmo Grafana.

## 4. Requisitos Não Funcionais Atendidos

* **Escalabilidade:** Como a ingestão de IoT gera muito fluxo, ela foi separada utilizando RabbitMQ. O `Ingestion Service` pode escalar horizontalmente para receber requisições de milhares de sensores, enquanto o `Alert Service` processa o consumo conforme capacidade, protegendo o banco contra sobrecarga de writes e Deadlocks.
* **Portabilidade:** Todos os componentes foram containerizados (Docker) com suporte nativo de orquestração via manifestos Kubernetes (Deployments/Services), podendo rodar no Minikube/Kind, ou na AWS EKS/Azure AKS.
* **Estabilidade e Testabilidade:** Criada esteira de CI/CD (GitHub Actions) validando o build das imagens Docker e os testes unitários da lógica de serviço Core na compilação.

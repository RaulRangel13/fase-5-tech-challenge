# Entregáveis Mínimos – Checklist e Evidências

Este documento mapeia os entregáveis do Hackathon AgroSolutions para a documentação e ao código do repositório, e registra o **fechamento dos gaps** identificados.

---

## 1. Desenho da Solução MVP

| Item | Onde encontrar | Observação |
|------|----------------|------------|
| **Diagrama da arquitetura** | `docs/ARCHITECTURE.md` – diagrama C4 (Contexto/Container) em Mermaid | Identity, Management, Ingestion, Alert, RabbitMQ, PostgreSQL, Prometheus, Grafana |
| **Justificativa técnica** | `docs/ARCHITECTURE.md` – seção "Componentes e Responsabilidades" | Microsserviços por domínio; mensageria para ingestão; persistência de leituras e alertas no Alert Service |
| **Requisitos não funcionais** | `docs/ARCHITECTURE.md` – seção "Requisitos Não Funcionais Atendidos" e `docs/NON-FUNCTIONAL.md` | Escalabilidade, resiliência, observabilidade, segurança |

---

## 2. Demonstração da Infraestrutura

| Item | Evidência |
|------|-----------|
| **Aplicação rodando (local ou nuvem)** | Docker Compose: `docker-compose up --build -d`. Kubernetes: `k8s/` (Minikube/Kind). Ver `docs/SETUP.md`. |
| **Kubernetes** | Manifestos em `k8s/infra/` (postgres, rabbitmq, prometheus, grafana) e `k8s/apps/` (identity, management, ingestion, alert). |
| **Grafana** | Provisionado no Compose e no K8s; datasources e dashboards em `grafana/`. Dashboard de negócio: **AgroSolutions - Monitoramento por Talhão** (histórico e alertas). |
| **Prometheus** | Stack Prometheus/Grafana: `prometheus.yml` na raiz; Grafana com datasource Prometheus; serviços expõem `/metrics`. |

*Requisito aceita **Zabbix ou Prometheus**; esta solução usa **Prometheus + Grafana**.*

---

## 3. Demonstração da Esteira de CI/CD

| Item | Evidência |
|------|-----------|
| **Pipeline de deploy** | GitHub Actions em `.github/workflows/ci.yml`: build da solução, testes unitários e build das imagens Docker (Identity, Management, Ingestion, Alert). |
| **Testes unitários (obrigatórios para deploy local)** | Projeto `tests/AGRO.Tests/`: **FarmServiceTests** (Management), **AuthServiceTests** (Identity), **AlertServiceTests** (Alert). Cobertura: cadastro de fazenda/talhão, criação de campo, registro/login, credenciais inválidas, listagem de alertas, status por talhão, histórico de telemetria. Execução: `dotnet test` na raiz (ou `dotnet test tests/AGRO.Tests/AGRO.Tests.csproj`). |

---

## 4. Demonstração do MVP (aplicação funcional)

| Recurso | Implementação |
|---------|----------------|
| **Autenticação do produtor rural** | Identity Service: `POST /api/auth/register`, `POST /api/auth/login`. JWT usado no Management. |
| **Cadastro de propriedade/talhão** | Management Service: `POST /api/farms`, `POST /api/farms/{id}/fields`. Protegido com JWT. |
| **Envio de dados de sensores (simulação)** | Ingestion Service: `POST /api/telemetry` (fieldId, umidade, temperatura, precipitação). Dados publicados no RabbitMQ e consumidos pelo Alert Service. |
| **Visualização dos dados e alertas no dashboard** | Grafana: dashboard **AgroSolutions - Monitoramento por Talhão** (gráficos de umidade/temperatura/precipitação, tabela de alertas, status do talhão). APIs do Alert Service: `GET /api/alerts`, `GET /api/telemetry`, `GET /api/status/field/{fieldId}` para integrações ou uso direto. |

## Resumo para o vídeo de demonstração

- **Entregável 1:** Mostrar `docs/ARCHITECTURE.md` (diagrama + justificativa + NFRs).
- **Entregável 2:** Mostrar containers/pods (Docker Desktop ou `kubectl get pods`), Grafana e Prometheus acessíveis.
- **Entregável 3:** Mostrar aba **Actions** do GitHub (workflow com build + testes + imagens) e rodar `dotnet test` localmente.
- **Entregável 4:** Fluxo no Swagger (register → login → farms → fields → telemetry) e dashboard Grafana "AgroSolutions - Monitoramento por Talhão" com gráficos e alertas.

Roteiro detalhado do vídeo: `docs/VIDEO_SCRIPT.md`.

# Projeto AgroSolutions (Hackathon 8NETT) - Guia de Desenvolvimento

## 1. Visão Geral
- **Objetivo**: Desenvolvimento de um MVP de plataforma de IoT para agricultura de precisão.
- **Contexto**: A AgroSolutions busca modernizar o monitoramento de recursos hídricos e produtividade agrícola.
- **Meta**: Vencer o Hackathon com uma solução robusta, escalável e bem documentada.

## 2. Escopo Funcional

### 2.1. Autenticação & Identidade
- Login seguro para produtores rurais (E-mail/Senha).
- Gestão de tokens JWT.

### 2.2. Gestão de Propriedades (Core Domain)
- Cadastro de Propriedades Rurais.
- Cadastro de Talhões (áreas de plantio dentro das propriedades).
- Associação de Culturas aos Talhões.

### 2.3. Ingestão de Dados IoT
- API dedicada para recebimento de telemetria.
- Dados simulados:
  - Umidade do Solo.
  - Temperatura.
  - Precipitação (Chuva).
- Associação dos dados ao Talhão específico.

### 2.4. Monitoramento & Dashboard
- Visualização de status em tempo real.
- Histórico de medições.
- Indicadores visuais (Normal, Alerta, Crítico).
- **Nota**: Dashboard implementado diretamente no Grafana.

### 2.5. Motor de Alertas Inteligentes
- Processamento assíncrono de regras de negócio.
- Exemplo de Regra: `Umidade < 30%` por `24h` -> **Alerta de Seca (Risco de Perda)**.
- Notificações para o produtor.

## 3. Arquitetura & Stack Tecnológica

### 3.1. Stack Principal
- **Linguagem**: .NET 8 (C#).
- **API Framework**: ASP.NET Core Web API (Controllers).
- **Banco de Dados**: PostgreSQL.
- **Mensageria**: RabbitMQ.
- **Observabilidade**: Prometheus & Grafana.
- **Deploy**: Docker & Kubernetes.

### 3.2. Padrões Obrigatórios
- **Clean Architecture**: Separação clara de responsabilidades.
- **DDD (Domain-Driven Design)**: Foco no domínio, entidades ricas, agregados.
- **SOLID & Clean Code**: Código limpo, testável e manutenível.
- **Microsserviços**: Separação física dos contextos delimitados.

## 4. Estrutura de Microsserviços
Cada serviço deve possuir sua estrutura independente (Domain, Application, Infrastructure, API).

1.  **Identity Service**: Autenticação, gestão de usuários.
2.  **Management Service**: Cadastro de Fazendas e Talhões.
3.  **Ingestion Service**: API de entrada de dados IoT (High Throughput).
4.  **Alert Service**: Worker/Processor de regras de negócio e alertas.

## 5. Infraestrutura Local (DevOps)
- **Docker Compose**: Orquestração completa do ambiente de desenvolvimento (DB, Broker, APIs, Monitoring).
- **Kubernetes (K8s)**: Manifestos para deploy (Kind/Minikube).

## 6. Documentação Obrigatória (/docs)
A ser gerada durante o desenvolvimento:
1.  `ARCHITECTURE.md`: Desenhos, diagramas C4/Mermaid, decisões arquiteturais.
2.  `NON-FUNCTIONAL.md`: Detalhes de escalabilidade, resiliência e observabilidade.
3.  `SETUP.md`: Guia "Zero to Hero" para rodar o projeto localmente.
4.  `TEST_SCENARIOS.md`: Payloads e scripts para validar fluxos (ex: gerar alerta).
5.  `VIDEO_SCRIPT.md`: Roteiro para a apresentação (pitch) do Hackathon.

## 7. Diretrizes Finais
- **Código Comentado**: Business logic deve ser auto-explicativa ou comentada.
- **Sem Frontend Customizado**: Foco total no Backend + Grafana.
- **Robustez**: Tratamento de exceções, validações e logs estruturados.

# Non-Functional Requirements (NFR)

## 1. Escalabilidade
- **Horizontal**: O serviço de Ingestão (`Ingestion Service`) é stateless e pode ter múltiplas réplicas rodando atrás do Load Balancer (Ingress) para suportar aumento na carga de sensores.
- **Assíncrona**: O processamento de mensagens no `Alert Service` escala aumentando o número de consumidores (instances) conectados à fila do RabbitMQ.

## 2. Disponibilidade e Resiliência
- **Retries**: Implementação de políticas de Retry com Backoff Exponencial (using Polly) nas chamadas HTTP e conexões com Banco/Broker.
- **Filas**: O uso do RabbitMQ garante que, se o `Alert Service` cair, as mensagens não são perdidas, apenas acumuladas até o serviço voltar (Durable Queues).
- **Health Checks**: Todos os serviços expõem endpoints `/health` para que o orquestrador (K8s/Docker) possa reiniciar containers travados.

## 3. Observabilidade
- **Métricas**:
  - Aplicação: Requisições/seg, Latência, Taxa de Erro (HTTP 5xx).
  - Negócio: Total de medições recebidas, Total de alertas gerados.
  - Infra: CPU, Memória, Disco (via cAdvisor/NodeExporter se houver, ou métricas básicas do container).
- **Logs**:
  - Logs estruturados (JSON) no console (stdout) para fácil agregação futura.
  - Níveis de Log controlados via variável de ambiente (Debug em Dev, Info/Warning em Prod).

## 4. Segurança
- **Autenticação**: JWT (JSON Web Tokens) com tempo de expiração curto.
- **Segredos**: Senhas de banco e chaves de API injetadas via Variáveis de Ambiente (Environment Variables), jamais hardcoded.
- **Rede**: Em produção, apenas a porta 80/443 do Gateway é exposta; serviços conversam internamente na rede do Cluster.

## 5. Performance
- **Ingestão**: API otimizada para escrita rápida (Fire-and-forget para a fila).
- **Banco**: Uso de índices adequados nas colunas de busca (ex: `DeviceId`, `Timestamp`).
- **Eficiência**: Validações de domínio rápidas ("Fail Fast").

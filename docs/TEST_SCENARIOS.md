# Cenários de Teste & Payloads

## 1. Automação de Infraestrutura
Para realizar os testes sistêmicos, certifique-se que subiu e aplicou os manifestos (ou Docker Compose) descritos no `SETUP.md`.

## 2. Autenticação (Identity Service)

### 2.1. Criar Produtor
**POST** `http://localhost:30001/api/auth/register` (K8s NodePort) ou `http://localhost:5001/api/auth/register` (Compose)
```json
{
  "email": "produtor@agro.com",
  "password": "Password123!"
}
```

### 2.2. Login
**POST** `http://localhost:30001/api/auth/login` (K8s NodePort) ou `http://localhost:5001/api/auth/login` (Compose)
```json
{
  "email": "produtor@agro.com",
  "password": "Password123!"
}
```
**Esperado**: `200 OK` com `token`. Salve o Token para a etapa 3.

---

## 3. Gestão (Management Service)

No Swagger do Management (`http://localhost:5002/swagger`), clique em **Authorize** e cole **apenas o token** (sem escrever "Bearer" na frente). O Swagger envia no header: `Authorization: Bearer SEU_TOKEN`.

### 3.1. Criar Propriedade
**POST** `http://localhost:30002/api/farms` (K8s) ou `http://localhost:5002/api/farms` (Compose)
```json
{
  "name": "Fazenda Sol Nascente",
  "location": "Ribeirão Preto - SP"
}
```
*Salve o `id` da fazenda retornado*.

### 3.2. Criar Talhão
**POST** `http://localhost:30002/api/farms/{id_fazenda}/fields` (K8s) ou `http://localhost:5002/api/farms/{id_fazenda}/fields` (Compose)
```json
{
  "name": "Talhão A - Soja",
  "areaHectares": 50.5,
  "cropType": "Soybean"
}
```
*Salve o `id` do Talhão retornado para simular o sensor*.

---

## 4. Ingestão (Simulação de Sensores - Ingestion Service)

### 4.1. Enviar Telemetria Normal
**POST** `http://localhost:30003/api/telemetry` (K8s) ou `http://localhost:5003/api/telemetry` (Compose)
```json
{
  "fieldId": "ID_DO_TALHAO",
  "soilMoisture": 45.0,
  "temperature": 28.5,
  "rainfall": 0.0
}
```
**Esperado**: `202 Accepted` ("Data queued for processing").

### 4.2. Enviar Telemetria Crítica (Risco de Seca)
Envie múltiplas requisições com umidade abaixo de 30% para simular a condição de acionamento do Workers.

**POST** `/api/telemetry`
```json
{
  "fieldId": "ID_DO_TALHAO",
  "soilMoisture": 15.0,
  "temperature": 32.0,
  "rainfall": 0.0
}
```

### 4.3. Visualizar histórico, alertas e status no Grafana (Dashboard do Produtor)
1. Abra o Grafana: `http://localhost:3000` (login `admin` / `admin`)
2. Vá na pasta **Tech Challenge**
3. Abra o dashboard **AgroSolutions - Monitoramento por Talhão**
4. Selecione o `fieldId` do talhão e visualize:
   - Umidade / Temperatura / Precipitação (séries temporais)
   - Alertas (tabela)
   - Status do talhão

---

## 5. Alert Service – Alertas, Histórico e Status

### 5.1. Listar alertas por talhão
**GET** `http://localhost:30004/api/alerts?fieldId=ID_DO_TALHAO` (K8s NodePort) ou `http://localhost:5004/api/alerts?fieldId=ID_DO_TALHAO` (Compose)  
Alternativa: **GET** `/api/alerts/field/{fieldId}`

### 5.2. Histórico de telemetria (gráficos)
**GET** `http://localhost:30004/api/telemetry?fieldId=ID_DO_TALHAO&from=2025-01-01&to=2025-12-31&limit=500` (K8s) ou `http://localhost:5004/api/telemetry?fieldId=ID_DO_TALHAO&from=2025-01-01&to=2025-12-31&limit=500` (Compose)

### 5.3. Status do talhão (Normal / Alerta de Seca / Risco de Praga)
**GET** `http://localhost:30004/api/status/field/{fieldId}` (K8s) ou `http://localhost:5004/api/status/field/{fieldId}` (Compose)

---

## 6. Validação da Mensageria e Alertas
1. Acesse a interface do RabbitMQ em http://localhost:15672 (guest/guest). Verifique a fila `telemetry_queue`.
2. Acesse o **Alert Service Logs**. Com umidade < 30% por mais de 24h, verá "ALERTA DE SECA!"; dados dos sensores são persistidos para histórico.
3. Acesse o **Grafana** em http://localhost:32000 (K8s) ou http://localhost:3000 (Compose). Faça login com `admin` / `admin` e visualize o painel de "Métricas AgroSolutions". Note o aumento do RPS na ingestão.

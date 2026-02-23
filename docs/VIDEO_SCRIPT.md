# Roteiro Detalhado do Vídeo de Demonstração (Máx 15 Minutos)

Este roteiro é um **passo a passo exato** do que você deve mostrar na tela e falar durante a gravação do vídeo do Hackathon AgroSolutions. **Vamos usar o Swagger** de cada serviço para demonstrar o funcionamento prático.

---

## 1. Introdução e Desenho da Solução (Entregável 1) - `~3 min`

**📍 Onde você deve estar:** Abra o arquivo `docs/ARCHITECTURE.md` no seu editor de código (VS Code) ou mostre a renderização do diagrama C4 no GitHub.
**🗣️ O que falar:**
> *"Olá, somos o time [Seu Time]. Desenhamos uma arquitetura baseada em **Microsserviços** (.NET 8) para a plataforma AgroSolutions, separando os domínios de Identity, Management, Ingestion e Alert.*
> *Para garantir alta escalabilidade na ingestão de dados dos sensores IoT, utilizamos **RabbitMQ** como mensageria assíncrona. Assim, os dados chegam pela Ingestão e vão para a fila. O serviço de Alerta consome essa fila, avalia as regras e salva no PostgreSQL, protegendo o banco contra gargalos.*
> *Toda a telemetria técnica é servida para o **Prometheus** expor num dashboard **Grafana**."*

---

## 2. Demonstração da Infraestrutura (Entregável 2) - `~2 min`

**📍 Onde você deve estar:** Abra a tela do Docker Desktop (na aba Containers) OU o seu terminal (no caso de Minikube: `kubectl get pods`).
**🗣️ O que falar:**
> *"Nossa aplicação foi containerizada e orquestrada. Aqui podemos ver os containers do PostgreSQL, RabbitMQ, Prometheus, Grafana e os 4 Microsserviços rodando perfeitamente. Toda a infraestrutura sobe automaticamente com scripts predefinidos."*

---

## 3. Demonstração da Esteira de CI/CD (Entregável 3) - `~2 min`

**📍 Onde você deve estar:** Abra o repositório do projeto no GitHub e clique na aba **"Actions"**.
**🗣️ O que falar:**
> *"Para esteira de entrega, configuramos o **GitHub Actions**. Quando fazemos um push para a branch `main`, o pipeline realiza o Build da solução e executa nossos **Testes Unitários em xUnit**, garantindo a qualidade do `FarmService` e outras regras de negócio antes de avançar. Com o teste verde, a pipeline constrói as imagens Docker (CI)."*

---

## 4. Demonstração do MVP Funcionando (Entregável 4) - `~7 min`

Nesta etapa, usaremos as abas do navegador abertas no **Swagger** de cada microsserviço. Mantenha os Swagger's abertos antes de começar a gravar. Todos respondem na rota raiz (ex: `/api/auth/register`).

### Passo 4.1: Cadastro e Autenticação (Identity Service)
**📍 Onde você deve estar:** Aba do Swagger do Identity Service (`http://localhost:5001/swagger`).
1. Vá no endpoint `POST /api/auth/register`. Clique em **Try it out**.
2. **Payload:**
```json
{
  "email": "produtor@agro.com",
  "password": "Password123!"
}
```
3. Clique em **Execute** e mostre que deu status 200/201.
4. Vá no endpoint `POST /api/auth/login`. Cole o mesmo payload:
```json
{
  "email": "produtor@agro.com",
  "password": "Password123!"
}
```
5. Clique em **Execute**. No corpo da resposta (`Response body`), **Copie o texto do Token Jwt gerado**.

### Passo 4.2: Cadastro da Fazenda (Management Service)
**📍 Onde você deve estar:** Aba do Swagger do Management Service (`http://localhost:5002/swagger`).
1. Vá até o topo da tela e clique no botão **Authorize** (Cadeado Verde).
2. Escreva `Bearer ` (com espaço) e cole o Token que você copiou no passo anterior. Clique em **Authorize** e feche.
3. Vá no endpoint `POST /api/farms`.
4. **Payload:**
```json
{
  "name": "Fazenda AgroSolutions",
  "location": "São Paulo"
}
```
5. **Execute**. Mostre o 201 Created. **Copie o `id` da Fazenda gerada**.
6. Vá no endpoint `POST /api/farms/{id}/fields`. Cole o **ID da Fazenda** no campo de parâmetro (`farmId`).
7. **Payload:**
```json
{
  "name": "Talhão de Soja Premium",
  "areaHectares": 25.5,
  "cropType": "Soja"
}
```
8. **Execute**. Mostre o 200 OK. **Copie o `id` do Talhão gerado** (você vai usar ele para o sensor agorinha).

### Passo 4.3: Ingestão e Dashboard Grafana
**📍 Onde você deve estar:** Aba do Swagger do Ingestion Service (`http://localhost:5003/swagger`) e uma aba no Grafana (`http://localhost:3000` > Dashboards > Tech Challenge > AgroSolutions Metrics).
1. No Swagger do Ingestion, vá em `POST /api/telemetry`.
2. **Payload** (Coloque o ID do Talhão copiado):
```json
{
  "fieldId": "COLE-O-ID-DO-TALHAO-AQUI",
  "soilMoisture": 45.0,
  "temperature": 28.5,
  "rainfall": 0.0
}
```
3. Clique em **Execute** várias vezes seguidas (para gerar fluxo).
4. **🗣️ O que falar:** *"A telemetria de umidade normal (45%) entrou rapidamente no broker RabbitMQ com status 202."*
5. **Ação:** Troque de aba para o **Grafana**. Mostre o gráfico de Requisições ("Requests per Second") oscilando, comprovando a observabilidade em tempo real provida pelo Prometheus configurado no C#.

### Passo 4.4: Alerta Inteligente
1. Volte ao Swagger do Ingestion Service (`POST /api/telemetry`).
2. Mude o valor de **soilMoisture** para **15.0** (Crítico).
3. **Payload:**
```json
{
  "fieldId": "COLE-O-ID-DO-TALHAO-AQUI",
  "soilMoisture": 15.0,
  "temperature": 32.0,
  "rainfall": 0.0
}
```
4. Clique em **Execute**.
5. **Ação Rápida:** Troque para a tela do Docker Desktop (ou terminal) e mostre os Logs do container `agro_alert` (Alert Service).
6. **🗣️ O que falar:** *"Processamos uma métrica crítica. No log do Worker do Alert Service, o RabbitMQ consumiu o JSON e logo em seguida printou 'ALERTA DE SECA DETECTADO! Umidade 15%'. Isso gerou o alerta de risco no sistema de forma transparente e performática."*

---

## 5. Encerramento - `~1 min`

- **🗣️ O que falar:** *"Obrigado! Esta arquitetura prova que IoT e monitoramento podem convergir numa aplicação robusta, monitorada, fácil de escalar e plenamente aderente aos princípios de Clean Architecture. Todo o código está no GitHub."*

# Video Script - Pitch (15 min)

## 1. Introdução (2 min)
- **Apresentador**: Olá, nós somos o time [Nome do Time].
- **Problema**: O desperdício de água e a perda de safras por falta de monitoramento em tempo real.
- **Solução**: AgroSolutions, uma plataforma inteligente IoT que conecta o campo à nuvem.

## 2. Arquitetura Técnica (3 min)
- Mostrar diagrama do `ARCHITECTURE.md`.
- Explicar a escolha de Microsserviços para escalabilidade.
- Destacar o uso do RabbitMQ para não perder dados dos sensores.
- Citar Clean Architecture e .NET 8 como base sólida.

## 3. Demonstração Prática (7 min)
1.  **Login**: Mostrar autenticação rápida.
2.  **Cadastro**: Criar "Fazenda Demo" e "Talhão de Teste".
3.  **Simulação**: Rodar script que envia 100 requisições de telemetria.
    - Mostrar Grafana atualizando em tempo real ("Live").
4.  **Alerta**: Enviar dado crítico (Umidade 10%).
    - Mostrar o "Alert Service" processando (logs).
    - Mostrar o painel do Grafana ficando vermelho ou exibindo "Risco de Seca".

## 4. Diferenciais e Observabilidade (2 min)
- Mostrar o Dashboard do Prometheus/Grafana com métricas de saúde da API.
- Reafirmar que a solução é "Cloud Native" (Containers/K8s).

## 5. Conclusão (1 min)
- Resumo dos benefícios (Economia, Previsibilidade).
- Agradecimento.

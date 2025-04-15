# Implementação De Observabilidade E Monitoramento Com SEQ, Prometheus E OpenTelemetry

* Status: proposed
* Deciders: Hernane Beserra
* Date: 2025-04-10

Technical Story: Observabilidade e Monitoramento

## Context and Problem Statement

A arquitetura atual consiste em múltiplos serviços (TransactionService, QueryService, Worker e ApiGateway) que necessitam de uma solução abrangente de observabilidade para:
- Coleta e análise de logs estruturados
- Coleta e visualização de  monitoramento de métricas
- Rastreamento de fluxos de requisições (traces)

## Decision Drivers

* **Observabilidade Completa**: Logs + Métricas + Traces em solução integrada
* **Minimizar Vendor Lock-in**: Preferência por padrões abertos (OpenTelemetry, Prometheus)
* **TCO (Total Cost of Ownership)**: Custos de licença vs esforço de manutenção
* **Fit Arquitetural**: Adequação a arquitetura de microsserviços e cloud-native

## Considered Options

* SEQ
* Grafana
* Dynatrace
* Prometheus
* OpenTelemetry
* Splunk
* Datadog
* Jaeger

## Decision Outcome

Chosen option: "SEQ", because Implementar uma estratégia de observabilidade baseada em três pilares:

1. **Logs Estruturados**:
   - Todos os serviços enviarão logs estruturados para o SEQ
   - Serviços incluídos: TransactionService, QueryService, Worker e ApiGateway

2. **Métricas**:
   - Utilizar OpenTelemetry para coleta de métricas
   - Todos os componentes exporão métricas no formato compatível com Grafana
   - Grafana oferece melhor visualização que Dynatrace para casos de uso customizados
   - Custo mais eficiente que soluções proprietárias como Dynatrace

3. **Distributed Tracing**:
   - Implementar rastreamento distribuído com OpenTelemetry
   - Capturar fluxos completos de requisições entre serviços
   - OpenTelemetry é o padrão emergente para instrumentação
   - Jaeger oferece boa visualização e é compatível com OpenTelemetry
   - Solução open-source com boa comunidade

**Alternativas Consideradas**
1. **ELK Stack (Elasticsearch, Logstash, Kibana)** para logs
   - Rejeitado devido à complexidade de configuração e manutenção

2. **Grafana Loki** para logs
   - Rejeitado por não oferecer vantagens significativas sobre SEQ no contexto atual

3. **Jaeger** para tracing
   - Rejeitado em favor do OpenTelemetry por sua maior interoperabilidade

**Compliance:**
- Atende aos requisitos de observabilidade (logs, métricas, traces)
- Alinhado com princípios cloud-native
- Considera balanceamento entre custo e benefício
  
**Notas:**
Implementação pode ser feita em fases:
1. Primeiro logs e métricas
2. Depois implementar tracing distribuído
3. Avaliar necessidade de ferramentas adicionais para alertas

### Positive Consequences

* Visibilidade unificada do sistema
* Capacidade de detectar e diagnosticar problemas rapidamente
* Monitoramento em tempo real do desempenho do sistema
* Correlação entre logs, métricas e traces
* Stack moderna e amplamente adotada
* Boa integração entre os componentes
* Balanceamento entre custo e funcionalidade

### Negative Consequences

* Sobrecarga adicional nos serviços para coleta de dados
* Curva de aprendizado para as ferramentas adotadas
* Necessidade de manutenção da infraestrutura de observabilidade
* Manutenção do Prometheus (se auto-gerenciado)

## Links

* [Diagrama de Observabilidade e Monitoramento](../../../management/diagrams/observability-diagram.md)
* [Pagina Inicial](../../../README.md)

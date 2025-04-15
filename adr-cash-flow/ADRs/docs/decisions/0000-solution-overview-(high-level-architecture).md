# Solution Overview (High-Level Architecture)

* Status: Proposed
* Deciders: Hernane Beserra
* Date: 2025-04-10

Technical Story: High-Level Architecture

## Context and Problem Statement

Um comerciante precisa controlar seu fluxo de caixa diário com entradas (débitos e créditos) e requer um relatório que forneça o saldo diário consolidado.

## Decision Drivers

* Escalabilidade: a solução deve lidar com o aumento dos volumes de transações.
* Desempenho: Operações de leitura rápida para consultas de equilíbrio.
* Consistência: Garanta a integridade dos dados entre modelos de gravação e leitura.
* Segurança: autenticação e autorização para acesso à API.
* Observabilidade: registro e rastreamento para depuração e monitoramento.

## Considered Options

* Arquitetura monolítica
* Microsserviços com CQRs e fornecimento de eventos
* Abordagem sem servidor

## Decision Outcome

Chosen option: "Microsserviços com CQRs e fornecimento de eventos", because :
- Escalabilidade: os microsserviços permitem escala independente.
- Desempenho: Redis Cache + CQRS otimiza operações pesadas de leitura.
- Consistência: O fornecimento de eventos garante auditabilidade e recuperação.
- Segurança: Validação JWT via API Gateway + Serviço de Identidade.
- Observabilidade: o SEQ fornece logs e traços estruturados.

| Component                | Responsibility                             | Technology                                                    |
| ------------------------ | ------------------------------------------ | ------------------------------------------------------------- |
| API Gateway              | Routes requests, JWT validation            | (e.g., Ocelot, Azure APIM, AWS API Gateway)                   |
| Identity Service         | Generates JWT for authentication           | (e.g., Keycloak, Auth0)                                       |
| Transaction Service      | Handles write operations (debits/credits)  | (e.g., .NET, Spring Boot)                                     |
| Read Service             | Provides consolidated balance queries      | (e.g., .NET, Spring Boot)                                     |
| RabbitMQ                 | Publishes domain events (transaction logs) | (e.g., RabbitMQ, Kafka, Azure ServiceBus, SQS, OCI Streaming) |
| MongoDB (Event Sourcing) | Stores transaction events                  | (e.g., MongoDB, Azure CosmosDB )                              |
| MySQL (Read Model)       | Stores daily balance for fast queries      | (e.g., MySQL, Oracle, SqlServer)                              |
| Redis                    | Caches balance to reduce DB load           | Redis                                                         |
| SEQ                      | Centralized logging and tracing            | (e.g., SEQ, Datadog)                                          |

### Positive Consequences

* Alta disponibilidade e isolamento de falhas.
* Otimizado para ambas as gravações (fornecimento de eventos) e leituras (CQRs).
* Limpe a trilha de auditoria por meio de logs de eventos.

### Negative Consequences

* Maior complexidade no manuseio de eventos.
* Requer eventual gerenciamento de consistência.
* Organização operacional mais alta (múltiplos serviços).

## Pros and Cons of the Options

### Monolithic Architecture

Manipulação de aplicativos únicos todas as transações e relatórios.

* Bom, porque implantação mais simples, nenhuma comunicação entre serviços.
* Ruim, porque difícil de escalar, fortemente acoplado, ponto único de falha.

### Microservices with CQRS & Event Sourcing

Separa os modelos Write (Transações) e Read (Balance).Usa a arquitetura orientada a eventos para consistência.

* Bom, porque a separação escalável, resiliente e clara de preocupações.
* Ruim, porque mais complexo, requer manuseio de eventos.

### Serverless Approach

Event-driven, pay-per-use.

* Bom, porque econômico para baixo tráfego, escala automática.
* Ruim, porque o bloqueio do fornecedor, o frio começa, eventuais desafios de consistência.

## Links

* [Diagrama de Componentes](../../../management/diagrams/componentes-diagram.md)
* [Diagrama de Fluxos](../../../management/diagrams/data-flow-diagram.md)
* [Diagrama de Domínio](../../../management/diagrams/domain-diagram.md)
* [Diagrama de Sequencia](../../../management/diagrams/sequence-diagram.md)
* [Pagina Inicial](../../../README.md)

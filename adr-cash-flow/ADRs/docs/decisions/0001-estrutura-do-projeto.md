# Estrutura Do Projeto

* Status: Proposed
* Deciders: Hernane Beserra
* Date: 2025-04-10

Technical Story: Estrutura do projeto

## Context and Problem Statement

Decomposição do sistema sob construção segregada de responsabilidades

## Decision Drivers

* Desejo dividir o sistema em partes responsáveis ​​por seu domínio
* O sistema deve ser capaz de alterar os componentes sem afetar outras partes do sistema

## Considered Options

* N-Layers
* Hexagonal Architecture
* Onion Architecture
* Clean Architecture
* Vertical Slice Architecture
* CQRS
* CQRS + Clean Architecture

## Decision Outcome

Chosen option: "CQRS + Clean Architecture", because :
- Escalabilidade: os microsserviços permitem escala independente.
- Desempenho: Redis Cache + CQRS otimiza operações pesadas de leitura.
- Consistência: O fornecimento de eventos garante auditabilidade e recuperação.
- Segurança: Validação JWT via API Gateway + Serviço de Identidade.
- Observabilidade: o SEQ fornece logs e traços estruturados.

**Componentes da Solução do Projeto**
| Componente                  | Responsabilidade                                                                                | Tecnologia                                |
| --------------------------- | ----------------------------------------------------------------------------------------------- | ----------------------------------------- |
| BankAccount.Transaction.API | (EntryPoint) Rotas solicitações para transações de depósitos e saques                           | MinimalAPI + Swagger + JWT                |
| BankAccount.Consolidate.API | (EntryPoint) Rotas de consultas rápida para saldos diários e topos extratos                     | MinimalAPI + Swagger + JWT                |
| BankAccount.Subscriber      | (EntryPoint) Consome eventos de transação (ie, depósitos e saques) e mantém no banco de leitura | Consumer                                  |
| BankAccount.Application     | Generates JWT for authentication                                                                | Commands, Queries, Validators, DTOs       |
| BankAccount.Domain          | Handles write operations (debits/credits)                                                       | Aggregates, ValueObjects, Enums           |
| BankAccount.Infrastructure  | Provides consolidated balance queries                                                           | MongoDB, MySQL, MassTransit, Polly        |
| BankAccount.CrossCutting    | Publishes domain events (transaction logs)                                                      | Logging, Security, Redis.Cache            |

### Positive Consequences

* O padrão CQRS traz o conceito de readmodel (consolidado) e writeodel (transações). Além disso, permite que as equipes trabalhem em partes do sistema em paralelo.
* O padrão de arquitetura limpa traz com DDD - conceito de desenvolvimento orientado por domínio. Com isso uma arquitetura mais concêntrica, onde o domínio é a parte mais importante (núcleo). Isso ajuda a implementar o padrão CQRS.
* O padrão de fornecimento de eventos traz a facilidade de reprocessamento de eventos manipulados no sistema CQRS. Se um evento não foi processado em um consumidor devido a qualquer falha, é fácil criar mecanismos para reprocessar esses eventos e, assim, manter o snapshot agregado {domain} atualizado.
* O uso de cache distribuído (Redis) traz desempenho ao sistema nas instâncias de cache de Caso Readmodel.in Caso of Scalability pode ajudar no desempenho do sistema.

### Negative Consequences

* Pode trazer uma maior curva de aprendizado para a primeira instância devido à complexidade da implementação do padrão CQRS, mas à abordagem de um segundo padrão de arquitetura para mitigar esse tempo pode ser usado.
* Os testes manuais no meio do desenvolvimento do sistema (depuração) podem ser vistos como negativos, porque, ao usar o CQRS, usaremos protocolos do Messenger. Com isso, a liberação e2e, ou seja, janelas e readmodel podem apresentar desafios.

## Links

* [Estrutura de pasta e projetos dentro da SolutionProject .NET](..\miscs\estrutura-pasta-do-projeto.md)
* [Pagina Inicial](../../../README.md)

# Estrutura De Pasta Do Projeto
```pgsql
/cash-flow
│
├── Application
|   ├── CommandHandlers
│   ├── Commands
|   ├── EventHandlers
│   ├── Queries
│   ├── QueryHandlers
│   ├── Services
│   ├── DTOs
│   └── Validators
│
├── Domain
│   ├── Abstractions
│   ├── Aggregates
│   ├── Constants
│   ├── Documents
│   ├── Entities
│   ├── Events
│   ├── Exceptions
│   ├── ValueObjects
│   ├── Services
│   └── Enums
│
├── Infrastructure
│   ├── Configuration
│   ├── Persistence (Mongo, MySQL)
│   ├── Messaging (RabbitMQ)
│   ├── Cache (Redis)
│   └── Resilience (Polly)
│
├── CrossCutting.Infrastructure
│   └── Observability (SEQ)
│
├── EntryPoints
│   ├── Transaction.API (MinimalAPI. ie, WriteModel [MongoDB + RabbitMQ])
│   ├── Consolidate.API (MinimalAPI, ie, ReadModel [MySQL + Redis])
│   └── Transaction.Sub (EventConsumer, ie, ReadModel [RabbitMQ + MySql])
```

Organização da estrutura do projeto de solução .net (MVP).

## Decision Outcome
Inicialmente como MVP, iremos criar um projeto com a estrutura do Clean Architecture como o Core da aplicação. Haverá 3 entrypoints usando o mesmo core da aplicação devido ser do mesmo domínio/subdomínio.

Consciente da separação dos entrypoints em repositórios diferentes para manter o IaC e CI/CD de forma atômica.
Já previsto que cada EntryPoint deve ser conteinerizado (ie, criar o Dockerfile em cada um). Ou seja, cada entrypoint deve ser um serviço único e independente dentro da solução arquitetônica. Isto quer dizer, deve se valer as premissas:
- um serviço não pode impactar outro em situação de downtime;
- deve ser capaz de escalar o serviço de forma horizontal e até mesmo se necessário verticalmente (ie, caso este container estiver em uma POD K8s);

## Diagramas
[Componentes da Solução do Projeto .NET](../../../management/diagrams/componentes-solution-project-diagram.md)

[Voltar](../decisions/0001-estrutura-do-projeto.md)
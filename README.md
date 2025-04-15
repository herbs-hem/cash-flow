# cash-flow
Cash flow control system

![Build](https://github.com/herbs-hem/cash-flow/actions/workflows/dotnet.yml/badge.svg?branch=develop)
![Code Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/herbs-hem/f73a416622385d938620d3b03344737d/raw/code-coverage.json)

## Cash Flow
Este projeto de aplicação tem sido criado a partir da iniciativa **_.Net Software Engineer/Architect Practicum Test_**. 

Haverá 3 `EntryPoitns` nesta solução em um futuro próximo, são eles:
1) CashFlow.Transaction.Api (ie, Web Api)
2) CashFlow.Consolidation.Api (ie, Web Api)
3) CashFlow.Transaction.Sub (ie, transaction subscriber - consumer)

Todos eles seguem a abordagem do padrão de arquitetura de software (eg, Clean Architecture) para manter os entrypoints como plug&play.

### _Kestrel (dev-test locally)_
Defina o projeto cash-flow-api como projeto de inicialização. Ou seja, clique com botão direito do mouse sobre o projeto e marque o item `Set as Startup Project` do menu.

- Iniciando a web api service: 
> $\color{green}user@domain:$ $\color{lightblue}../cash-flow/src/VRY.CashFlow.Transaction.Api$ > dotnet run

### _Swagger (Only Development mode)_
``` bash
http://localhost:5000/swagger/index.html
```

## Cash Flow Test
### Unit Tests
Os testes de unidade tem sido implementado afim de cobrir a maior parte do código contra quaisquer questão.
- Rodando o unit tests:
> $\color{green}user@domain:$ $\color{lightblue}../cash-flow/tests/VRY.CashFlow.Tests$ > dotnet test --verbosity normal

### Acceptance Tests
Os testes de aceitação tem sido implementado para cobrir as especificações de cenários obrigatórios e provar que o produto cumpri os requisitos funcionais.
Um report é gerado tão logo executado os testes de aceitação, este pode ser encontrado na pasta `Reports` na pasta raiz da solução.

Requerimentos técnicos para executar estes testes:
- Instale a Doc da SpecFlow: Living Doc:
>  dotnet tool install --global SpecFlow.Plus.LivingDoc.CLI
- Para gerar os relatórios de recursos:
    - Execute os testes de aceitação
        - sem tags
        > $\color{green}user@domain:$ $\color{lightblue}../cash-flow/tests/VRY.CashFlow.AcceptanceTests$ > $\color{orange}dotnet$ test --verbosity normal 
        - com tags (ie, @regressionTests, @successScenarios e assim por diante)
        > $\color{green}user@domain:$ $\color{lightblue}../cash-flow/tests/VRY.CashFlow.AcceptanceTests$ > $\color{orange}dotnet$ test --filter "Category=morning & Category=successScenarios" --verbosity normal 

    - Execute a linha de comando LinVingDoc:
    > $\color{green}user@domain:$ $\color{lightblue}../cash-flow/tests/VRY.CashFlow.AcceptanceTests$ > $\color{orange}livingDoc$ test-assembly .\bin\Debug\net8\VRY.CashFlow.AcceptanceTests.dll -t .\bin\Debug\net8\TestExecution.json -o ..\\..\reports\acceptance-tests.html

_**Remark:**_ _{root} => Este é um espaço reservado para palavras, significa que o local onde você baixou essa solução do projeto._

### _Outstading tasks:_
- Na branch `feature/poc-clean-arch-way` o projeto será segregado respeitando a estrutura do projeto definida pela arquitetura limpa;
- Na branch `feature/acceptance-tests` serão implementados os testes de aceitação;
- Na branch `feature/unit-tests` serão inclusos os demais testes de unidade para obter uma cobertura de 95%;
- Na branch `feature/smoke-tests` serão inclusos os testes usando newman para ser usados na pipeline como parte do Build (CI);
- Na branch `feature/containerization` serão inclusos / ajustados os Dockerfiles e os docker-composes; ie, Incluir os containers do docker para executar localmente; Nesse caso, simulará um container no modo de produção, esse container pode ser usado em Orchestrator de containers. por exemplo:
  - OpenShit - Redhat
  - AKS - Azure
  - OKE - OCI
  - ECS - AWS
  - EKS - AWS
  - GKE - GCP
  - Swarm Docker 
  - Kubernetes 
  - e assim por diante

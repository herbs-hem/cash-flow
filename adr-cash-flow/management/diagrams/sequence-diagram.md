# Proveniência dos dados
title: "Fluxo de Transações: Diagrama de Sequencia" \
author: Hernane Beserra \
date: "2025-04-10"

## Objetivo:
Mostrar a sequência de chamadas entre os componentes para um determinado caso de uso (ex: “Criar uma transação bancária”).

## Target: 
Demonstra entendimento de sincronismo, timing, responsabilidades e acoplamento.

### Diagramas
O diagrama abaixo mostra a sequencia entre os componentes do projeto para executar uma transação bancaria (ie, deposito ou saque).

#### Fluxo de Sequencia da Transação API (eg, deposito ou saque)
![Fluxo de Sequencia da Transação API (eg, deposito ou saque)](../imgs/transaction-api-sequence-diagram.png)

#### Fluxo de Sequencia da Transação Subscriber (eg, deposito ou saque)
![Fluxo de Sequencia da Transação Subscriber (eg, deposito ou saque)](../imgs/transaction-sub-sequence-diagram.png)

#### Fluxo de Sequencia da Consolidação API (eg, saldo e extratos)
![Fluxo de Sequencia da Consolidação API (eg, saldo e extratos)](../imgs/consolidation-api-sequence-diagram.png)


[Voltar](../../ADRs/docs/decisions/0000-solution-overview-(high-level-architecture).md)
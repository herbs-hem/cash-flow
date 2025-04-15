# AD - Architecture Decisions

Este diretório contém ADRs - Registros de decisão da arquitetura para o projeto Cash Flow.

[Solução Arquitetônica Visão Geral -(high-level-architecture)](./ADRs/docs/decisions/0000-solution-overview-(high-level-architecture).md) \
[Estrutura do Projeto de Soluções (.NET)](./ADRs/docs/decisions/0001-estrutura-do-projeto.md) \
[Observabilidade e Monitoramento](./ADRs/docs/decisions/0002-implementação-de-observabilidade-e-monitoramento-com-seq,-prometheus-e-opentelemetry.md)

## Gestão dos diagramas
Todos os diagramas estão alocados dentro de um único arquivo do draw.io (.drawio).

Para contribuir com os diagramas propostos, siga as instruções:
1) Acesse: [Draw.io](https://app.diagrams.net)
2) Abra um diagram pelo menu: Arquivo > Abrir De > Dispositivo
3) Selecione o arquivo disponibilizado nesta documentação: [Arquitetura de Solução](./management/assets/solution-architecture-overview.drawio)

**_Observações:_** 
**IMPORTANTE**
- Todos os diagramas devem ser salvos como imagem (.png) dentro do diretório `./management/imgs`. Estes devem ser referenciados em seus respectivos arquivos markdown (.md).
- Os arquivos markdown que representam os diagramas devem conter uma breve descrição seguindo o modelo proposto e serviram como detalhes dos MADRs - Markdown Architecture Decision Records. 
- For new ADRs, please use [adr-template.md](./templates/adr-template.md) as basis.
- More information on MADR is available at <https://adr.github.io/madr/>.
- General information about architectural decision records is available at <https://adr.github.io/>.

[Voltar](../README.md) 
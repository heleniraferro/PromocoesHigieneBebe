# Spike da Fase 0 (T001–T003)

Programa descartável que testa como coletar as ofertas nas 4 drogarias e grava as respostas reais como fixtures para os testes. Ele respeita o Artigo VII da constituição: poucas páginas, 3 segundos entre requisições, nenhum desvio de bloqueio.

## Como rodar (no seu computador)

```bash
cd tools/PromocoesHigieneBebe.Spike
dotnet build
dotnet run -- instalar-navegador     # só na primeira vez: baixa o Chromium do Playwright
dotnet run -- vtex                   # Drogaria São Paulo, Rosário e Pague Menos (~2 min)
dotnet run -- drogasil               # Drogasil (~1 min)
# ou: dotnet run -- tudo --cep 78005000
```

## O que ele grava

| Onde | O quê |
| --- | --- |
| `tests/PromocoesHigieneBebe.Infrastructure.Tests/Fixtures/<drogaria>/` | Respostas JSON e HTML reais (fixtures) |
| `tools/PromocoesHigieneBebe.Spike/RELATORIO.md` | Status de cada endpoint, quantos produtos vieram, exemplos de nome e preço, regionalização |

### VTEX (T001 e T003)

Para cada loja: consulta a regionalização pelo CEP de Cuiabá (`/api/checkout/pub/regions`) e testa três endpoints de busca na página 1 (Intelligent Search v1, Intelligent Search legado e catálogo legado). Depois busca a página 2 só no primeiro endpoint que trouxe produtos.

### Drogasil (T002)

Abre `/search?w=<termo>&p=<n>` num Chromium headless, grava o HTML depois que a página carrega e guarda as respostas JSON da rede que contêm preço. O relatório lista as URLs dessas respostas, que são candidatas à API interna da página.

## Depois de rodar

1. Confira o `RELATORIO.md`.
2. Faça o commit dos arquivos gerados (`RELATORIO.md` e a pasta `Fixtures`) na branch `001-busca-ofertas-fraldas`.
3. Com isso, as próximas tarefas (T004 a T006) atualizam o `research.md` e o contrato da VTEX com o que foi confirmado.

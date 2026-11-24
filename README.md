# PromocoesHigieneBebe

Comparador de preços de fraldas nas drogarias Drogasil, Drogaria São Paulo, Drogaria Rosário e Pague Menos, ordenado por **preço por unidade**.

O projeto é desenvolvido com **Spec-Driven Development**, no layout do [GitHub Spec Kit](https://github.github.com/spec-kit/index.html), com **DDD**, **Clean Architecture** e **Clean Code**.

## Onde está cada coisa

| Arquivo | Conteúdo |
| --- | --- |
| [`.specify/memory/constitution.md`](.specify/memory/constitution.md) | Regras inegociáveis do projeto |
| [`CLAUDE.md`](CLAUDE.md) | Como o Claude Code deve trabalhar neste repositório |
| [`specs/001-busca-ofertas-fraldas/spec.md`](specs/001-busca-ofertas-fraldas/spec.md) | O quê e por quê do MVP |
| [`specs/001-busca-ofertas-fraldas/plan.md`](specs/001-busca-ofertas-fraldas/plan.md) | Stack, estrutura e desenho técnico |
| [`specs/001-busca-ofertas-fraldas/data-model.md`](specs/001-busca-ofertas-fraldas/data-model.md) | Glossário, contextos, agregados e esquema |
| [`specs/001-busca-ofertas-fraldas/research.md`](specs/001-busca-ofertas-fraldas/research.md) | Decisões e alternativas avaliadas |
| [`specs/001-busca-ofertas-fraldas/contracts/`](specs/001-busca-ofertas-fraldas/contracts/) | Porta de coleta, API VTEX e rotas da Web |
| [`specs/001-busca-ofertas-fraldas/tasks.md`](specs/001-busca-ofertas-fraldas/tasks.md) | 71 tarefas em ordem, com rastreabilidade |
| [`specs/001-busca-ofertas-fraldas/quickstart.md`](specs/001-busca-ofertas-fraldas/quickstart.md) | Como rodar e validar |

## Roteiro de features

| Feature | Conteúdo | Situação |
| --- | --- | --- |
| 001-busca-ofertas-fraldas | Coleta das 4 drogarias, busca por tamanho, filtros, preferências, login com passkey | Spec pronta |
| 002-alertas-de-preco | Histórico de preços e alertas por Telegram, WhatsApp e SMS | A especificar |
| 003-multiusuario-e-categorias | Contas para outras pessoas, CEP por usuário, lenço umedecido e pomada antiassadura | A especificar |

## Fluxo de trabalho com o Claude Code

1. Revise a `spec.md`. Os esclarecimentos da feature 001 já foram resolvidos (coleta diária; preço com condição fora da ordenação).
2. Peça ao Claude Code para executar as tarefas do `tasks.md` na ordem, uma por vez, começando pela Fase 0.
3. Para uma feature nova, crie `specs/00N-nome/spec.md` primeiro, depois o plano e as tarefas.

Opcional: o Specify CLI do Spec Kit pode instalar os comandos `/speckit.*` no Claude Code; os arquivos deste repositório já seguem o layout dele.

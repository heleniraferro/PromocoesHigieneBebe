# CLAUDE.md — FraldasPromo

Comparador pessoal de preços de fraldas (e, no futuro, lenço umedecido e pomada antiassadura) nas drogarias **Drogasil, Drogaria São Paulo, Drogaria Rosário e Pague Menos**, ordenado por **preço por unidade**.

Este projeto segue **Spec-Driven Development**. Antes de qualquer código, leia nesta ordem:

1. `.specify/memory/constitution.md`: regras inegociáveis (DDD, Clean Architecture, Clean Code, TDD, segurança)
2. `specs/<feature>/spec.md`: o quê e por quê
3. `specs/<feature>/plan.md`: como (stack, estrutura, decisões)
4. `specs/<feature>/data-model.md`: linguagem ubíqua, agregados e objetos de valor
5. `specs/<feature>/tasks.md`: a ordem de execução

Feature ativa: **`specs/001-busca-ofertas-fraldas`**

## Como trabalhar aqui

- Implemente **uma tarefa por vez**, na ordem do `tasks.md`. Tarefas marcadas `[P]` podem ser feitas em paralelo.
- Em toda tarefa de código: escreva o teste, veja-o falhar, implemente, veja-o passar e refatore.
- Ao concluir uma tarefa, marque `[X]` no `tasks.md` e faça um commit no padrão Conventional Commits citando o ID (`feat(ofertas): T029 adiciona PrecoPorUnidade`).
- Achou um requisito faltando ou errado? **Pare** e proponha a mudança na `spec.md` antes de codar.
- Nunca crie código que não esteja coberto por uma tarefa.
- Nunca acesse os sites reais das drogarias em testes. Use as fixtures gravadas.

## Comandos

```bash
dotnet build -warnaserror                 # build sem avisos
dotnet test                               # todos os testes
dotnet test tests/FraldasPromo.Domain.Tests
dotnet format --verify-no-changes         # estilo
dotnet list package --vulnerable          # dependências
docker compose -f deploy/docker-compose.yml up -d   # ambiente local
```

## Arquitetura em uma linha

`Web` (Razor Pages + HTMX) e `Worker` (Hangfire) → `Infrastructure` (EF Core/PostgreSQL, coletores VTEX e Playwright) → `Application` (casos de uso e portas) → `Domain` (agregados e objetos de valor, sem framework).

## Regras rápidas

- Domínio em português sem acento; sufixos técnicos em inglês (`OfertaRepository`, `ColetarOfertasHandler`).
- Sem MediatR, sem AutoMapper, sem `DateTime.Now` (use `TimeProvider`), sem `.Result`/`.Wait()`, sem `Html.Raw`.
- `Result` para falha esperada; exceção só para o que é excepcional.
- Objeto de valor em vez de primitivo (`Dinheiro`, `Tamanho`, `Embalagem`, `UrlDeProduto`).
- Logs estruturados, sem dados pessoais.

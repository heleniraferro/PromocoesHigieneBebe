# Tarefas: 001-busca-ofertas-fraldas

**Entrada:** [spec.md](./spec.md), [plan.md](./plan.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [research.md](./research.md)

**Formato:** `- [ ] T### [P] [USn] Descrição — caminho`

- `[P]`: pode rodar em paralelo (arquivos diferentes, sem dependência pendente)
- `[USn]`: história da spec atendida
- Testes vêm **antes** da implementação correspondente e devem falhar primeiro (Artigo V)
- Ao concluir: marcar `[X]` e fazer commit `tipo(escopo): T### descrição`

---

## Fase 0: Viabilidade (bloqueia a História 1)

- [ ] T001 Criar console app de spike que consulta a Intelligent Search v1 e a legada nas lojas VTEX (Drogaria São Paulo, Rosário, Pague Menos) com o termo "fralda G" e salva as respostas JSON — `tools/FraldasPromo.Spike/`
- [ ] T002 No spike, abrir a busca da Drogasil com Playwright, listar as requisições de rede e salvar a resposta que traz os preços — `tools/FraldasPromo.Spike/`
- [ ] T003 Testar a regionalização com um CEP de Cuiabá nas lojas VTEX e na Drogasil e registrar como funciona — `tools/FraldasPromo.Spike/NOTAS.md`
- [ ] T004 Gravar 2 a 3 respostas por drogaria como fixtures — `tests/FraldasPromo.Infrastructure.Tests/Fixtures/<drogaria>/`
- [ ] T005 Montar o arquivo com 50 descrições reais e o tamanho, tipo, unidades e pacotes esperados de cada uma — `tests/FraldasPromo.Domain.Tests/Fixtures/descricoes.csv`
- [ ] T006 Atualizar `research.md` (R2, R3) e `contracts/vtex-intelligent-search.md` com o que foi confirmado e remover as marcações "a validar"

**Ponto de controle:** se alguma drogaria não puder ser coletada sem violar o Artigo VII, atualizar a spec antes de seguir.

---

## Fase 1: Setup

- [ ] T007 Criar a solução e os projetos conforme o plano — `FraldasPromo.slnx`, `src/*`, `tests/*`
- [ ] T008 [P] Configurar `Directory.Build.props`: `net10.0`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`
- [ ] T009 [P] Configurar gerenciamento central de pacotes — `Directory.Packages.props`
- [ ] T010 [P] Criar `.editorconfig` com as convenções C# (namespace por arquivo, `var` quando o tipo é evidente, nomes, ordem de usings)
- [ ] T011 [P] Fixar o SDK — `global.json`
- [ ] T012 [P] Pipeline de CI: restore, `build -warnaserror`, `format --verify-no-changes`, testes com cobertura e `list package --vulnerable` — `.github/workflows/ci.yml`
- [ ] T013 [P] PostgreSQL local sem porta pública em produção (porta só no override de desenvolvimento) — `deploy/docker-compose.yml`, `deploy/docker-compose.override.yml`, `deploy/.env.example`
- [ ] T014 [P] `.gitignore` e `README.md` com visão geral e link para a constituição

---

## Fase 2: Fundação (bloqueia todas as histórias)

- [ ] T015 [P] Testes de arquitetura: `Domain` sem dependências; `Application` só depende de `Domain`; nenhum tipo de `Infrastructure` em `Application` — `tests/FraldasPromo.ArchitectureTests/DependenciasTests.cs`
- [ ] T016 [P] Testes de `Result` e `Error` — `tests/FraldasPromo.Domain.Tests/Compartilhado/ResultTests.cs`
- [ ] T017 Implementar `Result`, `Result<T>` e `Error` — `src/FraldasPromo.Domain/Compartilhado/`
- [ ] T018 [P] Implementar `Entity<TId>`, `AggregateRoot<TId>` (lista de eventos), `ValueObject` e `IDomainEvent` — `src/FraldasPromo.Domain/Compartilhado/`
- [ ] T019 Criar `FraldasDbContext` com schemas `ofertas` e `preferencias` e nomes em snake_case — `src/FraldasPromo.Infrastructure/Persistencia/`
- [ ] T020 [P] Fixture do Testcontainers com PostgreSQL e aplicação das migrations — `tests/FraldasPromo.Infrastructure.Tests/PostgresFixture.cs`
- [ ] T021 Registrar dependências de `Application` e `Infrastructure` e `TimeProvider.System` — `*/DependencyInjection.cs`

**Ponto de controle:** build sem avisos, CI verde, testes de arquitetura passando.

---

## Fase 3: História 1 — Coleta das ofertas (P1) 🎯 MVP

**Objetivo:** coletar, interpretar e gravar as ofertas das 4 drogarias.
**Teste independente:** coleta a partir das fixtures grava os produtos e cotações esperados.

### Testes (escrever primeiro)

- [ ] T022 [P] [US1] Testes de `Tamanho`: interpretação, sinônimos (`EG`, `EXG`, `RN Plus`), nenhum ou dois tamanhos — `tests/FraldasPromo.Domain.Tests/Ofertas/TamanhoTests.cs`
- [ ] T023 [P] [US1] Testes de `Dinheiro`, `Embalagem` e `PrecoPorUnidade` (kit 3 × 60 por R$ 180,00 = R$ 1,00; faixa plausível) — `tests/FraldasPromo.Domain.Tests/Ofertas/ObjetosDeValorTests.cs`
- [ ] T024 [P] [US1] Testes de `UrlDeProduto`: aceita o domínio da drogaria; rejeita outro host e `http` — `tests/FraldasPromo.Domain.Tests/Ofertas/UrlDeProdutoTests.cs`
- [ ] T025 [P] [US1] Testes de `Marca` e `Linha`: normalização e marcas próprias — `tests/FraldasPromo.Domain.Tests/Ofertas/MarcaTests.cs`
- [ ] T026 [P] [US1] Testes do `InterpretadorDeDescricao` com os cenários da spec e o `descricoes.csv` (≥ 95% corretos, nenhum errado) — `tests/FraldasPromo.Domain.Tests/Ofertas/InterpretadorDeDescricaoTests.cs`
- [ ] T027 [P] [US1] Testes dos agregados `Produto`, `Cotacao` e `Coleta` (invariantes e transições de situação) — `tests/FraldasPromo.Domain.Tests/Ofertas/AgregadosTests.cs`

### Domínio

- [ ] T028 [P] [US1] Enumerações inteligentes `Drogaria` e `Tamanho` — `src/FraldasPromo.Domain/Ofertas/`
- [ ] T029 [P] [US1] Objetos de valor `Dinheiro`, `Embalagem`, `PrecoPorUnidade`, `Sku`, `Marca`, `Linha`, `CondicaoComercial`, `UrlDeProduto` — `src/FraldasPromo.Domain/Ofertas/`
- [ ] T030 [US1] `InterpretadorDeDescricao` e `MotivoDescarte` (depende de T028 e T029) — `src/FraldasPromo.Domain/Ofertas/`
- [ ] T031 [US1] Agregados `Produto`, `Cotacao` (evento `CotacaoRegistrada`) e `Coleta` — `src/FraldasPromo.Domain/Ofertas/`

### Aplicação

- [ ] T032 [US1] Portas `IColetorDeOfertas`, `OfertaBruta`, `IProdutoRepository`, `ICotacaoRepository`, `IColetaRepository`, `IUnitOfWork` e `ColetaBloqueadaException` — `src/FraldasPromo.Application/Abstractions/`
- [ ] T033 [US1] Testes do `ColetarOfertasHandler` com dublês: aceite, descarte com motivo, duplicado, falha e bloqueio — `tests/FraldasPromo.Application.Tests/Ofertas/ColetarOfertasHandlerTests.cs`
- [ ] T034 [US1] `ColetarOfertasCommand` e `ColetarOfertasHandler` — `src/FraldasPromo.Application/Ofertas/Coletar/`

### Infraestrutura

- [ ] T035 [P] [US1] Configurações EF de `Produto`, `Cotacao` e `Coleta` e migration inicial — `src/FraldasPromo.Infrastructure/Persistencia/Configurations/`
- [ ] T036 [US1] Repositórios e unidade de trabalho, com testes de integração no PostgreSQL — `src/FraldasPromo.Infrastructure/Persistencia/Repositories/`, `tests/FraldasPromo.Infrastructure.Tests/Persistencia/`
- [ ] T037 [P] [US1] Testes do `VtexColetor` com fixtures: paginação, produto com vários itens, URL relativa, 403 repetido vira bloqueio — `tests/FraldasPromo.Infrastructure.Tests/Coleta/VtexColetorTests.cs`
- [ ] T038 [US1] `VtexColetor`, `VtexOptions` e políticas de resiliência — `src/FraldasPromo.Infrastructure/Coleta/Vtex/`
- [ ] T039 [P] [US1] Testes do `DrogasilColetor` com páginas servidas localmente a partir das fixtures — `tests/FraldasPromo.Infrastructure.Tests/Coleta/DrogasilColetorTests.cs`
- [ ] T040 [US1] `DrogasilColetor` com Playwright — `src/FraldasPromo.Infrastructure/Coleta/Drogasil/`
- [ ] T041 [US1] Host `Worker` com Hangfire no PostgreSQL: um job diário por drogaria (cron no fuso de Cuiabá, com defasagem entre drogarias), `DisableConcurrentExecution` e `AutomaticRetry` — `src/FraldasPromo.Worker/`
- [ ] T042 [US1] Dockerfile do worker sobre a imagem oficial do Playwright para .NET, com usuário não root — `src/FraldasPromo.Worker/Dockerfile`
- [ ] T043 [US1] Teste de ponta a ponta: a coleta das 4 fixtures grava os produtos e cotações esperados — `tests/FraldasPromo.Infrastructure.Tests/Coleta/ColetaPontaAPontaTests.cs`

**Ponto de controle:** História 1 funciona e é testável sozinha.

---

## Fase 4: História 2 — Ver as ofertas mais baratas por tamanho (P1) 🎯 MVP

**Objetivo:** lista única ordenada por preço por fralda.
**Teste independente:** com ofertas pré-cadastradas, `/ofertas?tamanho=G` devolve a lista correta.

- [ ] T044 [P] [US2] Testes do `BuscarOfertasHandler`: validação, três ordenações e indisponíveis no fim — `tests/FraldasPromo.Application.Tests/Ofertas/BuscarOfertasHandlerTests.cs`
- [ ] T045 [US2] `BuscarOfertasQuery`, `OfertaDto`, `Ordenacao`, `IConsultaDeOfertas` e `BuscarOfertasHandler` — `src/FraldasPromo.Application/Ofertas/Buscar/`
- [ ] T046 [US2] View `ofertas.ofertas_atuais` (migration) e `ConsultaDeOfertas` com projeção, com testes de integração para "só o tamanho escolhido" (SC-005) e preço por unidade (SC-004) — `src/FraldasPromo.Infrastructure/Persistencia/`
- [ ] T047 [US2] Projeto Web: layout, HTMX servido localmente, cabeçalhos de segurança e CSP — `src/FraldasPromo.Web/`
- [ ] T048 [US2] Página inicial com chips de tamanho e fragmento `_ListaDeOfertas` na rota `/ofertas` — `src/FraldasPromo.Web/Pages/`
- [ ] T049 [P] [US2] Testes da Web: lista ordenada, descrição com HTML malicioso sai codificada, links com `noopener` — `tests/FraldasPromo.Web.Tests/OfertasTests.cs`
- [ ] T050 [US2] Manifest PWA e ícones — `src/FraldasPromo.Web/wwwroot/`

**Ponto de controle:** Histórias 1 e 2 entregam o MVP mínimo localmente.

---

## Fase 5: História 3 — Filtros e preferências (P2)

- [ ] T051 [P] [US3] Testes de `PreferenciaDeBusca`: padrão com todas as marcas e tipo "ambos"; marcas não podem ficar vazias — `tests/FraldasPromo.Domain.Tests/Preferencias/PreferenciaDeBuscaTests.cs`
- [ ] T052 [US3] `PreferenciaDeBusca` e `FiltroTipo` — `src/FraldasPromo.Domain/Preferencias/`
- [ ] T053 [P] [US3] Testes de `ObterPreferenciaHandler` e `SalvarPreferenciaHandler` — `tests/FraldasPromo.Application.Tests/Preferencias/`
- [ ] T054 [US3] Handlers, repositório, configuração EF no schema `preferencias` e migration — `src/FraldasPromo.Application/Preferencias/`, `src/FraldasPromo.Infrastructure/Persistencia/`
- [ ] T055 [US3] Filtros de marca, tipo e linha na tela; `POST /preferencias` via HTMX com antiforgery — `src/FraldasPromo.Web/Pages/`
- [ ] T056 [P] [US3] Testes da Web: filtros aplicados, preferência restaurada e POST sem token devolve 400 — `tests/FraldasPromo.Web.Tests/PreferenciasTests.cs`

**Ponto de controle:** História 3 funciona sem quebrar as anteriores.

---

## Fase 6: História 4 — Confiar no dado exibido (P2)

- [ ] T057 [P] [US4] Testes de `ObterSituacaoDasDrogariasHandler`: idade do dado, "n de 4" e falhas nomeadas — `tests/FraldasPromo.Application.Tests/Ofertas/SituacaoTests.cs`
- [ ] T058 [US4] `ObterSituacaoDasDrogariasHandler` e consulta — `src/FraldasPromo.Application/Ofertas/Situacao/`
- [ ] T059 [US4] Fragmento `_SituacaoDasLojas` e preço com condição em linha separada — `src/FraldasPromo.Web/Pages/Shared/`
- [ ] T060 [P] [US4] Testes da Web: aviso de loja fora, idade acima de 30 h em destaque e condição separada — `tests/FraldasPromo.Web.Tests/SituacaoTests.cs`

**Ponto de controle:** História 4 funciona sem quebrar as anteriores.

---

## Fase 7: História 5 — Acesso protegido (P3)

- [ ] T061 [US5] ASP.NET Core Identity no schema `identidade`, cookie seguro, bloqueio após 5 tentativas e cadastro fechado — `src/FraldasPromo.Web/`
- [ ] T062 [US5] Passkeys (.NET 10) e 2FA; páginas `Conta/Entrar` e `Conta/Passkeys` — `src/FraldasPromo.Web/Pages/Conta/`
- [ ] T063 [US5] Comando para criar o usuário único, sem senha no repositório — `src/FraldasPromo.Web/Comandos/CriarUsuario.cs`
- [ ] T064 [US5] Política de autorização global (`FallbackPolicy`) e limite de requisições no login — `src/FraldasPromo.Web/Program.cs`
- [ ] T065 [P] [US5] Testes da Web: anônimo redirecionado, cadastro bloqueado, bloqueio de conta e limite de requisições — `tests/FraldasPromo.Web.Tests/AcessoTests.cs`

**Ponto de controle:** MVP publicável.

---

## Fase 8: Acabamento e deploy

- [ ] T066 [P] Dockerfile da web com usuário não root — `src/FraldasPromo.Web/Dockerfile`
- [ ] T067 Caddyfile com HTTPS e compose de produção com contas de banco separadas (script de criação de papéis) — `deploy/Caddyfile`, `deploy/docker-compose.yml`, `deploy/postgres/init.sql`
- [ ] T068 [P] Revisar logs estruturados e garantir que nenhum contém dado pessoal — `src/**`
- [ ] T069 [P] Dependabot e secret scanning — `.github/dependabot.yml`
- [ ] T070 Executar o `quickstart.md` inteiro e o checklist de segurança
- [ ] T071 Medir SC-001 (lista em até 2 s) e SC-006 (coleta em até 10 min) e registrar os resultados — `specs/001-busca-ofertas-fraldas/quickstart.md`

---

## Dependências e ordem

- Fase 0 → Fase 1 → Fase 2 → Histórias.
- História 1 e História 2 podem andar em paralelo depois da Fase 2: a História 2 usa dados pré-cadastrados nos testes.
- Histórias 3 e 4 dependem da História 2 (tela). História 5 depende da Fase 2 e da Web criada em T047.
- Dentro de cada história: testes → domínio → aplicação → infraestrutura → web.

### Exemplo de paralelismo (História 1)

```text
T022, T023, T024, T025, T026, T027  (testes de domínio, arquivos diferentes)
T028, T029                          (domínio, depois dos testes)
T035, T037, T039                    (EF e testes dos coletores)
```

## Rastreabilidade

| Requisito | Tarefas |
| --- | --- |
| FR-001 | T037–T043 |
| FR-002 | T041 |
| FR-003 | T003, T038, T040 |
| FR-004 | T026, T030 |
| FR-005 | T022, T028 |
| FR-006 | T025, T029, T030 |
| FR-007 | T026, T030, T034 |
| FR-008 | T033, T034 |
| FR-009 | T031, T037 |
| FR-010 | T024, T029 |
| FR-011 | T027, T031, T034 |
| FR-012 | T033, T041 |
| FR-013 | T048 |
| FR-014 | T023, T029, T046 |
| FR-015 | T044, T046 |
| FR-016 | T044, T045 |
| FR-017 | T048, T059 |
| FR-018 | T044 |
| FR-019 | T057–T059 |
| FR-020 | T046 |
| FR-021 | T055, T056 |
| FR-022 | T051, T052 |
| FR-023 | T053–T055 |
| FR-024 | T064, T065 |
| FR-025 | T062 |
| FR-026 | T061, T065 |
| FR-027 | T061, T064, T065 |
| SC-001, SC-006 | T071 |
| SC-002 | T055, T070 |
| SC-003 | T005, T026 |
| SC-004 | T023, T046 |
| SC-005 | T046 |
| SC-007 | T033, T041 |

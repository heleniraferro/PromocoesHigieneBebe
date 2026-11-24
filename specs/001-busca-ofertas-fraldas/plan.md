# Plano de implementação: 001-busca-ofertas-fraldas

**Branch:** `001-busca-ofertas-fraldas` · **Data:** 2026-11-24 · **Spec:** [spec.md](./spec.md)

## Resumo

Monólito modular em .NET 10 com dois hosts. O `Worker` coleta as ofertas das 4 drogarias com Hangfire: VTEX Intelligent Search para Drogaria São Paulo, Rosário e Pague Menos, e Playwright para a Drogasil. Ele interpreta as descrições no domínio e grava produtos e cotações no PostgreSQL. A `Web` (Razor Pages + HTMX, Identity com passkey) consulta a view de ofertas atuais e mostra a lista ordenada por preço por unidade.

## Contexto técnico

| Item | Escolha |
| --- | --- |
| Linguagem e runtime | C# 14 / .NET 10 (LTS) |
| Web | ASP.NET Core Razor Pages + HTMX 2, PWA (manifest + service worker só para instalação) |
| Autenticação | ASP.NET Core Identity com passkeys e 2FA |
| Persistência | PostgreSQL + EF Core 10 (Npgsql), migrations versionadas |
| Agendamento | Hangfire + Hangfire.PostgreSql |
| Coleta | `HttpClient` tipado + `System.Text.Json` (VTEX); Microsoft.Playwright (Drogasil) |
| Resiliência | `Microsoft.Extensions.Http.Resilience` (retry com backoff, timeout, circuit breaker) |
| Testes | xUnit v3, Shouldly, NSubstitute, Testcontainers.PostgreSql, `WebApplicationFactory`, NetArchTest.Rules |
| Qualidade | `.editorconfig`, analisadores .NET (`recommended`), `TreatWarningsAsErrors`, `dotnet format`, cobertura com coverlet |
| Infra | Docker Compose: `caddy`, `web`, `worker`, `postgres`; GitHub Actions para CI |
| Plataforma alvo | Linux x64 em VPS; navegador móvel como cliente principal |
| Metas de desempenho | Lista em até 2 s (SC-001); coleta das 4 drogarias em até 10 min (SC-006) |
| Restrições | Coleta mínima de 1 h por drogaria; no máximo 2 requisições simultâneas por site |
| Escala | 1 usuário nesta feature; alguns milhares de produtos e cotações por dia |

## Verificação da constituição

| Artigo | Como o plano atende | OK |
| --- | --- | --- |
| I. Spec antes de código | spec, plan e tasks prontos; tarefas citam FR e US | Sim |
| II. DDD | Contextos `Ofertas` e `Preferencias`; agregados `Produto`, `Cotacao`, `Coleta`, `PreferenciaDeBusca`; objetos de valor em `data-model.md` | Sim |
| III. Clean Architecture | 4 camadas + 2 hosts; testes de arquitetura | Sim |
| IV. Clean Code | `.editorconfig`, analisadores, warnings como erro, `Result`, `TimeProvider` | Sim |
| V. Testes primeiro | Tarefas de teste antes de cada implementação; fixtures gravadas | Sim |
| VI. Segurança | `UrlDeProduto` com lista de domínios, CSP, antiforgery, passkey, contas de banco separadas, coletor sem root | Sim |
| VII. Coleta responsável | API VTEX preferida; Playwright só na Drogasil; limites de frequência e concorrência; parar ao ser bloqueado | Sim |
| VIII. Simplicidade | Sem MediatR/AutoMapper; um monólito; dependências justificadas em `research.md` | Sim |
| IX. Observabilidade | Agregado `Coleta` com contagens e motivos; logs estruturados | Sim |

## Estrutura do repositório

```text
PromocoesHigieneBebe/
├── .specify/memory/constitution.md
├── specs/001-busca-ofertas-fraldas/
│   ├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
│   └── contracts/
├── src/
│   ├── PromocoesHigieneBebe.Domain/
│   │   ├── Compartilhado/            # Entity, AggregateRoot, ValueObject, Result, Error, IDomainEvent
│   │   ├── Ofertas/                  # Drogaria, Tamanho, Dinheiro, Embalagem, PrecoPorUnidade, Marca, Linha,
│   │   │                             # UrlDeProduto, Sku, CondicaoComercial, Produto, Cotacao, Coleta,
│   │   │                             # InterpretadorDeDescricao, MotivoDescarte, eventos
│   │   └── Preferencias/             # PreferenciaDeBusca, FiltroTipo
│   ├── PromocoesHigieneBebe.Application/
│   │   ├── Abstractions/             # IColetorDeOfertas, IProdutoRepository, ICotacaoRepository,
│   │   │                             # IColetaRepository, IConsultaDeOfertas, IUnitOfWork
│   │   ├── Ofertas/Coletar/          # ColetarOfertasCommand, ColetarOfertasHandler
│   │   ├── Ofertas/Buscar/           # BuscarOfertasQuery, BuscarOfertasHandler, OfertaDto, Ordenacao
│   │   ├── Ofertas/Situacao/         # ObterSituacaoDasDrogariasHandler
│   │   ├── Preferencias/             # ObterPreferenciaHandler, SalvarPreferenciaHandler
│   │   └── DependencyInjection.cs
│   ├── PromocoesHigieneBebe.Infrastructure/
│   │   ├── Persistencia/             # FraldasDbContext, Configurations/, Migrations/, Repositories/, ConsultaDeOfertas
│   │   ├── Coleta/Vtex/              # VtexColetor, VtexOptions, modelos JSON
│   │   ├── Coleta/Drogasil/          # DrogasilColetor (Playwright), modelos JSON
│   │   ├── Coleta/Configuracao/      # MarcasConhecidas, MarcasProprias, limites
│   │   └── DependencyInjection.cs
│   ├── PromocoesHigieneBebe.Web/             # Razor Pages, Identity, HTMX, PWA, cabeçalhos de segurança
│   └── PromocoesHigieneBebe.Worker/          # Host do Hangfire, jobs recorrentes por drogaria
├── tests/
│   ├── PromocoesHigieneBebe.Domain.Tests/
│   ├── PromocoesHigieneBebe.Application.Tests/
│   ├── PromocoesHigieneBebe.Infrastructure.Tests/   # Testcontainers + Fixtures/ (respostas gravadas)
│   ├── PromocoesHigieneBebe.Web.Tests/              # WebApplicationFactory
│   └── PromocoesHigieneBebe.ArchitectureTests/      # NetArchTest
├── tools/PromocoesHigieneBebe.Spike/          # Fase 0: console app de teste de coleta (descartável)
├── deploy/                            # docker-compose.yml, Caddyfile, .env.example
├── .github/workflows/ci.yml
├── .editorconfig · Directory.Build.props · Directory.Packages.props · global.json
├── PromocoesHigieneBebe.slnx
└── CLAUDE.md · README.md
```

**Regras de dependência (verificadas por teste):**

- `Domain` não referencia nenhum outro projeto nem pacote de infraestrutura.
- `Application` referencia só `Domain`.
- `Infrastructure` referencia `Application` e `Domain`.
- `Web` e `Worker` referenciam `Infrastructure` e `Application`, só para registrar dependências e chamar handlers.
- Nenhum tipo de `Infrastructure` aparece em assinatura pública de `Application`.

## Desenho das peças principais

### Porta de coleta

```csharp
public interface IColetorDeOfertas
{
    Drogaria Drogaria { get; }
    IAsyncEnumerable<OfertaBruta> ColetarAsync(string termo, CancellationToken cancellationToken);
}
```

Detalhes em `contracts/porta-coletor.md`.

### Fluxo de coleta (`ColetarOfertasHandler`)

1. Cria `Coleta` em andamento para a drogaria.
2. Para cada termo configurado (`fralda`, `fralda pants`), percorre `IColetorDeOfertas.ColetarAsync`.
3. Para cada `OfertaBruta`, aplica o `InterpretadorDeDescricao`. Se for descarte, chama `Coleta.RegistrarDescarte(motivo)`. Se for aceite, faz upsert do `Produto` por (drogaria, SKU), registra a `Cotacao` e chama `Coleta.RegistrarAceite()`.
4. Elimina duplicados por SKU dentro da mesma coleta.
5. Conclui a coleta e salva tudo numa unidade de trabalho por lote de 50 itens.
6. Em caso de `ColetaBloqueadaException` (403 repetido ou captcha), chama `MarcarBloqueada`. Em outra falha, chama `Falhar` (FR-012: o job das outras drogarias é independente).

### Fluxo de busca (`BuscarOfertasHandler`)

1. Valida a `BuscarOfertasQuery` (tamanho obrigatório, pelo menos uma marca).
2. `IConsultaDeOfertas` projeta direto da view `ofertas.ofertas_atuais` para `OfertaDto`, já filtrando e ordenando no banco.
3. Devolve as ofertas e a `SituacaoDasDrogariasDto`.

### Web

- Páginas: `/` (busca + resultados), `/Conta/Entrar`, `/Conta/Passkeys`.
- HTMX: o formulário de filtros faz `hx-get="/ofertas"` e troca só o fragmento `_ListaDeOfertas`. O token antiforgery vai no cabeçalho `RequestVerificationToken` para requisições que alteram estado (salvar preferência).
- Cabeçalhos: CSP `default-src 'self'; img-src 'self' https://<CDNs das drogarias>; script-src 'self'; style-src 'self'; frame-ancestors 'none'`, HSTS, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`.
- Limite de requisições: `AddRateLimiter` com janela fixa por usuário (60 por minuto) e política específica no login.
- Links externos com `rel="noopener noreferrer"` e `target="_blank"`.

### Worker

- Um job recorrente diário por drogaria (`coleta-drogasil`, `coleta-drogaria-sao-paulo`...), pela expressão cron de `Coleta:Cron` no fuso `America/Cuiaba` (FR-002). As 4 drogarias começam com 10 minutos de diferença entre si (`Coleta:DefasagemEntreDrogarias`), para não concentrar carga.
- `DisableConcurrentExecution` por drogaria; `AutomaticRetry(Attempts = 2)`.
- O container roda como usuário não root, com a imagem base oficial do Playwright para .NET.

## Configuração

```jsonc
// appsettings.json (sem segredos)
{
  "Coleta": {
    "Cron": "0 5 * * *",                 // uma vez por dia, 05:00
    "FusoHorario": "America/Cuiaba",
    "DefasagemEntreDrogarias": "00:10:00",
    "DadoVelhoApos": "30:00:00",         // destaque na tela (História 4)
    "CepPadrao": "78000000",            // CEP de Cuiabá; ajustar na Fase 0
    "Termos": [ "fralda", "fralda pants" ],
    "MaximoDePaginas": 10,
    "RequisicoesSimultaneasPorSite": 2,
    "PrecoPorUnidadeMinimo": 0.20,
    "PrecoPorUnidadeMaximo": 10.00
  },
  "Drogarias": {
    "DrogariaSaoPaulo": { "BaseUrl": "https://www.drogariasaopaulo.com.br" },
    "DrogariaRosario":  { "BaseUrl": "https://www.drogariarosario.com.br" },
    "PagueMenos":       { "BaseUrl": "https://www.paguemenos.com.br" },
    "Drogasil":         { "BaseUrl": "https://www.drogasil.com.br" }
  }
}
```

Segredos (`ConnectionStrings__Web`, `ConnectionStrings__Worker`) chegam por variável de ambiente ou Docker secrets.

## Fases

| Fase | Objetivo | Saída |
| --- | --- | --- |
| 0. Viabilidade | Console app que coleta "fralda G" nas 4 drogarias | Respostas gravadas como fixtures; decisão sobre API v1/legada e regionalização; ajuste do `research.md` |
| 1. Fundação | Solução, padrões de código, CI, blocos de domínio compartilhados | Build e CI verdes, testes de arquitetura |
| 2. História 1 | Coleta completa | Produtos e cotações gravados a partir das fixtures |
| 3. História 2 | Lista ordenada | Busca por tamanho funcionando localmente |
| 4. Histórias 3 e 4 | Filtros, preferências, situação das lojas | MVP completo sem login |
| 5. História 5 | Login com passkey | MVP publicável |
| 6. Acabamento | Deploy, segurança, quickstart | Publicado na VPS |

## Complexity Tracking

| Exceção à constituição | Por que é necessária | Alternativa mais simples rejeitada porque |
| --- | --- | --- |
| Dois hosts (`Web` e `Worker`) em vez de um | Isolar o Chromium e a coleta da aplicação exposta à internet (Artigo VI) | Um host só colocaria o navegador automatizado no mesmo processo da Web |
| Playwright como dependência pesada | A Drogasil não entrega preço sem JavaScript | Parar de cobrir a Drogasil reduziria o valor do produto |

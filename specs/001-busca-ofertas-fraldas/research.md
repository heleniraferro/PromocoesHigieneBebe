# Pesquisa e decisões: 001-busca-ofertas-fraldas

Cada decisão segue o formato: **Decisão**, **Por quê**, **Alternativas avaliadas**. As comparações completas por segurança e usabilidade estão na aba "Estudo de arquitetura" do documento de escopo.

## R1. Como cada drogaria expõe a busca (verificado em 2026-11-24)

| Drogaria | Plataforma | Busca no site | Preço no HTML inicial |
| --- | --- | --- | --- |
| Drogasil | Própria (Next.js, grupo RD) | `/search?w=<termo>&p=<página>` | Não, carregado por JavaScript |
| Drogaria São Paulo | VTEX (CMS antigo) | `/pesquisa?q=<termo>` | Não |
| Drogaria Rosário | VTEX IO | `/<termo>?map=ft` | Não |
| Pague Menos | VTEX IO | `/<termo>` (a confirmar) | Não |

- **Decisão:** um coletor genérico VTEX para 3 drogarias e um coletor específico para a Drogasil.
- **Por quê:** três drogarias compartilham a mesma plataforma; um adaptador atende as três.
- **Pendência (Fase 0):** a chamada direta à API VTEX foi recusada a partir do ambiente de pesquisa. É preciso validar de um computador ou servidor comum, com cabeçalhos de navegador.

## R2. Coleta nas lojas VTEX

- **Decisão:** usar a Intelligent Search API da VTEX, preferindo a versão v1 (`/api/intelligent-search/v1/...`) e com recurso à versão legada (`/api/io/_v/api/intelligent-search/product_search/...`) se a v1 não estiver ativa na loja.
- **Por quê:** a API é pública, não exige credencial e devolve JSON com preço, preço anterior, disponibilidade e link. Em julho de 2026 a VTEX lançou a v1, que substitui a legada, com cache HTTP e regionalização explícita.
- **Alternativas:** API de catálogo legada (`/api/catalog_system/pub/products/search`), mais antiga e menos rica; Playwright, lento e mais arriscado.
- **Regionalização:** testar na Fase 0 o parâmetro de regionalização da v1 (ou o cookie de segmento da legada) com um CEP de Cuiabá.

## R3. Coleta na Drogasil

- **Decisão:** Playwright for .NET com Chromium headless, interceptando a resposta JSON que a própria página usa para preencher os preços. Ler o DOM só como recurso.
- **Por quê:** o HTML inicial mostra "Carregando..." no lugar do preço. Ler a resposta de rede é mais estável que seletores CSS.
- **Alternativas:** chamar direto a API interna (pode ser possível; investigar na Fase 0, porque eliminaria o navegador); serviço pago de raspagem (descartado: contorna bloqueios, o que viola o Artigo VII).

## R4. Front-end

- **Decisão:** ASP.NET Core Razor Pages + HTMX, renderizado no servidor, instalável como PWA.
- **Por quê:** abre rápido no 4G; sessão por cookie `HttpOnly` sem token no navegador; antiforgery e codificação de HTML nativos; um projeto só em C#.
- **Alternativas:** Blazor WebAssembly (download inicial pesado e API exposta com token), Blazor Server (exige conexão contínua, ruim em rede móvel), React/Next.js (segunda stack).

## R5. Autenticação

- **Decisão:** ASP.NET Core Identity no .NET 10, com passkey como método principal e e-mail + senha + 2FA como alternativa. Cadastro fechado.
- **Por quê:** o .NET 10 traz suporte nativo a cadastro e login por passkey no Identity; é resistente a phishing e é o login mais rápido para o usuário.
- **Alternativas:** login com Google (fica para a feature de multiusuário); provedores gerenciados (Auth0, Entra External ID, Keycloak), exagerados para o tamanho atual.

## R6. Persistência

- **Decisão:** PostgreSQL com EF Core e Npgsql, um schema por contexto delimitado.
- **Por quê:** gratuito, roda em container, suporta `jsonb` e índices adequados para a view de ofertas atuais.
- **Alternativas:** SQL Server Express (imagem pesada, limite de 10 GB); SQLite (só para experimentos da Fase 0).

## R7. Agendamento

- **Decisão:** Hangfire com armazenamento no PostgreSQL (`Hangfire.PostgreSql`), hospedado no `Worker`. Um job recorrente por drogaria. O painel fica desligado em produção ou restrito a administrador autenticado.
- **Por quê:** novas tentativas automáticas, histórico de execução e um job por drogaria isolam falhas (FR-012).
- **Alternativas:** Quartz.NET (sem painel); `BackgroundService` com `PeriodicTimer` (sem nova tentativa).

## R8. Testes

- **Decisão:** xUnit v3, Shouldly, NSubstitute, Testcontainers para PostgreSQL, `WebApplicationFactory` para a Web e NetArchTest.Rules para regras de arquitetura.
- **Por quê:** padrão do ecossistema .NET; Shouldly no lugar de FluentAssertions porque a versão 8 do FluentAssertions passou a ter licença comercial.
- **Fixtures:** respostas reais gravadas na Fase 0 (JSON da VTEX, JSON/HTML da Drogasil), versionadas em `tests/FraldasPromo.Infrastructure.Tests/Fixtures`.

## R9. Sem MediatR e sem AutoMapper

- **Decisão:** handlers chamados diretamente por injeção de dependência e mapeamento manual.
- **Por quê:** menos mágica e menos dependências (Artigo VIII); os dois pacotes passaram a ter licença comercial em 2025.

## R10. Hospedagem

- **Decisão:** VPS com Docker Compose: `caddy` (proxy HTTPS), `web`, `worker` e `postgres`. Só o `caddy` publica portas (443 e 80 para redirecionar).
- **Por quê:** custo fixo baixo e Chromium do Playwright sem restrições. Detalhes de segurança na aba "Estudo de arquitetura".

## Fontes

- Microsoft Learn: [Passkeys no ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/passkeys)
- VTEX: [Intelligent Search API v1 (nota de lançamento)](https://developers.vtex.com/updates/release-notes/2026-07-08-new-intelligent-search-api-v1) e [Catálogo headless](https://developers.vtex.com/docs/guides/headless-catalog)
- GitHub Spec Kit: [documentação](https://github.github.com/spec-kit/index.html)

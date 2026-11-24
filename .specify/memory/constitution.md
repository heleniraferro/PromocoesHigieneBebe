# Constituição do PromocoesHigieneBebe

**Versão:** 1.0.0 · **Ratificada em:** 2026-11-24 · **Última alteração:** 2026-11-24

Esta constituição define as regras que não se negociam no projeto. Toda spec, plano, tarefa e linha de código deve obedecer a ela. Quando um plano precisar violar um artigo, a violação deve ser registrada em "Complexity Tracking" no `plan.md`, com justificativa, antes de qualquer código.

---

## Artigo I — Spec antes de código (Spec-Driven Development)

1. Nenhuma implementação começa sem `spec.md`, `plan.md` e `tasks.md` da feature, revisados.
2. A spec descreve **o quê e por quê**, sem tecnologia. O plano descreve **como**. As tarefas descrevem **em que ordem**.
3. Mudou o requisito? Primeiro se altera a spec, depois o plano e as tarefas, e só então o código.
4. Toda tarefa referencia ao menos um requisito (`FR-xxx`) ou história (`USx`). Código sem tarefa correspondente não entra.
5. Ambiguidade é marcada como `[NEEDS CLARIFICATION: ...]` e resolvida antes do plano.

## Artigo II — Domínio no centro (DDD)

1. **Linguagem ubíqua:** os termos do glossário em `data-model.md` são usados igualmente na conversa, na spec, no código, nos testes e no banco. Termo novo entra primeiro no glossário.
2. **Idioma:** nomes do domínio em português, sem acento (`Oferta`, `Drogaria`, `PrecoPorUnidade`). Sufixos de padrões técnicos ficam em inglês, como é convenção no .NET (`Repository`, `Handler`, `Configuration`, `Async`).
3. **Contextos delimitados:** `Ofertas` (núcleo), `Preferencias` (suporte), `Identidade` (genérico, ASP.NET Core Identity) e `Alertas` (suporte, fase 2). Um contexto não acessa as tabelas de outro.
4. **Agregados** protegem suas invariantes. O estado só muda por métodos com intenção (`RegistrarCotacao`, nunca `set` público). Construtores privados com fábricas (`Criar`) que validam a entrada.
5. **Objetos de valor** para todo conceito com regra. Nada de obsessão por tipos primitivos: preço é `Dinheiro`, tamanho é `Tamanho`, link é `UrlDeProduto`.
6. **O domínio não conhece framework:** o projeto `Domain` não referencia EF Core, ASP.NET, Hangfire, Playwright nem pacotes de infraestrutura.
7. **Eventos de domínio** registram fatos relevantes no passado (`CotacaoRegistrada`) e são publicados depois da persistência.

## Artigo III — Clean Architecture

1. As dependências apontam para dentro: `Web` e `Worker` → `Infrastructure` → `Application` → `Domain`.
2. `Application` define portas (interfaces) e `Infrastructure` implementa adaptadores (coletores, repositórios, relógio).
3. Casos de uso são classes pequenas, uma por intenção (`ColetarOfertasHandler`, `BuscarOfertasHandler`), chamadas diretamente por injeção de dependência. **Sem MediatR e sem AutoMapper**: o mapeamento é explícito e manual.
4. Leitura e escrita podem seguir caminhos diferentes (CQRS leve): consultas de tela usam projeções diretas para DTOs de leitura, sem carregar agregados.
5. As regras são verificadas por testes de arquitetura (`PromocoesHigieneBebe.ArchitectureTests`) no CI.

## Artigo IV — Clean Code e convenções C#

1. Seguir as [convenções de código C# da Microsoft](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) e o `.editorconfig` do repositório.
2. `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, analisadores do .NET em nível `recommended`, namespaces por arquivo e gerenciamento central de pacotes (`Directory.Packages.props`).
3. Nomes revelam intenção, sem abreviações (`unidadesPorPacote`, nunca `qtd`). Métodos fazem uma coisa só e cabem numa tela; acima de 20 linhas, extrair um método.
4. Cláusulas de guarda no início e retorno antecipado, em vez de `if` aninhado. Nada de número mágico: usar constante nomeada ou objeto de valor.
5. Falha esperada (produto sem quantidade, loja fora do ar) retorna `Result`. Exceção só para o que é realmente excepcional.
6. `async` de ponta a ponta, com `CancellationToken` em toda operação de E/S. Nunca `.Result` ou `.Wait()`.
7. Sem estado estático mutável. Tempo vem de `TimeProvider`, nunca de `DateTime.Now`.
8. Comentário explica o **porquê**; o **o quê** fica no nome. Código comentado não é versionado.
9. Commits no padrão [Conventional Commits](https://www.conventionalcommits.org/pt-br/) (`feat:`, `fix:`, `test:`, `refactor:`, `docs:`), uma intenção por commit.

## Artigo V — Testes primeiro

1. **TDD** no `Domain` e na `Application`: o teste que falha vem antes do código.
2. Nomes de teste no padrão `Metodo_Cenario_ResultadoEsperado`, com corpo em Arrange / Act / Assert.
3. Ferramentas: xUnit, Shouldly (asserções), NSubstitute (dublês) e Testcontainers (PostgreSQL real nos testes de integração).
4. Coletores são testados com **respostas gravadas** (JSON e HTML em `tests/.../Fixtures`). O CI nunca acessa o site real das drogarias.
5. Cobertura mínima de 90% no `Domain` e de 80% na `Application`. Todo bug corrigido ganha um teste que o reproduz.
6. Uma tarefa só está pronta quando os testes passam e o build está sem avisos.

## Artigo VI — Segurança por padrão

1. **Dados coletados não são confiáveis.** Nome, link, imagem e preço vindos das drogarias são validados no domínio (`UrlDeProduto` só aceita os domínios das 4 drogarias) e codificados na saída. `Html.Raw` é proibido.
2. HTTPS obrigatório, HSTS, cookie `HttpOnly` + `Secure` + `SameSite`, antiforgery em todo formulário e requisição HTMX, e Content-Security-Policy sem script inline.
3. Login pelo ASP.NET Core Identity com passkey como padrão. Bloqueio após tentativas falhas e limite de requisições por usuário.
4. Segredos nunca no repositório: variáveis de ambiente ou Docker secrets. Secret scanning ligado.
5. Princípio do menor privilégio: contas de banco separadas para `Web` e `Worker`; o container do coletor roda sem root, sem segredos e sem portas de entrada.
6. Dados pessoais (LGPD): coletar o mínimo, mascarar em log e permitir exclusão.
7. Dependências verificadas no CI (`dotnet list package --vulnerable`) e atualizadas pelo Dependabot.

## Artigo VII — Coleta responsável

1. Coleta agendada com intervalo mínimo de 1 hora por drogaria e no máximo 2 requisições simultâneas por site.
2. Preferir a API pública documentada (VTEX Intelligent Search) ao navegador automatizado. Playwright só onde não houver alternativa.
3. Nunca contornar captcha, bloqueio, login ou paywall. Bloqueou: registrar, alertar e parar.
4. Respeitar o `robots.txt` e não republicar preços em massa.

## Artigo VIII — Simplicidade

1. YAGNI: nada é construído para uma fase futura antes da hora. Extensibilidade só onde a spec pede (novas drogarias, novas categorias).
2. Um monólito modular com dois hosts (`Web` e `Worker`). Sem microsserviços, sem barramento de mensagens externo.
3. Uma dependência nova precisa de justificativa no `research.md`.

## Artigo IX — Observabilidade

1. Logs estruturados com `ILogger` e templates de mensagem (nunca interpolação de string).
2. Cada coleta registra drogaria, itens lidos, itens aceitos, itens descartados (com motivo), duração e erro.
3. Nenhum log contém telefone, e-mail, token ou cookie.

---

## Governança

- A constituição prevalece sobre qualquer outro documento ou instrução.
- Alterar um artigo exige registrar o motivo, atualizar a versão (SemVer: MAJOR remove ou redefine princípio; MINOR adiciona; PATCH esclarece) e revisar as specs afetadas.
- Toda revisão de PR confere: tarefa referenciada, testes primeiro, arquitetura respeitada e checklist de segurança.

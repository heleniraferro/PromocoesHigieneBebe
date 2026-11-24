# Quickstart: validar a feature 001

## Pré-requisitos

- .NET SDK 10
- Docker (para PostgreSQL e Testcontainers)
- PowerShell ou bash para instalar os navegadores do Playwright

## Subir o ambiente local

```bash
cp deploy/.env.example deploy/.env           # preencher senhas locais
docker compose -f deploy/docker-compose.yml up -d postgres
dotnet ef database update -p src/PromocoesHigieneBebe.Infrastructure -s src/PromocoesHigieneBebe.Web
pwsh src/PromocoesHigieneBebe.Worker/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet run --project src/PromocoesHigieneBebe.Worker   # agenda e executa as coletas
dotnet run --project src/PromocoesHigieneBebe.Web      # https://localhost:5001
```

## Roteiro de validação (por história)

| História | Passo | Resultado esperado |
| --- | --- | --- |
| US1 | `dotnet test tests/PromocoesHigieneBebe.Infrastructure.Tests --filter Coleta` | Fixtures das 4 drogarias interpretadas; descartes com motivo |
| US1 | Disparar a coleta da Pague Menos pelo painel do Hangfire (só em desenvolvimento) | Registro em `ofertas.coletas` com situação `Concluida` e contagens |
| US2 | Abrir `/`, escolher G | Lista única ordenada por preço por fralda; primeira com "Mais barato" |
| US2 | Conferir 10 ofertas, incluindo 3 kits, contra a calculadora | Valores iguais (SC-004) |
| US3 | Desmarcar Huggies, escolher Pants, recarregar a página | Filtros mantidos e respeitados |
| US4 | Parar o acesso à Drogaria Rosário (variável `Drogarias__DrogariaRosario__BaseUrl` inválida) e coletar | "3 de 4 lojas" com o nome da Rosário |
| US5 | Abrir `/` numa janela anônima | Redireciona para `/Conta/Entrar` |
| US5 | Cadastrar passkey e entrar com ela | Acesso sem senha |

## Checklist de segurança antes de publicar

- [ ] `dotnet list package --vulnerable` sem itens
- [ ] Cabeçalhos CSP, HSTS e nosniff presentes (conferir em securityheaders.com)
- [ ] Painel do Hangfire inacessível de fora
- [ ] PostgreSQL sem porta publicada no `docker-compose.yml`
- [ ] Container do `worker` rodando como não root
- [ ] Nenhum segredo no repositório (secret scanning sem alertas)

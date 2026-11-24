using PromocoesHigieneBebe.Spike;

// Fase 0 da feature 001: valida como coletar as ofertas das 4 drogarias e grava
// as respostas reais como fixtures para os testes.
// Uso: dotnet run -- <comando> [--cep 78005000]
//   vtex                 Drogaria São Paulo, Rosário e Pague Menos (T001 e T003)
//   drogasil             Drogasil com Playwright (T002)
//   instalar-navegador   Baixa o Chromium usado pelo Playwright
//   tudo                 vtex + drogasil

var comando = args.FirstOrDefault()?.ToLowerInvariant() ?? "ajuda";
var cep = LerOpcao(args, "--cep") ?? Configuracao.CepPadrao;
var saida = Saida.Criar();

using var cancelamento = new CancellationTokenSource();
Console.CancelKeyPress += (_, evento) =>
{
    evento.Cancel = true;
    cancelamento.Cancel();
};

switch (comando)
{
    case "vtex":
        await new SondagemVtex(saida).ExecutarAsync(cep, cancelamento.Token);
        break;
    case "drogasil":
        await new SondagemDrogasil(saida).ExecutarAsync(cancelamento.Token);
        break;
    case "tudo":
        await new SondagemVtex(saida).ExecutarAsync(cep, cancelamento.Token);
        await new SondagemDrogasil(saida).ExecutarAsync(cancelamento.Token);
        break;
    case "instalar-navegador":
        return Microsoft.Playwright.Program.Main(["install", "chromium"]);
    default:
        MostrarAjuda();
        return 1;
}

saida.GravarRelatorio();
Console.WriteLine($"\nRelatório: {saida.CaminhoDoRelatorio}");
Console.WriteLine($"Fixtures:  {saida.PastaDeFixtures}");
return 0;

static string? LerOpcao(string[] argumentos, string nome)
{
    var indice = Array.IndexOf(argumentos, nome);
    return indice >= 0 && indice + 1 < argumentos.Length ? argumentos[indice + 1] : null;
}

static void MostrarAjuda() => Console.WriteLine(
    """
    Uso: dotnet run -- <comando> [--cep 78005000]

      vtex                 Drogaria São Paulo, Rosário e Pague Menos
      drogasil             Drogasil (precisa do Chromium: rode instalar-navegador antes)
      instalar-navegador   Baixa o Chromium do Playwright
      tudo                 vtex + drogasil
    """);

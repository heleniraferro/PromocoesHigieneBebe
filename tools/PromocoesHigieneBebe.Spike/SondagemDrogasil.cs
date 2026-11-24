using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace PromocoesHigieneBebe.Spike;

/// T002: abre a busca da Drogasil num Chromium headless, grava o HTML da página e
/// as respostas JSON da rede que parecem trazer preços.
internal sealed class SondagemDrogasil(Saida saida)
{
    private const string Codigo = "drogasil";
    private const int TempoLimiteDaPaginaEmMs = 60_000;
    private const int EsperaExtraEmMs = 3_000;
    private static readonly string[] IndicadoresDePreco = ["price", "preco", "valor", "finalprice"];

    public async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        saida.Titulo("Drogasil (Playwright)");

        using var playwright = await Playwright.CreateAsync();
        await using var navegador = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var contexto = await navegador.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = Configuracao.UserAgent,
            Locale = "pt-BR",
        });
        var pagina = await contexto.NewPageAsync();

        var respostas = new ConcurrentQueue<IResponse>();
        pagina.Response += (_, resposta) => respostas.Enqueue(resposta);

        foreach (var termo in Configuracao.Termos)
        {
            for (var numero = 1; numero <= Configuracao.PaginasPorTermo; numero++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                respostas.Clear();

                var continuar = await SondarPaginaAsync(pagina, respostas, termo, numero);
                if (!continuar)
                {
                    saida.Linha("Bloqueio detectado. Parando a Drogasil (Artigo VII).");
                    return;
                }

                await Task.Delay(Configuracao.PausaEntreRequisicoes, cancellationToken);
            }
        }
    }

    /// Devolve false quando a página indica bloqueio (403 ou captcha).
    private async Task<bool> SondarPaginaAsync(IPage pagina, ConcurrentQueue<IResponse> respostas, string termo, int numero)
    {
        var url = $"{Configuracao.BuscaDrogasil}?w={Uri.EscapeDataString(termo)}&p={numero}";
        var resposta = await pagina.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = TempoLimiteDaPaginaEmMs,
        });
        await pagina.WaitForTimeoutAsync(EsperaExtraEmMs);

        var status = resposta?.Status ?? 0;
        var html = await pagina.ContentAsync();
        var slug = termo.Replace(' ', '-');
        var arquivoHtml = $"pagina-{slug}-p{numero}.html";
        saida.GravarFixture(Codigo, arquivoHtml, html);

        var precosNaTela = await pagina.Locator("text=R$").CountAsync();
        saida.Linha($"\"{termo}\" p{numero}: HTTP {status}, {precosNaTela} textos com \"R$\" na tela → `{arquivoHtml}`");

        if (status == 403 || html.Contains("captcha", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        await GravarRespostasComPrecoAsync(respostas, slug, numero);
        return true;
    }

    private async Task GravarRespostasComPrecoAsync(ConcurrentQueue<IResponse> respostas, string slug, int numero)
    {
        var indice = 0;
        foreach (var resposta in respostas.Where(EhJson))
        {
            string corpo;
            try
            {
                corpo = await resposta.TextAsync();
            }
            catch (PlaywrightException)
            {
                continue; // corpo descartado pelo navegador (redirecionamento ou navegação)
            }

            if (!IndicadoresDePreco.Any(indicador => corpo.Contains(indicador, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            indice++;
            var arquivo = $"rede-{slug}-p{numero}-{indice:00}.json";
            saida.GravarFixture(Codigo, arquivo, corpo);
            saida.Linha($"  resposta com preço: `{SemQueryString(resposta.Url)}` ({resposta.Request.Method}) → `{arquivo}`");
        }

        if (indice == 0)
        {
            saida.Linha("  nenhuma resposta JSON com preço; usar o HTML como fonte");
        }
    }

    private static bool EhJson(IResponse resposta) =>
        resposta.Status == 200
        && resposta.Headers.TryGetValue("content-type", out var tipo)
        && tipo.Contains("json", StringComparison.OrdinalIgnoreCase);

    private static string SemQueryString(string url)
    {
        var indice = url.IndexOf('?', StringComparison.Ordinal);
        return indice < 0 ? url : url[..indice];
    }
}

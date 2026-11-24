using System.Net;
using System.Text.Json;

namespace PromocoesHigieneBebe.Spike;

/// T001 e T003: testa os endpoints de busca VTEX e a regionalização por CEP
/// nas lojas Drogaria São Paulo, Drogaria Rosário e Pague Menos.
internal sealed class SondagemVtex(Saida saida)
{
    private const int LimiteDeBloqueios = 2;

    public async Task ExecutarAsync(string cep, CancellationToken cancellationToken)
    {
        using var http = CriarHttpClient();

        foreach (var loja in Configuracao.LojasVtex)
        {
            saida.Titulo($"{loja.Nome} (VTEX)");
            await SondarLojaAsync(http, loja, cep, cancellationToken);
        }
    }

    private async Task SondarLojaAsync(HttpClient http, LojaVtex loja, string cep, CancellationToken cancellationToken)
    {
        var bloqueios = 0;
        var regionId = await ObterRegionIdAsync(http, loja, cep, cancellationToken);

        foreach (var termo in Configuracao.Termos)
        {
            var endpointQueFuncionou = await DescobrirEndpointAsync(http, loja, termo, regionId, cancellationToken);
            if (endpointQueFuncionou is null)
            {
                bloqueios++;
                if (bloqueios >= LimiteDeBloqueios)
                {
                    saida.Linha("Nenhum endpoint respondeu em dois termos seguidos. Parando esta loja (Artigo VII).");
                    return;
                }

                continue;
            }

            for (var pagina = 2; pagina <= Configuracao.PaginasPorTermo; pagina++)
            {
                await TentarAsync(http, loja, endpointQueFuncionou, termo, pagina, regionId, cancellationToken);
            }
        }
    }

    private async Task<string?> ObterRegionIdAsync(HttpClient http, LojaVtex loja, string cep, CancellationToken cancellationToken)
    {
        var url = new Uri(loja.BaseUrl, $"/api/checkout/pub/regions?country=BRA&postalCode={cep}");
        var (status, corpo) = await BuscarAsync(http, url, cancellationToken);
        saida.Linha($"Regionalização (CEP {cep}): HTTP {(int)status}");
        if (status != HttpStatusCode.OK || corpo is null)
        {
            return null;
        }

        saida.GravarFixture(loja.Codigo, "regioes.json", corpo);
        var regionId = ExtrairRegionId(corpo);
        saida.Linha(regionId is null ? "regionId não encontrado na resposta" : $"regionId obtido: `{regionId}`");
        return regionId;
    }

    /// Testa os endpoints candidatos na página 1 e devolve o primeiro que trouxe produtos.
    private async Task<EndpointVtex?> DescobrirEndpointAsync(
        HttpClient http, LojaVtex loja, string termo, string? regionId, CancellationToken cancellationToken)
    {
        EndpointVtex? primeiroComProdutos = null;
        foreach (var endpoint in EndpointVtex.Candidatos)
        {
            var quantidade = await TentarAsync(http, loja, endpoint, termo, pagina: 1, regionId, cancellationToken);
            if (quantidade > 0 && primeiroComProdutos is null)
            {
                primeiroComProdutos = endpoint;
            }
        }

        saida.Linha(primeiroComProdutos is null
            ? $"\"{termo}\": nenhum endpoint trouxe produtos"
            : $"\"{termo}\": usar **{primeiroComProdutos.Nome}**");
        return primeiroComProdutos;
    }

    private async Task<int> TentarAsync(
        HttpClient http, LojaVtex loja, EndpointVtex endpoint, string termo, int pagina, string? regionId,
        CancellationToken cancellationToken)
    {
        var url = endpoint.Montar(loja.BaseUrl, termo, pagina, regionId);
        var (status, corpo) = await BuscarAsync(http, url, cancellationToken);
        if (status != HttpStatusCode.OK || corpo is null)
        {
            saida.Linha($"{endpoint.Nome} p{pagina}: HTTP {(int)status}");
            return 0;
        }

        var resumo = ResumoDeProdutos.Ler(corpo);
        var arquivo = $"{endpoint.Codigo}-{Slug(termo)}-p{pagina}.json";
        saida.GravarFixture(loja.Codigo, arquivo, corpo);
        saida.Linha($"{endpoint.Nome} p{pagina}: HTTP 200, {resumo.Quantidade} produtos → `{arquivo}`");
        foreach (var exemplo in resumo.Exemplos)
        {
            saida.Linha($"  exemplo: {exemplo}");
        }

        return resumo.Quantidade;
    }

    private static async Task<(HttpStatusCode Status, string? Corpo)> BuscarAsync(
        HttpClient http, Uri url, CancellationToken cancellationToken)
    {
        await Task.Delay(Configuracao.PausaEntreRequisicoes, cancellationToken);
        try
        {
            using var resposta = await http.GetAsync(url, cancellationToken);
            var corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);
            return (resposta.StatusCode, corpo);
        }
        catch (HttpRequestException excecao)
        {
            Console.WriteLine($"  falha de rede em {url.Host}: {excecao.Message}");
            return (HttpStatusCode.ServiceUnavailable, null);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine($"  tempo esgotado em {url.Host}");
            return (HttpStatusCode.RequestTimeout, null);
        }
    }

    private static string? ExtrairRegionId(string json)
    {
        using var documento = JsonDocument.Parse(json);
        if (documento.RootElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var regiao in documento.RootElement.EnumerateArray())
        {
            if (regiao.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
            {
                return id.GetString();
            }
        }

        return null;
    }

    private static HttpClient CriarHttpClient()
    {
        var http = new HttpClient { Timeout = Configuracao.TempoLimiteDaRequisicao };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(Configuracao.UserAgent);
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR");
        return http;
    }

    private static string Slug(string texto) => texto.Replace(' ', '-');
}

/// Endpoints candidatos de busca VTEX (ver contracts/vtex-intelligent-search.md).
internal sealed record EndpointVtex(string Codigo, string Nome, Func<Uri, string, int, string?, Uri> Montar)
{
    public static readonly EndpointVtex[] Candidatos =
    [
        new("is-v1", "Intelligent Search v1",
            (baseUrl, termo, pagina, regionId) => new Uri(baseUrl,
                $"/api/intelligent-search/v1/product-search?query={Uri.EscapeDataString(termo)}&page={pagina}" +
                $"&count={Configuracao.ItensPorPagina}&locale=pt-BR{ParametroDeRegiao(regionId)}")),
        new("is-legado", "Intelligent Search legado",
            (baseUrl, termo, pagina, regionId) => new Uri(baseUrl,
                $"/api/io/_v/api/intelligent-search/product_search/?query={Uri.EscapeDataString(termo)}&page={pagina}" +
                $"&count={Configuracao.ItensPorPagina}&locale=pt-BR{ParametroDeRegiao(regionId)}")),
        new("catalogo-legado", "Catálogo legado",
            (baseUrl, termo, pagina, _) =>
            {
                var inicio = (pagina - 1) * Configuracao.ItensPorPagina;
                var fim = inicio + Configuracao.ItensPorPagina - 1;
                return new Uri(baseUrl,
                    $"/api/catalog_system/pub/products/search?ft={Uri.EscapeDataString(termo)}&_from={inicio}&_to={fim}");
            }),
    ];

    private static string ParametroDeRegiao(string? regionId) =>
        regionId is null ? string.Empty : $"&regionId={Uri.EscapeDataString(regionId)}";
}

/// Lê o suficiente da resposta para conferir que vieram produtos com preço.
internal sealed record ResumoDeProdutos(int Quantidade, IReadOnlyList<string> Exemplos)
{
    private const int QuantidadeDeExemplos = 3;

    public static ResumoDeProdutos Ler(string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            var produtos = ObterProdutos(documento.RootElement);
            var exemplos = produtos.Take(QuantidadeDeExemplos).Select(Descrever).ToList();
            return new ResumoDeProdutos(produtos.Count, exemplos);
        }
        catch (JsonException)
        {
            return new ResumoDeProdutos(0, ["resposta não é JSON"]);
        }
    }

    private static List<JsonElement> ObterProdutos(JsonElement raiz)
    {
        if (raiz.ValueKind == JsonValueKind.Array)
        {
            return raiz.EnumerateArray().Select(produto => produto.Clone()).ToList();
        }

        if (raiz.ValueKind == JsonValueKind.Object
            && raiz.TryGetProperty("products", out var produtos)
            && produtos.ValueKind == JsonValueKind.Array)
        {
            return produtos.EnumerateArray().Select(produto => produto.Clone()).ToList();
        }

        return [];
    }

    private static string Descrever(JsonElement produto)
    {
        var nome = Texto(produto, "productName") ?? "(sem nome)";
        var preco = PrimeiraOferta(produto) is { } oferta && oferta.TryGetProperty("Price", out var valor)
            ? valor.ToString()
            : "?";
        return $"{nome} — R$ {preco}";
    }

    private static JsonElement? PrimeiraOferta(JsonElement produto)
    {
        if (!produto.TryGetProperty("items", out var itens) || !TemItens(itens))
        {
            return null;
        }

        var item = itens[0];
        if (!item.TryGetProperty("sellers", out var vendedores) || !TemItens(vendedores))
        {
            return null;
        }

        return vendedores[0].TryGetProperty("commertialOffer", out var oferta) ? oferta : null;
    }

    private static bool TemItens(JsonElement lista) =>
        lista.ValueKind == JsonValueKind.Array && lista.GetArrayLength() > 0;

    private static string? Texto(JsonElement elemento, string propriedade) =>
        elemento.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;
}

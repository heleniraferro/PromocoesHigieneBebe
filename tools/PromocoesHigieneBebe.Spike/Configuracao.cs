namespace PromocoesHigieneBebe.Spike;

/// Parâmetros da sondagem. Os limites seguem o Artigo VII da constituição (coleta responsável).
internal static class Configuracao
{
    /// CEP de Cuiabá usado nos testes de regionalização. Ajuste com --cep.
    public const string CepPadrao = "78005000";

    public static readonly string[] Termos = ["fralda g", "fralda pants g"];

    /// Poucas páginas: o objetivo é gravar amostras, não coletar tudo.
    public const int PaginasPorTermo = 2;

    public const int ItensPorPagina = 24;

    /// Pausa entre requisições ao mesmo site, para não sobrecarregar.
    public static readonly TimeSpan PausaEntreRequisicoes = TimeSpan.FromSeconds(3);

    public static readonly TimeSpan TempoLimiteDaRequisicao = TimeSpan.FromSeconds(30);

    public const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0 Safari/537.36";

    public static readonly LojaVtex[] LojasVtex =
    [
        new("drogaria-sao-paulo", "Drogaria São Paulo", new Uri("https://www.drogariasaopaulo.com.br")),
        new("drogaria-rosario", "Drogaria Rosário", new Uri("https://www.drogariarosario.com.br")),
        new("pague-menos", "Pague Menos", new Uri("https://www.paguemenos.com.br")),
    ];

    public static readonly Uri BuscaDrogasil = new("https://www.drogasil.com.br/search");
}

internal sealed record LojaVtex(string Codigo, string Nome, Uri BaseUrl);

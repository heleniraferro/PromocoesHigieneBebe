# Contrato: porta de coleta (`IColetorDeOfertas`)

Porta definida em `FraldasPromo.Application/Abstractions`. Cada drogaria tem um adaptador em `FraldasPromo.Infrastructure/Coleta`.

```csharp
public interface IColetorDeOfertas
{
    /// Drogaria atendida por este adaptador.
    Drogaria Drogaria { get; }

    /// Percorre todas as páginas de resultado do termo, até o limite configurado.
    /// Devolve os itens como vieram, sem interpretar a descrição.
    /// Lança ColetaBloqueadaException quando a drogaria bloqueia (403 repetido, captcha).
    IAsyncEnumerable<OfertaBruta> ColetarAsync(string termo, CancellationToken cancellationToken);
}

public sealed record OfertaBruta(
    string Sku,
    string Descricao,
    string? MarcaInformada,        // marca que a drogaria informa, quando existe
    decimal Preco,                 // preço sem condição
    decimal? PrecoAnterior,        // "de"
    decimal? PrecoCondicional,     // preço com condição (cadastro, leve X pague Y)
    string? DescricaoCondicao,
    bool Disponivel,
    string Url,                    // absoluta
    string? UrlImagem);
```

## Regras para todo adaptador

1. Não interpreta tamanho, quantidade nem marca: isso é papel do domínio (`InterpretadorDeDescricao`).
2. Converte URLs relativas em absolutas usando a `BaseUrl` da drogaria.
3. Respeita `RequisicoesSimultaneasPorSite` e `MaximoDePaginas`.
4. Usa política de resiliência: timeout de 30 s por requisição, 2 novas tentativas com backoff exponencial e circuit breaker.
5. Identifica a si mesmo com um User-Agent de navegador comum. Não usa proxies rotativos nem resolve captcha.
6. Em 403 por duas vezes seguidas ou página de captcha, lança `ColetaBloqueadaException`.
7. Registra em log: termo, página, itens na página e duração.
8. É testado com respostas gravadas (`Fixtures/<drogaria>/*.json|html`) por meio de um `HttpMessageHandler` falso. O Playwright é testado com páginas servidas localmente.

## Adaptadores

| Adaptador | Drogarias | Fonte |
| --- | --- | --- |
| `VtexColetor` | Drogaria São Paulo, Drogaria Rosário, Pague Menos | Intelligent Search API (v1 ou legada), ver `vtex-intelligent-search.md` |
| `DrogasilColetor` | Drogasil | Playwright: abre `/search?w=<termo>&p=<n>` e captura a resposta JSON de preços; DOM como recurso |

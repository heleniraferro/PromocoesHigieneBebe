# Contrato externo: VTEX Intelligent Search (a validar na Fase 0)

Este arquivo registra o que o `VtexColetor` espera da API. Os itens marcados **a validar** precisam ser confirmados com uma chamada real na Fase 0, e a resposta deve ser gravada como fixture.

## Endpoints candidatos

| Versão | Caminho | Situação |
| --- | --- | --- |
| v1 | `GET {BaseUrl}/api/intelligent-search/v1/product-search/...` | Preferida (lançada em julho de 2026). **A validar:** caminho exato e parâmetros |
| Legada | `GET {BaseUrl}/api/io/_v/api/intelligent-search/product_search/?query={termo}&page={n}&count={50}&locale=pt-BR` | Recurso, se a v1 não responder na loja |

## Campos usados da resposta (a validar)

| Campo esperado | Uso em `OfertaBruta` |
| --- | --- |
| `products[].productId` ou `items[].itemId` | `Sku` (preferir o `itemId` do SKU) |
| `products[].productName` | `Descricao` |
| `products[].brand` | `MarcaInformada` |
| `products[].link` / `linkText` | `Url` (`{BaseUrl}/{linkText}/p`) |
| `items[].images[0].imageUrl` | `UrlImagem` |
| `items[].sellers[0].commertialOffer.Price` | `Preco` |
| `items[].sellers[0].commertialOffer.ListPrice` | `PrecoAnterior` (quando maior que `Price`) |
| `items[].sellers[0].commertialOffer.AvailableQuantity` | `Disponivel` (> 0) |
| `items[].sellers[0].commertialOffer.Teasers` / `DiscountHighLight` | `DescricaoCondicao` e `PrecoCondicional` (**a validar**: formato varia por loja) |
| `recordsFiltered` ou `pagination` | Saber quando parar de paginar |

## Regionalização (a validar)

- v1: parâmetro explícito de regionalização (segundo a nota de lançamento, a v1 não depende do cookie de segmento).
- Legada: cookie `vtex_segment` com `regionId` obtido pela simulação de CEP.
- Objetivo: preço para um CEP de Cuiabá (FR-003). Se não for possível, registrar no `research.md` e exibir aviso na tela.

## Itens de produto com vários SKUs

Um produto VTEX pode ter vários `items` (ex.: tamanhos P, M e G no mesmo produto). Cada `item` vira uma `OfertaBruta` separada, com a descrição composta por `productName` + `name` do item, para que o domínio identifique o tamanho certo.

# Modelo de domínio e dados: 001-busca-ofertas-fraldas

## Glossário (linguagem ubíqua)

Os termos abaixo são usados igualmente na conversa, na spec, no código, nos testes e no banco.

| Termo | Significado | No código |
| --- | --- | --- |
| Drogaria | Uma das redes consultadas | `Drogaria` (enumeração inteligente) |
| Produto | Uma fralda vendida por uma drogaria, identificada pelo SKU da drogaria | `Produto` (agregado) |
| Descrição | O nome do produto como a drogaria escreve | `Produto.Descricao` |
| Marca | Fabricante (Pampers, Huggies...) ou "marca própria" da drogaria | `Marca` (objeto de valor) |
| Linha | Subfamília da marca (Confort Sec, Premium Care...) | `Linha` (objeto de valor, opcional) |
| Tipo de fralda | Fralda comum (com fita) ou pants (calça) | `TipoFralda` (enum: `Fralda`, `Pants`) |
| Tamanho | RN, RN+, P, M, G, XG, XXG ou XXXG | `Tamanho` (enumeração inteligente) |
| Embalagem | Unidades por pacote × pacotes no kit | `Embalagem` (objeto de valor) |
| Kit | Venda de 2 ou mais pacotes iguais juntos | `Embalagem.PacotesNoKit > 1` |
| Cotação | O preço de um produto num instante | `Cotacao` (agregado) |
| Preço | Valor sem condição, que qualquer pessoa paga | `Cotacao.Preco` (`Dinheiro`) |
| Preço anterior | O "de" no "de/por" | `Cotacao.PrecoAnterior` |
| Condição comercial | Regra para pagar menos (cadastro, leve 3 pague 2) e o preço com ela | `CondicaoComercial` (objeto de valor) |
| Preço por unidade | Preço ÷ total de unidades da embalagem | `PrecoPorUnidade` (objeto de valor) |
| Oferta | Visão de leitura: produto + cotação mais recente + preço por unidade | `OfertaDto` (lado de leitura) |
| Coleta | Uma execução de coleta numa drogaria | `Coleta` (agregado) |
| Oferta bruta | O que o coletor devolve antes da interpretação | `OfertaBruta` (DTO da porta) |
| Interpretação | Transformar descrição em marca, linha, tipo, tamanho e embalagem | `InterpretadorDeDescricao` (serviço de domínio) |
| Descarte | Item rejeitado na interpretação, com motivo | `MotivoDescarte` (enum) |
| Preferência de busca | Último tamanho, marcas e tipo de um usuário | `PreferenciaDeBusca` (agregado) |

## Contextos delimitados

| Contexto | Tipo | Responsabilidade | Nesta feature |
| --- | --- | --- | --- |
| Ofertas | Núcleo | Coletar, interpretar, registrar cotações e responder consultas de oferta | Sim |
| Preferencias | Suporte | Guardar as escolhas de busca do usuário | Sim |
| Identidade | Genérico | Usuários, login, passkeys (ASP.NET Core Identity) | Sim, sem customização |
| Alertas | Suporte | Preço-alvo e notificações | Não (feature 002) |

`Preferencias` referencia o usuário só pelo `UsuarioId` (Guid) e não acessa as tabelas do Identity.

## Contexto Ofertas

### Enumerações inteligentes

**`Drogaria`**: `Drogasil`, `DrogariaSaoPaulo`, `DrogariaRosario`, `PagueMenos`.

- Propriedades: `Codigo` (string estável, ex.: `drogasil`), `Nome`, `DominiosPermitidos` (ex.: `www.drogasil.com.br`).
- Uma drogaria nova entra como novo valor + novo coletor, sem alterar as demais.

**`Tamanho`**: `RN`, `RNPlus`, `P`, `M`, `G`, `XG`, `XXG`, `XXXG`.

- Propriedades: `Codigo`, `Rotulo` (o que se exibe: `RN+`), `Ordem` (para ordenar do menor ao maior), `Sinonimos`.
- `Tamanho.Interpretar(string)` → `Result<Tamanho>`; aceita sinônimos (`EG` = `XG`, `EXG` = `XXG`, `RN Plus` = `RN+`), sem diferenciar maiúsculas e minúsculas.

### Objetos de valor

| Objeto | Campos | Invariantes |
| --- | --- | --- |
| `Dinheiro` | `Valor` (decimal) | Valor > 0; moeda BRL implícita; arredondamento só na formatação |
| `Embalagem` | `UnidadesPorPacote` (int), `PacotesNoKit` (int) | Unidades entre 1 e 300; pacotes entre 1 e 10; `TotalUnidades` = produto dos dois |
| `PrecoPorUnidade` | `Valor` (decimal) | Calculado por `PrecoPorUnidade.De(Dinheiro, Embalagem)`; faixa plausível de R$ 0,20 a R$ 10,00 (configurável), fora dela é descarte |
| `Marca` | `Nome`, `EhMarcaPropria` | Nome não vazio; comparação sem diferenciar maiúsculas/acentos |
| `Linha` | `Nome` | Opcional; normalizada (trim, espaços únicos) |
| `UrlDeProduto` | `Valor` (Uri) | HTTPS e host dentro de `Drogaria.DominiosPermitidos`; caso contrário, falha |
| `Sku` | `Valor` (string) | Não vazio, até 100 caracteres |
| `CondicaoComercial` | `Descricao`, `Preco` (`Dinheiro`) | Preço da condição menor que o preço sem condição |

### Agregado `Produto`

Raiz: `Produto`. Identidade natural: (`Drogaria`, `Sku`).

| Campo | Tipo |
| --- | --- |
| `Id` | `ProdutoId` (Guid fortemente tipado) |
| `Drogaria` | `Drogaria` |
| `Sku` | `Sku` |
| `Descricao` | string (até 300) |
| `Marca` | `Marca` |
| `Linha` | `Linha?` |
| `Tipo` | `TipoFralda` |
| `Tamanho` | `Tamanho` |
| `Embalagem` | `Embalagem` |
| `Url` | `UrlDeProduto` |
| `UrlImagem` | `UrlDeProduto?` |
| `VistoPelaUltimaVezEm` | `DateTimeOffset` |

Comportamentos:

- `Produto.Criar(...)` → `Result<Produto>`: valida todos os objetos de valor.
- `AtualizarDados(descricao, url, urlImagem, instante)`: quando a drogaria muda o nome ou o link.
- `MarcarComoVisto(instante)`: atualiza `VistoPelaUltimaVezEm`.

Invariantes: a `Url` pertence à `Drogaria` do produto; `Tamanho` e `Embalagem` são imutáveis depois da criação (se mudarem na drogaria, é outro SKU).

### Agregado `Cotacao`

Raiz: `Cotacao`. Histórico somente de inclusão: uma cotação nunca é alterada.

| Campo | Tipo |
| --- | --- |
| `Id` | `CotacaoId` |
| `ProdutoId` | `ProdutoId` |
| `ColetaId` | `ColetaId` |
| `Preco` | `Dinheiro` |
| `PrecoAnterior` | `Dinheiro?` |
| `Condicao` | `CondicaoComercial?` |
| `Disponivel` | bool |
| `ColetadaEm` | `DateTimeOffset` |

- `Cotacao.Registrar(...)` → `Result<Cotacao>`; dispara `CotacaoRegistrada` (usado pela feature 002, Alertas).
- Invariante: `PrecoAnterior`, quando existe, é maior que `Preco`.

### Agregado `Coleta`

| Campo | Tipo |
| --- | --- |
| `Id` | `ColetaId` |
| `Drogaria` | `Drogaria` |
| `IniciadaEm` / `FinalizadaEm` | `DateTimeOffset` |
| `Situacao` | `SituacaoColeta` (`EmAndamento`, `Concluida`, `Falhou`, `Bloqueada`) |
| `ItensLidos` / `ItensAceitos` | int |
| `Descartes` | lista de (`MotivoDescarte`, quantidade) |
| `Erro` | string? (sem dado pessoal) |

Comportamentos: `Iniciar`, `RegistrarAceite`, `RegistrarDescarte(motivo)`, `Concluir`, `Falhar(erro)`, `MarcarBloqueada`. Transições inválidas (concluir uma coleta já falha) retornam erro.

### Serviço de domínio `InterpretadorDeDescricao`

Entrada: `OfertaBruta` (descrição, marca informada pela drogaria, SKU, preços, link, disponibilidade). Saída: `Result<ProdutoInterpretado>` ou `MotivoDescarte`.

Regras:

1. Descartar se a descrição não contém "fralda" nem "pants" (ou se contém termos da lista de exclusão: "lenço", "toalha", "pomada", "geriátrica", "adulto").
2. Tamanho: exatamente um tamanho reconhecido; nenhum ou dois → descarte (`TamanhoNaoIdentificado`, `TamanhoAmbiguo`).
3. Unidades: número seguido de "un", "unid", "unidades" ou "tiras"; nomes de embalagem (Mega, Hiper, Jumbo, Econômica) não contam.
4. Kit: "Kit N" ou "N pacotes" → `PacotesNoKit = N`.
5. Tipo: "pants" ou "calça" → `Pants`; senão `Fralda`.
6. Marca: primeiro pela marca informada pela drogaria; depois pela lista de marcas conhecidas; por fim pela lista de marcas próprias da drogaria.

Motivos de descarte (`MotivoDescarte`): `NaoEhFralda`, `TamanhoNaoIdentificado`, `TamanhoAmbiguo`, `QuantidadeNaoIdentificada`, `PrecoInvalido`, `PrecoPorUnidadeImplausivel`, `UrlForaDoDominio`, `Duplicado`.

## Contexto Preferencias

### Agregado `PreferenciaDeBusca`

| Campo | Tipo |
| --- | --- |
| `UsuarioId` | Guid (chave) |
| `Tamanho` | `Tamanho?` |
| `Marcas` | conjunto de `Marca` |
| `Tipo` | `FiltroTipo` (`Fralda`, `Pants`, `Ambos`) |
| `Linha` | `Linha?` |
| `AtualizadaEm` | `DateTimeOffset` |

- `PreferenciaDeBusca.Padrao(usuarioId)`: todas as marcas padrão, tipo `Ambos`, sem tamanho.
- `Atualizar(tamanho, marcas, tipo, linha, instante)`: o conjunto de marcas não pode ficar vazio.

## Lado de leitura

`OfertaDto` (consulta, sem agregado): `ProdutoId`, `Drogaria`, `Descricao`, `Marca`, `Linha`, `Tipo`, `Tamanho`, `TotalUnidades`, `Preco`, `PrecoPorUnidade`, `PrecoCondicional`, `DescricaoCondicao`, `Disponivel`, `Url`, `UrlImagem`, `ColetadaEm`.

`SituacaoDasDrogariasDto`: por drogaria, `UltimaColetaConcluidaEm` e `UltimaSituacao`.

## Esquema relacional (PostgreSQL)

Schemas por contexto: `ofertas`, `preferencias`, `identidade`.

```text
ofertas.produtos
  id uuid PK
  drogaria varchar(30) NOT NULL
  sku varchar(100) NOT NULL
  descricao varchar(300) NOT NULL
  marca varchar(60) NOT NULL
  marca_propria boolean NOT NULL
  linha varchar(80) NULL
  tipo varchar(10) NOT NULL
  tamanho varchar(5) NOT NULL
  unidades_por_pacote int NOT NULL CHECK (unidades_por_pacote BETWEEN 1 AND 300)
  pacotes_no_kit int NOT NULL CHECK (pacotes_no_kit BETWEEN 1 AND 10)
  url varchar(500) NOT NULL
  url_imagem varchar(500) NULL
  visto_pela_ultima_vez_em timestamptz NOT NULL
  UNIQUE (drogaria, sku)
  INDEX (tamanho, tipo)

ofertas.coletas
  id uuid PK
  drogaria varchar(30) NOT NULL
  iniciada_em timestamptz NOT NULL
  finalizada_em timestamptz NULL
  situacao varchar(20) NOT NULL
  itens_lidos int NOT NULL
  itens_aceitos int NOT NULL
  descartes jsonb NOT NULL
  erro varchar(1000) NULL
  INDEX (drogaria, iniciada_em DESC)

ofertas.cotacoes
  id uuid PK
  produto_id uuid NOT NULL FK -> produtos
  coleta_id uuid NOT NULL FK -> coletas
  preco numeric(10,2) NOT NULL CHECK (preco > 0)
  preco_anterior numeric(10,2) NULL
  preco_condicional numeric(10,2) NULL
  descricao_condicao varchar(200) NULL
  disponivel boolean NOT NULL
  coletada_em timestamptz NOT NULL
  INDEX (produto_id, coletada_em DESC)

ofertas.ofertas_atuais (view)
  última cotação por produto, preço por unidade calculado,
  só produtos vistos na última coleta concluída da drogaria

preferencias.preferencias_de_busca
  usuario_id uuid PK
  tamanho varchar(5) NULL
  marcas jsonb NOT NULL
  tipo varchar(10) NOT NULL
  linha varchar(80) NULL
  atualizada_em timestamptz NOT NULL
```

A conta de banco do `Worker` escreve em `ofertas` e não tem acesso a `identidade` nem a `preferencias`. A conta do `Web` lê `ofertas` e escreve em `identidade` e `preferencias`.

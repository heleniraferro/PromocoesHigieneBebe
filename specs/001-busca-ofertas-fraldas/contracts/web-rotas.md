# Contrato: rotas da Web

Todas as rotas exigem autenticação, exceto as de `/Conta/*` (FR-024). Respostas de fragmento HTMX devolvem só o HTML parcial.

| Método | Rota | Tipo | Entrada | Saída |
| --- | --- | --- | --- | --- |
| GET | `/` | Página | — | Tela de busca com a preferência salva já aplicada e os resultados dela |
| GET | `/ofertas` | Fragmento HTMX | `tamanho` (obrigatório), `marcas[]`, `tipo` (`fralda`, `pants`, `ambos`), `linha`, `ordem` (`preco-por-unidade`, `preco`, `quantidade`) | `_ListaDeOfertas` + `_SituacaoDasLojas` |
| POST | `/preferencias` | Fragmento HTMX | Mesmos campos de `/ofertas` + token antiforgery | 204; salva a preferência (FR-023) |
| GET | `/Conta/Entrar` | Página | — | Login por passkey ou e-mail e senha |
| POST | `/Conta/Entrar` | Formulário | Credenciais + antiforgery | Redireciona para `/` |
| POST | `/Conta/Sair` | Formulário | Antiforgery | Redireciona para `/Conta/Entrar` |
| GET | `/Conta/Passkeys` | Página | — | Gerenciar passkeys do usuário |

## Validação de entrada

- `tamanho` deve ser um código de `Tamanho`; senão, 400 com mensagem amigável no fragmento.
- `marcas[]` só aceita marcas conhecidas ou `marcas-proprias`; vazio volta ao padrão.
- `linha` até 80 caracteres; nunca é interpolada em SQL nem renderizada sem codificação.

## Fragmento `_ListaDeOfertas`

Para cada `OfertaDto`, na ordem pedida:

- Preço por unidade (destaque) e etiqueta "Mais barato" na primeira oferta disponível.
- Descrição, drogaria, preço do pacote e total de unidades (ex.: "kit 3 × 60 un").
- Quando houver condição: "Com cadastro: R$ x,xx por fralda" em linha separada (FR-017).
- Link para a drogaria com `rel="noopener noreferrer"` e `target="_blank"`.
- Indisponíveis ao final, esmaecidas.
- Sem resultado: "Nenhuma fralda {tamanho} dessas marcas" e um botão para incluir todas as marcas.

## Fragmento `_SituacaoDasLojas`

"Atualizado há {idade} · {n} de 4 lojas" e, se houver falha, o nome das drogarias que ficaram de fora (FR-019). Idade acima de 30 h em destaque (`Coleta:DadoVelhoApos`), o que indica que a coleta diária falhou.

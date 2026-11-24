# Especificação da feature: Busca das ofertas de fralda mais baratas

**Branch:** `001-busca-ofertas-fraldas`
**Criada em:** 2026-11-24
**Status:** Revisada; esclarecimentos resolvidos em 2026-11-24

## Esclarecimentos

### Sessão 2026-11-24

- P: Qual o intervalo da coleta? → R: Uma vez por dia (FR-002).
- P: O preço para cadastrados entra na ordenação? → R: Não; "mais barato" é o preço que qualquer pessoa paga (FR-018).
- Consequência: com coleta diária, o dado só é considerado velho acima de 30 horas, o que indica uma coleta perdida (História 4, cenário 4).
**Origem:** documento "Automação de busca de fraldas — Estudo de UX e Escopo" e aba "Estudo de arquitetura"

> Esta spec descreve **o quê** e **por quê**. Tecnologia fica no `plan.md`.

---

## Contexto

Quem compra fralda com frequência perde de 15 a 20 minutos por compra (estimativa) abrindo quatro sites, filtrando resultados e calculando o preço por unidade de cada pacote. Os pacotes variam muito (de 30 a 94 unidades, além de kits com 2 ou 3 pacotes), então o preço do pacote não serve para comparar.

Esta feature entrega o MVP: o sistema coleta periodicamente as ofertas das 4 drogarias, e o usuário escolhe o tamanho e vê numa lista única a fralda mais barata **por unidade**, com link para comprar na própria drogaria. As 4 drogarias entregam em Cuiabá.

**Fora desta feature:** histórico de preços, alertas (WhatsApp, Telegram, SMS), contas para outras pessoas, CEP configurável, outras categorias (lenço umedecido, pomada), compra automática, frete.

---

## Cenários de usuário e testes *(obrigatório)*

### História 1 — Coleta das ofertas das 4 drogarias (Prioridade: P1)

O sistema busca periodicamente as fraldas à venda nas 4 drogarias, entende o que é cada produto (marca, linha, tipo, tamanho, unidades) e guarda o preço do momento.

**Por que esta prioridade:** sem dados coletados e normalizados, nenhuma comparação é possível.

**Teste independente:** rodar uma coleta usando respostas gravadas de cada drogaria e conferir os produtos e preços registrados.

**Cenários de aceite:**

1. **Dado** que a resposta gravada da Pague Menos traz "Fralda Pampers Confort Sec G 92 Unidades" por R$ 119,90, **quando** a coleta roda, **então** fica registrado um produto Pampers, linha Confort Sec, tamanho G, 92 unidades, com cotação de R$ 119,90.
2. **Dado** um produto "Kit 3 Fraldas Pampers Pants Ajuste Total G 90 Unidades", **quando** a coleta roda, **então** a embalagem fica com 90 unidades por pacote, 3 pacotes e total de 270 unidades.
3. **Dado** um item que não é fralda (ex.: "Lenço Umedecido Pampers 192 unidades"), **quando** a coleta roda, **então** o item é descartado e o motivo fica registrado.
4. **Dado** um item cujo tamanho ou quantidade não pode ser identificado no nome, **quando** a coleta roda, **então** o item é descartado e o motivo fica registrado.
5. **Dado** que uma drogaria não responde, **quando** a coleta roda, **então** as outras 3 são coletadas normalmente e a falha fica registrada com a drogaria e o horário.

---

### História 2 — Ver as ofertas mais baratas por tamanho (Prioridade: P1)

O usuário escolhe um tamanho e vê numa lista única, das 4 drogarias, as ofertas ordenadas da menor para a maior **preço por fralda**, com link para a página do produto.

**Por que esta prioridade:** é o valor central do produto e responde à pergunta "onde está a fralda mais barata no tamanho que eu uso?".

**Teste independente:** com ofertas pré-cadastradas, escolher o tamanho G e conferir a ordem, os valores por unidade e os links.

**Cenários de aceite:**

1. **Dado** ofertas G de 4 drogarias, **quando** escolho o tamanho G, **então** vejo uma única lista ordenada por preço por fralda, crescente, e a primeira tem a etiqueta "Mais barato".
2. **Dado** um kit de 3 × 60 unidades por R$ 180,00, **quando** ele aparece na lista, **então** o preço por fralda exibido é R$ 1,00.
3. **Dado** ofertas de tamanhos M e G, **quando** escolho G, **então** nenhuma oferta de M aparece.
4. **Dado** uma oferta na lista, **quando** toco nela, **então** a página do produto abre na drogaria correspondente.
5. **Dado** uma oferta indisponível (sem estoque), **quando** a lista é exibida, **então** ela aparece no fim, marcada como indisponível.

---

### História 3 — Filtrar por marca e tipo, e lembrar a escolha (Prioridade: P2)

O usuário restringe a lista às marcas que aceita e ao tipo de fralda (fralda comum, pants ou ambos), e na próxima visita a última escolha já vem marcada.

**Por que esta prioridade:** reduz ruído e transforma a busca recorrente em um toque, mas a História 2 já entrega valor sem ela.

**Teste independente:** salvar uma preferência, sair, voltar e conferir que tamanho, marcas e tipo vêm marcados e a lista respeita os filtros.

**Cenários de aceite:**

1. **Dado** um primeiro acesso, **quando** abro a busca, **então** as marcas Pampers, BabySec, Huggies, MamyPoko, Personal e "Marcas próprias" vêm todas marcadas e o tipo é "Ambos".
2. **Dado** que desmarco Huggies, **quando** busco G, **então** nenhuma oferta Huggies aparece.
3. **Dado** que escolho "Pants", **quando** busco G, **então** só aparecem fraldas do tipo pants.
4. **Dado** que busquei G com Pampers e Huggies, **quando** volto outro dia, **então** G, Pampers e Huggies já vêm marcados.
5. **Dado** que informei uma linha (ex.: "Premium Care"), **quando** busco, **então** só aparecem ofertas dessa linha.

---

### História 4 — Confiar no dado exibido (Prioridade: P2)

O usuário sabe quando os preços foram coletados, se alguma drogaria ficou de fora e qual preço depende de condição (cadastro, "leve 3 pague 2").

**Por que esta prioridade:** sem isso, o "mais barato" pode ser um preço velho ou um preço que o usuário não consegue pagar.

**Teste independente:** com uma drogaria marcada como falha e uma oferta com condição, conferir os avisos e a separação dos preços.

**Cenários de aceite:**

1. **Dado** uma coleta concluída há 2 horas, **quando** vejo a lista, **então** aparece "Atualizado há 2 h · 4 de 4 lojas".
2. **Dado** que a última coleta da Drogaria Rosário falhou, **quando** vejo a lista, **então** aparece "3 de 4 lojas" e um aviso dizendo qual loja ficou de fora.
3. **Dado** uma oferta com preço para cadastrados, **quando** ela é exibida, **então** o preço sem condição é o principal e o preço com condição aparece em linha separada, com o texto da condição.
4. **Dado** dados com mais de 30 horas (a coleta diária falhou), **quando** abro a lista, **então** a idade do dado aparece em destaque.

---

### História 5 — Acesso protegido (Prioridade: P3)

Só o usuário cadastrado acessa o sistema publicado na internet, entrando por passkey (digital ou rosto) ou, como alternativa, e-mail e senha com segundo fator.

**Por que esta prioridade:** o sistema roda em nuvem e não pode ficar aberto; mas, em ambiente local, as Histórias 1 a 4 já podem ser validadas sem login.

**Teste independente:** acessar qualquer página sem estar autenticado e ser redirecionado ao login; entrar com passkey e acessar a busca.

**Cenários de aceite:**

1. **Dado** um visitante não autenticado, **quando** acessa qualquer página, **então** é levado à tela de login.
2. **Dado** o usuário cadastrado com passkey, **quando** entra, **então** acessa a busca sem digitar senha.
3. **Dado** 5 tentativas de senha erradas, **quando** tenta a sexta, **então** a conta fica bloqueada temporariamente.
4. **Dado** que o cadastro está fechado, **quando** alguém tenta criar conta, **então** a operação não é permitida.

---

### Casos de borda

- Mesmo produto aparece duas vezes na resposta da drogaria (anúncio patrocinado e resultado normal): registrar uma vez só.
- Tamanho escrito de formas diferentes (`XG` e `EG`; `XXG` e `EXG`; `RN+` e `RN Plus`): tratar como o mesmo tamanho.
- Nomes de embalagem (`Mega`, `Hiper`, `Jumbo`, `Econômica`) não indicam quantidade e não podem ser confundidos com ela.
- Nome com dois tamanhos (ex.: "P/M"): descartar, porque o tamanho é ambíguo.
- Preço zero, negativo ou com preço por fralda fora da faixa plausível: descartar e registrar.
- Link do produto fora do domínio da drogaria: descartar.
- Produto que some da drogaria: deixa de aparecer na lista após a coleta seguinte.
- Nenhuma oferta para os filtros escolhidos: mostrar "Nenhuma fralda G dessas marcas" e a opção de incluir todas as marcas.
- Drogaria bloqueia a coleta (captcha, erro 403 repetido): parar a coleta daquela drogaria e registrar. Nunca tentar contornar.

---

## Requisitos *(obrigatório)*

### Requisitos funcionais

**Coleta (História 1)**

- **FR-001**: O sistema DEVE coletar as ofertas de fralda infantil das drogarias Drogasil, Drogaria São Paulo, Drogaria Rosário e Pague Menos.
- **FR-002**: O sistema DEVE executar a coleta automaticamente uma vez por dia, em horário configurável (padrão: 05:00, horário de Cuiabá), para que os preços estejam atualizados no início do dia.
- **FR-003**: O sistema DEVE consultar os preços como entregues em Cuiabá, usando um CEP fixo de Cuiabá quando a drogaria permitir regionalização.
- **FR-004**: O sistema DEVE identificar no nome do produto a marca, a linha, o tipo (fralda ou pants), o tamanho, as unidades por pacote e a quantidade de pacotes do kit.
- **FR-005**: O sistema DEVE reconhecer os tamanhos RN, RN+, P, M, G, XG, XXG e XXXG e seus sinônimos.
- **FR-006**: O sistema DEVE reconhecer as marcas Pampers, BabySec, Huggies, MamyPoko e Personal, e classificar como "marca própria" as marcas próprias de cada drogaria, a partir de uma lista configurável.
- **FR-007**: O sistema DEVE descartar itens que não são fralda, itens com tamanho ambíguo ou não identificado, itens sem quantidade e itens com preço inválido, registrando o motivo de cada descarte.
- **FR-008**: O sistema DEVE eliminar itens duplicados na mesma coleta.
- **FR-009**: O sistema DEVE registrar, para cada oferta, o preço sem condição, o preço anterior ("de"), o preço com condição e o texto da condição (quando houver) e a disponibilidade.
- **FR-010**: O sistema DEVE aceitar apenas links de produto do domínio oficial da respectiva drogaria.
- **FR-011**: O sistema DEVE registrar, para cada coleta, drogaria, horário, itens lidos, aceitos e descartados, duração e erro (quando houver).
- **FR-012**: A falha na coleta de uma drogaria NÃO DEVE impedir a coleta das demais.

**Busca e comparação (Histórias 2 e 4)**

- **FR-013**: O usuário DEVE poder escolher exatamente um tamanho entre RN, RN+, P, M, G, XG, XXG e XXXG.
- **FR-014**: O sistema DEVE calcular o preço por unidade como preço sem condição ÷ (unidades por pacote × pacotes no kit), com arredondamento para centavos apenas na exibição.
- **FR-015**: O sistema DEVE exibir as ofertas do tamanho escolhido numa lista única, ordenada por preço por unidade crescente, com as indisponíveis ao final.
- **FR-016**: O sistema DEVE permitir ordenar também por preço do pacote e por quantidade de unidades.
- **FR-017**: Cada oferta exibida DEVE mostrar preço por unidade, descrição, drogaria, preço do pacote, link para a drogaria e, quando houver, o preço com condição em linha separada com o texto da condição.
- **FR-018**: A ordenação NÃO DEVE usar o preço com condição: "mais barato" é sempre o preço que qualquer pessoa paga.
- **FR-019**: O sistema DEVE exibir a idade da última coleta e quantas drogarias responderam, nomeando as que falharam.
- **FR-020**: O sistema DEVE mostrar a lista a partir dos dados já coletados, sem esperar uma coleta nova.

**Filtros e preferências (História 3)**

- **FR-021**: O usuário DEVE poder filtrar por marcas (múltipla escolha), tipo (fralda, pants, ambos) e linha (texto opcional).
- **FR-022**: No primeiro acesso, todas as marcas padrão DEVEM vir marcadas e o tipo DEVE ser "ambos".
- **FR-023**: O sistema DEVE lembrar o último tamanho, as marcas e o tipo usados pelo usuário.

**Acesso (História 5)**

- **FR-024**: Todas as páginas, exceto login, DEVEM exigir autenticação quando o sistema estiver publicado.
- **FR-025**: O usuário DEVE poder entrar por passkey e, como alternativa, por e-mail e senha com segundo fator.
- **FR-026**: O cadastro de novos usuários DEVE estar fechado nesta feature.
- **FR-027**: O sistema DEVE bloquear temporariamente a conta após 5 tentativas de login falhas e limitar a quantidade de requisições por usuário.

### Entidades-chave

- **Drogaria**: uma das 4 redes, com nome e domínio oficial.
- **Produto**: uma fralda vendida por uma drogaria, com marca, linha, tipo, tamanho e embalagem.
- **Embalagem**: unidades por pacote e pacotes no kit; define o total de unidades.
- **Cotação**: o preço de um produto num momento, com preço anterior, preço com condição e disponibilidade.
- **Coleta**: uma execução de coleta numa drogaria, com contagens, duração e resultado.
- **Preferência de busca**: o último tamanho, as marcas e o tipo escolhidos por um usuário.

Definições completas e invariantes ficam em `data-model.md`.

---

## Critérios de sucesso *(obrigatório)*

- **SC-001**: A lista de ofertas aparece em até 2 segundos depois que o usuário escolhe o tamanho.
- **SC-002**: A partir da tela inicial, com as preferências salvas, o usuário chega à oferta mais barata com no máximo 2 toques.
- **SC-003**: Numa amostra de 50 nomes reais de produtos, pelo menos 95% têm tamanho e quantidade identificados corretamente, e nenhum é identificado com valor errado (na dúvida, descarta).
- **SC-004**: O preço por unidade confere com o cálculo manual em 10 ofertas escolhidas ao acaso, incluindo 3 kits.
- **SC-005**: Nenhuma oferta de tamanho diferente do escolhido aparece na lista.
- **SC-006**: Uma coleta completa das 4 drogarias termina em até 10 minutos.
- **SC-007**: A queda de uma drogaria afeta apenas as ofertas dela; as demais continuam atualizadas.

---

## Premissas

- Preço exibido é o do site para Cuiabá; preço de loja física e de aplicativo não fazem parte.
- Uma oferta "mais barata" considera só o preço sem condição.
- Na feature 001 existe um único usuário, criado por linha de comando ou seed.
- A ordem e a quantidade de resultados das drogarias podem mudar sem aviso; a coleta lê todas as páginas de resultado até um limite configurável.

## Dependências

- APIs públicas e páginas de busca das 4 drogarias (ver `research.md`).
- Viabilidade validada na Fase 0 (teste de coleta) antes das tarefas da História 1.

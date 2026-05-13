# Tutorial — Framework de mini-jogos educacionais (Unity)

Este guia descreve o que já está implementado neste repositório e como autores (designers/educadores) e programadores trabalham com **Bootstrap**, **Hub**, **cenas aditivas** `MG_*` e **ScriptableObjects**.

---

## 1. Fluxo de execução

1. **Cena Bootstrap** — Carrega serviços persistentes (`BootstrapScope` registra áudio, input, progresso e localização em `AppContext`). O `AppSessionController` carrega em seguida a cena do Hub (por padrão o nome é `Hub`).
2. **Cena Hub** — O `HubWorldController` lê um `HubConfigurationSO` (catálogo + nós do mapa). Cada nó carrega **aditivamente** a cena cujo nome está no `MiniGameCatalogSO` (campo *additive scene name*).
3. **Mini-jogo** — Um componente que herda `MiniGameBase` inicia no `Start` **somente se** `AppContext` já estiver inicializado e existir um `MiniGameConfigSO` atribuído. Ao terminar a sessão, o hub é solicitado via `MiniGameSessionHub` e o `HubWorldController` descarrega cujos nomes começam com `MG_`.

**Convenção importante:** cenas de mini-jogo aditivas devem usar o prefixo **`MG_`** no nome do asset de cena, para o descarregamento automático funcionar (veja `HubWorldController`).

---

## 2. Primeiro uso no Editor

### Gerar conteúdo de demonstração

No menu Unity:

**`Edu Framework` → `Generate Default Content (SOs + Scenes + Hub Wire)`**

Isso cria (sob `Assets/EduFramework/`):

- ScriptableObjects de exemplo (`ScriptableObjects/Generated/`), incluindo `MiniGameCatalog_Default`, `HubConfiguration_Default`, desafios e `MiniGameConfig` por jogo.
- Cenas aditivas em `Scenes/MiniGames/` (`MG_Consonants`, `MG_Counting`, etc.).
- Prefabs de shell em `Prefabs/UI/` (se ainda não existirem).
- O **prefab template** de múltipla escolha em `Prefabs/MiniGames/MiniGame_Template_MultipleChoice.prefab`.
- Entradas extras em **File → Build Settings** para as cenas `MG_*`.

Abra a cena **Hub** em `Assets/Scenes/Hub.unity` (ou a cena do seu projeto que contém `HubWorldController`), confira o `HubConfigurationSO` no Inspector após a geração, e inicie o jogo a partir da cena **Bootstrap** em `Assets/Scenes/Bootstrap.unity`.

> **Nota:** O *wire* automático procura uma cena **`Hub.unity`** no projeto (por exemplo `Assets/Scenes/Hub.unity`), abre essa cena e grava o `HubConfigurationSO` no `HubWorldController`. Se não houver cena com esse nome ou a abertura falhar, aparece um aviso no Console: nesse caso atribua manualmente o campo **Configuration** ao `HubConfiguration_Default.asset`.

---

## 3. Camadas de dados (autoria sem código)

Ordem típica de criação:

| Ordem | Asset | Menu *Create* |
|------|--------|----------------|
| 1 | Desafios (`ChallengeSO` e tipos derivados) | `Edu/Data/Challenge/...` |
| 2 | `ChallengeSetSO` — lista de desafios | `Edu/Data/Challenge Set` |
| 3 | `DifficultyProfileSO` (rodadas, número de opções, etc.) | `Edu/Data/Difficulty Profile` |
| 4 | `MiniGameConfigSO` — `gameId`, set, dificuldade, opcional `StoryBookSO`, cues de áudio | `Edu/Data/Mini Game Config` |
| 5 | `MiniGameCatalogSO` — uma entrada por jogo: `gameId`, chave de nome, **nome da cena aditiva** | `Edu/Data/Mini Game Catalog` |
| 6 | `HubConfigurationSO` — referência ao catálogo + lista de nós (`linkedGameId` = `gameId` do catálogo) | `Edu/Data/Hub Configuration` |

Chaves de conceito (ex.: `phoneme:/b/`) ficam nos desafios e alimentam o **ProgressService** quando `MiniGameBase` chama `RaiseAnswerEvaluated`.

---

## 4. Prefab template de mini-jogo (múltipla escolha)

### Gerar só o template

**`Edu Framework` → `Create Template Prefab (Multiple Choice Mini-Game)`**

Arquivo gerado: **`Assets/EduFramework/Prefabs/MiniGames/MiniGame_Template_MultipleChoice.prefab`**

Contém no mesmo objeto raiz:

- `MultipleChoiceShellView` — monta a fileira de botões a partir do `MultipleChoiceChallengeSO`.
- `MiniGameMultipleChoice` — laço de rodadas genérico usando o `ChallengeSet` do `MiniGameConfigSO`.

### Usar o template em um jogo novo

1. **Duplique** uma cena `MG_*.unity` existente ou crie uma cena vazia e salve como `Assets/EduFramework/Scenes/MiniGames/MG_MeuJogo.unity` (nome com prefixo `MG_`).
2. Arraste o prefab **`MiniGame_Template_MultipleChoice`** para a hierarquia (ou substitua o `MiniGameRoot` gerado pelo script).
3. No Inspector do raiz, em **Mini Game Multiple Choice**, arraste o seu **`MiniGameConfigSO`** (e, se usar, canais de eventos `AnswerEvaluatedEventChannelSO` / `MiniGameSessionEventChannelSO`).
4. Garanta que o **`gameId`** dentro do `MiniGameConfigSO` seja o mesmo usado no catálogo e no nó do hub.
5. Adicione a cena em **File → Build Settings** e marque-a como incluída no build.
6. No `MiniGameCatalogSO`, adicione uma entrada com **Additive Scene Name** = nome da cena (sem `.unity`).
7. No `HubConfigurationSO`, adicione um nó com **Linked Game Id** = mesmo `gameId`.

Para jogos que **não** são múltipla escolha pura, use as cenas/componentes já existentes como referência: `MiniGameCounting`, `MiniGameMemory`, `MiniGameStoryGaps`, `MiniGameClassification`, `MiniGameSyllableBuilder`.

---

## 5. Programador: estender com um mini-jogo próprio

1. Subclasse **`MiniGameBase`** e implemente `RunSessionRoutine()` como `IEnumerator` (padrão dos jogos atuais).
2. Use `Context.Audio`, `Context.Progress`, `Context.Localization`, etc.
3. Ao avaliar uma resposta, chame `RaiseAnswerEvaluated` e `PlayFeedback` quando fizer sentido.
4. Registre o tipo no **Bootstrap** não é necessário — o `MiniGameBase` resolve tudo via `AppContext` após o bootstrap.

---

## 6. Ferramentas úteis

- **Exportação para professor:** `Edu Framework` → `Teacher Export…` — lê o JSON de progresso do slot escolhido em `Application.persistentDataPath` e gera CSV.

---

## 7. Resumo rápido

| Objetivo | Onde |
|----------|------|
| Serviços globais | Cena Bootstrap + `BootstrapScope` |
| Mapa e carregamento | Hub + `HubWorldController` + `HubConfigurationSO` |
| Registro de jogos | `MiniGameCatalogSO` |
| Conteúdo de uma sessão | `MiniGameConfigSO` → `ChallengeSetSO` → desafios |
| Template visual MC | `MiniGame_Template_MultipleChoice.prefab` |
| Descarregar mini-jogos | Nomes de cena começando com `MG_` |

Para a visão de arquitetura completa (áudio, narração, BNCC, shells genéricos), alinhe este tutorial ao documento de planejamento do produto que descreve o framework educacional.

# Framework de Mini-Jogos Educacionais — Manual do Usuário

> Um guia completo, passo a passo, para **desenvolvedores**, **game designers** e **educadores**
> que vão usar o Framework Unity de Mini-Jogos Educacionais (esqueleto com Hub + 6 mini-jogos,
> projetado para crianças de 4 a 6 anos).

---

## Sumário

1. [Introdução](#1-introdução)
2. [Primeiros passos](#2-primeiros-passos)
3. [Conceitos principais (em linguagem simples)](#3-conceitos-principais-em-linguagem-simples)
4. [Estrutura do projeto explicada](#4-estrutura-do-projeto-explicada)
5. [Como usar o framework (seção principal)](#5-como-usar-o-framework-seção-principal)
   - [5.1 Criar um novo mini-jogo SEM CÓDIGO](#51-criar-um-novo-mini-jogo-sem-código)
   - [5.2 Editar um mini-jogo existente](#52-editar-um-mini-jogo-existente)
6. [Customização da UI (muito importante)](#6-customização-da-ui-muito-importante)
7. [Uso do sistema de áudio](#7-uso-do-sistema-de-áudio)
8. [Adicionando conteúdo educacional](#8-adicionando-conteúdo-educacional)
9. [Testando o jogo](#9-testando-o-jogo)
10. [Guia de depuração](#10-guia-de-depuração)
11. [Estendendo o framework (avançado)](#11-estendendo-o-framework-avançado)
12. [Boas práticas para design educacional](#12-boas-práticas-para-design-educacional)
13. [Checklist para publicar um novo mini-jogo](#13-checklist-para-publicar-um-novo-mini-jogo)
14. [Apêndice: glossário e referência de menus](#14-apêndice-glossário-e-referência-de-menus)

---

## 1. Introdução

### O que é este framework

Este é um **framework Unity (C#) para construir mini-jogos educacionais dirigidos por um hub**,
voltado a aulas de letramento inicial e numeracia inicial. Já vem pronto, fora da caixa, com:

- Uma cena **Bootstrap** persistente que inicializa os serviços principais (áudio, input, salvamento, idioma).
- Uma cena **Hub** com um mapa interativo de nós (um por mini-jogo).
- **Seis templates de mini-jogo** carregados aditivamente a partir do Hub:
  1. **Consonants** — múltipla escolha fonética
  2. **Counting** — discriminação de quantidades
  3. **Memory** — pares de som/imagem
  4. **Story** — história interativa com lacunas de múltipla escolha
  5. **Classify** — arrastar tokens para o recipiente correto
  6. **Syllables** — formar uma palavra tocando sílabas na ordem certa
- **Assets de dados em ScriptableObject** para desafios, dificuldade, cues de áudio, catálogo
  e regras de desbloqueio — para que autores criem conteúdo sem escrever código.

### Para quem é

| Leitor | O que ganha |
|--------|---------------|
| **Educadores / autores de conteúdo** | Fluxo arraste-e-solte: duplicar um template, encaixar arte e áudio, clicar em Play. |
| **Game designers (não programadores)** | Pipeline de conteúdo orientado a dados via ScriptableObjects e prefabs. |
| **Desenvolvedores Unity** | Pontos de extensão claros (`MiniGameBase`, canais de evento, interfaces de serviço) para criar novos tipos de mini-jogo e sistemas. |

### Problemas que resolve

- **Cada novo mini-jogo é só dados + uma cena** — não precisa de C# por jogo a menos que o comportamento seja realmente novo.
- **Um único Hub** controla navegação, desbloqueios e carregamento aditivo; mini-jogos ficam isolados.
- **Áudio em primeiro lugar**: a narração tem sua própria lógica de prioridade/fila/interrupção, para que crianças pré-leitoras não dependam de texto.
- **Analytics para o professor** já vêm embutidas: cada resposta é registrada por chave de conceito e pode ser exportada para CSV.

---

## 2. Primeiros passos

### 2.1 Software necessário

- **Unity 6 (6000.4.5f1)** — versão exata fixada em `ProjectSettings/ProjectVersion.txt`. Abra com o **Unity Hub** → *Open* → selecione a pasta do projeto. O Unity Hub oferecerá instalar o editor correspondente se não estiver instalado.
- Pacote **Unity Input System** (já incluído em `Packages/`).
- **Git** (opcional, recomendado para versionar o conteúdo).

> ⚠️ **Atenção:** abrir o projeto com uma versão mais antiga do Unity pode corromper as cenas e os arquivos `*.asset`. Sempre use a versão fixada.

### 2.2 Abrindo o projeto

1. Abra o **Unity Hub**.
2. Clique em **Open** → navegue até a raiz do projeto (a pasta que contém `Assets/`, `Packages/`, `ProjectSettings/`).
3. Aguarde a primeira importação (5–15 minutos em uma máquina nova — o Unity constrói o banco de assets).
4. Quando o editor abrir, procure o menu **`Edu Framework`** na barra superior. Se ele aparecer, o código do framework compilou corretamente.

### 2.3 Gerar o conteúdo de demonstração (primeira execução)

Antes de apertar Play, gere os dados e cenas de demo:

1. Na barra de menu, clique em **`Edu Framework → Generate Default Content (SOs + Scenes + Hub Wire)`**.
2. Observe a Console esperando a mensagem *"Edu Framework: default content generated."*
3. Isso cria os ScriptableObjects de demonstração, as seis cenas `MG_*` de mini-jogo, os prefabs de shell,
   adiciona as cenas em **Build Settings**, e conecta o **`HubConfigurationSO`** à cena do Hub.

### 2.4 Aperte Play

1. Abra **`Assets/Scenes/Bootstrap.unity`** (esta é a cena de entrada).
2. Confira se o GameObject raiz da cena Bootstrap tem todos estes componentes: `BootstrapScope`, `AudioDirector`, `InputRouter`, `ProgressService`, `LocalizationService` **e** `AppSessionController` (ver `BootstrapScope.cs`). Todos são obrigatórios para o boot.
3. Aperte **Play**.
4. A cena Bootstrap carrega, registra os serviços no `AppContext`, e em seguida o `AppSessionController` carrega a cena **Hub**.
5. Você verá seis botões verdes no mapa. Clique em um → o mini-jogo carrega aditivamente → você joga → ao terminar, retorna ao Hub.

> 💡 **Dica:** se apertar Play em `Hub.unity` direto e aparecer *"AppContext not ready"*, sempre comece por `Bootstrap.unity`. Os serviços só são inicializados na cena Bootstrap.

### 2.5 Visão geral das pastas (uma linha cada)

| Pasta | O que mora aqui |
|--------|----------------|
| `Assets/Scenes/` | As duas cenas persistentes pelas quais você começa: `Bootstrap.unity` e `Hub.unity`. |
| `Assets/Scripts/` | Todo o código C#, agrupado por sistema (Core, Audio, Input, UI, Hub, MiniGames, Progress, Data, Editor). |
| `Assets/EduFramework/Docs/` | Este manual e os tutoriais em PT-BR. |
| `Assets/EduFramework/Prefabs/` | Prefabs reutilizáveis: shells de UI (múltipla escolha, drag-drop) e o template de mini-jogo. |
| `Assets/EduFramework/Scenes/MiniGames/` | As seis cenas aditivas de mini-jogo (`MG_*.unity`). |
| `Assets/EduFramework/ScriptableObjects/Generated/` | Assets de demo criados pelo *Generate Default Content*. |
| `Packages/` | Pacotes Unity (Input System, etc.). Não edite manualmente. |
| `Library/`, `Temp/`, `Logs/` | Gerados pelo Unity. **Nunca commite, nunca edite, pode deletar com segurança.** |

---

## 3. Conceitos principais (em linguagem simples)

### O que é um "Mini-jogo"?

Um **mini-jogo** é uma atividade curta para a criança (≈1–3 minutos). Tecnicamente, ele é:

- Uma **cena** cujo nome começa com `MG_` (por exemplo, `MG_Consonants.unity`).
- Um **GameObject raiz** com um componente que herda de **`MiniGameBase`** (por exemplo, `MiniGameMultipleChoice`).
- Um asset **`MiniGameConfigSO`** arrastado para o campo `_configAsset` desse componente.

O ciclo de vida do `MiniGameBase` roda automaticamente: no `Start`, ele lê a config, monta a UI shell, faz perguntas ao jogador, registra respostas e avisa o Hub quando termina.

### O que é o "Hub"?

O **Hub** é o mapa do mundo mostrado entre os mini-jogos. É uma cena Unity (`Hub.unity`) com um único componente, **`HubWorldController`**, que:

1. Lê um asset `HubConfigurationSO`.
2. Cria um **botão de nó** para cada entrada `HubMapNode`.
3. Ao clicar em um nó, carrega **aditivamente** a cena `MG_*` correspondente, encontrada no `MiniGameCatalogSO`.
4. Quando o mini-jogo termina, descarrega toda cena cujo nome comece com `MG_` e mostra o canvas do Hub novamente.

### O que são ScriptableObjects?

Um **ScriptableObject (SO)** é um asset Unity que guarda **dados**, não comportamento. Pense nele como um arquivo JSON tipado que mora na janela Project.

- Crie pelo menu **`Assets → Create → Edu → …`** (ou clique com o botão direito na janela Project).
- **Dê dois cliques** no asset para editar os campos no Inspector.
- **Arraste** o asset para campos de MonoBehaviour para criar referências.
- São **compartilhados**: mude um `MiniGameConfigSO` e toda cena que o usa enxerga os novos valores.

Exemplos neste projeto:
`AudioCueSO`, `MultipleChoiceChallengeSO`, `ChallengeSetSO`, `DifficultyProfileSO`,
`MiniGameConfigSO`, `MiniGameCatalogSO`, `HubConfigurationSO`, `UnlockRuleSO`,
`LocalizedTableSO`, `BnccTagSO`, `StoryBookSO`, `StoryPageSO`.

### Como os dados conduzem o jogo

Esta é a cadeia de referências em tempo de execução:

```
HubConfigurationSO
   ├── MiniGameCatalogSO   ─── entrada: { gameId, displayNameKey, additiveSceneName }
   └── HubMapNode[]        ─── por nó: { nodeId, linkedGameId, uiPosition, unlockRule }

Cena MG_<jogo>.unity
   └── GameObject MiniGameRoot
        ├── MiniGameMultipleChoice (script) ─── _configAsset → MiniGameConfigSO
        └── MultipleChoiceShellView (script)

MiniGameConfigSO
   ├── _challengeSet → ChallengeSetSO ─── List<ChallengeSO>
   ├── _difficulty   → DifficultyProfileSO
   ├── _successCue   → AudioCueSO
   └── _neutralRetryCue → AudioCueSO
```

A mesma string `gameId` amarra tudo: ela aparece no `MiniGameConfigSO`, no `MiniGameCatalogEntry` e em `HubMapNode.LinkedGameId`. **IDs trocados são a causa #1 de "o nó não faz nada".**

---

## 4. Estrutura do projeto explicada

### 4.1 Scripts (`Assets/Scripts/`)

Resumo amigável do que cada pasta contém:

| Pasta | O que faz |
|--------|-------------|
| `Core/` | Sistema de boot. `AppContext` é o service locator global. `BootstrapScope` registra os serviços no `Awake`. `AppSessionController` carrega o Hub depois do bootstrap e acompanha o relógio da sessão. |
| `Core/Events/` | Canais de evento em ScriptableObject (`AnswerEvaluatedEventChannelSO`, `MiniGameSessionEventChannelSO`, `VoidEventChannelSO`) — barramentos opcionais para desacoplar Hub/UI/analytics. |
| `Audio/` | `AudioDirector` (fachada), `SfxPlayer`, `NarrationController` (fila com prioridade), `MusicController` (ducking), `AudioCueSO`, enum `AudioPriority`. |
| `Input/` | `InputRouter` — encapsula o Unity Input System e expõe propriedades semânticas como `PrimaryClickPressedThisFrame`. |
| `UI/` | UI compartilhada: `MultipleChoiceShellView`, `DragDropSlotsShellView`, `DraggableUI`, `DropBinView`, `FeedbackKit`, `GameplayUiUtility`. |
| `Hub/` | `HubWorldController` (cria nós, carrega/descarrega cenas `MG_*`) e `MiniGameSessionHub` (o delegate `RequestExitToHub`). |
| `MiniGames/` | `MiniGameBase` (ciclo de vida), `MiniGameContext`, `EvaluationResult`, `ChallengePicker`, e uma classe concreta por jogo (`MiniGameMultipleChoice`, `MiniGameCounting`, `MiniGameMemory`, `MiniGameStoryGaps`, `MiniGameClassification`, `MiniGameSyllableBuilder`). |
| `Progress/` | `ProgressService` (salva localmente em JSON sob `Application.persistentDataPath`) e `ProgressExportUtility` (CSV para o professor). |
| `Localization/` | `LocalizationService` com tabela de fallback pt-BR no código e suporte a assets `LocalizedTableSO`. |
| `Data/` | Todos os **tipos** de ScriptableObject (Challenge, ChallengeSet, MiniGameConfig, MiniGameCatalog, HubConfiguration, UnlockRule, LocalizedTable, BnccTag, StoryBook). |
| `Editor/` | Ferramentas só de editor: `EduDefaultContentBuilder` (o menu *Generate Default Content*) e `TeacherExportWindow`. |

### 4.2 Prefabs (`Assets/EduFramework/Prefabs/`)

- **`UI/MultipleChoiceShellView.prefab`** — Prefab vazio com o componente `MultipleChoiceShellView`, usado como pai dos tiles de escolha montados em runtime.
- **`UI/DragDropSlotsShellView.prefab`** — Mesma ideia para jogos de arrastar e soltar.
- **`MiniGames/MiniGame_Template_MultipleChoice.prefab`** — Raiz pronta para jogos de múltipla escolha. Carrega `MultipleChoiceShellView` **e** `MiniGameMultipleChoice` no mesmo objeto raiz — arraste para uma cena `MG_*` nova, atribua sua config, pronto.

### 4.3 ScriptableObjects (`Assets/EduFramework/ScriptableObjects/Generated/`)

O menu **`Generate Default Content`** preenche esta pasta com um exemplar de cada:
`Ch_*.asset` (desafios), `Set_*.asset` (challenge sets), `Cfg_*.asset` (configs de mini-jogo),
`Difficulty_Default.asset`, `Unlock_Always.asset`, `MiniGameCatalog_Default.asset`,
`HubConfiguration_Default.asset`, `StoryPage_01.asset`, `StoryBook_Demo.asset`.

Você pode continuar autorando dentro desta pasta ou criar uma pasta irmã para seu próprio conteúdo (ex.: `ScriptableObjects/Curso2025/`). O framework localiza os assets por **referências diretas**, não pelo nome da pasta.

### 4.4 Cenas (`Assets/Scenes/` e `Assets/EduFramework/Scenes/MiniGames/`)

- **`Bootstrap.unity`** — cena permanente de serviços. Carregada primeiro.
- **`Hub.unity`** — o mapa do mundo.
- **`MG_*.unity`** — seis cenas aditivas de mini-jogo. Cada uma contém um único GameObject `MiniGameRoot` com um componente `MiniGame*` nele.

> ⚠️ **Regra de nomenclatura:** o **nome do arquivo da cena precisa começar com `MG_`** (ver `HubWorldController.UnloadMiniGamesRoutine`). Qualquer outro nome não será descarregado quando o jogador voltar ao Hub.

### 4.5 Áudio

Os assets de áudio (clips) podem ficar em qualquer pasta sob `Assets/` (recomendamos `Assets/EduFramework/Audio/Voice/`, `Sfx/`, `Music/`). Os clips são referenciados indiretamente via assets **`AudioCueSO`** — nunca aponte um `AudioClip` puro em um GameObject; sempre embrulhe em uma cue para manter prioridade/cooldown/volume consistentes.

### 4.6 UI

**Não existe um canvas de UI pré-fabricado.** A UI deste framework é construída **em tempo de execução** pelos componentes shell (`MultipleChoiceShellView`, `DragDropSlotsShellView`, etc.) e pelo helper `GameplayUiUtility.CreateOverlayCanvas`. Para customizar a UI, você pode:

- substituir o canvas construído em runtime por um prefab próprio e atribuí-lo ao campo `_choicesParent` / `_binsParent` da shell, ou
- estender a shell e sobrescrever o layout (ver [§ 6](#6-customização-da-ui-muito-importante)).

---

## 5. Como usar o framework (seção principal)

### 5.1 Criar um novo mini-jogo SEM CÓDIGO

Use este caminho quando seu novo mini-jogo se encaixa no **padrão de múltipla escolha** ("ouvir / olhar → escolher o tile certo").

#### Passo 1 — Confirme que o template existe

No menu, clique em **`Edu Framework → Create Template Prefab (Multiple Choice Mini-Game)`**.

Isso cria (se ainda não existir) **`Assets/EduFramework/Prefabs/MiniGames/MiniGame_Template_MultipleChoice.prefab`** e o destaca (ping) na janela Project. Ele contém um único GameObject com `MultipleChoiceShellView` e `MiniGameMultipleChoice` juntos.

#### Passo 2 — Autorar as cues de áudio

Você normalmente precisa de três cues por desafio:

1. **Prompt** — "Qual começa com B?"
2. **Sucesso** — pequeno tilintar positivo.
3. **Tentar de novo neutro** — voz calma, "vamos tentar de novo" (nunca dura).

Para cada:
- Arraste o `.wav` / `.mp3` para `Assets/EduFramework/Audio/Voice/` ou similar.
- **Botão direito → `Create → Edu → Audio → Audio Cue`**.
- Atribua o clip ao campo **Clip**, escolha a **Priority** (`Tutorial` para prompts, `Celebration` para sucesso, `Correction` para tentar de novo) e ajuste **Volume Scale** se necessário.

#### Passo 3 — Autorar os desafios

Para cada pergunta:
- **Botão direito → `Create → Edu → Data → Challenge → Multiple Choice`**.
- No Inspector:
  - **Id**: uma string curta e estável, como `mc_letter_b`.
  - **Concept keys**: uma ou mais chaves de analytics, ex.: `phoneme:/b/`. Elas aparecem no CSV do professor, por criança.
  - **Stimulus Image**: sprite central opcional.
  - **Option Images**: lista ordenada de sprites para os tiles.
  - **Option Ids**: lista paralela de rótulos (usados pelo renderizador como texto acessível).
  - **Correct Index**: índice (base 0) da resposta certa na lista de opções.
  - **Prompt Narration**: arraste aqui o `AudioCueSO` do prompt.
  - **BNCC Tags**: array opcional de `BnccTagSO` para relatórios curriculares.

#### Passo 4 — Agrupar em um Challenge Set

- **Botão direito → `Create → Edu → Data → Challenge Set`**.
- Arraste todos os `MultipleChoiceChallengeSO` autorados para a lista **Challenges**.

#### Passo 5 — Escolher um perfil de dificuldade

Você pode reaproveitar `Difficulty_Default.asset` do conteúdo gerado. Para criar um novo:

- **Botão direito → `Create → Edu → Data → Difficulty Profile`**.
- Configure **Rounds** (quantas perguntas por sessão), **Wrong Streak Before Simplify** (após tantos erros seguidos, derruba um distrator), **Min/Max Choice Count**.

#### Passo 6 — Criar o MiniGameConfig

- **Botão direito → `Create → Edu → Data → Mini Game Config`**.
- Nomeie como, por exemplo, `Cfg_MeuNovoJogo.asset`.
- Preencha:
  - **Game Id**: uma string única, ex.: `letters_b`. **Anote — você vai reusar duas vezes.**
  - **Challenge Set**: arraste o set do Passo 4.
  - **Difficulty**: arraste o perfil do Passo 5.
  - **Success Cue / Neutral Retry Cue**: arraste as cues do Passo 2.

#### Passo 7 — Criar a cena aditiva

1. **File → New Scene → Empty**.
2. Salve em **`Assets/EduFramework/Scenes/MiniGames/MG_MeuNovoJogo.unity`** (o **prefixo `MG_` é obrigatório** para o Hub saber descarregar).
3. Arraste **`MiniGame_Template_MultipleChoice.prefab`** da janela Project para a Hierarchy.
4. Selecione a instância do prefab, encontre o componente **Mini Game Multiple Choice** no Inspector e arraste seu `Cfg_MeuNovoJogo.asset` para o campo **Config Asset**.
5. Salve a cena (**Ctrl/Cmd+S**).
6. **File → Build Settings → Add Open Scenes** (ou arraste o arquivo de cena para a lista).

#### Passo 8 — Conectar ao Hub

1. Abra `MiniGameCatalog_Default.asset` (ou o seu próprio catálogo).
2. Adicione uma nova entrada:
   - **Game Id**: `letters_b` (igual ao da config).
   - **Display Name Key**: `mg.letters_b` (chave para o nome localizado; pode ser igual ao id se ainda não localizou).
   - **Additive Scene Name**: `MG_MeuNovoJogo` (sem a extensão `.unity`).
   - **Recommended Age Min/Max**: ex.: 4 / 6.
3. Abra `HubConfiguration_Default.asset`.
4. Adicione um novo `HubMapNode`:
   - **Node Id**: `node_letters_b`.
   - **Linked Game Id**: `letters_b` (precisa bater).
   - **Anchored UI Position**: ex.: `(0, -300)` para posicionar abaixo dos nós existentes.
   - **Unlock Rule**: arraste `Unlock_Always.asset` (ou um `RequireMiniGamesCompletedRuleSO` se quiser destravar gradualmente).

#### Passo 9 — Testar

- Abra `Assets/Scenes/Bootstrap.unity`.
- Aperte Play.
- Seu novo nó deve aparecer no mapa. Clique nele → sua cena carrega → as perguntas rodam → ao sair, volta ao Hub.

> 💡 **Dica:** o **Display Name Key** (ex.: "mg.letters_b") usa o `linkedGameId` como fallback quando não há linha de localização correspondente. Você pode lançar um novo jogo sem mexer na tabela de localização — só espere o rótulo na tela mostrar o id cru até você cadastrar uma tradução.

### 5.2 Editar um mini-jogo existente

Você quase nunca precisa abrir a cena para mudar comportamento — a maior parte mora no SO.

#### Mudar a dificuldade

1. Abra o **`Cfg_*.asset`** do jogo (ex.: `Cfg_Consonants.asset`).
2. Abra o **`DifficultyProfileSO`** vinculado (ex.: `Difficulty_Default.asset`).
3. Ajuste:
   - **Rounds** — mais = sessão mais longa.
   - **Wrong Streak Before Simplify** — menor = misericórdia mais rápida.
   - **Max Choice Count** — menos distratores = mais fácil.
4. Aperte Play.

> ⚠️ `Difficulty_Default.asset` é **compartilhado** entre os seis jogos de demo. Para mudar a dificuldade de **um** jogo sem afetar os outros, duplique o asset (**Ctrl/Cmd+D**), renomeie para `Difficulty_Consonants.asset` e atribua somente à config das consoantes.

#### Trocar assets (sprites, áudio)

- Coloque os novos arquivos em `Assets/EduFramework/Art/` ou `Audio/`.
- Abra o **challenge SO** que referenciava o asset antigo.
- Arraste o novo sprite/cue para o campo apropriado.
- Sem código, sem recompilar.

#### Mudar o texto exibido no nó

- Abra `MiniGameCatalog_Default.asset`.
- Edite **Display Name Key** da entrada — ou, se você tiver um `LocalizedTableSO`, edite a linha correspondente a essa chave.

#### Ajustar uma única pergunta

- Localize o `Ch_*.asset` (ex.: `Ch_MC_A.asset`).
- Mude o **Correct Index**, troque uma **Option Image** ou atualize a cue do **Prompt Narration**.

#### Calibrar a força da animação de feedback

- Selecione o `MiniGameRoot` na cena `MG_*` e encontre o **`FeedbackKit`** (adicionado automaticamente pelo `MiniGameBase`).
- Edite **Pulse Seconds**, **Correct Scale** e **Wrong Scale** no Inspector.

---

## 6. Customização da UI (muito importante)

Hoje o framework **constrói a maior parte da UI em tempo de execução** via pequenos helpers em C#. Isso deixa o demo leve, mas significa que customizar UI é um processo de dois passos: **(a)** decidir se você quer sobrescrever partes pelo Inspector ou substituindo prefabs; **(b)** preservar defaults acessíveis.

### 6.1 Onde a UI mora

| Onde | O que faz | Como customizar |
|-------|--------------|------------------|
| `GameplayUiUtility.CreateOverlayCanvas(name, parent)` | Cria um Canvas Screen-Space-Overlay com referência 1920×1080 e `GraphicRaycaster`. | Substitua por um prefab de canvas próprio e parente a shell nele, evitando a auto-criação. |
| `GameplayUiUtility.CreateChoiceButton(parent, label, image)` | Cria um botão de 220×220 com imagem + rótulo, usado pelos jogos de múltipla escolha e contagem. | Estenda a shell e forneça sua própria fábrica de botões, **ou** substitua em runtime via `OnInitialized()` em uma subclasse. |
| `MultipleChoiceShellView` | Distribui os botões de escolha horizontalmente em `_choicesParent` (40 px de padding lateral, 200 px superior/inferior, 24 px de spacing). | Atribua um `_choicesParent` já estilizado no Inspector para pular a versão auto-gerada. |
| `DragDropSlotsShellView` | Dois HorizontalLayoutGroups: linha de bins em cima, linha de tokens embaixo. | Idem — atribua seus próprios `_binsParent` e `_tokensParent`. |
| `HubWorldController` | Constrói os nós do hub (Image 180×180 verde + Button + Text). | Substitua por uma versão dirigida por prefab (ver § 11.1). |
| `FeedbackKit` | Toca o pulso de escala não-punitivo no acerto/erro. | Campos no Inspector: `_pulseSeconds`, `_correctScale`, `_wrongScale`. |

### 6.2 Mudando o estilo visual (cores, fontes, sprites, estilo dos botões)

#### Cores

A UI construída em runtime usa estas cores fixas no código:

| Elemento | Cor | Origem |
|---------|-------|--------|
| Nó do hub (desbloqueado) | RGB `(0.35, 0.75, 0.45)` — verde | `HubWorldController.BuildMap()` |
| Nó do hub (bloqueado) | RGB `(0.55, 0.55, 0.55)` — cinza | `HubWorldController.BuildMap()` |
| Botão de escolha (sem sprite) | RGB `(0.85, 0.9, 1)` — azul pálido | `GameplayUiUtility.CreateChoiceButton` |
| Bin do drop | RGB `(0.95, 0.95, 1)` — quase branco | `DragDropSlotsShellView.BuildBins` |
| Carta da memória | RGB `(0.2, 0.45, 0.85)` — azul | `MiniGameMemory.MakeCard` |

Para mudar **sem subclassar**, o caminho mais limpo é atribuir um **sprite** ao Image/Button relevante. As fábricas ignoram a cor de fallback quando há imagem.

Para mudar **via código** (controle do designer), copie o helper relevante para sua própria classe utilitária (ex.: `BrandedUiUtility`) e roteie sua shell subclassada por ela. Ver § 11 para o padrão.

#### Fontes

O texto em runtime usa a fonte built-in **`LegacyRuntime.ttf`** (ver `GameplayUiUtility.BuiltinRuntimeFont`). Para usar uma fonte da marca:

1. Coloque seu `.ttf` ou `.otf` em `Assets/EduFramework/UI/Fonts/`.
2. Em `GameplayUiUtility.cs`, troque o getter `BuiltinRuntimeFont` para carregar sua fonte:

   ```csharp
   public static Font BuiltinRuntimeFont =>
       _builtinRuntimeFont ??= Resources.Load<Font>("Fonts/ChildFriendly");
   ```

   (Coloque a fonte em `Assets/Resources/Fonts/` para o `Resources.Load` encontrar.)
3. Recompile — todo rótulo (hub, botões de escolha, prompts, cartas da memória) passa a usá-la.

> 💡 **Dica:** se preferir migrar para **TextMeshPro**, substitua `Text` por `TMP_Text` em `GameplayUiUtility.CreateChoiceButton` e `HubWorldController.BuildMap`. Teste primeiro em `MG_Consonants`.

#### Sprites

- **Ícone do nó no hub**: estenda `HubWorldController` para ler um sprite de cada `HubMapNode` e atribuí-lo ao `Image` auto-criado. O caminho mais limpo é adicionar um campo `_icon` em `HubMapNode` (`HubConfigurationSO.cs`) e aplicar em `BuildMap()`.
- **Sprite do tile de escolha**: basta preencher o array **Option Images** do `MultipleChoiceChallengeSO` — `CreateChoiceButton` consome o sprite e limpa a cor de fallback automaticamente.
- **Cartas de memória**: hoje exibem um texto `?`. Para flip com sprites, adicione campos de sprite face A/B em `MemoryPairChallengeSO` e edite `MiniGameMemory.Flip()` para trocar `Image.sprite` em vez de mudar `Text.text`.

#### Estilo dos botões

O jeito mais simples de reestilizar todo `CreateChoiceButton`:

1. Abra `Assets/Scripts/UI/GameplayUiUtility.cs`.
2. Em `CreateChoiceButton`, mude `rt.sizeDelta` (default `220, 220`), adicione um contorno (`go.AddComponent<Outline>()`) ou ajuste `btn.transition = Selectable.Transition.SpriteSwap` e atribua sprites da marca.

### 6.3 Adaptando para crianças (a seção mais importante)

Este framework é voltado a **crianças de 4 a 6 anos**, então:

- **Alvos de toque ≥ 64 px na tela**. Os botões de escolha default têm 220×220 — mantenha pelo menos 180×180 mesmo em layouts menores.
- **Rótulos grandes e legíveis**: `resizeTextForBestFit` está ligado com `resizeTextMaxSize: 32` nos botões de escolha. Não baixe disso para menos de 18.
- **Áudio primeiro, texto depois**: todo desafio deve preencher **Prompt Narration**. O rótulo na tela é tratado como alt-text acessível, não como pista principal.
- **Feedback não-punitivo**: respostas erradas tocam uma cue *neutral retry* (tier `Correction`) e um **encolhimento** (escala 0,96) — nunca um "buzzer", nunca flash vermelho. Mantenha `FeedbackKit._wrongScale` próximo de 1,0.
- **Ritmo calmo**: cada rodada espera 0,45 s após o feedback (`yield return new WaitForSecondsRealtime(0.45f)`). Aumente esse valor em `MiniGameMultipleChoice` / `MiniGameCounting` etc. para grupos mais novos.
- **Sem pressão de tempo**: rodadas são limitadas por `DifficultyProfileSO.Rounds`, nunca por relógio.

### 6.4 Criando novos elementos de UI

#### Um novo estilo de botão

Crie um **prefab** chamado, por exemplo, `Button_Choice_Branded.prefab` em `Assets/EduFramework/Prefabs/UI/` com sua estilização. Depois crie uma subclasse de `MultipleChoiceShellView` que instancia esse prefab em vez de chamar `GameplayUiUtility.CreateChoiceButton`. Troque o script no prefab template pela sua subclasse.

#### Um novo painel (ex.: menu de pausa, configurações, cabeçalho do hub)

1. Crie um prefab `Prefabs/UI/PauseOverlay.prefab` com a hierarquia do painel.
2. Na cena **Hub**, coloque o prefab como irmão do `HubCanvas` (ou exponha um campo novo em `HubWorldController`).
3. Acione-o por um botão no hub ou por um atalho de teclado em `AppSessionController`.

#### Um novo popup de feedback

Hoje, `FeedbackKit.Play(correct, target)` só faz o pulso de escala. Para adicionar um popup:

1. Autoraqua um `Popup_Correct.prefab` e um `Popup_Wrong.prefab` em `Prefabs/UI/`.
2. Adicione `[SerializeField] GameObject _correctPopupPrefab;` / `_wrongPopupPrefab;` no `FeedbackKit`.
3. Em `FeedbackKit.Play()`, instancie o prefab apropriado sob o canvas e depois `Destroy` ao fim do pulso.

### 6.5 Boas práticas de UI

- **Redundância áudio + visual**: todo elemento interagível deve ter ícone E cue de narração. Nunca dependa só de cor (compatível com daltonismo).
- **Acessibilidade**: mantenha o `EventSystem` (criado automaticamente em `GameplayUiUtility.EnsureEventSystem`) para teclado, mouse e toque funcionarem.
- **Evite dependência de texto**: em SOs de desafio, trate `OptionIds` como alt-text. Se o público não lê, preencha `OptionImages` e mantenha `OptionIds` como IDs legíveis por máquina (`fruit_apple`, `fruit_pear`).
- **Layouts que respiram**: use `HorizontalLayoutGroup` / `VerticalLayoutGroup` com pelo menos 24 px de spacing e 40 px de padding lateral (corresponde ao default do framework).
- **Não brigue com o Canvas Scaler**: resolução de referência é `1920×1080`, match-mode `0.5`. Se precisa de outro aspect, mude em `GameplayUiUtility.CreateOverlayCanvas` uma vez só — nunca por prefab.

---

## 7. Uso do sistema de áudio

### 7.1 Arquitetura (em uma tela)

```
        AppContext.Audio  (IAudioDirector)
                │
       ┌────────┴────────────────┐
       │                         │
  SfxPlayer            NarrationController
  (PlayOneShot)        (fila com prioridade, voz única)
                                  │
                          AudioCueSO[] enfileiradas
                                  │
                          MusicController.SetDucked(true)
                          enquanto há narração tocando
```

`AudioDirector` é o único ponto de entrada de áudio que você deve chamar a partir do gameplay (`Context.Audio` dentro de `MiniGameBase`). Ele controla os sliders de volume (master, sfx, narration) e expõe:

- `PlaySfxCue(AudioCueSO)` — não bloqueante, sem ducking.
- `EnqueueNarration(AudioCueSO)` — adiciona à fila de narração respeitando a prioridade.
- `StopNarration()` — limpa a fila + voz ativa.

### 7.2 Prioridade de áudio (regras de interrupção)

De `AudioPriority.cs`:

| Tier | Valor | Use para |
|------|-------|---------|
| Ambient | 0 | Loops de fundo, nunca interrompido nem interrompe. |
| Celebration | 10 | Jingles de sucesso. |
| Correction | 40 | Voz calma de "tente de novo". |
| Tutorial | 60 | A voz da professora dando o prompt. **Sempre vence.** |

Regras (ver `NarrationController.Enqueue`):

- Uma cue de prioridade **maior** **limpa a fila e para a voz atual**.
- Uma cue de **mesma prioridade** vai para a **fila** se `SameTierQueues = true` (default), ou **substitui** a voz atual caso contrário.
- Uma cue de prioridade **menor** **sempre** vai para o fim da fila.
- Uma cue com `CooldownSeconds > 0` é **silenciosamente descartada** se foi tocada recentemente.

### 7.3 Adicionando uma fala de narração (sem código)

1. Coloque seu `.wav` no projeto (ex.: `Audio/Voice/pt_BR/letters/`).
2. Botão direito → **`Create → Edu → Audio → Audio Cue`**, nomeie como `Cue_PromptLetraB`.
3. No Inspector dela:
   - **Clip** → arraste o wav.
   - **Priority** → `Tutorial`.
   - **Volume Scale** → 1,0 a não ser que a gravação esteja muito alta.
   - **Cooldown Seconds** → 0 (use 2–3 para cues repetitivas como sons de hover).
   - **Subtitle Localization Key** → opcional, casado com `LocalizedTableSO` para legendas.
4. Arraste a cue para o campo `_promptNarration` de algum `MultipleChoiceChallengeSO` — pronto.

### 7.4 Conectando áudio a ações

- **Prompt por desafio**: `ChallengeSO._promptNarration` (tocado automaticamente em cada rodada por `MiniGameMultipleChoice` / `Counting` / etc.).
- **Sucesso / retry por jogo**: `MiniGameConfigSO._successCue` (tier SFX) e `_neutralRetryCue` (tier Correction) — usados por `MiniGameBase.PlayFeedback`.
- **Áudio de hover por opção** (múltipla escolha): `MultipleChoiceChallengeSO._optionHoverAudio[]` — um por índice de opção.
- **Flip de carta de memória**: `MemoryPairChallengeSO._cardFaceA` / `_cardFaceB` — ainda não tocado automaticamente, mas acessível a partir de uma subclasse.
- **Palavra-alvo das sílabas**: `SyllableChallengeSO._targetWordAudio` — tocado uma vez por rodada antes dos tiles aparecerem.

### 7.5 Boas práticas para áudio educacional

- **Use um só dublador ao longo do currículo.** Crianças identificam o timbre.
- **Mantenha prompts abaixo de 4 segundos.** Prompts longos viram ruído de fundo.
- **Sempre case áudio com movimento** (pulso do `FeedbackKit`), para crianças surdas ou com perda auditiva ainda enxergarem algo acontecer.
- **Nunca toque mais de 2 cues por desafio** (prompt + retry). Mantenha `SameTierQueues = true` para erros repetidos não acumularem cues.
- **Use tier `Correction` para retries**, não `Tutorial` — assim um `Tutorial` (prompt re-perguntado) ainda interrompe um retry.
- **Master volume default é 1,0** — exponha um slider no hub se a acústica da sala variar.

---

## 8. Adicionando conteúdo educacional

Regra geral: **crie o SO primeiro, jogue-o em um `ChallengeSetSO`, aponte o `MiniGameConfigSO` para o set**. Todos os seis jogos de demo seguem esse padrão.

### 8.1 Nova pergunta de múltipla escolha

`Create → Edu → Data → Challenge → Multiple Choice` →
preencha `Id`, `ConceptKeys`, `OptionImages`, `OptionIds`, `CorrectIndex`, `PromptNarration` → arraste para um `ChallengeSetSO`. Ver § 5.1 passo 3.

### 8.2 Novo exercício de contagem / quantidade

`Create → Edu → Data → Challenge → Quantity` →
- **Target Count** (1–6).
- **Token Sprite** (o objeto contado; uso futuro).
- **Number Clips**: array de `AudioClip`, índice 0 = "um", índice 1 = "dois" etc.
- **Concept Keys**: `count:5`, `count:quantity`.

### 8.3 Novo par de memória

`Create → Edu → Data → Challenge → Memory Pair` →
- **Pair Id** é a chave de correspondência. **Dois desafios de memória que dividem o mesmo `PairId` formam um par.** (Ver `MiniGameMemory.RunSessionRoutine`.)
- **Card Face A / B**: o áudio que toca quando a carta vira.

> 💡 **Dica:** para um par (A↔B), autore **um** `MemoryPairChallengeSO` com um `PairId` único — o jogo de memória cria duas cartas por desafio automaticamente.

### 8.4 Nova rodada de classificação

`Create → Edu → Data → Challenge → Classification` →
- **Items**: lista de `{ sprite, categoryId }`.
- **Bin Labels**: lista de IDs de categoria (viram os nomes exibidos dos bins e as chaves de correspondência).

A checagem (`MiniGameClassification.RunSessionRoutine`) compara o `categoryId` de cada token com o `CategoryId` do bin.

### 8.5 Novo desafio de sílabas

`Create → Edu → Data → Challenge → Syllable Builder` →
- **Syllable Parts In Order**: ex.: `["CA", "SA"]` para soletrar *casa*.
- **Syllable Sprites**: array paralelo para os tiles visuais.
- **Target Word Audio**: cue com a palavra inteira; tocada antes dos tiles aparecerem.

Os tiles são embaralhados na tela; a criança precisa tocar **na ordem original**. Um toque errado zera o progresso e toca a cue de retry.

### 8.6 Nova página / livro de história

1. `Create → Edu → Data → Challenge → Multiple Choice` para a lacuna (a imagem escolhida preenche o espaço).
2. `Create → Edu → Data → Story Page` → defina `Illustration` (arte da página) e `GapChallenge` (o MC do passo 1).
3. `Create → Edu → Data → Story Book` → arraste as páginas em ordem.
4. Arraste o livro para `MiniGameConfigSO._storyBook`.
5. Adicione uma entrada no catálogo com **Additive Scene Name** apontando para uma cena `MG_*` cujo root seja `MiniGameStoryGaps`.

### 8.7 Novo clip de áudio

Coloque o arquivo em `Assets/EduFramework/Audio/Voice|Sfx|Music/`. O Unity importa automaticamente. Depois embrulhe em um `AudioCueSO` (ver § 7.3) — nunca referencie `AudioClip` puro a partir do gameplay.

### 8.8 Novo sprite / imagem

Coloque o PNG em `Assets/EduFramework/Art/`. No inspector de import, defina **Texture Type: Sprite (2D and UI)**. Arraste para o campo SO apropriado.

### 8.9 Nova tag BNCC

`Create → Edu → Data → BNCC Tag` → preencha `Official Code` (ex.: `EI03EF03`) e `Display Name`. Referencie em qualquer `ChallengeSO._bnccTags` ou `MiniGameCatalogEntry._bnccTags`. Tags não controlam gameplay; enriquecem o CSV do professor.

### 8.10 Nova string localizada

1. `Create → Edu → Data → Localized Table` (uma por projeto basta).
2. Adicione linhas: `{ key: "mg.letters_b", ptBR: "Letra B", secondary: "" }`.
3. Na cena **Bootstrap**, encontre o `LocalizationService` e arraste a tabela no array `_tables`.
4. O framework procura primeiro por `pt-BR`, depois cai em `secondary` para outros idiomas. Defina **Default Language Id** no serviço se quiser builds não-pt-BR.

---

## 9. Testando o jogo

### 9.1 Testar um mini-jogo isolado (loop rápido)

Mini-jogos **dependem dos serviços do Bootstrap**, então abrir só `MG_Consonants.unity` e apertar Play vai logar *"AppContext not ready"*.

O loop correto:

1. Mantenha **`Bootstrap.unity`** como a **única** cena na hierarquia em tempo de edição (a `MG_*` chega em runtime).
2. Em **File → Build Settings**, garanta que `Bootstrap` é índice 0 e a `MG_*` relevante está incluída.
3. Para pular o Hub durante teste, mude temporariamente `AppSessionController._hubSceneName` para sua cena (`MG_Consonants`) — assim o boot loader já vai direto. Reverta antes de fazer o build.

### 9.2 Testar o Hub

- Abra `Hub.unity` **com** `Bootstrap.unity` já carregada (arraste as duas para a Hierarchy ou, mais confiável, sempre comece pelo `Bootstrap` e deixe o `AppSessionController` carregar o Hub).
- Verifique que cada nó carrega sua cena e descarrega corretamente quando a sessão termina.
- Use **Window → Analysis → Profiler** durante uma sessão para confirmar que memória é liberada entre mini-jogos (o unload aditivo deve derrubar ~30–80 MB dependendo dos assets).

### 9.3 Testar desbloqueios

- Autoraqua um `RequireMiniGamesCompletedRuleSO` com um dos seus `gameId`s de demo em `_requiredGameIds`.
- Arraste-o para o **Unlock Rule** de um nó do hub.
- Primeiro boot: nó cinza, sem clique. Jogue e termine o jogo exigido → saia → reabra (o progresso é salvo em `Application.persistentDataPath/profiles/slot_01/progress_v1.json`) → o nó agora está verde.

### 9.4 Resetar o progresso para um teste limpo

Apague (ou renomeie) `Application.persistentDataPath/profiles/slot_01/progress_v1.json`.

No Windows o caminho normalmente é
`C:\Users\<voce>\AppData\LocalLow\<CompanyName>\<ProductName>\profiles\slot_01\progress_v1.json`.

### 9.5 Cenários comuns de teste

| Cenário | Resultado esperado |
|---------|-----------------|
| Clicar em qualquer nó do hub | Canvas do hub se esconde, cena aditiva carrega, mini-jogo começa depois que os serviços do Bootstrap inicializam. |
| Clicar na resposta errada | `FeedbackKit` encolhe, cue de retry neutro de `MiniGameConfigSO._neutralRetryCue` toca (se atribuída). |
| Clicar na resposta certa | `FeedbackKit` aumenta, cue de sucesso toca (se atribuída). |
| Completar N rodadas | Mini-jogo emite evento `Completed` em `MiniGameSessionEventChannelSO`, progresso incrementa, controle volta ao hub. |
| Forçar perda de foco (Alt-Tab) | `AppSessionController.IsPausedForFocus` vira `true` e o relógio da sessão para de acumular. |

---

## 10. Guia de depuração

### Referências ausentes / "O nó não carrega nada"

Sintomas: clicar num nó do hub, nada acontece (nenhuma cena carrega).

Verificações (em ordem):
1. Abra `MiniGameCatalog_Default.asset`. Encontre a entrada. **Additive Scene Name** precisa bater com o nome do arquivo de cena, **sem** `.unity`.
2. `File → Build Settings` → a cena deve estar na lista **e marcada**.
3. O `HubMapNode._linkedGameId` (em `HubConfigurationSO`) precisa ser igual ao `_gameId` da entrada.

### Log "AppContext not ready"

Você abriu uma cena que precisa dos serviços do Bootstrap sem rodar o Bootstrap antes.

Correção: sempre dê Play em `Bootstrap.unity`. Ou, em código, adicione guardas `if (!AppContext.IsInitialized) return;` (o `MiniGameBase` já faz isso).

### Log "Missing MiniGameConfigSO"

O GameObject raiz da cena `MG_*` tem um componente `MiniGame*` mas nenhuma config foi arrastada para `_configAsset`.

Correção: abra a cena, selecione o root, arraste o `Cfg_*.asset` certo no Inspector.

### Áudio não toca

Em ordem:
1. O `AudioClip` está atribuído no `AudioCueSO`? `_clip == null` ⇒ a cue é descartada em silêncio.
2. O **Volume Scale** da cue é > 0? O **Master/Narration/Sfx Volume** do `AudioDirector` é > 0?
3. Já há uma narração de **prioridade maior** tocando? Cues de tier menor vão para o fim da fila.
4. **Cooldown Seconds** rejeitou? Replays dentro do cooldown são descartados.
5. Confira que o áudio do Unity não está globalmente mudo (ícone do alto-falante na toolbar do editor).
6. Abra o GameObject do `BootstrapScope` — ele precisa ter um `AudioDirector` (e `SfxPlayer`/`NarrationController`/`MusicController` são adicionados automaticamente em `Awake`).

### UI não atualiza / botões não aparecem

- `MultipleChoiceShellView.Bind()` está sendo chamada com `challenge == null`? Confira que o `ChallengeSetSO` tem ao menos um `MultipleChoiceChallengeSO` para esse jogo.
- O desafio tem zero `OptionImages` **e** zero `OptionIds`. O `BuildSubsetIndices` ainda assume 2 por default, mas os botões ficam em branco.
- O `EventSystem` se perdeu entre cenas. O framework cria um automaticamente em `GameplayUiUtility.EnsureEventSystem`, mas se você removeu essa chamada, adicione no `Awake` da sua shell custom.
- Vários Canvases empilhados com o mesmo `sortingOrder`. O canvas do hub usa `20`; o auto-canvas do mini-jogo usa `100`. Se seu canvas custom esconder o mini-jogo, aumente o `sortingOrder`.

### Canais de evento silenciosos

`AnswerEvaluatedEventChannelSO` e `MiniGameSessionEventChannelSO` são referências **opcionais** no `MiniGameBase`. Se você esquecer de atribuir, o evento simplesmente não dispara — o registro de progresso continua funcionando porque `RaiseAnswerEvaluated` sempre chama `ProgressService.RecordAnswer`.

### Progresso não está salvando

- Abra o GameObject `BootstrapScope`; confirme que ele tem o `ProgressService` (resolvido em `Awake`).
- `ProgressService.NotifySessionCompleted` é chamado em `MiniGameBase.SessionWrapper` ao fim da rotina. Se seu jogo custom encerra sem terminar a rotina (ex.: throw, return antecipado), chame `Context.Progress.NotifySessionCompleted(gameId)` manualmente.

---

## 11. Estendendo o framework (avançado)

Para desenvolvedores.

### 11.1 Criar um tipo totalmente novo de mini-jogo (regras customizadas)

1. **Classe C#** em `Assets/Scripts/MiniGames/` herdando de `MiniGameBase`:

   ```csharp
   public sealed class MiniGameRhythm : MiniGameBase
   {
       protected override void OnInitialized()
       {
           GameplayUiUtility.EnsureEventSystem();
       }

       protected override IEnumerator RunSessionRoutine()
       {
           var diff = Config.Difficulty;
           var rounds = diff != null ? diff.Rounds : 3;
           for (var r = 0; r < rounds; r++)
           {
               yield return RunOneRound();
           }
       }

       IEnumerator RunOneRound()
       {
           var sw = Stopwatch.StartNew();
           bool? ok = null;
           // ...seu loop de UI + input aqui...
           while (!ok.HasValue) yield return null;
           sw.Stop();
           var result = new EvaluationResult(ok.Value, new[] { "rhythm:beat" }, (float)sw.Elapsed.TotalSeconds);
           RaiseAnswerEvaluated(result);
           PlayFeedback(result);
       }
   }
   ```

2. **Opcionalmente** crie uma nova subclasse de `ChallengeSO` para esse gameplay (ex.: `RhythmChallengeSO`). Coloque em `Assets/Scripts/Data/` com um `[CreateAssetMenu(menuName = "Edu/Data/Challenge/Rhythm")]`.

3. **Cena**: crie `MG_Rhythm.unity` com um GameObject raiz carregando seu componente. Atribua um `MiniGameConfigSO` via o campo `_configAsset`.

4. **Catálogo + Hub**: adicione um `MiniGameCatalogEntry` e um `HubMapNode` para ele (ver § 5.1 passo 8).

### 11.2 Adicionar um novo serviço (ex.: sink de analytics)

1. Defina uma interface `IAnalyticsSink` em `Assets/Scripts/Core/ServiceContracts.cs`.
2. Implemente um `MonoBehaviour` no GameObject do Bootstrap (ex.: `MyHttpAnalytics : MonoBehaviour, IAnalyticsSink`).
3. Adicione um campo em `AppServices` (`Assets/Scripts/Core/AppContext.cs`) e um getter em `AppContext` para os chamadores acessarem.
4. Registre no `BootstrapScope.Awake()` ao lado de `audio`/`input`/`progress`/`localization`.
5. Encaminhe `AnswerEvaluatedEvent` para o sink se inscrevendo no seu `AnswerEvaluatedEventChannelSO` a partir de um `MonoBehaviour` separado na cena Bootstrap.

### 11.3 Substituir o hub construído em runtime por um mapa baseado em prefab

1. Construa seu layout de hub como prefab: um Canvas, um `RectTransform` raiz do mapa e um prefab de botão por nó.
2. Remova o código de auto-build de `HubWorldController` (`EnsureCanvas`, `BuildMap`) e exponha um `[SerializeField] HubNodeView _nodePrefab;` que instancia **seu** prefab sob seu map root, passando os dados de `HubMapNode`.
3. Mantenha o fluxo `LoadMiniGameRoutine` / `UnloadMiniGamesRoutine` — é o contrato com o resto do framework.

### 11.4 Usar os canais de evento para amarrar sistemas

`AnswerEvaluatedEventChannelSO` e `MiniGameSessionEventChannelSO` são event buses em SO. Você pode:

- Criar um asset compartilhado (`Events_AnswerEvaluated.asset`).
- Atribuí-lo em todo campo `MiniGameBase._answerEvents`.
- Adicionar listeners `MonoBehaviour` (ex.: um `HudScoreView` no Hub) que se inscrevem no evento C# em `OnEnable`.

Isso impede que o hub/UI referenciem tipos concretos `MiniGame*`.

### 11.5 Mantendo a consistência arquitetural

- **Não leia `AppContext` fora da fronteira `MiniGameBase.OnInitialized()`** — é nele que o snapshot `MiniGameContext` é tirado. Sempre passe por `Context.Audio`, `Context.Progress`, etc.
- **Não referencie tipos `MiniGame*` a partir do Hub.** O Hub só conhece strings `gameId` e nomes de cena.
- **Nunca chame `SceneManager.LoadScene` a partir do gameplay.** Use `MiniGameSessionHub.RequestExitToHub?.Invoke()` para retornar; o Hub é dono da gestão de cenas.
- **Mantenha o prefixo `MG_`** em toda cena aditiva de mini-jogo; `HubWorldController.UnloadMiniGamesRoutine` faz match por esse prefixo.
- **Use o caminho `Edu/...` no `[CreateAssetMenu]` dos SOs**. Autores procuram conteúdo sob um único menu raiz.

---

## 12. Boas práticas para design educacional

### 12.1 Loops de feedback

- **Em até 200 ms após uma ação, o feedback visual dispara** (`FeedbackKit.Play` é instantâneo).
- **O feedback de áudio chega em até 600 ms** (a espera de 0,45 s após cada rodada + a duração da própria cue).
- **Feedback de progresso é tardio**: não mostre "subiu de nível" no meio da sessão; exiba no mapa do hub após a sessão.

### 12.2 Sistemas de encorajamento

- Trate a **success cue** como celebração, não validação. Use takes variados (a mesma `AudioCueSO` com várias gravações alternadas via uma pequena subclasse) para evitar fadiga auditiva.
- Após três acertos seguidos, escale para uma celebração **mais forte**. É fácil adicionar via uma subclasse de `MiniGameBase` que rastreia streaks e escolhe uma cue diferente.
- **Nunca** exiba pontuação numérica durante uma sessão de 4–6 anos. Mantenha contadores escondidos no `ProgressService` e mostre só no CSV do professor.

### 12.3 Tolerância a erros

- O `DifficultyProfileSO` default reduz as opções em 1 após 2 erros seguidos (`WrongStreakBeforeSimplify = 2`, `ChoiceCountForStreak`). Mantenha — é andaime silencioso.
- A cue **neutral retry** usa linguagem não-punitiva por design ("vamos tentar de novo", nunca "errado"). Audite toda gravação de retry.
- **Tentativas infinitas por pergunta.** Nenhum mini-jogo deste framework conta erros contra as rodadas concluídas.

### 12.4 Adaptação cultural (muito importante)

Este framework foi construído para uso em **escolas brasileiras**, com espaço explícito para áudio em **línguas indígenas**:

- `LocalizedRow.secondary` é reservado para um segundo idioma ao lado do `ptBR`.
- `LocalizedTableSO.TryGet` retorna `secondary` quando `CurrentLanguageId` não começa com `pt`.
- **Você pode entregar voz em língua indígena** sem escrever código: autore uma segunda `AudioCueSO` cujo clip seja a gravação indígena e troque qual cue é atribuída como `PromptNarration` por build de conteúdo. (Para troca de idioma em runtime, estenda `AudioCueSO` com um `LocalizedAudioClipSet` e faça o `NarrationController` resolver o clip via `LocalizationService.CurrentLanguageId`.)
- **Imagem importa tanto quanto palavra.** Ao escolher sprites para `MultipleChoiceChallengeSO.OptionImages`, prefira objetos culturalmente familiares (frutas regionais, animais locais) em vez de arte stock genérica.
- **Evite texto embutido em sprites** — texto dentro da imagem não localiza. Mantenha o texto no array `OptionIds` para os rótulos passarem por `LocalizationService.Get`.
- **Direção de leitura**: o framework assume da esquerda para a direita. Se vai mirar conteúdo em árabe, inverta a ordem dos filhos do `HorizontalLayoutGroup` em `MultipleChoiceShellView`.

---

## 13. Checklist para publicar um novo mini-jogo

Use esta lista antes de mergulhar um mini-jogo novo no currículo.

**Conteúdo e dados**
- [ ] `MiniGameConfigSO` criado e nomeado `Cfg_<jogo>.asset`.
- [ ] `_gameId` definido como uma string única e estável (snake_case).
- [ ] `ChallengeSetSO` vinculado, com pelo menos **3 desafios** (o suficiente para um shuffle).
- [ ] Cada desafio tem pelo menos **uma Concept Key** (ex.: `phoneme:/b/`).
- [ ] `DifficultyProfileSO` atribuído (`Rounds ≥ 3`, `MaxChoiceCount` adequado à idade).

**Áudio**
- [ ] **Prompt narration** preenchido em todo desafio (prioridade `Tutorial`).
- [ ] **Success cue** e **Neutral retry cue** preenchidos no `MiniGameConfigSO`.
- [ ] Todas as cues usam a `AudioPriority` correta.
- [ ] Gravado por uma voz consistente; picos abaixo do clipping.

**UI**
- [ ] Todos os tiles de escolha têm sprite ou `OptionIds` não vazio.
- [ ] Testado em **1920×1080** e **1366×768** (default do canvas scaler).
- [ ] Nenhuma cor crua usada como única pista de feedback (toda ação tem movimento + áudio).
- [ ] Testado com **tela de toque** se o laboratório usa uma.

**Conexão com o hub**
- [ ] `MiniGameCatalogEntry` adicionado: `gameId` bate, `additiveSceneName` correto (sem `.unity`).
- [ ] Cena em **File → Build Settings**, marcada.
- [ ] Nome da cena começa com `MG_`.
- [ ] `HubMapNode` adicionado com o mesmo `linkedGameId`, uma `anchoredUiPosition` sensata e uma `UnlockRule`.

**Comportamento e progresso**
- [ ] Mini-jogo volta para o hub no fim da sessão (auto-unload funciona porque o nome começa com `MG_`).
- [ ] Progresso incrementa após a sessão: abra `progress_v1.json` e confirme que `sessionsCompleted` subiu.
- [ ] Resposta errada dispara o retry neutro; sem sons agressivos.
- [ ] Perda de foco da janela pausa o relógio da sessão (`AppSessionController._pausedForFocus`).

**Revisão educacional**
- [ ] Pelo menos uma **tag BNCC** em cada desafio (`_bnccTags`).
- [ ] Linguagem revisada por professor(a) (vocabulário adequado a 4–6 anos).
- [ ] Revisão cultural feita se o lançamento for em uma nova comunidade.

**Teste final**
- [ ] Cold-start a partir de `Bootstrap.unity` roda o loop Hub → mini-jogo → Hub sem warnings.
- [ ] Sem erros vermelhos na Console durante uma sessão completa.
- [ ] **`Edu Framework → Teacher Export…`** para o mesmo slot escreve um CSV com linhas para as concept keys do novo jogo.

---

## 14. Apêndice: glossário e referência de menus

### Glossário

| Termo | Significado |
|------|---------|
| **AppContext** | Service locator estático inicializado no Bootstrap. Fornece `Audio`, `Input`, `Progress`, `Localization`. |
| **AudioCueSO** | ScriptableObject que embrulha um `AudioClip` com prioridade, volume, cooldown e chave de legenda. |
| **BNCC** | *Base Nacional Comum Curricular* — o currículo oficial brasileiro. `BnccTagSO` permite marcar desafios com códigos oficiais de habilidade. |
| **ChallengeSO** | Os dados de uma pergunta/rodada. Subclasses: Multiple Choice, Quantity, Memory Pair, Classification, Syllable. |
| **ChallengeSetSO** | Lista ordenada de desafios. Referenciado por `MiniGameConfigSO`. |
| **Concept key** | String curta (`phoneme:/b/`, `count:3`) usada para fatiar analytics no CSV do professor. |
| **DifficultyProfileSO** | Quantidade de rodadas e regras de adaptação de alternativas. |
| **EvaluationResult** | Struct (`Correct`, `ConceptKeys`, `LatencySeconds`) passada do mini-jogo para `RaiseAnswerEvaluated`. |
| **FeedbackKit** | Componente que toca o pulso de escala em acerto/erro. |
| **Hub** | A cena do mapa do mundo (`Hub.unity`) com um único `HubWorldController`. |
| **MiniGameBase** | MonoBehaviour abstrato com o ciclo de vida compartilhado e o accessor `Context`. |
| **MiniGameConfigSO** | Config por mini-jogo: `gameId`, challenge set, dificuldade, cues de sucesso/retry, story book opcional. |
| **MiniGameCatalogSO** | O registro: entradas mapeiam `gameId` → nome da cena + chave de exibição. |
| **MiniGameSessionHub** | Action estática `RequestExitToHub` que mini-jogos invocam ao terminar. |
| **ScriptableObject (SO)** | Asset de dados Unity, editado no Inspector, referenciado via drag-and-drop. |
| **Shell** | Controlador de UI que monta a interface de gameplay em runtime (`MultipleChoiceShellView`, `DragDropSlotsShellView`). |

### Menus do Editor

| Menu | O que faz |
|------|--------------|
| `Edu Framework → Generate Default Content (SOs + Scenes + Hub Wire)` | Cria todos os SOs de demo, as seis cenas `MG_*`, os prefabs de shell, o prefab template, adiciona cenas em Build Settings e liga a config do hub na cena `Hub.unity`. |
| `Edu Framework → Create Template Prefab (Multiple Choice Mini-Game)` | (Re)cria apenas o `MiniGame_Template_MultipleChoice.prefab`. |
| `Edu Framework → Teacher Export…` | Abre uma janela para exportar o JSON de progresso de um slot escolhido como CSV amigável ao professor. |

### Referência `Assets → Create → Edu`

| Caminho | Tipo | Observações |
|------|------|-------|
| `Edu / Audio / Audio Cue` | `AudioCueSO` | Embrulhe cada clip. |
| `Edu / Data / Challenge Set` | `ChallengeSetSO` | Lista de `ChallengeSO`. |
| `Edu / Data / Challenge / Multiple Choice` | `MultipleChoiceChallengeSO` | Tipo mais flexível de desafio. |
| `Edu / Data / Challenge / Quantity` | `QuantityChallengeSO` | Jogo de contagem. |
| `Edu / Data / Challenge / Memory Pair` | `MemoryPairChallengeSO` | Dois desafios com o mesmo `PairId` formam um par. |
| `Edu / Data / Challenge / Classification` | `ClassificationChallengeSO` | Categorização por drag-drop. |
| `Edu / Data / Challenge / Syllable Builder` | `SyllableChallengeSO` | Montagem ordenada de sílabas. |
| `Edu / Data / Difficulty Profile` | `DifficultyProfileSO` | Compartilhado entre os seis jogos por default. |
| `Edu / Data / Mini Game Config` | `MiniGameConfigSO` | Um por jogo. |
| `Edu / Data / Mini Game Catalog` | `MiniGameCatalogSO` | Um por projeto. |
| `Edu / Data / Hub Configuration` | `HubConfigurationSO` | Um por projeto; o hub lê este. |
| `Edu / Data / Story Page` / `Story Book` | `StoryPageSO` / `StoryBookSO` | Para `MiniGameStoryGaps`. |
| `Edu / Data / Unlock / Always Unlocked` | `AlwaysUnlockedRuleSO` | Nó sempre aberto. |
| `Edu / Data / Unlock / Require Completed Games` | `RequireMiniGamesCompletedRuleSO` | Destrava por `gameId`s. |
| `Edu / Data / Localized Table` | `LocalizedTableSO` | Linhas de localização (pt-BR + secondary). |
| `Edu / Data / BNCC Tag` | `BnccTagSO` | Metadados curriculares. |
| `Edu / Events / Answer Evaluated` | `AnswerEvaluatedEventChannelSO` | Event bus opcional. |
| `Edu / Events / MiniGame Session Event` | `MiniGameSessionEventChannelSO` | Event bus opcional. |
| `Edu / Events / Void Event` | `VoidEventChannelSO` | Event bus genérico opcional. |

---

*Veja também:*
- `Assets/EduFramework/Docs/Tutorial_Framework_PTBR.md` — tutorial conciso em português.
- `Assets/EduFramework/Docs/Guia_ScriptableObjects_PTBR.md` — referência completa dos ScriptableObjects em português.
- `Assets/EduFramework/Docs/User_Manual_EN.md` — versão em inglês deste manual.

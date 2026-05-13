# UI Toolkit para designers (neste repositório)

Este guia resume **onde** editar o aspeto visual sem alterar a lógica dos mini-jogos.

## Pastas

| Pasta | Conteúdo |
|-------|-----------|
| `Assets/EduFramework/UI Toolkit/Resources/EduUI/` | UXML e USS carregados em runtime (`Resources.Load("EduUI/…")` **sem** extensão). |
| `Assets/EduFramework/UI Toolkit/Resources/EduUI/Theme.uss` | Tokens de espaçamento, cores, raios, `.edu-min-touch`, `.edu-drop-bin`, `.edu-safe-area`. |
| `Assets/EduFramework/UI Toolkit/Resources/EduUI/EduDefaultPanelSettings.asset` | Criado pelo menu **Edu Framework → UI Toolkit → Ensure Default PanelSettings** (ou ao gerar conteúdo default). |

## Fluxo de trabalho

1. Abra um UXML (ex.: `MultipleChoiceShell.uxml`, `HubMap.uxml`) no **UI Builder**.
2. Ajuste layout, USS ou classes; mantenha os **`name=`** estáveis usados pelo C# (ver tabela abaixo). **Não use `var(--token)` em atributos `style` inline no UXML** — o Unity não resolve isso contra o USS importado e o `UIDocument` pode falhar com *"could not be cloned"*. Prefira valores literais no UXML ou regras só no `.uss`.
3. Guarde o ficheiro e faça **commit** no Git.
4. Valide no Editor (Play) e, quando possível, num **dispositivo móvel** (toque + área segura).

## Nomes estáveis (não renomear sem avisar dev)

| `name` | Uso |
|--------|-----|
| `map-root` | Contentor dos nós do hub. |
| `hub-node` | Raiz do template de nó (clone). |
| `node-label` | Texto do nó. |
| `choices-row` | Linha de opções de múltipla escolha. |
| `choice-button` / `choice-label` | Tile de escolha (`ChoiceTile.uxml`). |
| `counting-root`, `prompt-label`, `numbers-row` | Jogo de contagem. |
| `memory-root`, `memory-grid` | Memória. |
| `syllable-root`, `syllables-row` | Sílabas. |
| `dnd-root`, `bins-row`, `tokens-row` | Classificação / arrastar e soltar. |
| `shell-root` | Raiz da shell de múltipla escolha (classe `edu-feedback-root` para pulso). |

## Classes USS semânticas

- **`edu-feedback-root`** — alvo do `FeedbackKit` (escala).
- **`edu-drop-bin`** — zona de largar; o código define `userData` = id da categoria.
- **`edu-min-touch`** — tamanho mínimo de alvo tocável.
- **`edu-safe-area`** — margens `safe-area-inset` para notch / home indicator.

## Cena PoC

Menu **Edu Framework → UI Toolkit → Create Foundation Test Scene** cria `Assets/Scenes/UITK_FoundationTest.unity` com `FoundationPoc.uxml` (título + botão mock).

## Referências

- Manual: [§ 4.6 e § 6](Manual_Usuario_PTBR.md#46-ui-ui-toolkit).
- Plano / contexto do agente: [Prompt_Plano_UI_Toolkit_UX_PTBR.md](Prompt_Plano_UI_Toolkit_UX_PTBR.md).

# Prompt / contexto: plano UI Toolkit + UX designer

Este ficheiro guarda um **resumo executável** do plano de migração para UI Toolkit (foco UX designer), para reutilizar em sessões de agente sem reanexar o plano completo.

## Objetivo

- **Stack principal:** UI Toolkit (`UIDocument`, UXML, USS, `PanelSettings`) para Hub, shells e mini-jogos.
- **Dados:** continuam em ScriptableObjects (`Assets/Scripts/Data/`, `Assets/EduFramework/ScriptableObjects/`); C# só apresenta e reage a input.
- **Plataformas:** PC (rato/teclado) e mobile (toque); `PanelSettings` + tokens USS; área segura; alvos tocáveis ≥ ~44 pt (`.edu-min-touch`).

## Estrutura de pastas (repo)

- `Assets/EduFramework/UI Toolkit/Resources/EduUI/` — UXML/USS + `EduDefaultPanelSettings.asset` (gerado por menu/editor).
- Presenters: `HubWorldController`, `MultipleChoiceShellView`, `DragDropSlotsShellView`, mini-jogos em `Assets/Scripts/MiniGames/`.
- `GameplayUiUtility` — apenas `EnsureEventSystem()` (+ Input System).

## DoD por fase (resumo)

- **S:** PanelSettings + tema + UXML blank + PoC (`UITK_FoundationTest.unity`); uGUI antigo removido dos fluxos principais; manual com nota de migração.
- **M:** Hub + múltipla escolha + story + `FeedbackKit` UITK; `MiniGameBase` sem dependência de `Canvas`.
- **L:** DnD/classificação + counting/syllables/memory; zero `UnityEngine.UI` em gameplay ativo; `EduDefaultContentBuilder` alinhado; testes PC + mobile.

## Menus úteis (Unity Editor)

- `Edu Framework → Generate Default Content` — gera SOs/cenas e garante `EduDefaultPanelSettings`.
- `Edu Framework → UI Toolkit → Ensure Default PanelSettings`
- `Edu Framework → UI Toolkit → Create Foundation Test Scene`

## Documentação

- Manual PT/EN § 4.6 e § 6 atualizados para UITK.
- Guia designers: [UI_Toolkit_Designers_PTBR.md](UI_Toolkit_Designers_PTBR.md).

*(Conteúdo derivado do plano interno de migração; não substitui o repositório nem decisões de produto finais.)*

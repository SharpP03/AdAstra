@AGENTS.md

## Claude Code specifics
- Gamedev tasks: start with the `router` skill to pick the relevant skills (e.g. `unity-physics`, `unity-input-system`, `game-feel`). For the branch/task procedure use the `github-task` skill.
- The gamedev skills target Unity 6.3 LTS; this project is on 6000.2 — verify version-specific APIs before using them.
- Unity MCP (`UnityMCP`) needs the Unity Editor open on this project. Before mutations, read `mcpforunity://editor/state` (not compiling, not in Play Mode). Details: `unity-mcp-skill`.
- While editing C#: use LSP diagnostics (csharp-lsp) as the fast check. After adding, renaming or deleting a `.cs` file, run `refresh_unity` first — the generated `.csproj` lists files explicitly, so LSP doesn't see new files until Unity regenerates it.
- Before reporting a step as compiling: `refresh_unity` → wait for compilation → `read_console`. Unity's compiler is authoritative (warnings, serialization); LSP doesn't replace it.
- Play Mode check: `manage_editor` play on `SampleScene`, `read_console` for exceptions, then stop. This smoke check is the default verification; gameplay scenarios are tested by the user (end each step report with 2–4 "how to test" points). Run scripted Play Mode scenarios only when the user asks or when a change cannot be verified otherwise — keep them short and say why.

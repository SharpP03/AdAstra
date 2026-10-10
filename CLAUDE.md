@AGENTS.md

## Claude Code specifics
- Gamedev tasks: start with the `router` skill to pick the relevant skills (e.g. `unity-physics`, `unity-input-system`, `game-feel`). For the branch/task procedure use the `github-task` skill.
- The gamedev skills target Unity 6.3 LTS; this project is on 6000.2 — verify version-specific APIs before using them.
- Unity MCP (`UnityMCP`) needs the Unity Editor open on this project. Before mutations, read `mcpforunity://editor/state` (not compiling, not in Play Mode). Details: `unity-mcp-skill`.
- After every C# change: `refresh_unity` → wait for compilation → `read_console`. Only then report the step as compiling.
- Play Mode check: `manage_editor` play on `SampleScene`, `read_console` for exceptions, then stop.

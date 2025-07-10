Repository layout:
- MermaidAPI: node.js server providing Mermaid rendering.
- Olve.Diagrams: C# class library with Flowchart and Timelines subfolders.
- Olve.Diagrams.UI: Blazor WebAssembly UI using the library.
- Olve.Diagrams.Tests: Unit tests for the library.
- TaskDrawer.sln: solution including library, tests and UI.
- Flowchart tasks can include '(done)' and '(blocked)' states.
- Blocked tasks appear with a red 4px border in generated Mermaid diagrams.

Coding guidelines:
- Use conventional commits (feat:, fix:, chore: etc.).
- Keep all AGENTS.md files current when code or context changes.


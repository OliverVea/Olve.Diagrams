Repository layout:
- MermaidAPI: node.js server providing Mermaid rendering.
- Olve.Diagrams: C# class library with Flowchart and Timelines subfolders.
- Olve.Diagrams.UI: Blazor WebAssembly UI using the library.
- Olve.Diagrams.Tests: Unit tests for the library.
- TaskDrawer.sln: solution including library, tests and UI.
- Flowchart tasks can include '(done)' and '(blocked)' states.
- Explicitly blocked tasks have a red 4px border in generated Mermaid diagrams; other blocked tasks keep the default border.

Coding guidelines:
- Use conventional commits (feat:, fix:, chore: etc.).
- Keep all AGENTS.md files current when code or context changes.


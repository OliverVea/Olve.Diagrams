Flowchart module:
- Converts task lists to Mermaid graphs using MermaidGenerator and Scriban template.
- TaskListParser parses tasks; TaskListSorter orders them; TaskName provides typed names.
- MermaidGenerator builds graph nodes and edges; colors indicate done/blocked status.
- Blocked tasks get a red 4px border in the generated Mermaid graph.
- Tasks support '(done)' and '(blocked)' markers; blocked tasks propagate to dependents.
- Blockers may reference a task's qualified name like '1a'.


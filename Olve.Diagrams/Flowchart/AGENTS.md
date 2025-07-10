Flowchart module:
- Converts task lists to Mermaid graphs using MermaidGenerator and Scriban template.
- TaskListParser parses tasks; TaskListSorter orders them; TaskName provides typed names.
- MermaidGenerator builds graph nodes and edges; colors indicate done/blocked status.
- Tasks support '(done)' and '(blocked)' markers; blocked tasks propagate to dependents.


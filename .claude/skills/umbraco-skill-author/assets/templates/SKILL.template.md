---
name: <skill-name>
description: >
  <What it does in one line>. Use this whenever the user asks to <trigger phrase>,
  <another real user wording>, or <adjacent goal they'd phrase it as>.
  SKIP: <non-applicable cases, e.g. non-Umbraco projects or Umbraco < X>.
---

# <Skill Title>

<One-paragraph intro.>

<!-- Only if more than one approach: -->
| | **A — <name>** (default) | **B — <name>** |
|---|---|---|
| Reference | [approach-a.md](references/approach-a.md) | [approach-b.md](references/approach-b.md) |
| Source | <custom code / official docs> | <...> |
| Best for | <...> | <...> |

### How to decide
Default to A. Choose B when <condition>. Prefer the Umbraco Developer MCP; if unavailable, walk
the user through it manually — only fall back when <condition>.

## Version compatibility
Targets **Umbraco <X>+**. <Any "introduced in vX" API notes.>

## Best practices
- <Domain guidance, not just restated code.>

## Validation
Objective assertions live in [`evals/evals.json`](evals/evals.json); run them with
`umbraco-skill-evaluator`.

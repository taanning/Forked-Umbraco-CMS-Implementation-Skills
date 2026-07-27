# Skill templates

Ready-to-copy skeletons for a new skill. They live as real files under
[`../assets/templates/`](../assets/templates/) so you copy files rather than transcribe from a code
fence — less drift, and the templates can be validated like any other asset. Copy the ones you
need into your new skill's folder, rename, and replace every `<Placeholder>`. Delete parts you don't
need (a single-approach skill may need no `references/` and no `assets/`).

## Folder

```
plugins/<plugin>/skills/<skill-name>/
├── SKILL.md
├── references/<approach>.md      # one per approach (optional)
├── assets/<File>.cs              # code templates with <Placeholder> tokens (optional)
├── scripts/<helper>.py           # deterministic helpers the skill runs (optional)
└── evals/evals.json
```

## The templates

| Copy this | To here | Notes |
|---|---|---|
| [`assets/templates/SKILL.template.md`](../assets/templates/SKILL.template.md) | `SKILL.md` | Drop the decision table if there's only one approach. |
| [`assets/templates/approach.template.md`](../assets/templates/approach.template.md) | `references/<approach>.md` | One copy per approach; cross-link the alternatives. |
| [`assets/templates/evals.template.json`](../assets/templates/evals.template.json) | `evals/evals.json` | Keep the build-honesty expectation. |

Frontmatter `name` must match the skill's folder name, and every `<Placeholder>` (including the
`<skill-name>` in the eval JSON) must be replaced before shipping.

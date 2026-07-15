---
name: umbraco-skill-author
description: >
  A framework for building Umbraco skills in this marketplace: how to structure, scaffold, write,
  and audit a skill so it matches the house conventions. Use this whenever the user wants to start
  or build a new skill, e.g. "make a new skill for X", "how should I structure this skill", "turn
  these docs into a skill", "scaffold a skill", or "get this skill ready to ship". Ends with a
  self-audit checklist. Hands off to `umbraco-skill-evaluator` for the eval loop.
  SKIP: non-skill work.
---

# Umbraco Skill Author

A framework, checklist, and guide for building Umbraco skills. Follow it to go from an idea to a
shippable skill that matches the house conventions, then self-audit before handing off.

The shape of every skill: a **thin** SKILL.md that routes, detail in `references/`, code templates
in `assets/`, and objective assertions in `evals/evals.json`.

**Golden-standard example:** [`umbraco-sitemap`](../../../plugins/implementation/skills/umbraco-sitemap)
is the current reference skill. When in doubt, open it and copy its shape.

## How to use this

1. **Build** — walk [`references/authoring-steps.md`](references/authoring-steps.md): scope →
   scaffold → SKILL.md → references → assets → evals. Copy-paste skeletons are in
   [`references/skill-template.md`](references/skill-template.md).
2. **Audit** — before shipping, self-check against
   [`references/conformance-checklist.md`](references/conformance-checklist.md).
3. **Hand off** — pass the finished skill to `umbraco-skill-evaluator` to run with-skill vs.
   baseline and prove it earns its keep, then iterate from the results.

## Core principles (what the checklist enforces)

- **Thin SKILL.md.** It routes; it doesn't teach everything. Push detail, per-approach steps, and
  `if/else`-style branching into `references/` or `assets/` — separate files, not inline.
- **Docs are the source of truth.** Don't reproduce Umbraco API code from memory; link the docs
  and tell the agent to fetch them first. Ship verbatim code in `assets/` only when it's genuinely
  not in the docs (and say so).
- **`.md` doc links.** Fetch-me doc links point at the `.md` page (e.g. `.../composing.md`), or link
  a sibling skill instead of raw docs (progressive discovery / lazy-loading).
- **Don't duplicate.** A doc link in a reference file isn't repeated in SKILL.md.
- **Build honesty.** Never claim a verified build you didn't run.
- **Right place.** content-modelling → `plugins/content-modelling/skills/`; build-out/delivery →
  `plugins/implementation/skills/`; authoring tooling → `.claude/skills/`.

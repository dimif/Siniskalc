# Copilot Instructions

The repo rules are in `CLAUDE.md` in the root folder of the repo.

Logging rules (where to log, levels, message templates, what never to log) are in `docs/LOGGING.md`.
The `logging-reviewer` agent (`.github/agents/logging-reviewer.agent.md`, `.claude/agents/logging-reviewer.md`) checks code against them.

Never push to a remote unless the owner asked for it or agreed to it, and the agreement must be the answer to an explicit, separate question from you about that push; approval of a plan or of a list of actions does not count (see "Working in this repo" in `CLAUDE.md`).

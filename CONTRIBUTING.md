# Contributing to Escape From NDTB

## Getting Started

1. Clone the repository
2. Run `scripts/setup-hooks.sh` to install git hooks
3. Open the Unity project in `src/` with Unity `6000.4.10f1`

## Code Style

C# style rules are enforced via `.editorconfig` at the repository root.
Your IDE will highlight violations automatically. Key rules:

- **Access modifiers must be explicit** — no implicit `private` or `internal`
- **Naming conventions:**
  - `private` fields: `_camelCase`
  - `private static` fields: `s_CamelCase`
  - `public` fields / properties: `PascalCase`
  - Interfaces: `IPascalCase`
  - Methods: `PascalCase`
  - Local variables: `camelCase`

## Pre-commit Hook

The pre-commit hook runs `dotnet format src/Lint.csproj --verify-no-changes` on commit.
This enforces the full `.editorconfig` style rules (whitespace, naming conventions, etc.).

To enable it:

```bash
scripts/setup-hooks.sh
```

If the hook fails, run `dotnet format src/Lint.csproj` to auto-fix formatting issues.

## Pull Request Process

1. Keep changes focused on `src/Assets/Scripts/`
2. Ensure whitespace formatting passes (`dotnet format whitespace`)
3. All CI checks must pass before merging
4. PRs to `main` require review

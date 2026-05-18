# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Detester is a .NET library (NuGet package `Detester`) for writing deterministic, reliable tests against AI applications. It wraps any `Microsoft.Extensions.AI` `IChatClient` in a fluent builder that sends prompts and asserts on the responses.

## Build & Test Commands

Targets **.NET 10** only. Uses **central package management** — all package versions live in `Directory.Packages.props`, never in `.csproj` files. Add packages with `<PackageReference Include="X" />` (no `Version`) and add the version to `Directory.Packages.props`.

```bash
dotnet restore
dotnet build --no-restore

# Unit tests — these projects are OutputType=Exe (xUnit v3 / Microsoft Testing Platform).
# CI runs them via `dotnet run`, NOT `dotnet test`:
dotnet run --project test/Detester.Tests/Detester.Tests.csproj

# Integration tests (live Azure OpenAI — see env vars below):
dotnet run --project test/Detester.IntegrationTests/Detester.IntegrationTests.csproj
```

Run a single test (Microsoft Testing Platform filter syntax):

```bash
dotnet run --project test/Detester.Tests/Detester.Tests.csproj -- --filter-method "Detester.Tests.DetesterBuilderTests.MethodName"
```

`dotnet test --filter "FullyQualifiedName~Name"` also works locally, but CI uses the `dotnet run` form above — match it when reproducing CI failures.

### Build constraints

- `src/Detester` builds with `TreatWarningsAsErrors=true` and `StyleCop.Analyzers` is a global analyzer, so **StyleCop violations fail the build**. Public APIs require XML doc comments. `LangVersion` is `preview`.
- The two test projects do *not* treat warnings as errors.

### Integration test environment

`AzureOpenAIChatClientFixture` throws on construction if these are not set, so the entire integration suite fails fast without them:

- `AzureOpenAI__ApiKey`
- `AzureOpenAI__Endpoint`
- `AzureOpenAI__ChatDeploymentName`

## Architecture

Single shipping project: **`src/Detester`**. There is no separate assembly for abstractions — interfaces/types live in `src/Detester/Abstractions/` under the namespace `Detester.Abstraction` (note: singular), while the implementation namespace is `Detester`.

Core flow (per-prompt step model, v2.0+):

- `DetesterFactory.Create(IChatClient)` or `new DetesterBuilder(IChatClient[, ChatOptions])` → an `IDetesterBuilder`.
- The builder maintains a `List<PromptStep>` (`PromptStep.cs`). Each `WithPrompt` adds a step; `Should*` assertions attach to the **current (last-added) step**. Assertions declared before any prompt buffer into a `pendingStep` that the first prompt absorbs. `WithPrompts(a,b,c)` creates three steps; subsequent assertions attach to the last.
- `EvaluateInternalAsync` (in `DetesterBuilder.cs`) is the single execution engine. It builds one growing `ChatMessage` conversation (optional system instruction + each prompt + each assistant reply appended) and sends each step's prompt sequentially through the same `IChatClient`. **Each step's assertions are evaluated only against that step's response** — not against every response (this changed in 2.0).
- `EvaluateAsync` returns a structured `EvaluationResult` (public, in `Detester.Abstraction`) and does **not** throw on assertion failure. `AssertAsync` calls it and throws an aggregated `DetesterException` (preserving the original per-assertion message wording, and the JSON `InnerException`). `AssertReliablyAsync` loops it counting `EvaluationResult.Passed`. `DetesterDataset` runs it per case.
- Expectations are stored as small internal records per step: `EqualityExpectation`, `FunctionCallExpectation`, `JsonExpectation`, `SemanticExpectation`, `JudgeExpectation`, plus string lists. Semantic assertions need `WithEmbeddingGenerator`; judge assertions use `WithJudge` (or fall back to the main client).
- New `IDetesterBuilder` members are **default interface methods** (throwing default, overridden by `DetesterBuilder`) so external implementers don't break — keep this pattern when adding interface members.
- Configuration misuse throws (`DetesterException` for no prompts; `InvalidOperationException` for semantic-without-generator or `OrShouldContainResponse` with no prior assertion; `ArgumentException` for bad inputs). Assertion *failures* are data in `EvaluationResult`, surfaced as exceptions only by `AssertAsync`/`AssertReliablyAsync`.

Unit tests use `MockChatClient` / `CallbackMockChatClient` / `FakeEmbeddingGenerator` (in `test/Detester.Tests/`) — extend these rather than introducing a new mock when adding coverage. `CallbackMockChatClient` is the tool for per-prompt-varying responses (needed to test scoping).

## Conventions

- New builder methods: declare on `IDetesterBuilder` first with XML docs, implement on `DetesterBuilder` returning `this`, validate inputs early (`ArgumentException` for bad values, `ArgumentNullException` for nulls), and add an expectation record if it needs per-response evaluation in `DoAssertAsync`.
- Prefer `Microsoft.Extensions.AI` abstractions over direct provider SDKs in `src/Detester`.
- Update `README.md` for user-facing API changes; update the package `<Version>` in `src/Detester/Detester.csproj` and `Directory.Packages.props` for releases.

## Note on existing AI-assistant docs

`.github/copilot-instructions.md` is partially out of date: it references a separate `Detester.Abstraction` project and factory methods (`CreateWithOpenAI`, `CreateWithAzureOpenAI`, `Create(options)`, `DetesterOptions`) that **do not exist** in the codebase. Trust the actual source over that file.

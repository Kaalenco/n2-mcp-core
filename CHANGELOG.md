# Changelog

## 3.0.1 - 2026-11-24

Version unification: all N2 packages move to 3.0.1.

- Version: N2.Mcp.Core jumps from 1.1.1 to 3.0.1 (AssemblyVersion 3.0.0.0, FileVersion 3.0.1.0).
- Target frameworks: `netstandard2.0`, `net10.0`, `net11.0` (dropped `netstandard2.1` and `net8.0`).
  The sample server and the unit tests target `net10.0` and `net11.0`.
- Dependencies: per-framework references. `net10.0` and `netstandard2.0` use 10.0.15
  (Microsoft.Extensions.Configuration.Abstractions, System.Text.Json); `net11.0` uses
  Microsoft.Extensions.Configuration.Abstractions 11.0.0 and the in-box System.Text.Json.
  Microsoft.SourceLink.GitHub 11.0.100. Calculator: Microsoft.Extensions.DependencyInjection
  10.0.15 / 11.0.0. Test packages updated (NUnit 5.0.0, NUnit3TestAdapter 6.3.1,
  Microsoft.NET.Test.Sdk 18.12.0, coverlet.collector 10.1.1).
- Publishing: the workflow no longer pushes to nuget.org. A push to trunk publishes to the
  private `vault-packages` feed (`src/nuget.config`, secret `NUGET_PAT`); nuget.org is promoted
  from that feed by the n2-publishing repository.

Earlier history is in README.md.

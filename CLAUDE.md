# GOSAvaloniaServices: servicos nao visuais

Servicos reutilizaveis para aplicativos Avalonia (.NET 10): dialogos, notificacoes,
tema, navegacao, sem ser controles de UI. 6 projetos entram no `Nimloth.sln` e 1 no
`Sindarin.sln`. Solucao propria: `GOS Avalonia Services.sln`.
Regras compartilhadas em `../CLAUDE.md`; regras de Avalonia em `../.claude/rules/avalonia.md`.

- Cada servico tem interface propria (`GOSDialogServicesInterface`,
  `GOSNotificationInterface`) para permitir DI e mock. Manter esse padrao.
- DI pelo `BaseLibrary.DependencyInjection`.
- Testes em `test/<Projeto>.Tests` (xUnit + Moq + FluentAssertions). O primeiro e `test/GOSBaseInsection.Tests`
  (E095), headless (`Avalonia.Headless.XUnit`, `[AvaloniaFact]`, paralelismo desligado), no modelo do
  `GOSAvaloniaControl/test/GOSCustomControl.Tests`.

## Comandos

- `dotnet build "GOS Avalonia Services.sln" --no-restore`
- Consumidor: `dotnet build ../Nimloth/Nimloth.sln --no-restore`

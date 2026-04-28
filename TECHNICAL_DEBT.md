# Technical Debt

## Critical

### Thread safety in Home.cs
`Home.cs:261` — `DiscardOldUpdates()` acquires its own lock on `_entityUpdateHistory` despite already being called within a locked block. Reentrant but misleading.

## High

### Bare `throw new Exception()` throughout Comms.cs
10 instances (lines 239, 253, 256, 263, 289, 297, 316, 318, 433, 457). Should use specific exception types (`AuthenticationException`, `TimeoutException`, etc).

### Dangerous LINQ
`OzricService.cs:364` uses `devices.First(...)` which throws if no match. Should be `FirstOrDefault()` with null handling.

### Hardcoded IP
`Comms.cs:24`: `ws://192.168.2.48:8123/api/websocket`. Should be configurable.

### Graph deserialization string replacement hack
`Graph.cs:258-262` does `json.Replace("OnOff", "binary")` etc. globally. Could corrupt node IDs or string values containing these substrings.

### Unsafe casts without null checks
- `MainLayout.razor:441`: `(List<GraphEditAction>)result.Data` with no null/type guard.
- `MainLayout.razor:422`: `GetCustomAttribute<EditDialogAttribute>()!` assumes attribute always exists.

### No error handling in Program.cs startup
`Program.cs:96` — `await ozricEngine.Start(CancellationToken.None)` has no try-catch. If HA is unreachable, the entire app crashes with no UI.

### Environment variables logged including secrets
`Program.cs:43-45` prints all env vars including `CORE_TOKEN`.

## Medium

### God classes
- `Comms.cs` (485 lines) — connection, auth, routing, heartbeat, reconnection
- `AreaGraphView.razor` (995 lines, growing) — diagram init, node/edge management, edit history, undo/redo, unassigned-entity panel
- `MainLayout.razor` (551 lines) — app shell, graph lifecycle, subscriptions, dialogs, settings, download/upload

### Home.cs still owns its own comms
The Engine→Service decoupling (CommandBatcher, main loop) landed for the live update path, but `Home` (in `Ozric.Engine.Live`) still takes `IComms` and issues its own sync requests for entity/area/device lists. Engine remains coupled to comms transitively via `Home`. To finish the split, `Home` should be populated by the service with snapshot data and stop owning a connection.

### Public fields instead of properties
- `GraphNode.cs` — `public List<Pin> inputs` / `public List<Pin> outputs`
- `EngineStatus` fields: `comms`, `states`, `paused` (all lowercase public fields)
- Mixed naming: `Engine.paused` (lowercase) vs `Engine.IsReady()` (PascalCase)

### Console.WriteLine as logging in OzricService
`OzricService.cs` has 15 `Console.WriteLine` calls — the dashboard-wide Logger refactor swept everywhere except here, and the new `MainLoop` (added during the engine/service split) uses Console too. `SaveGraph()` also prints the entire graph JSON to console on every save.

### Static mutable state
`HaConnectionInfo.cs` uses static properties for `BaseHttpUrl`/`Token`. Makes testing difficult and creates hidden global coupling.

### Dead code
- `AreaGraphView.razor` — ~80 lines of commented-out 9.0 migration code
- `LightDialog.razor:86-89` — empty `OnCSMChanged` method
- `PaletteView.razor:63` — `NumNodesUsing()` returns hardcoded `1` with a "Placeholder for actual logic" comment

## Low

### Test coverage gaps
- Zero tests for any Dashboard component, `GraphEditState`, `EditHistory`, or `DataService`
- Test names are vague (`canProcessSunEvents`)
- New `Ozric.Service.Tests` covers `CommandBatcher` merging only — service-layer flow (loop, paused gate, comms→engine handoff) not covered

### Missing CancellationTokenSource disposal
`Comms.cs:174, 210` creates `CancellationTokenSource` without `using`.

### Incomplete IDisposable
`Comms.Dispose()` (line 225) doesn't dispose `pendingEvents` (`BlockingCollection<T>`) or `receivedMessages` (`BufferBlock<T>`).

### Sentry DSN hardcoded
`Program.cs:89` has the DSN string literal in source.

### Reflection without caching
`AreaGraphView.razor:982` uses runtime `Activator.CreateInstance` for unassigned-entity node creation, with no caching.

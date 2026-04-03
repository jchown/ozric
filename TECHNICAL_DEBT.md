# Technical Debt

## Critical

### ~~Memory leaks from incomplete event disposal~~ FIXED
- ~~`AreaGraphView.razor:191-196` — `Dispose()` only unsubscribes 3 of 8+ event handlers.~~ All 8 handlers now unsubscribed.
- ~~`ColorValuePicker.razor` — creates a `Timer` with no `IDisposable` implementation.~~ Now implements `IDisposable`, disposes timer.

### ~~Fire-and-forget async without error handling~~ FIXED
- ~~`MainLayout.razor:298` — `_ = Task.Run(...)` in `OnClickDone()`.~~ Now uses `Tasks.Run(...)`.
- ~~`Comms.cs:311` — `_ = Task.Run(MessagePump)` where `MessagePump` is `async void`.~~ Now uses `Tasks.Run(...)`, `MessagePump` is `async Task`.
- ~~`ColorValuePicker.razor:298` — fire-and-forget `Task.Run` for preview commands.~~ Now uses `Tasks.Run(...)`. Timer callback also wrapped in try-catch.

### ~~Race conditions in CommandBatcher~~ FIXED
- ~~`CommandBatcher.cs` — `handlers` dictionary accessed outside lock scope.~~ `Send()` now snapshots both `commands` and `handlers` under the same lock.

### Thread safety in Home.cs
- `Home.cs:256` — `DiscardOldUpdates()` acquires its own lock on `_entityUpdateHistory` despite already being called within a locked block. Reentrant but misleading.

## High

### Bare `throw new Exception()` throughout Comms.cs
9 instances (lines 240, 254, 257, 264, 290, 298, 317, 434, 458). Should use specific exception types (`AuthenticationException`, `TimeoutException`, etc).

### Dangerous LINQ
`OzricService.cs:274` uses `devices.First(...)` which throws if no match. Should be `FirstOrDefault()` with null handling.

### Hardcoded IP
`Comms.cs:25`: `ws://192.168.2.48:8123/api/websocket`. Should be configurable.

### Graph deserialization string replacement hack
`Graph.cs:263-273` does `json.Replace("OnOff", "binary")` etc. globally. Could corrupt node IDs or string values containing these substrings.

### Unsafe casts without null checks
- `MainLayout.razor:407`: `(List<GraphEditAction>)result.Data` with no null/type guard.
- `MainLayout.razor:389`: `GetCustomAttribute<EditDialogAttribute>()!` assumes attribute always exists.

### No error handling in Program.cs startup
`Program.cs:97` — `await ozricEngine.Start(CancellationToken.None)` has no try-catch. If HA is unreachable, the entire app crashes with no UI.

### Environment variables logged including secrets
`Program.cs:44-46` prints all env vars including `CORE_TOKEN`.

## Medium

### God classes
- `Comms.cs` (486 lines) — connection, auth, routing, heartbeat, reconnection
- `AreaGraphView.razor` (852 lines) — diagram init, node/edge management, edit history, undo/redo
- `MainLayout.razor` (515 lines) — app shell, graph lifecycle, subscriptions, dialogs, settings, download/upload

### Public fields instead of properties
- `Node.cs:28-29` — `public List<Pin> inputs` / `public List<Pin> outputs`
- `EngineStatus` fields: `comms`, `states`, `paused` (all lowercase public fields)
- Mixed naming: `Engine.paused` (lowercase) vs `Engine.IsReady()` (PascalCase)

### Console.WriteLine as logging
`OzricService.cs` has 13 `Console.WriteLine` calls. `SaveGraph()` (line 148) prints the entire graph JSON to console on every save.

### Static mutable state
`HaConnectionInfo.cs` uses static properties for `BaseHttpUrl`/`Token`. Makes testing difficult and creates hidden global coupling.

### Dead code
- `AreaGraphView.razor` — ~80 lines of commented-out 9.0 migration code
- `LightDialog.razor:87-90` — empty `OnCSMChanged` method
- `PaletteView.razor:67` — `NumNodesUsing()` returns hardcoded `1`
- `EntityNode.cs:16-17` — commented-out field

## Low

### Test coverage gaps
- Zero tests for any Dashboard component, `GraphEditState`, `EditHistory`, or `DataService`
- Only 16 test files total, all focused on engine logic
- Test names are vague (`canProcessSunEvents`)

### Missing CancellationTokenSource disposal
`Comms.cs:175-176, 211-212` creates `CancellationTokenSource` without `using`.

### Incomplete IDisposable
`Comms.Dispose()` (line 226) doesn't dispose `pendingEvents` (`BlockingCollection<T>`) or `receivedMessages`.

### Sentry DSN hardcoded
`Program.cs:90` has the DSN string literal in source.

### Reflection without caching
`AreaGraphView.razor:497-503` uses runtime reflection for node model creation on every node, with no caching.

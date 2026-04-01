# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ozric is a dynamic graph-based automation engine for Home Assistant, deployed as a [Home Assistant Add-on](https://github.com/jchown/ozric-addon). It provides a visual node editor (Blazor Server UI) for building automation graphs that control lights, switches, and other HA entities.

## Build Commands

```bash
dotnet restore              # Restore dependencies
dotnet build                # Build all projects
dotnet test                 # Run all xUnit tests
dotnet test --filter "FullyQualifiedName~ClassName.TestName"  # Run a single test
dotnet publish -c Release   # Publish release build
```

The solution targets .NET 9.0 (via global.json with `rollForward: major`).

## Solution Structure

| Project | Purpose |
|---------|---------|
| **Ozric.Engine** | Core graph engine: nodes, edges, values, HA WebSocket communication, serialization |
| **Ozric.Service** | Service layer: engine lifecycle (start/stop/restart), graph persistence, validation |
| **Ozric.Dashboard** | Blazor Server UI with MudBlazor + Z.Blazor.Diagrams for visual graph editing |
| **OzricEngineTests** | xUnit tests for the engine |
| **OzricUI** | Legacy UI (net6.0, superseded by Ozric.Dashboard) |

## Architecture

### Graph Engine (Ozric.Engine)

The engine executes a DAG (directed acyclic graph) of **Nodes** connected by **Edges**.

- **Node** (`Graph/Node.cs`): Abstract base with `OnInit()` and `OnUpdate()` lifecycle methods. Each node has typed input/output **Pins**.
- **NodeType** (`Graph/NodeType.cs`): Enum of all node types (Light, Switch, Tween, IfAll, DayPhases, etc.)
- **Values** (`Values/`): Type-safe pin values — `Binary`, `Number`, `Color`, `Mode` (enum `ValueType`)
- **Edges** (`Graph/Edge.cs`): Connect an output Pin on one node to an input Pin on another

**Node categories:**
- `Graph/Entities/` — Home Assistant entity wrappers (Light, Switch, BinarySensor, MediaPlayer, Person)
- `Graph/Logic/` — Logic/transform nodes (IfAll, IfAny, BinaryChoice, Tween, NumberCompare, ModeSwitch)
- `Graph/Environment/` — Environmental inputs (Weather, SkyBrightness, DayPhases)

### Engine Main Loop (Live/Engine.cs)

The engine initializes all nodes, then enters a loop: update nodes in dependency order, wait for HA state changes or timeout, repeat. Uses a `CommandBatcher` to throttle entity updates (max 5 per 15 seconds per entity).

### Home Assistant Communication (Live/Comms.cs)

WebSocket-based communication using `Messages/` (55+ message types). Connects via `SUPERVISOR_TOKEN` (add-on mode) or `CORE_TOKEN` (local dev).

### Serialization

Custom polymorphic JSON using `System.Text.Json`. Derived types are registered via `[TypeKey]` attribute on concrete classes. Custom converters in `Serialization/` handle Node, Value, ClientCommand, and ServerMessage hierarchies.

### Service Layer (Ozric.Service/OzricService.cs)

Manages engine lifecycle. Loads/saves `graph.json` from disk. Validates graph integrity on startup (removes broken edges). Infers node area assignments from HA entity/device configs.

### Dashboard (Ozric.Dashboard)

Blazor Server app using:
- **MudBlazor** for UI components
- **Z.Blazor.Diagrams** for the visual graph editor
- **SignalR** for real-time updates
- Undo/redo via `GraphEditState`
- Area-based navigation (rooms/zones from HA)

Entry point: `Ozric.Dashboard/Program.cs` — configures Kestrel, connects to HA Supervisor API, starts OzricService, serves Blazor app.

## Key Patterns

- **Adding a new node type**: Create class in appropriate `Graph/` subdirectory, extend `Node`, add entry to `NodeType` enum, add `[TypeKey(NodeType.YourType)]` attribute, implement `OnInit`/`OnUpdate`
- **Pin connections**: Pins are typed (`ValueType`). Edges connect output pins to input pins of matching types
- **Testing**: Tests use mock implementations in `OzricEngineTests/mocks/` (MockComms, MockHome, MockEngine)
- **Storage**: Graph saved as JSON to `/data` (Linux/Docker) or `~/.ozric/data` (local dev)

# Settlers of Catan — Multiplayer Game Engine

A full-stack multiplayer Catan implementation built to explore clean architecture, real-world database tradeoffs, and deployment patterns in .NET.

## Why This Project

Catan's complexity lives in business logic, not infrastructure. The game has dozens of interconnected rules — turn sequencing, resource management, trade validation, robber placement — that demand a clear separation between domain logic and everything around it. This project was an exercise in building that separation correctly: designing a domain model from scratch, pushing infrastructure concerns to the edges, and discovering how clean architecture actually helps when the rules are genuinely complicated.

It was also a vehicle for learning .NET as a full-stack framework, working with databases in a real context (not just ORMs), and understanding what Aspire and cloud deployment actually entail beyond the abstractions.

## Architecture

### Domain-First Design

The core game engine lives in the domain layer as isolated business logic. Turn rules, settlement placement, trade validation, and robber mechanics are pure functions with no infrastructure dependencies. This wasn't optional — Catan's rules are intricate enough that without this isolation, adding or changing a rule became fragile quickly.

**Trade-off**: More code upfront (entities, value objects, domain services), but the payoff is testability and maintainability when the rule set grows.

### Redis Over SQL

The game state is ephemeral — a match lives in memory, players disconnect and reconnect, and once a game ends, there's no need to keep it. A traditional relational database adds cost, complexity, and latency for persistence you don't actually need.

Chose Redis as the primary store. Game state is serialized to JSON, with Lua scripts handling atomic operations (e.g., "increment resource AND validate not exceeding hand limit" as a single operation). This avoided the transaction-coordination overhead of distributed SQL.

**Trade-off**: Lost schema validation and complex queries, but gained simplicity, cost efficiency, and sub-millisecond access patterns. The repository pattern abstracts Redis details, so the domain layer never knows it's there.

### SignalR for Real-Time Updates

Multiplayer games require two-way communication with sub-second latency. SignalR is mature, handles reconnection transparently, and integrates seamlessly with the .NET stack. Considered WebSocket implementations directly, but the scaffolding savings (groups, hubs, automatic serialization) were worth the abstraction.

Every command (place settlement, trade, roll dice) needs to send the updated game state to all players in that match. This meant dozens of handlers with identical code: execute command, grab the game, serialize it, send to group. Rather than repeat that pattern, used a decorator to wrap command handlers — the decorator handles the broadcast, the command handler focuses on the domain logic.

**Trade-off**: A small amount of reflection overhead, but eliminated hundreds of lines of boilerplate. The pattern works cleanly here because the cross-cutting concern (broadcast to players) is identical across all commands.

### Full Game State Events, Not Incremental Diffs

After each action, the server broadcasts the entire current game state to all players. This could seem wasteful, but Catan's state is small — a handful of resource counts, a few dozen board positions, up to 4 active players. Incremental updates would have saved bandwidth at the cost of reconciliation logic, ordering guarantees, and debugging complexity.

**Trade-off**: Simpler client code and server logic in exchange for negligible network cost.

### 3D Board in Three.js + React

The physical board needed to be rendered in 3D with interactive elements (clickable hexes, piece placement, animation). Three.js is the de facto standard and has solid tooling for loading external models — useful if board variants with custom assets become a feature later.

React handles the UI layer (player panels, trade dialogs, turn timer). Familiar stack, and the separation keeps game logic out of component code.

## Tech Stack

- **Backend**: C# / .NET 9, Aspire for orchestration
- **Real-Time**: SignalR
- **Data**: Redis (in-memory store with Lua scripts for atomicity)
- **Frontend**: React, TypeScript
- **3D Rendering**: Three.js
- **Deployment**: Azure Container Apps

## Running Locally

```bash
aspire run
```

This starts the full stack: Redis, the backend API, and the development frontend. Open `localhost:5173` in your browser.

To deploy, the Aspire project handles container orchestration and resource provisioning.

## What This Taught

Building Catan forced clarity on what "clean architecture" actually means in practice — it's not an abstraction layer, it's a discipline. Choosing Redis was a reminder that the best tool isn't always the fanciest one; it's the one that fits the problem. And living with SignalR in production showed the value of picking boring, proven technology for coordination problems.

These lessons apply directly to building backend systems at scale: domain-driven design prevents rule-spaghetti, pragmatic infrastructure choices avoid over-engineering, and early attention to observability (which Aspire bakes in) pays dividends.

# Airly ATC

Airly ATC is the desktop controller application for the Airly global flight-simulation network.

## Current scope

This first build establishes the ATC application shell and the core position-ownership model:

- Worldwide airport/position architecture
- One human controller per ATC position
- FIFO controller queues
- Automatic AI fallback when a position has no human controller
- Automatic AI release when a human takes a position
- Automatic AI resumption when a human leaves and no queued controller is waiting
- Desktop UI foundation for the future live radar, voice, SimBrief, and Airly realtime services

## Project

- .NET 8
- WPF
- Windows desktop
- C#

## Run

Open `Airly-ATC.sln` in Visual Studio 2022+ with the .NET 8 SDK installed and run the project.

The current build uses local demo data for the position-management UI. The next implementation stage will replace the demo data with the Airly Network API and global airport data service.

## Architecture target

```
Airly ATC
  |
  +-- Airport Data
  +-- ATC Position Manager
  +-- Queue Manager
  +-- AI Fallback Controller
  +-- Airly Realtime Client
  +-- Voice Client
  +-- SimBrief Integration
  +-- Live Radar
```

The position manager is authoritative about human ownership. The AI controller is active only when a position has no human owner.

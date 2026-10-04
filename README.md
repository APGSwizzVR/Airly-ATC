# Airly ATC

Airly ATC is the Windows controller client for the Airly network.

This revision replaces the original position-list prototype with the foundation of a VATSIM-style controller workstation:

- Radar scope with live simulated traffic movement
- Aircraft tags, headings, altitude and squawk
- Flight strips
- Worldwide-position search architecture
- DEL / GND / TWR / APP positions
- Human controller ownership
- Controller queues
- Automatic Airly AI fallback when no human owns a position
- Position release and automatic queue promotion
- COM1 frequency display and controller radio UI
- ATIS and controller workspace entry points
- Zoomable radar
- Network status and UTC clock

The current traffic is deliberately simulated so the desktop application can run without a backend. The next network layer will replace this simulation with Airly WebSocket state, real simulator traffic, live controller ownership, voice routing, flight plans and worldwide airport/procedure data.

## Architecture

The controller client is intentionally separated from the authoritative network:

Airly Client / Simulator -> Airly Network Gateway -> Airly ATC Client

The server will own aircraft state, controller positions, queues, flight plans, frequencies, handoffs and AI/human state. The desktop client is a presentation and control surface.

## Build

Requires .NET 8 SDK on Windows.

    dotnet restore
    dotnet build
    dotnet run

This repository is not a copy of VATSIM software. It implements Airly's own network and UI while targeting comparable controller workflow and functionality.

# DWIS.DAQBridge.UDPTopSideData.Source

## Purpose
Provides a UDP broadcast source that emits top side drilling real-time signals for the UDP Top Side Data bridge. This helper program is primarily used to generate or replay datagrams for development, testing, and integration validation.

## What It Does
- Reads configuration to determine the UDP port (and culture where applicable).
- Broadcasts UDP datagrams containing top side drilling signals.
- Acts as a deterministic signal generator for downstream bridge components.

## Typical Use Cases
- Local development of the UDP bridge pipeline.
- End-to-end tests for parsing and semantic mapping.
- Integration demos where a live rig feed is unavailable.

## Related Projects
- `DWIS.DAQBridge.UDPTopSideData.Model` for signal semantics and configuration types.
- `DWIS.DAQBridge.UDPTopSideData.Server` and `DWIS.DAQBridge.UDPTopSideData.Sink` for ingestion and publishing.

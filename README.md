# DAQ Bridge: UDP Top Side Data

## Overview
This solution provides an end-to-end bridge between a top side drilling data logger that broadcasts UDP datagrams and the D-WIS Blackboard. It ingests real-time UDP signals, maps them to the D-WIS semantic vocabulary, and publishes machine-readable measurements for downstream consumers.

## Solution Components
- `DWIS.DAQBridge.UDPTopSideData.Model`: Defines the semantic data model, manifests, and query metadata for each top side signal.
- `DWIS.DAQBridge.UDPTopSideData.Server`: The deployable bridge service that listens for UDP datagrams, maps fields to the model, and publishes to the D-WIS Blackboard.
- `DWIS.DAQBridge.UDPTopSideData.Source`: A helper generator that broadcasts UDP datagrams for development, testing, and demos.
- `DWIS.DAQBridge.UDPTopSideData.Sink`: A consumer that reads and validates the published signals from the D-WIS Blackboard.

## Typical Data Flow
1. The UDP logger (or the `Source` simulator) broadcasts top side drilling data.
2. The `Server` ingests and semantically annotates the signals.
3. Measurements are published to the D-WIS Blackboard.
4. The `Sink` (or other consumers) queries and consumes the signals.

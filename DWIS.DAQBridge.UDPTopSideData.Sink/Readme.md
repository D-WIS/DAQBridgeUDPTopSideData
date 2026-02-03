# DWIS.DAQBridge.UDPTopSideData.Sink

## Purpose
Provides a sink/consumer application that reads semantically annotated top side drilling signals from the D-WIS Blackboard. This project is typically used to validate that the UDP bridge publishes correct data and semantics.

## What It Does
- Connects to the D-WIS Blackboard.
- Queries for top side drilling signals using the UDP Top Side Data semantic model.
- Ingests and displays/consumes the measurements for verification or integration testing.

## Typical Use Cases
- Validating end-to-end data delivery from UDP source to D-WIS.
- Integration tests for semantic queries and signal availability.
- Demonstrations of downstream consumption without a full client stack.

## Related Projects
- `DWIS.DAQBridge.UDPTopSideData.Model` for semantic definitions and query metadata.
- `DWIS.DAQBridge.UDPTopSideData.Server` for the ingestion and publishing pipeline.
- `DWIS.DAQBridge.UDPTopSideData.Source` for a UDP datagram generator.

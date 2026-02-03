# DWIS.DAQBridge.UDPTopSideData.Model

## Purpose
Defines the semantic model and configuration types for the UDP Top Side Data bridge. The classes in this project describe how UDP logger fields map to the D-WIS vocabulary (nouns, verbs, quantities, and reference frames) so downstream components can publish consistent, machine-readable drilling signals.

## What It Contains
- `UDPTopSideData`: a `DWISData` model with semantic annotations and manifest/SPARQL metadata for each signal.
- `ConfigurationForUDP`: UDP ingestion settings such as `UDPPort` and `Culture`.

## Typical Signals Modeled
- Depth at bottom of string (BOS)
- Flow rate in at top of string
- Active pit volume
- Block position (hook position)
- Standpipe pressure (SPP)
- Rate of penetration (ROP)
- Hook load at anchor and at top drive
- Surface torque and surface rotational speed (RPM)
- Drilling fluid temperatures in and out
- Weight on bit (WOB)

## Used By
Referenced by the UDP Top Side Data source/server projects to translate incoming UDP datagrams into semantically annotated D-WIS measurements.

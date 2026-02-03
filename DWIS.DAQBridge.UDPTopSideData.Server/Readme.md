# DWIS.DAQBridge.UDPTopSideData.Server

## Purpose
Hosts the UDP Top Side Data bridge service that ingests top side drilling datagrams and publishes semantically annotated measurements to the D-WIS Blackboard. This project is the deployable server (Docker image) that wires the UDP source to D-WIS output.

## What It Does
- Listens for UDP datagrams produced by a top side drilling logger.
- Parses fields into the `DWIS.DAQBridge.UDPTopSideData.Model` schema.
- Applies semantic annotations and manifests for D-WIS.
- Publishes the resulting measurements to the D-WIS Blackboard.

## Key Components
- `Worker`: the background service that processes incoming UDP data and performs the mapping/publish pipeline.
- `config.json`: runtime configuration, including UDP port and culture settings.
- `Dockerfile`: container build for deployment.

## Related Projects
- `DWIS.DAQBridge.UDPTopSideData.Model` for the semantic data model.
- `DWIS.DAQBridge.UDPTopSideData.Source` for a UDP test/simulator.
- `DWIS.DAQBridge.UDPTopSideData.Sink` for downstream publishing integration.

# Deployment
Install and run a replicated DWIS Blackboard. Here is the installation command:
```sh
docker run  -dit --name blackboard -P -p 48030:48030/tcp --hostname localhost  digiwells/ddhubserver:latest --useHub --hubURL https://dwis.digiwells.no/blackboard/applications
```

Install and run the UDP Top-side Drilling Data to DWIS bridge run the following command:
```sh
docker run -dit --name UDPTopSideData -v c:\Volumes\DWISDAQBridgeUDPTopSideDataServer:/home digiwells/dwisdaqbridgeudptopsidedataserver:stable
```
Here is an example config.json file for a docker based configuration.
```json
{
  "LoopDuration": "00:00:00.1000000",
  "GeneralBlackboard": "opc.tcp://host.docker.internal:48030",
  "UDPPort": "1502",
  "Culture": "nb-NO"
}
```


# Bug & Performance Validation Report

Generated: 2026-04-06 00:30:41 +03:00

## Test Definitions

1. `stop-listening`: Start/stop `TcpSocketServer`; process should exit cleanly.
2. `null-query`: `RestApiClient.ConstructRequest` with null query value should not throw.
3. `remote-endpoint-pattern`: Source scan should not contain unsafe `RemoteEndPoint).Port` dereference.
4. `wsrequest-dispose`: `WebSocketRequest` completion should dispose timeout resources.
5. `constructrequest-perf`: micro-benchmark for request construction throughput.

## Before (Current Code)

| Test | ExitCode | Result | Output |
|---|---:|---|---|
| `stop-listening` | -532462766 | FAIL | Unhandled exception. System.Net.Sockets.SocketException (10004): A blocking operation was interrupted by a call to WSACancelBlockingCall. / at System.Net.Sockets.Socket.UpdateStatusAfterSocketErrorAndThrowException(Socke... |
| `null-query` | 1 | FAIL | NullReferenceException: Object reference not set to an instance of an object. |
| `remote-endpoint-pattern` | 1 | FAIL | Exception: unsafe RemoteEndPoint port dereference pattern still present |
| `wsrequest-dispose` | 1 | FAIL | Exception: CancellationTokenSource is not disposed after completion |
| `constructrequest-perf` | 0 | PASS | PERF_MS=7861;ITER=2500;PARAMS=160 |

## After (Patched Code)

| Test | ExitCode | Result | Output |
|---|---:|---|---|
| `stop-listening` | 0 | PASS | PASS: stop-listening-clean |
| `null-query` | 0 | PASS | PASS: null-query-safe |
| `remote-endpoint-pattern` | 0 | PASS | PASS: remote-endpoint pattern safe |
| `wsrequest-dispose` | 0 | PASS | PASS: wsrequest resources disposed |
| `constructrequest-perf` | 0 | PASS | PERF_MS=1072;ITER=2500;PARAMS=160 |

## Before vs After

| Test | Before | After | Note |
|---|---|---|---|
| `stop-listening` | FAIL (-532462766) | PASS (0) | Status changed |
| `null-query` | FAIL (1) | PASS (0) | Status changed |
| `remote-endpoint-pattern` | FAIL (1) | PASS (0) | Status changed |
| `wsrequest-dispose` | FAIL (1) | PASS (0) | Status changed |
| `constructrequest-perf` | PASS (0) | PASS (0) | PERF_MS: 7861 -> 1072 (delta -6789 ms, -86.36%) |

# Unified application hub

One authenticated endpoint, `/hubs/app`, handles private notifications and lecture code synchronization. `AppHub` is the only hub. Its user ID provider is nested inside `AppHub.cs`, and the group name is produced by a normal hub helper method; `IAppClient.cs` defines the outgoing events. Browser clients connect with `withCredentials: true`; the existing HttpOnly access cookie authenticates the connection without exposing tokens to JavaScript. Connections close when authentication expires. Refresh through the existing HTTP endpoint, then reconnect and rejoin files.

## Client-to-server methods

- `JoinFile(fileId)` checks file access and joins its file group without returning data. File and lecture lifecycle events also target that file group. The lecture list provides the optional `FileId`; the frontend uses it to retrieve the file through the global cache decorator.
- `LeaveFile(fileId)` removes that connection's file subscription without a database read.
- `SaveChanges(fileId, batch)` is restricted by `[Authorize(Roles = ...)]` to Instructor/Admin. Application resolves the file’s lecture and checks ownership and requires a live lecture.

`OnConnectedAsync` validates the authenticated user identifier and browser origin. SignalR clears group memberships when a connection closes. UserService reads the authenticated principal through IHttpContextAccessor. No hub filter is registered; validation and application exceptions use SignalR’s default error handling. Detailed errors remain disabled.

## Server-to-client methods

- `ReceiveNotification(notification)` delivers a notification only to the specified authenticated user, across that user's connections. Server code calls `ISignalRService.ReceiveNotification(userId, notification)`; clients cannot invoke a notification broadcast method.
- `UpdateCode(changes)` sends only accepted edit batches to the file group, including the instructor. It never includes the full source file.
- `LectureEnded(lectureId)` and `FileDeleted(lectureId, fileId)` notify subscribers of lifecycle changes.

`CodeSnapshotDTO` contains `Code`, file name/type, IDs, and revision. The application snapshot query/handler supplies the current code, using SQL entities with loaded access relationships. There is no hub GetSnapshot method. The frontend receives the file ID in the lecture list and retrieves file data separately; the global cache decorator serves the file detail DTO. HTTP file updates are for lecture preparation; live edits use `SaveChanges`. File deletion is blocked while live. Every accepted code update is committed to SQL. Redis only caches reads.

## Edit protocol

```json
{
  "batchId": "<new UUID for this batch>",
  "baseRevision": 0,
  "changeSets": [
    { "changes": [ { "rangeOffset": 0, "rangeLength": 0, "text": "int main() {}" } ] }
  ]
}
```

Each change set is one Monaco `onDidChangeModelContent` event. Copy its `rangeOffset`, `rangeLength`, and `text`. Offsets are UTF-16, matching JavaScript and .NET strings. Edits within one event refer to the same original document and are applied from right to left; change sets are applied in their original event order. Both editors must use the same initial source text and line endings.

Batch events over roughly 100–300 ms without dropping any events. Keep one unacknowledged batch per file and queue new local edits until acknowledgment. If an acknowledgment is lost, retrieve the current file before retrying. A stale base revision is rejected. The file-data response includes `SavedContent` and `SavedRevision` to restore the document.

`SaveChanges` returns no data; the `UpdateCode` event acknowledges the accepted revision for everyone subscribed to that file, including the instructor. `UpdateCode` carries the file ID, lecture ID, batch ID, base revision, new revision, and change sets. The instructor must not reapply their own echoed batch because those edits already exist locally. Students apply only a batch whose base revision matches their local document. Ignore already-applied revisions; on a gap or out-of-order batch, stop applying edits until authoritative source text has been obtained. Guard programmatic editor updates so they do not generate another outgoing edit batch.

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${apiUrl}/hubs/app`, { withCredentials: true })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", notification => showNotification(notification));
connection.on("UpdateCode", batch => applyOrResynchronize(batch));
connection.on("LectureEnded", lectureId => markLectureEnded(lectureId));
connection.on("FileDeleted", (lectureId, fileId) => removeLectureFile(lectureId));
connection.onreconnected(async () => {
  await joinAndSynchronize();
});

await connection.start();
await joinAndSynchronize(); // retrieve file data using FileId from the lecture list and reconcile queued edits
// Everyone: await connection.invoke("JoinFile", fileId);
// Instructor: await connection.invoke("SaveChanges", fileId, batch);
```

## Redis and persistence

The global caching decorator handles Redis reads and invalidation through `IRedisService`. `GetLectureFileDetailQuery` and `SaveChangesCommand` share the key `file:{fileId}`. The edit handler loads the SQL entity and its access relationships, checks access and revision, applies edits, and commits through the unit of work. The decorator then removes affected cache prefixes. The command returns CodeChangesDTO. Finally, Hub.SaveChanges adds the instructor to the file group and forwards that DTO directly to IAppClient.UpdateCode on Clients.Group, so every subscribed connection, including the instructor, receives the changes exactly once. It never caches the update response. The next file query loads SQL and fills Redis again.

Every accepted code update is stored in SQL immediately. Ending a lecture changes its status; it does not flush code from Redis. Losing or expiring Redis data causes a database read, without losing saved code. EF optimistic concurrency protects against two writers updating the same saved file version. Concurrent lecture completion and edits still require coordination if multiple instructor connections are supported.

SignalR notifications are best-effort and not durable. Scrutor decorates ISignalRService with SignalRServiceLoggingDecorator in the API AddDecorators extension. Failed broadcasts are logged without failing already accepted changes; reconnect/revision-gap handling must resynchronize. The current SignalR registration uses the in-process lifetime manager. Deploy one API instance until a Redis backplane or managed SignalR service is configured; ordinary cache operations do not distribute broadcasts between API instances.

## Tests

Workflow tests cover cached edits, stale revisions, authorization, SQL persistence, and hub responses.

`Cors:AllowedOrigins` is a JSON array of origin URLs. Both CORS registration and the hub origin check read that array. Environment variables can override entries using `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.

## Access checks

`IAcademicAccess` performs synchronous checks on supplied domain entities. It has no repositories or database operations. Handlers and the hub load the required relationships through the `GetById` overload with explicit include expressions before calling it. Includes load only the caller's active enrollment. Repositories have no per-request entity cache; cold reads are checked in their handlers, and cached results are checked before being returned. Cached lists are validated with a single filtered query. The entities remain untracked; mutations attach only the root being changed.

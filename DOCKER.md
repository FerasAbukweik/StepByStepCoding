# Docker development stack

The root `.env` supplies the SQL Server, JWT, Redis and Seq secrets. Keep it local; it is excluded from Git and image build contexts. Its variables are grouped with comments. The generated values are for this local installation.

```sh
docker compose run --rm --build migrations
docker compose up -d --build
```

- Frontend: http://localhost:4000 (or `FRONTEND_PORT`).
- Seq: http://localhost:5341 (or `SEQ_PORT`), user `admin`, password from `SEQ_ADMIN_PASSWORD`.
- API and SignalR are proxied through the frontend at `/api` and `/hubs`.
- SQL Server and Redis are available only inside the Compose network.
- API logs go to Console, daily files in `api_logs`, and Seq.

Apply migrations to both databases with:

```sh
docker compose run --rm --build migrations
```

This starts SQL Server if needed, waits for its health check, then updates the security database followed by the application database. Connection settings come from the same Compose environment as the backend, using `.env`. Existing migrations are skipped; databases are created if missing. The command exits with an error on the first failed update. The databases are updated separately, so an update already completed is retained if the second one fails; rerun after fixing the cause.

The migration service uses the `tools` profile and does not run during normal `docker compose up`. No SDK or EF tool installation is required on the host.

Changing the Seq first-run password in `.env` does not change the password in an existing Seq data volume. Use Seq's account settings for an existing installation.

If Docker fails with a missing `docker-credential-desktop`, repair the local Docker credential-helper configuration. For public images, a temporary empty Docker configuration can also be used:

```sh
mkdir -p /tmp/codeclass-docker-config
docker --config /tmp/codeclass-docker-config compose build
```

## Initial admin

After applying migrations, rebuild and start the backend. When `ADMIN_SEED_ENABLED=true`, startup creates the admin using `ADMIN_FULL_NAME`, `ADMIN_EMAIL`, and `ADMIN_PASSWORD` from `.env`. Read the generated credentials in your local `.env`.

Seeding reuses the registration transaction with the Admin role and does not issue tokens. An existing admin is left unchanged, including its password. If that email belongs to a non-admin, startup stops instead of promoting the account. Public signup still cannot grant Admin. Set `ADMIN_SEED_ENABLED=false` after the first successful setup if desired. `Program.cs` explicitly calls the scoped `AdminSeeder` before starting the HTTP server. It does not run as a hosted/background service or while applying migrations.

## Personal workspace files

Apply the new `AddWorkspaceFiles` migration before starting the updated backend. It creates `WorkspaceFiles` with a unique `UserId` index; existing lecture files remain in `LectureFiles`. Accounts live in the separate security database, so `UserId` is a logical account reference, like student identifiers in enrollments.

`GET /api/auth/me` returns `fileId` and lazily creates the user's single workspace file if needed. `GET /api/files/{id}` reads either kind of file; the previous `/api/lecturefiles/{id}` route remains supported. Personal files can only be read or modified by their owner. Lecture access retains section/enrollment rules.

`AppHub.SaveChanges` persists code, file name, language and revision, then broadcasts the committed changes to the file group, including the sender. The shared code-page component loads its file in `ngOnInit`, supports autosave and download, and preserves drafts on conflicts. Code execution is not provided.

Frontend API GET responses are cached in memory for 30 seconds, with pending requests shared. Authentication requests, including Me, are excluded. Successful writes and realtime changes invalidate the cache; logout clears it. The file service stores all file responses in one map keyed by ID and clears the active file on leaving its editor so it is refreshed on reopening.

To update an existing Docker installation:

```sh
docker compose run --rm --build migrations
docker compose up -d --build backend frontend
```


## Student code execution

Piston is an optional service in the `execution` Compose profile. The API and
shared editor support C++, Python and Java, with stdin, compiler errors and stdout.
Execution uses the current editor draft and does not overwrite the saved file.

**Host requirement:** the official Piston container requires `privileged: true`.
Its sandbox limits student processes, networking, CPU time, memory and output,
but privileged Docker is not a complete security boundary from the host. Use a
dedicated Linux VM for untrusted student workloads. No project files, application
secrets or Docker socket are mounted into this service.

After explicitly accepting this requirement on the selected host:

```bash
docker compose --profile execution up -d piston
docker compose --profile execution run --rm piston-setup
docker compose up -d --build backend frontend
```

The setup installs the pinned language versions from `.env` into the
`piston_packages` volume. It is safe to repeat; installed versions are skipped.
The initial installation needs internet access and may take several minutes.
The execution API is available only inside the Docker network; no host port is exposed.

`POST /api/files/{id}/run` accepts `code` and `stdin`. File access is checked
before calling Piston. Compilation/runtime failures are returned as results;
an unavailable engine returns 503. Two executions can run concurrently;
additional requests receive 429. Requests do not cache or invalidate saved files.
Ordinary `docker compose up` does not start the privileged execution profile.

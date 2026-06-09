<p align="center">
  <img src="logo/logo_full.png" alt="Random Reel" width="480"/>
</p>

# Random Reel — Jellyfin Plugin

A Jellyfin plugin that plays random clips from any library or playlist, starting at a random position. Designed for a serendipitous viewing experience on TVs and browsers — pick a folder, choose a clip duration, and let it run indefinitely.

---

## Features

- **Standalone TV app** — accessible at `/RandomReel/app`, works in any browser including LG TV and Android TV
- **Quick Connect login** — sign in without typing a password: generate a code on the TV, approve it from the Jellyfin app on your phone
- **Random start position** — clips begin at a random timestamp, excluding a configurable margin at the start and end of each file
- **Clip duration selector** — choose 1, 2, 5, 10, 20 minutes or full video directly in the app (overrides the plugin config)
- **Auto-advance** — when the clip duration expires, the next random clip starts automatically
- **Session-aware deduplication** — already-played clips are excluded from the pool until all items have been seen (configurable)
- **Progress bar + countdown** — a thin bar at the bottom of the screen shows how much time is left on the current clip

---

## Requirements

- Jellyfin **10.11.x**
- Docker (for building) — no local .NET installation needed

---

## Installation

### 1 — Build the plugin

On any machine with Docker:

```bash
git clone <this-repo>
cd jellyfin-plugin-shuffle

docker run --rm \
  -v "$(pwd)":/src -w /src \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  dotnet build Jellyfin.Plugin.RandomReel/Jellyfin.Plugin.RandomReel.csproj \
    -c Release --nologo -v quiet

mkdir -p dist
cp Jellyfin.Plugin.RandomReel/bin/Release/net9.0/Jellyfin.Plugin.RandomReel.dll dist/
cp meta.json dist/
```

### 2 — Copy files to the server

Place both files from `dist/` into a dedicated folder under Jellyfin's plugin directory:

```
<jellyfin-config>/plugins/RandomReel/
├── Jellyfin.Plugin.RandomReel.dll
└── meta.json
```

Example — if your `docker-compose.yml` has:

```yaml
volumes:
  - /srv/jellyfin/config:/config
```

Then put the files in `/srv/jellyfin/config/plugins/RandomReel/`.

### 3 — Restart Jellyfin

```bash
docker compose restart jellyfin
```

Open **Dashboard → Plugins** and confirm *Random Reel* shows status **Active**.

---

## Usage

Open the TV app in any browser:

```
http://<your-jellyfin-server>:8096/RandomReel/app
```

1. Enter your server URL (pre-filled if you open the page from Jellyfin itself)
2. Sign in with username/password **or** use Quick Connect
3. Select a library or playlist
4. Choose a clip duration
5. Playback starts automatically — use the on-screen **Next** and **Stop** buttons, or your remote's arrow keys

---

## Configuration

Open **Dashboard → Plugins → Random Reel** to set server-side defaults:

| Setting | Default | Description |
|---|---|---|
| Playback Duration | 10 min | Default clip duration (can be overridden per-session in the TV app) |
| Edge Exclusion | 5 min | Margin skipped at the start and end of each file |
| Allow Repeats in Session | false | If true, clips can be replayed before the pool is exhausted |

---

## How It Works

### Server side (`ShuffleController`)

`GET /RandomReel/Next?folderId={id}&durationMinutes={n}` returns a random item from the folder with a random start position, respecting edge exclusion and session deduplication. The optional `durationMinutes` parameter overrides the plugin configuration. Session state is kept in-memory and resets when the server restarts.

### Client side (`tvapp.html`)

A self-contained HTML page served at `/RandomReel/app`. It authenticates against the Jellyfin API (password or Quick Connect), loads the user's libraries and playlists, and plays video using an HTML5 `<video>` element.

For each clip it calls `/Items/{id}/PlaybackInfo` to obtain a `PlaySessionId`, then streams via `/Videos/{id}/stream` with `StartTimeTicks` and `PlaySessionId` so Jellyfin honours the random seek position through transcoding.

---

## Development

```bash
# Build, inject index.html and start a local Jellyfin instance
./deploy.sh

# Open
open http://localhost:8096
```

`deploy.sh` is intended for local development only. It builds the plugin, starts a `jellyfin-shuffle-dev` container via Docker Compose, and hot-deploys the DLL.

---

## Project structure

```
Jellyfin.Plugin.RandomReel/
├── Api/
│   ├── ShuffleController.cs     # GET /RandomReel/Next, POST /RandomReel/Session/Reset
│   ├── ShuffleNextResponse.cs   # Response model
│   └── InjectController.cs      # GET /RandomReel/app (serves the TV web app)
├── Configuration/
│   ├── PluginConfiguration.cs
│   └── configPage.html          # Admin dashboard config page
├── Web/
│   └── tvapp.html               # Standalone TV app
└── Plugin.cs
meta.json
deploy.sh                        # Local dev only: build + hot-deploy
docker-compose.yml               # Local dev only
```

---

## Acknowledgements

Built on top of:

- **[jellyfin-plugin-template](https://github.com/jellyfin/jellyfin-plugin-template)** — official Jellyfin plugin scaffold (project structure, build config, StyleCop ruleset, `Plugin` / `PluginConfiguration` boilerplate)

---

## License

GPL-3.0 — see [LICENSE](LICENSE).

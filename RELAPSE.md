# FPKGi-Relapse

Development fork of FPKGi focused on a Relapse-friendly PlayStation 5 install path.

## v0.1 bootstrap

- Probe `http://127.0.0.1:8844/api/version` on PS5 startup.
- If PKG Manager is reachable, select the Relapse/PKG Manager backend.
- Hand local package paths and web package URLs to `POST /api/install`.
- Preserve the upstream UnityOrbisBridge / etaHEN path as fallback.
- Keep the existing catalog, UI, queue, downloader, and JSON handling.
- Disable upstream self-update in this dev fork.
- Defer `Delete After Install` on Relapse v0.1 so the installer cannot lose its source mid-read.

## Requirement

Start PKG Manager before launching FPKGi-Relapse.

## Boundary

This bridge changes install handoff only. It does not add firmware-specific FPKG execution patches or kernel support.

## Next milestones

1. Poll `/api/status` for install progress.
2. Add `/api/cancel` integration.
3. Make queue removal conditional on a successful installer handoff.
4. Add a visible backend/status indicator.
5. Investigate automatic PKG Manager startup/reconnection.

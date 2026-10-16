# PetPing v0.17 — Windows laptop acceptance checklist

**Release status:** unsigned design preview, not a stable release. Keep existing local data. User testing is still pending.

**Preview source:** https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38922662056

## Obtain and install
1. Open the GitHub Actions run and find **Artifacts**.
2. Select `PetPing-Installer-v0.17-unsigned` (or the portable `PetPing-Windows-v0.17-unsigned`). Unzip the download. The installer ZIP contains a setup executable; portable ZIP contains the main executable.
3. Before installing, **Quit PetPing from its tray menu**, not merely close the dashboard. Copy `%LOCALAPPDATA%\PetPing` to a safe folder as a precaution.
4. Run the installer. It is unsigned and may trigger a Windows SmartScreen warning; proceed only if the executable came from this repository's own GitHub Actions.
5. Open PetPing and check the sidebar, app titlebar and tray all use the Companion Beacon (Logo C). The installed version should be **v0.17**.

## Hands-on test sequence
- [ ] **Home:** Today's and upcoming counts match saved reminders; no sample/fake tasks appear; Add reminder works.
- [ ] **Reminders:** Create a one-time reminder due in ~2 minutes and check title/date/time; mark Done; create another and Snooze 10m; edit a pending reminder and reopen it to verify saving.
- [ ] **Recurring:** Create daily and weekly reminders; restart the tray app and verify they remain.
- [ ] **My Pets:** Select Milo (Cat), Buddy (Dog), Luna (Bunny); each selection has visible highlighting and the desktop reminder uses that selected species.
- [ ] **Pet Studio:** Change color, size, accessory and motion; preview Done/Snooze effects without changing actual reminders; quit/restart to verify persistence.
- [ ] **Settings:** Toggle voice, use Test voice, expand/collapse Quick help, open local data folder, optionally enable Start with Windows and verify behavior after login.
- [ ] **Compact layout:** Resize window down to minimum 810×640; confirm the Home cards, reminder date/time/repeat and pet/studio controls remain readable with scrolling, no controls cut off.
- [ ] **Display scaling:** Test Windows display scaling at 100%, 125%, 150%, 200% and large text if practical. Check focus highlight and keyboard Tab/Enter.
- [ ] **Tray/icon:** Verify Logo C on light/dark taskbars, taskbar/titlebar/Start menu and installer; check 16px clarity. Closing dashboard should keep tray agent running; explicit Quit should stop reminders.
- [ ] **Recovery:** Test lock/unlock and sleep/wake. PetPing cannot show a normal popup while Windows is asleep or on the secure lock screen; check reasonable recovery after resuming, not impossible exact-time behavior.
- [ ] **Data integrity:** Previously saved reminders and pet profiles still exist after upgrade; uninstall should not delete `%LOCALAPPDATA%\PetPing` data.

## Report failures
Provide: page, action, expected result, actual result, Windows version/scaling, any screenshot and whether PetPing was running in the tray. Do not include private reminder text in public bug reports.

**Acceptance:** CI completed; all hands-on boxes above remain **unchecked** until a real Windows test confirms them. Passing these tests can unlock a stable v0.17 decision, then the locked v0.18 reminder-engine milestone.

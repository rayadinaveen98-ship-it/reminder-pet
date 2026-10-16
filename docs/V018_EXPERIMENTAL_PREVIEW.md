# PetPing Windows v0.18.0 — EXPERIMENTAL isolated preview

**NOT a stable release. Not an in-place update for PetPing v0.17.**

This branch builds both the merged R2-A/R2-B custom scheduling features and a separately identifiable, experimental **Windows x64** package for hands-on evaluation. It is not an Android APK and is not distributed through production auto-update.

## Strict separation from v0.17
- Binary version: `0.18.0`, informational version `0.18.0-experimental`; installer and artifacts include `v0.18-experimental`.
- Installer display name: **PetPing Experimental**. New, distinct Inno AppId and `%LOCALAPPDATA%\Programs\PetPing-Experimental` installation directory. Does not intentionally replace v0.17.
- Data root in this experimental build only: **`%LOCALAPPDATA%\PetPing-Experimental`**. Reminders, cat/dog/bunny settings, appearance, speech preferences and diagnostics are isolated. Production v0.17 remains in `%LOCALAPPDATA%\PetPing`.
- The experimental instance mutex and Windows startup registration name differ from v0.17. It will not relink production startup or rewrite v0.17 profile JSON.
- First custom reminder asks for user confirmation, and the experimental reminder store preserves a **non-overwriting pre-custom JSON snapshot in the experimental folder**.
- Both apps may show desktop reminders if run at once; avoid running both during focused timing tests to distinguish notifications. They do **not** sync or share reminders.

## Obtain a CI test artifact
From the Windows GitHub Actions run for this branch, expand **Artifacts** and download `PetPing-Installer-v0.18-experimental-unsigned` or the portable `PetPing-Windows-v0.18-experimental-unsigned`.
Artifact links expire per retention policy and may require repository login. As unsigned code, Windows SmartScreen can display a warning. Only open known artifacts from this repository.

## Test plan
1. Back up production `%LOCALAPPDATA%\PetPing` externally as an extra precaution. Test the current v0.17 preview separately using `docs/V017_WINDOWS_PREVIEW_ACCEPTANCE.md`.
2. Install PetPing Experimental. Confirm a separate shortcut and a separate data folder. Confirm v0.17 saved reminders and pet selections are not imported or modified.
3. In Experimental Reminders, create a normal reminder; Done/Snooze (5/10/15/30/60min/custom), check popup pet and speech. Restart and verify persist.
4. Create Custom weekdays (Mon/Wed/Fri); check first-use warning and chosen preview, save, edit an upcoming event. Test Every N days and every N weeks, including start date.
5. Verify **no duplicate upcoming series** after snooze and restarting the experimental app. Verify disabled invalid interval, 0 days and no selected weekdays all refuse save.
6. Inspect `%LOCALAPPDATA%\PetPing-Experimental\reminders.pre-custom-v017.json` generated before the first custom reminder and never overwritten on subsequent new reminders.
7. At 810×640 and at 100/125/150/200% display scaling, verify keyboard/touch, focus order, screen reader announcements, and dropdown menu handling.
8. Test short real alarms, tray persistence and restart; record behavior after lock/sleep/wake. Exact reminders while PC is asleep or off cannot be guaranteed by a normal Windows tray app.
9. Uninstall PetPing Experimental and ensure both **production v0.17** and experimental local data survive. Neither installer nor uninstaller is allowed to destroy user reminders.

## Automated checks
- Combined R2-A + R2-B domain regression suite, XAML parse and WPF x64 publish.
- Icon generation, isolated experimental installer creation, reinstall and uninstall smoke.
- SHA-256 test that experimental install/reinstall/uninstall does not change **production** `PetPing` data or **experimental** `PetPing-Experimental` data.
- This test does not assert an in-place v0.15/v0.17→v0.18 migration; intentionally no production replacement takes place. Earlier v0.17 production installer upgrade smoke is validated separately on `main`.

## Remaining acceptance and release policy
Not yet production approved: physical device QA, interaction reliability, data portability/export/import, future series edit/delete semantics, handling of concurrent app instances, compatibility of v0.17 with newly created Custom enum files in any hypothetical shared-profile upgrade. Do not merge the development branch or promise reliable reminders until those gates are met.

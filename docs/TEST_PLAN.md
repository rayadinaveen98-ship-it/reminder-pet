# Windows quality test matrix (manual, first alpha)

## P0: Reminders are trustworthy
| Test | Expected |
|---|---|
| Create 1-minute future reminder, leave dashboard open | Single pet overlay near expected minute, title exact, voice if enabled |
| Create 1-minute reminder, close dashboard window | Tray remains; overlay appears when due |
| Create two reminders due same second | Two overlays presented sequentially, not overlapping |
| Snooze 10m | Same reminder is pending for a new due time and alerts once later |
| Click Done | Status Completed; no further alert for occurrence |
| Quit tray manually | Process stops and makes no background promise |
| Restart app with reminders saved | Previously scheduled pending reminders remain |
| Inject malformed JSON storage | Error appears; data must not be silently erased |
| Create empty title / past due time | Human-readable input validation error |
| Disable speech | Visual alert still works; no speech |
| Lock Windows session during reminder | Secure lock screen not overlaid; show queued alert on unlock if session signals were processed |
| Sleep through due moment and resume within 15m | Pet catches up; marked Delivered |
| Sleep through due moment and resume after 15m | Reminder shown as Missed, not spoken hours late |

## P1: Polish and durability
| Test | Expected |
|---|---|
| 2 monitors & different DPI scaling | Popup is fully on-screen and avoids taskbar |
| Play fullscreen game / presentation | Future policy avoids interruption |
| Disable/restrict Windows voice output | Popup still appears |
| Windows focus assist enabled | In-app overlay policy documented; future fallback handles system notification rules |
| Restart in different local timezone | One-time absolute instant remains the same |
| Start two app processes | No duplicate scheduler; second instance behavior clear |
| 500+ reminders | Reasonable UI speed, low idle CPU use |
| Turn on high contrast / text scaling | Controls remain usable, legible |

## Recording results
For each test capture machine Windows edition/build, UTC offset, voice installation, display scale, expected fire timestamp, observed popup timestamp, whether app and session were active, errors and screenshot if useful.

## Release gate
No "perfect", "exactly guaranteed", "runs after force-quit" or "locked-screen overlay" marketing claims. Publish only once P0 passes on actual Windows devices.

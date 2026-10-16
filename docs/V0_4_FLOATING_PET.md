# v0.4 — floating pet design notes

## Source of the change
The previous white rectangle looked like a standard notification card and obscured Netflix controls. The new popup is a floating WPF vector character to the right of a compact speech bubble. No cloud image model or external copyrighted art is needed.

## Interaction
- Entrance: companion slides/hops in from the right (420 ms), then gently bobs with a tail wag.
- Voice: unchanged local Windows text-to-speech announcement, honoring the existing Speak reminders preference.
- Click Done: stores Completed and animates away.
- Click Snooze 10m: schedules again and animates away.
- Click ×: closes the popup without acknowledging it. Reminder stays in Needs Attention.
- If not interacted with: auto-hide after 45 seconds, preserving Needs Attention.
- Hover pauses the auto-hide timer and restarts it on departure.
- Pressing X on the dashboard still hides the dashboard, not the reminder agent.

## Placement
- When foreground window occupies its monitor's work area (e.g., a maximized browser watching Netflix), pet appears at the upper right, away from playback controls and subtitles near the bottom.
- Otherwise popup appears above the bottom-right corner with taskbar clearance.
- Initial placement remains primary-monitor oriented. Accurate mixed-DPI multi-monitor selection is pending real-device testing.
- Does not draw over the secure Windows lock screen; follow-up delivery behavior remains in existing session-lock logic.

## Explicit trade-offs
- The mascot is hand-drawn, original WPF vector art, meant as the first free-floating visual rather than final commissioned character animation.
- "Walk" is implemented as a short smooth slide-and-hop, not frame-by-frame gait.
- Spoken reminder reliability must be checked separately on the user's Windows audio setup.
- No built-in video playback detection: placement uses *large foreground window* heuristic.
- Always-on-top popup remains visible over the user's active app for up to 45 seconds unless dismissed; more configurable attention modes come after alpha QA.

## Desktop acceptance checks
1. Open a maximized browser with a playing video; use "Test pet now" and confirm it appears near top-right.
2. Confirm no full-width white notification rectangle surrounds the cat; only the text bubble has a background.
3. Create a 1-minute reminder, close dashboard to system tray, wait for the pet and check voice separately.
4. Hover over popup longer than 45 sec; verify it remains, then move cursor away and verify auto-hide.
5. Close with ×; check Needs Attention remains and Snooze works.
6. Lock/unlock while the pet is visible; ensure it is not shown on secure lock screen.
7. Test at 100%, 125%, 150% scaling and a second monitor; report clipping or off-screen cases.

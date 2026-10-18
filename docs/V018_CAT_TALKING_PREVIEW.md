# Premium Cat Talking V1 — word-cued animation preview

## Purpose
Improve Milo the Cat's premium sprite performance during real on-device Windows text-to-speech rather than replaying an unrelated idle loop. This remains an experimental, unmerged PR on top of Walking V1 (#28), Idle V1 (#27), the reliability PR (#26), and the isolated experimental build.

## Technical signal
- `VoiceService` now listens to Windows `SpeechSynthesizer.SpeakProgress`. It raises a no-text `SessionWordStarted(Guid)` event **only for its active, session-tagged Prompt**. This avoids sending canceled reminders' stale cues to a newer alert. Only the currently active `PetWindow` receives a cue, on the WPF dispatcher, if it is still waiting for user action.
- Existing `SessionSpeakingChanged(true/false)` still controls whether the mouth animates; Stop/Mute/Done/Snooze don't cause talking after the reminder is dismissed.
- `CatTalkPlaybackRules` maps each word pulse to short open/rest/open/happy states; fallback mouth motion continues gently if a particular Windows voice does not report progress. It is NOT phoneme/viseme lip sync and cannot derive exact articulation from word starts alone.
- Respects Full/Gentle/Off preferences. Off is static. Speech text is never recorded as part of the animation.
- New optional `Cat_Talk_rest_164x218.png` is generated from the user-approved orange cat Idle source: face and open eyes stay unchanged while the mouth closes, unlike older Half-Blink frames. When missing, use older half-blink art, with an explicit missing-art message on My Pets.

## Artwork packaging
Use nine approved/derived transparent 164×218 images together in the user-facing complete package:
- Idle: `Cat_Idle_{open,half,closed,happy}_164x218.png`
- Walking: `Cat_Walk_{0,1,2,3}_164x218.png`
- Talking: `Cat_Talk_rest_164x218.png`

Artwork is loaded once and frozen. A single zip must include all nine sprite files. The raw GitHub Actions release artifact builds code only and **does not contain the user-approved binary artwork** until these are explicitly committed in a future media-import phase. Never direct users to GitHub artifacts alone expecting new visuals. The produced user ZIP is the testable app.

## Release gates
- CI must pass XAML/static, core tests, Windows .NET WPF compilation, experimental installer, data-preservation test. No changes to v0.17 main or production data.
- Manual laptop QA of TTS voices, speech start/stop/cancel, repeated reminder speech, mute, low-power CPU, 100-200% DPI, animations and WPF notification click targets.
- Audio playback can have variable voice pacing and word progress event behavior across installed Windows voices. Our simple word cues are an honest 2.5D approximation, not frame-accurate lip tracking.
- Stay DRAFT/unmerged until user approves.

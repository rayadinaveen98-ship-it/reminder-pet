# Cat V2 32-frame gait motion study — experimental in-app preview

**Not a finished 3D cat.** This is the previously produced 2.5D articulated gait study shown inside the actual Windows reminder popup for visual assessment. The user explicitly requested to see how it looks in the app. This preview must stay on an unmerged draft branch until visual approval.

## Scope
- Real `PetWindow` entrance, Cat+Apricot+No accessory ONLY, uses transparent frame sequence `Assets/CatWalkV2/CatV2_Rig_Proof_00.png` ... `31.png` (32 PNGs).
- Preloads all 32 into frozen `BitmapImage` objects at 218px decoded height (source 640×850), ~5MB decoded; changing frames does not reread disk.
- `CatV2WalkPlaybackRules` selects 0–31 at 32fps (1s cycle, Full), slower ~23fps (1.4s, Gentle) and static frame 0 (Off). Timer runs ~31ms only while Full V2 walking, ~43ms Gentle, 83ms when idle. Actual UI dispatcher can cause dropped frames under load; no realtime guarantees.
- On arrival, continues existing approved Cat V1 premium Idle/Talking/Done/Snooze artwork. Dog and Bunny / alternate cat palettes untouched.
- Automatically enables only if all 32 study frames are present and valid, in `Assets/CatWalkV2` next to the EXE or in isolated `%LOCALAPPDATA%\PetPing-Experimental\CatWalkV2`. Incomplete/corrupt set gracefully falls back to old four-frame walking. V2 art folder is separate from `Assets/CatIdle`.
- My Pets status visibly displays `CAT V2 MOTION STUDY ACTIVE` when the 32 frames load. Main window title: `PetPing Experimental — Cat V2 Gait Study`.
- The regular production profile `%LOCALAPPDATA%\PetPing` is never read by the V2 loader. No new data writes or schedule changes.

## Physical QA
1. Close prior PetPing Experimental using tray Quit, and unzip the entire **complete** experimental preview bundle. Old experimental installation may hold the same Windows instance mutex.
2. Choose **Cat**, **Apricot**, **No accessory**. In My Pets verify **CAT V2 MOTION STUDY ACTIVE**.
3. In Pet Studio choose **Preview on desktop** or create a reminder due soon. Watch feet contact during Walking from edge; then arrival swaps from V2 gait study to approved seated idle. Observe contrast: frames are a 2.5D composite and some leg/fur joins may look artificial.
4. Test Full/Gentle/Off motion, speaking, Done, Snooze, auto-dismiss, tray close; verify no reminder data loss or new popup layer intercepting buttons.
5. Inspect 100–200% display scaling, transparency, CPU, memory, and perceived naturalness.
6. Report subjective visual verdict; do **not** mark animation release-ready merely because CI passed.

## Packaging
The user-facing complete ZIP generated in conversation combines exact GitHub Actions Windows build at the PR head with:
- 9 existing approved Idle, Talking, four-frame V1 walking PNGs as fallbacks in `Assets/CatIdle`.
- 32 study frames in `Assets/CatWalkV2`, together with SHA256 manifest.
- A stable/expt-isolated installer wrapper that copies both art directories BEFORE the Inno installer auto-launches.

**Raw GitHub Actions CI artifact lacks all these conversation-generated art images.** Never direct users to a raw artifact as if it included Cat V2 visuals. The complete user-facing ZIP is required.

## Known limitations
- V2 gait studies animate legs independently but are **not** a real modeled/textured rigged 3D cat; they may look puppet-like.
- Walking → sitting can still jump, because the animation study has not yet produced an approved intermediate settle/turn clip.
- Foot planting is mechanically consistent relative to the artwork scene, but screen-space root translation can produce apparent sliding. Need visual approval and root-motion calibration.
- Production v0.17 remains unchanged; this is a throwaway preview, not the stable next version.

## Automated check gate
`CatRiggedGaitRules` validates 4-leg contact and IK kinematics and `CatV2WalkPlaybackRules` tests distinct frame mapping, boundaries, Gentle and Off, long and negative clocks, invalid settings. Combined Windows CI compiles WPF app and experimental installer and checks stable profile preservation. Physical test still mandatory.

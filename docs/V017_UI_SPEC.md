# PetPing v0.17 — UI Specification & Visual Review Gate

**Status:** Stage A source design and concept exploration. Windows v0.15 is unchanged. Visuals remain concept candidates pending approval.
**Editable Canva concept deck:** https://www.canva.com/d/rljAMk15QUasrej (8 slides; generated, requires editorial corrections below).
**Earlier approved style reference:** https://www.canva.com/d/0LRU2hT9b1v6vBO (7 pages; preserve original).
**Own-source identity candidates:** `design/petping-3/logos/`.
**Precisely authored editable mockups:** `design/petping-3/screens/v017-home-concept.svg` and `v017-reminders-concept.svg`; static sample content only.

## 1. Review criteria and correction notes

**Keep:** warm ivory / muted lavender / apricot / ink, humane character art, legible task-first structure, quiet premium visual hierarchy, clear quick-create and upcoming reminders, actual My Pets and Studio profiles.

**Correct before calling the mockups a lock:**
- No pet-care, grooming, feeding, health/care tasks, “pet parent”, pet watering, pet streaks or analytics as primary product.
- No mock online/offline device sync switch, forced login or extra Analytics primary navigation (not shipped).
- Real Windows app has **five main destinations**: Home, Reminders, My Pets, Pet Studio, Settings. Help under Settings.
- First-run has no pet-raising steps, no demand to enable Windows startup or voice, and always has Skip. Existing Windows users must skip onboarding safely.
- No claim that calendar, habits, advanced recurrence, cloud sync, or fancy pets already work. Future concepts may mark features as proposed.
- Current code: Windows **v0.15**. Planned UI exploration is **v0.17**. Do not change current installer version during Stage A.
- Generated logo images are only inspiration. Original editable identity source is the separate SVG mark family; final mark requires approval and small-size inspection.
- Auto-generated sample task copy should be professional, simple and meaningful, e.g. “Edit cinematic analysis,” “Send project update,” “Review storyboard,” not generic medication/health copy.

## 2. Desktop layout anatomy (layout dimensions in WPF DIPs)

### Shared shell
- Preferred design reference: 1280–1440 wide × 800–900 tall desktop; minimum target remains **810×640** until responsive QA proves change.
- Left persistent sidebar: 218 DIP wide at large widths. In narrow mode, consider 76-DIP icon strip **only after** keyboard/tooltip review; otherwise keep labels accessible through a collapsible menu.
- Global header: 72–76 DIP, page title left, action/status right. No excessive translucent titlebar controls.
- Main content: 28 DIP horizontal margin, scrollable. Cards 18–22 DIP corner radii and 20–24 DIP internal padding.
- Primary button 42–44 DIP minimum hit height; secondary at least 40 DIP; visible 2-DIP focus outline and automation names.
- Typography: Segoe UI. Page title 24–28 DIP, section 18–20, body 14–15, supporting 12–13; at 200% text scaling, allow wrapping and ScrollViewer.

### Color palette (first contrast screening; test after actual WPF render)
| Use | Candidate | Background | Contrast | Rule |
|---|---|---|---|---|
| Primary ink text | #24272E | ivory #F8F6F2 | 13.85:1 | good |
| Secondary text | #626672 | white #FFFFFF | 5.73:1 | good |
| White text in lavender button | #FFFFFF | #7562D4 | 4.71:1 | AA normal body threshold met |
| Dark UI body text | #F6F5FA | #191922 | 16.09:1 | good |
| Secondary dark body | #BCB9C8 | #262631 | 7.77:1 | good |
| **White text on mint** | #FFFFFF | #80C9AF | **1.93:1** | **FAIL** for small text; keep mint for decorative icon/backdrop with dark text |

Focus purple #5943C8 should remain visible on both light and warm surfaces, not solely through shadow.

## 3. Home — implementation-ready hierarchy

**User job:** “What should I do next, and can I make a reminder quickly?”

Above fold on 1280×800:
1. Personal greeting only if explicitly configured; default “Good morning” based on local device clock, no invented user name.
2. Compact positive companion hero occupying ~1/3 to 2/5 width, **not** obstructing task metrics. Authentic species selected from `PetProfileSettings.Current`; no fake pet photos.
3. Primary **+ Add reminder** button visible, leading to Reminders quick-create and focusing Title.
4. Three metrics **Today, Upcoming, Needs attention**. Values always derived from real `ReminderStore` and single local “now”; no made-up streaks.
5. Coming up list, first three pending reminders ordered by due date; readable task title and localized date/time. Link **See all reminders**.
6. Empty state: friendly short message “Nothing coming up. Add a reminder when you’re ready.”, clear CTA.
7. Announcements: local no account, local storage; status “Reminders running in tray” must reflect actual running state instead of a fabricated universal online pill.

Avoid redundant metrics and 7+ cards in one viewport. Keep spacious hero but never hide the next due.

## 4. Reminders — most important functional screen

**User job:** “Create, find, edit and act on a reminder, confidently.”

Desktop hierarchy:
1. Header “Reminders”, primary **+ New reminder** button or visible Quick-create card, not both large duplicated CTAs.
2. **Quick create**: Title (plain text up to 120 characters), Date, 24-hour Time, repeat (One time / Every day / Every week). Save primary; “More options” is a **future v0.18 drawer** until wired.
3. Tabs/status filter **All / Today / Upcoming / Needs attention / History**; clear count badge, no deceptive toasts.
4. Compact search and advanced filter region: search is a v0.18 functional feature; never put a non-working enabled search box in v0.17 preview pretending it works.
5. List rows have title, exact localized due date/time, Repeat label, status and visible edit/done/snooze/delete controls with keyboard names. Keep primary actions clear; optional overflow for uncommon destructive actions only.
6. Empty states per filter; history never silently omitted. Needs Attention must visually distinguish missed/unanswered but avoid panic red.
7. Delete confirmation explains recurring series impact; save failure preserves edit form and shows retry state.
8. At narrower widths date/time/repeat stack vertically and action bar wraps; never clip or make inputs microscopic.

Sample mock tasks should be marked **fictional examples**. Live app must never seed them into storage.

## 5. Other screens — v0.17 second pass specification

**My Pets:** three distinct original Cat, Dog, Bunny vector previews, clearly marked selected; each changes actual overlay character and retains its profile.
**Pet Studio:** large preview and tabs Appearance / Motion / Voice / Effects; current working options only in v0.17, future options labeled “Coming later.” Preview Done/Snooze never mutates reminders.
**Settings:** voice, start with Windows, accessibility motion, local data folder, help/about; avoid duplicating heavy configuration descriptions.
**Welcome/onboarding:** v0.20 implementation. Stage A can show a concept but not claim first-run exists yet.

## 6. Logo candidate acceptance before choosing ONE

A Friendly Signal / B Companion Monogram / C Companion Beacon all provided as original source SVGs. Print/raster test each at 16/24/32/48/128/256, light/dark system tray, pure black/white and pale/ink backgrounds. Verify accessible distinguishability and no tiny decorative detail that makes the mark ambiguous. User approves visual direction, then produce final vector master and WPF ICO; no trademark clearance has yet been performed.

## 7. Visual QA and implementation gate

- **Stage A complete:** editable concept review exists; original vector candidates exist; precise screen specification exists; color contrast checked; no product code change. User chooses a logo direction and UI theme direction.
- **Stage B after approval:** design tokens into WPF ResourceDictionary, shared buttons/typography/cards, Home and Reminders responsive native pages. Live counts and all existing functions retained. CI compiles and 122+ regression checks continue passing.
- **Stage C:** My Pets, Studio, Settings, help, empty/error/loading states aligned. Physical Windows 100/125/150/200 DPI and text scaling QA before shipping.
- **No automatic code merge of mockups.** Editable Canva and illustration output are guides, not executable UI or a production character rig.

## 8. Next decision requested from user

Review the three logo directions and Home/Reminders layout; choose **A/B/C** for refinement or request a hybrid, and say whether you prefer **Light-first cozy** vs **balanced light/dark**. Once approved, start v0.17 WPF implementation. If no selection yet, continue drafting UI architecture but do not label a candidate as the final brand.
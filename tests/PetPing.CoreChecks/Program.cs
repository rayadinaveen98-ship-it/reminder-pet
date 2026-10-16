using System.Text.Json;
using PetPing.Windows.Models;
using PetPing.Windows.Services;

var temp = Path.Combine(Path.GetTempPath(), "PetPing-Smoke-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temp);
int checks = 0;
void Check(bool pass, string reason)
{
    if (!pass) throw new Exception("FAILED: " + reason);
    Console.WriteLine("PASS: " + reason);
    checks++;
}
try
{
    var store = new ReminderStore(temp);
    Check(store.Reminders.Count == 0, "first run has an empty list");
    var current = DateTimeOffset.UtcNow;
    var due = current.AddHours(1);
    var reminder = store.Add("Test my film reminder", due);
    Check(reminder.Status == ReminderStatus.Pending, "new reminder is pending");
    Check(File.Exists(Path.Combine(temp, "reminders.json")), "reminder saved to disk");
    var restored = new ReminderStore(temp);
    Check(restored.Reminders.Count == 1 && restored.Reminders[0].Id == reminder.Id, "restart loads same reminder");

    Check(ReminderRules.Evaluate(reminder, current) == ReminderDeliveryDecision.None, "future reminder is not due");
    reminder.DueAt = current.AddSeconds(-20);
    Check(ReminderRules.Evaluate(reminder, current) == ReminderDeliveryDecision.Deliver, "near-late reminder delivers");
    reminder.DueAt = current.AddMinutes(-16);
    Check(ReminderRules.Evaluate(reminder, current) == ReminderDeliveryDecision.Missed, "old reminder is missed");
    reminder.Status = ReminderStatus.Completed;
    Check(ReminderRules.Evaluate(reminder, current) == ReminderDeliveryDecision.None, "completed reminder cannot fire");
    Check(!ReminderRules.NeedsAttention(reminder), "completed reminder does not need attention");
    Check(!ReminderRules.CanSnooze(reminder), "completed reminder cannot be snoozed");
    reminder.Status = ReminderStatus.Missed;
    Check(ReminderRules.NeedsAttention(reminder), "missed reminder stays in attention inbox");
    Check(ReminderRules.CanSnooze(reminder), "missed reminder can be snoozed");
    reminder.Status = ReminderStatus.Delivered;
    Check(ReminderRules.NeedsAttention(reminder), "unanswered reminder stays in attention inbox");

    restored.Reminders[0].Status = ReminderStatus.Delivered;
    restored.Reminders[0].DeliveredAt = current.AddMinutes(-5);
    restored.Save();
    Check(ReminderRules.ShouldRestoreAlert(restored.Reminders[0], current), "unanswered alert is recoverable");
    restored.Reminders[0].FinishedAt = current;
    Check(!ReminderRules.ShouldRestoreAlert(restored.Reminders[0], current), "acknowledged alert is not restored");
    restored.Reminders[0].FinishedAt = null;
    Check(!ReminderRules.ShouldRestoreAlert(restored.Reminders[0], current.AddMinutes(16)), "stale alert not replayed indefinitely");

    restored.Snooze(restored.Reminders[0], TimeSpan.FromMinutes(10));
    Check(restored.Reminders[0].Status == ReminderStatus.Pending && restored.Reminders[0].SnoozeCount == 1,
        "snooze schedules pending future reminder");
    Check(File.Exists(Path.Combine(temp, "reminders.json.bak")), "a backup is kept after replacement");

    restored.Mark(restored.Reminders[0], ReminderStatus.Completed);
    Check(new ReminderStore(temp).Reminders[0].Status == ReminderStatus.Completed,
        "completed state persists across restart");
    bool refusedSnooze = false;
    try { restored.Snooze(restored.Reminders[0], TimeSpan.FromMinutes(10)); }
    catch (InvalidOperationException) { refusedSnooze = true; }
    Check(refusedSnooze, "completed reminder rejects snooze");


    // Simulate an inaccessible temp-write target. Save() must roll back
    // in-memory state instead of pretending a reminder is delivered.
    var faultFolder = Path.Combine(temp, "fault-case");
    Directory.CreateDirectory(faultFolder);
    var faultStore = new ReminderStore(faultFolder);
    var pending = faultStore.Add("private reminder words", current.AddHours(2));
    var blockedTemp = Path.Combine(faultFolder, "reminders.json.tmp");
    Directory.CreateDirectory(blockedTemp);
    bool deliveryFailed = false;
    try { faultStore.Deliver(pending, current); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { deliveryFailed = true; }
    Check(deliveryFailed, "blocked write reports failed delivery");
    Check(pending.Status == ReminderStatus.Pending && pending.DeliveredAt is null,
        "failed delivery preserves Pending status");
    Check(new ReminderStore(faultFolder).Reminders[0].Status == ReminderStatus.Pending,
        "failed delivery does not change disk state");

    Directory.Delete(blockedTemp);
    faultStore.Deliver(pending, current);
    Check(pending.Status == ReminderStatus.Delivered &&
          new ReminderStore(faultFolder).Reminders[0].Status == ReminderStatus.Delivered,
        "successful delivery persists before presentation");

    Directory.CreateDirectory(blockedTemp);
    var originalDue = pending.DueAt;
    bool snoozeFailed = false;
    try { faultStore.Snooze(pending, TimeSpan.FromMinutes(10)); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { snoozeFailed = true; }
    Check(snoozeFailed && pending.Status == ReminderStatus.Delivered &&
          pending.DueAt == originalDue && pending.SnoozeCount == 0,
        "failed snooze rolls back status, due time and counter");
    bool completionFailed = false;
    try { faultStore.Mark(pending, ReminderStatus.Completed); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { completionFailed = true; }
    Check(completionFailed && pending.Status == ReminderStatus.Delivered,
        "failed completion retains Needs Attention");
    Directory.Delete(blockedTemp);

    var beforeCount = faultStore.Reminders.Count;
    Directory.CreateDirectory(blockedTemp);
    bool addFailed = false;
    try { faultStore.Add("not persisted", current.AddHours(3)); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { addFailed = true; }
    Check(addFailed && faultStore.Reminders.Count == beforeCount,
        "failed new reminder does not appear saved");
    Directory.Delete(blockedTemp);

    var diagFolder = Path.Combine(temp, "logs");
    var diagnostics = new DiagnosticsLog(diagFolder);
    diagnostics.Record("save_failed", new IOException("private reminder words"));
    var logContents = File.ReadAllText(Path.Combine(diagFolder, "diagnostics.log"));
    Check(logContents.Contains("save_failed") && logContents.Contains("IOException"),
        "diagnostic records event code and exception type");
    Check(!logContents.Contains("private reminder words"),
        "diagnostics do not leak private reminder text or exception messages");
    bool invalidEventRejected = false;
    try { diagnostics.Record("This contains user text!"); }
    catch (ArgumentException) { invalidEventRejected = true; }
    Check(invalidEventRejected, "diagnostic names are restricted to safe identifiers");

    var overlapping = faultStore.Add("another reminder", current.AddHours(2));
    Check(overlapping.Id != pending.Id && faultStore.Reminders.Count == 2,
        "two reminders may share a time without sharing identity");


    // Recurring series tests: each attempt is an immutable historical event,
    // with one future instance persisted atomically alongside it.
    var repeatFolder = Path.Combine(temp, "recurring");
    Directory.CreateDirectory(repeatFolder);
    var repeated = new ReminderStore(repeatFolder);
    var first = repeated.Add("Daily filmmaking practice", DateTimeOffset.Now.AddHours(2),
        RepeatPattern.Daily);
    Check(first.Repeat == RepeatPattern.Daily && first.SeriesId is not null &&
          first.RepeatLocalTime is not null, "daily series stores timezone and wall time");
    var seriesId = first.SeriesId;
    repeated.Deliver(first, DateTimeOffset.UtcNow);
    var child = repeated.Reminders.Single(r => r.SeriesId == seriesId && r.Status == ReminderStatus.Pending);
    Check(repeated.Reminders.Count == 2 && first.RepeatContinuationCreated &&
          child.Id != first.Id, "delivery creates one separate future event");
    Check(child.DueAt > DateTimeOffset.UtcNow && child.DueAt.ToLocalTime().Hour ==
          first.OriginalDueAt.ToLocalTime().Hour, "daily event keeps same local hour");
    repeated.Snooze(first, TimeSpan.FromMinutes(10));
    Check(first.RepeatContinuationCreated && first.SnoozeCount == 1,
        "snoozing a series event keeps continuation marker");
    repeated.Deliver(first, DateTimeOffset.UtcNow);
    Check(repeated.Reminders.Count == 2 &&
          repeated.Reminders.Count(r => r.SeriesId == seriesId && r.Id != first.Id) == 1,
        "re-delivering snoozed event does not create a duplicate tomorrow");
    repeated.Mark(first, ReminderStatus.Completed);
    Check(repeated.Reminders.Count == 2 && new ReminderStore(repeatFolder).Reminders.Count == 2,
        "completing prior event does not duplicate series after restart");

    var week = repeated.Add("Weekly planning", DateTimeOffset.Now.AddDays(1),
        RepeatPattern.Weekly);
    repeated.Deliver(week, DateTimeOffset.UtcNow);
    var nextWeek = repeated.Reminders.Single(r => r.SeriesId == week.SeriesId &&
                                                   r.Status == ReminderStatus.Pending);
    Check(nextWeek.DueAt > week.DueAt &&
          nextWeek.DueAt.ToLocalTime().DayOfWeek == week.DueAt.ToLocalTime().DayOfWeek,
        "weekly reminder stays on its original weekday");

    // Edits affect only upcoming unsent instances; history remains unchanged.
    repeated.Edit(child, "Edited daily work", DateTimeOffset.Now.AddHours(4),
        RepeatPattern.Weekly);
    Check(child.Title == "Edited daily work" && child.Repeat == RepeatPattern.Weekly &&
          child.Status == ReminderStatus.Pending, "edit changes next upcoming occurrence");
    Check(first.Title == "Daily filmmaking practice", "edit preserves historical title");
    bool refusedEdit = false;
    try { repeated.Edit(first, "Should fail", DateTimeOffset.Now.AddDays(3), RepeatPattern.None); }
    catch (InvalidOperationException) { refusedEdit = true; }
    Check(refusedEdit, "delivered historical occurrence cannot be edited");
    repeated.Edit(child, "Standalone work", DateTimeOffset.Now.AddDays(2), RepeatPattern.None);
    Check(child.Repeat == RepeatPattern.None && child.SeriesId is null,
        "editing upcoming recurrence to one time stops that series");

    // Restart after long downtime should jump to one future occurrence,
    // not flood the database with weeks of old reminder events.
    var backlogFolder = Path.Combine(temp, "long-downtime");
    Directory.CreateDirectory(backlogFolder);
    var backlog = new ReminderStore(backlogFolder);
    var missed = new Reminder
    {
        Title = "Resume series",
        DueAt = DateTimeOffset.UtcNow.AddDays(-90),
        OriginalDueAt = DateTimeOffset.UtcNow.AddDays(-90),
        Repeat = RepeatPattern.Daily,
        SeriesId = Guid.NewGuid(),
        RepeatTimeZoneId = TimeZoneInfo.Local.Id,
        RepeatLocalTime = new TimeSpan(9, 0, 0)
    };
    backlog.Reminders.Add(missed);
    backlog.Save();
    backlog.Mark(missed, ReminderStatus.Missed);
    Check(backlog.Reminders.Count == 2 &&
          backlog.Reminders.Single(r => r.Status == ReminderStatus.Pending).DueAt > DateTimeOffset.UtcNow,
        "90 days of downtime creates only one future daily event");
    backlog.Mark(missed, ReminderStatus.Completed);
    Check(backlog.Reminders.Count == 2, "marking old missed event done does not duplicate future event");

    // American DST: if 02:30 is skipped on the spring-forward day,
    // use the first valid time, then return to 02:30 on later days.
    var eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
    var beforeDst = new DateTime(2027, 3, 13, 2, 30, 0, DateTimeKind.Unspecified);
    var dstAnchor = new DateTimeOffset(beforeDst, eastern.GetUtcOffset(beforeDst));
    var dstRule = new Reminder
    {
        Title = "DST alarm", DueAt = dstAnchor, OriginalDueAt = dstAnchor,
        Repeat = RepeatPattern.Daily, SeriesId = Guid.NewGuid(),
        RepeatTimeZoneId = eastern.Id, RepeatLocalTime = new TimeSpan(2,30,0)
    };
    var dstOccurrence = RecurrenceRules.NextAfter(dstRule, dstAnchor.AddMinutes(1));
    Check(dstOccurrence.Month == 3 && dstOccurrence.Day == 14 &&
          dstOccurrence.Hour == 3 && dstOccurrence.Minute == 0,
        "DST spring-forward gap moves 02:30 to first valid local minute");
    dstRule.OriginalDueAt = dstOccurrence;
    var afterDst = RecurrenceRules.NextAfter(dstRule, dstOccurrence.AddMinutes(1));
    Check(afterDst.Day == 15 && afterDst.Hour == 2 && afterDst.Minute == 30,
        "next spring-forward recurrence restores original 02:30 wall time");

    var originalCount = repeated.Reminders.Count;
    var repeatBlocked = Path.Combine(repeatFolder, "reminders.json.tmp");
    Directory.CreateDirectory(repeatBlocked);
    var attempt = nextWeek;
    bool repeatWriteRejected = false;
    try { repeated.Deliver(attempt, DateTimeOffset.UtcNow); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { repeatWriteRejected = true; }
    Check(repeatWriteRejected && repeated.Reminders.Count == originalCount &&
          attempt.Status == ReminderStatus.Pending && !attempt.RepeatContinuationCreated,
        "failed recurring delivery rolls back both state and future occurrence");
    Directory.Delete(repeatBlocked);

    // v0.5 JSON was one-time-only and must migrate without rewriting fields.
    var legacyFolder = Path.Combine(temp, "legacy-json");
    Directory.CreateDirectory(legacyFolder);
    File.WriteAllText(Path.Combine(legacyFolder, "reminders.json"),
        JsonSerializer.Serialize(new[] { new {
            Id = Guid.NewGuid(), Title = "Old reminder",
            DueAt = DateTimeOffset.UtcNow.AddHours(5),
            OriginalDueAt = DateTimeOffset.UtcNow.AddHours(5),
            Status = 0, DeliveredAt = (DateTimeOffset?)null,
            FinishedAt = (DateTimeOffset?)null, SnoozeCount = 0
        } }));
    var legacy = new ReminderStore(legacyFolder);
    Check(legacy.Reminders.Count == 1 &&
          legacy.Reminders[0].Repeat == RepeatPattern.None &&
          legacy.Reminders[0].SeriesId is null,
        "previous-version one-time JSON loads without migration or recurrence");


    // v0.7: appearance preferences are independent from reminder data.
    var appearanceFolder = Path.Combine(temp, "pet-settings");
    var look = new PetAppearanceSettings(appearanceFolder);
    Check(look.Placement == PetPlacement.Auto && look.Motion == PetMotion.Full,
        "appearance defaults to auto location and playful motion");
    look.Update(PetPlacement.TopLeft, PetMotion.Gentle);
    Check(new PetAppearanceSettings(appearanceFolder).Placement == PetPlacement.TopLeft &&
          new PetAppearanceSettings(appearanceFolder).Motion == PetMotion.Gentle,
        "pet placement and motion persist across restart");
    look.Update(PetPlacement.BottomRight, PetMotion.Off);
    Check(new PetAppearanceSettings(appearanceFolder).Motion == PetMotion.Off,
        "motion-off preference persists");
    bool badAppearanceRejected = false;
    try { look.Update((PetPlacement)999, PetMotion.Full); }
    catch (ArgumentException) { badAppearanceRejected = true; }
    Check(badAppearanceRejected, "invalid appearance enum is rejected");

    var appearancePath = Path.Combine(appearanceFolder, "pet-appearance.json");
    var appearanceBefore = File.ReadAllText(appearancePath);
    var blockedAppearance = appearancePath + ".tmp";
    Directory.CreateDirectory(blockedAppearance);
    bool appearanceWriteRefused = false;
    try { look.Update(PetPlacement.BottomLeft, PetMotion.Full); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { appearanceWriteRefused = true; }
    Check(appearanceWriteRefused && look.Placement == PetPlacement.BottomRight &&
          look.Motion == PetMotion.Off && File.ReadAllText(appearancePath) == appearanceBefore,
          "failed settings write preserves last saved preferences");
    // Update() cleans up failed temporary files where possible.
    if (Directory.Exists(blockedAppearance)) Directory.Delete(blockedAppearance);

    File.WriteAllText(appearancePath, "corrupted appearance JSON");
    var fallback = new PetAppearanceSettings(appearanceFolder);
    Check(fallback.Placement == PetPlacement.Auto && fallback.Motion == PetMotion.Full &&
          File.ReadAllText(appearancePath) == "corrupted appearance JSON",
          "corrupted appearance file does not prevent defaults and is not silently erased");
    var reminderCountBefore = repeated.Reminders.Count;
    Check(new ReminderStore(repeatFolder).Reminders.Count == reminderCountBefore,
        "corrupted pet preferences never modify saved reminders");


    // v0.8 visual inbox filtering must never change reminder state.
    var viewNow = DateTimeOffset.Now;
    var inboxItems = new List<Reminder>
    {
        new() { Title = "Today pending", DueAt = viewNow, Status = ReminderStatus.Pending },
        new() { Title = "Tomorrow pending", DueAt = viewNow.AddDays(1), Status = ReminderStatus.Pending },
        new() { Title = "Unanswered today", DueAt = viewNow, Status = ReminderStatus.Delivered },
        new() { Title = "Missed yesterday", DueAt = viewNow.AddDays(-1), Status = ReminderStatus.Missed },
        new() { Title = "Finished yesterday", DueAt = viewNow.AddDays(-1), Status = ReminderStatus.Completed }
    };
    Check(ReminderInboxRules.Count(inboxItems, InboxFilter.All, DateTime.Today) == 5,
        "all reminders filter includes every persisted state");
    Check(ReminderInboxRules.Count(inboxItems, InboxFilter.Today, DateTime.Today) == 2,
        "today includes only today's active and unanswered reminders");
    Check(ReminderInboxRules.Count(inboxItems, InboxFilter.Upcoming, DateTime.Today) == 2,
        "upcoming includes pending today and future dates");
    Check(ReminderInboxRules.Count(inboxItems, InboxFilter.NeedsAttention, DateTime.Today) == 2,
        "needs attention includes delivered and missed regardless of day");
    Check(ReminderInboxRules.Count(inboxItems, InboxFilter.History, DateTime.Today) == 1,
        "history contains completed reminders");
    Check(ReminderRules.NeedsAttention(
          ReminderInboxRules.Select(inboxItems, InboxFilter.All, DateTime.Today)[0]),
        "all view prioritizes missed or unanswered alerts ahead of pending");
    Check(ReminderInboxRules.Select(inboxItems, InboxFilter.Upcoming, DateTime.Today)[0].Title ==
          "Today pending", "upcoming reminders are ordered by due date");
    Check(inboxItems.Count == 5 && inboxItems[0].Status == ReminderStatus.Pending,
        "filtering never changes reminder storage state");


    // v0.9: do not overwrite corrupted reminder data without a user's explicit
    // confirmation. Backup is an older snapshot, not a guaranteed full recovery.
    var recoveryDir = Path.Combine(temp, "backup-recovery");
    Directory.CreateDirectory(recoveryDir);
    var recoveryStore = new ReminderStore(recoveryDir);
    recoveryStore.Add("Original older reminder", current.AddHours(3));
    recoveryStore.Add("Newer reminder may not be in backup", current.AddHours(4));
    var recoveryPrimary = Path.Combine(recoveryDir, "reminders.json");
    var recoveryBackup = recoveryPrimary + ".bak";
    Check(File.Exists(recoveryBackup) && ReminderStore.HasValidBackup(recoveryDir),
        "second save created a readable previous-version backup");
    var backupBefore = File.ReadAllText(recoveryBackup);
    File.WriteAllText(recoveryPrimary, "damaged-primary-file-KEEP-ME");
    bool unreadableRejected = false;
    try { _ = new ReminderStore(recoveryDir); }
    catch (JsonException) { unreadableRejected = true; }
    Check(unreadableRejected && File.ReadAllText(recoveryPrimary) == "damaged-primary-file-KEEP-ME",
        "corrupted primary is rejected without automatic restore or deletion");
    Check(File.ReadAllText(recoveryBackup) == backupBefore,
        "backup inspection does not mutate existing backup");

    var retainedOriginal = ReminderStore.RestoreBackupAfterConfirmation(recoveryDir);
    Check(File.Exists(retainedOriginal) &&
          File.ReadAllText(retainedOriginal) == "damaged-primary-file-KEEP-ME",
        "explicit recovery keeps a separate original corrupted copy");
    var afterRecovery = new ReminderStore(recoveryDir);
    Check(afterRecovery.Reminders.Count == 1 &&
          afterRecovery.Reminders[0].Title == "Original older reminder",
        "restored backup contains prior saved state, not unsaved newer changes");
    Check(File.ReadAllText(recoveryBackup) == backupBefore,
        "restore never changes source backup");

    File.WriteAllText(recoveryBackup, "also-broken-backup");
    Check(!ReminderStore.HasValidBackup(recoveryDir),
        "corrupted backup is detected and cannot be offered for restore");
    bool invalidBackupRefused = false;
    try { ReminderStore.RestoreBackupAfterConfirmation(recoveryDir); }
    catch (InvalidDataException) { invalidBackupRefused = true; }
    Check(invalidBackupRefused &&
          new ReminderStore(recoveryDir).Reminders.Count == 1,
        "invalid backup cannot overwrite a healthy primary file");


    // v0.11 walk geometry never owns the whole screen and always ends in
    // the middle of the *selected* working area in WPF device-independent pixels.
    var fromLeft = PetWalkRules.Plan(0, 1920, 485, true);
    var fromRight = PetWalkRules.Plan(0, 1920, 485, false);
    Check(fromLeft.Start == 0 && fromRight.Start == 1435,
        "pet starts fully within its active monitor left or right edge");
    Check(Math.Abs(fromLeft.End - 717.5) < 0.001 &&
          Math.Abs(fromRight.End - fromLeft.End) < .001,
        "both entry directions end at the same central destination");
    var shifted = PetWalkRules.Plan(-1600, 1600, 485, false);
    Check(shifted.Start == -485 && shifted.End < 0 &&
          shifted.End > -1600,
        "pet placement handles secondary monitor with negative X origin");
    var narrow = PetWalkRules.Plan(200, 350, 485, true);
    Check(narrow.Start == 200 && narrow.End == 200,
        "pet is clamped on work area narrower than its window");
    var invalidGeometryRejected = false;
    try { _ = PetWalkRules.Plan(0, double.NaN, 400, true); }
    catch (ArgumentOutOfRangeException) { invalidGeometryRejected = true; }
    Check(invalidGeometryRejected, "nonfinite monitor metrics rejected safely");

    var speechDone = current.AddMinutes(1);
    Check(PetWalkRules.NextSpeechAt(speechDone, 60) == speechDone.AddSeconds(60),
        "spoken reminder repeat starts after prior narration finishes");
    Check(PetWalkRules.NextSpeechAt(speechDone, 120) ==
          speechDone.AddMinutes(2), "repeat interval may be lengthened");
    var invalidIntervalRejected = false;
    try { _ = PetWalkRules.NextSpeechAt(speechDone, 0); }
    catch (ArgumentOutOfRangeException) { invalidIntervalRejected = true; }
    Check(invalidIntervalRejected, "zero-delay infinite speech repeat cannot be scheduled");
    Check(PetWalkRules.IsVoiceRepeatAllowed(true, false, false, true, true),
        "active unmuted visible pet is eligible for speech repeat");
    Check(!PetWalkRules.IsVoiceRepeatAllowed(true, true, false, true, true),
        "muting stops new speech repetitions");
    Check(!PetWalkRules.IsVoiceRepeatAllowed(true, false, true, true, true),
        "Windows lock/suspend blocks speech repetition");
    Check(!PetWalkRules.IsVoiceRepeatAllowed(true, false, false, false, true),
        "hidden pet does not speak");
    Check(!PetWalkRules.IsVoiceRepeatAllowed(true, false, false, true, false),
        "reacting or acknowledged pet does not speak");
    Check(!PetWalkRules.IsVoiceRepeatAllowed(false, false, false, true, true),
        "disabled local Windows voice is never scheduled");


    // v0.12: new Home never fabricates reminder counts or mutates persistence.
    var localSampleDay = new DateTime(2026, 10, 16, 10, 0, 0, DateTimeKind.Unspecified);
    var sampleTime = new DateTimeOffset(localSampleDay, TimeZoneInfo.Local.GetUtcOffset(localSampleDay));
    var sampleTasks = new List<Reminder>
    {
        new() { Title = "Video edit", DueAt = sampleTime.AddHours(4), Status = ReminderStatus.Pending },
        new() { Title = "Tomorrow reading", DueAt = sampleTime.AddDays(1), Status = ReminderStatus.Pending },
        new() { Title = "Tomorrow writing", DueAt = sampleTime.AddDays(1).AddHours(3), Status = ReminderStatus.Pending },
        new() { Title = "Next week plan", DueAt = sampleTime.AddDays(7), Status = ReminderStatus.Pending },
        new() { Title = "Needs attention", DueAt = sampleTime.AddHours(-1), Status = ReminderStatus.Delivered },
        new() { Title = "Finished", DueAt = sampleTime, Status = ReminderStatus.Completed }
    };
    var snapshot = DashboardSummaryRules.Build(sampleTasks, localSampleDay);
    Check(snapshot.UpcomingCount == 4 && snapshot.AttentionCount == 1,
        "Home counts are computed from stored reminder states");
    Check(snapshot.TodayCount == 2,
        "Home Today reflects active pending and delivered reminders for local calendar day");
    Check(snapshot.NextReminders.Count == 3 &&
          snapshot.NextReminders[0].Title == "Video edit" &&
          snapshot.NextReminders[2].Title == "Tomorrow writing",
        "Home shows only the next three upcoming tasks sorted by due date");
    Check(sampleTasks.Count == 6 &&
          sampleTasks.All(r => r.Title != "Pet caretaker appointment"),
        "Home summary is read-only and uses actual human reminder titles");
    var emptyHome = DashboardSummaryRules.Build(Array.Empty<Reminder>(), DateTime.Today);
    Check(emptyHome.TodayCount == 0 && emptyHome.UpcomingCount == 0 &&
          emptyHome.AttentionCount == 0 && emptyHome.NextReminders.Count == 0,
        "new dashboard shows honest empty totals without fake sample data");


    // v0.13: all three selectable species and their customized actions must
    // persist independently from existing reminder and appearance files.
    var petsFolder = Path.Combine(temp, "multi-pets");
    var initialPet = new PetProfileSettings(petsFolder);
    Check(initialPet.Current.Species == PetSpecies.Cat &&
          initialPet.Current.RepeatSpeechSeconds == 60,
        "multi-pet settings default safely to Milo with one-minute repeat");

    var dogProfile = initialPet.Current with
    {
        Species = PetSpecies.Dog, Palette = PetPalette.Cocoa,
        Accessory = PetAccessory.Scarf, Size = PetSize.Large,
        Personality = PetPersonality.Calm, DoneAction = PetDoneAction.Wave,
        SnoozeAction = PetSnoozeAction.Stretch, RepeatSpeechSeconds = 120
    };
    initialPet.Update(dogProfile);
    var dogOnRestart = new PetProfileSettings(petsFolder);
    Check(dogOnRestart.Current == dogProfile &&
          PetProfileCatalog.Name(dogProfile.Species) == "Buddy the Dog",
        "selected dog and all eight customization fields survive restart");

    var bunnyProfile = dogProfile with
    {
        Species = PetSpecies.Bunny, Palette = PetPalette.Snow,
        Accessory = PetAccessory.Bow, Size = PetSize.Small,
        Personality = PetPersonality.Curious, DoneAction = PetDoneAction.Hop,
        SnoozeAction = PetSnoozeAction.Yawn, RepeatSpeechSeconds = 30
    };
    initialPet.Update(bunnyProfile);
    Check(new PetProfileSettings(petsFolder).Current == bunnyProfile,
        "bunny, bow, snow palette and shorter narration interval persist");
    Check(PetProfileCatalog.Name(PetSpecies.Cat) == "Milo the Cat" &&
          PetProfileCatalog.Name(PetSpecies.Bunny) == "Luna the Bunny",
        "cat and bunny names resolve from built-in species catalog");
    Check(PetProfileCatalog.Name(PetSpecies.Dog) !=
          PetProfileCatalog.Name(PetSpecies.Bunny),
        "selected dog and bunny are distinct real character identities");

    var petFile = Path.Combine(petsFolder, "pet-profile.json");
    var originalProfile = File.ReadAllText(petFile);
    bool invalidSpeciesRejected = false;
    try { initialPet.Update(bunnyProfile with { Species = (PetSpecies)999 }); }
    catch (ArgumentException) { invalidSpeciesRejected = true; }
    Check(invalidSpeciesRejected && initialPet.Current == bunnyProfile &&
          File.ReadAllText(petFile) == originalProfile,
        "unsupported species cannot corrupt saved selection");

    bool invalidNarrationRejected = false;
    try { initialPet.Update(bunnyProfile with { RepeatSpeechSeconds = 0 }); }
    catch (ArgumentException) { invalidNarrationRejected = true; }
    Check(invalidNarrationRejected && initialPet.Current == bunnyProfile,
        "unsafe rapid repeat interval is refused");

    var blockedProfileTemp = petFile + ".tmp";
    Directory.CreateDirectory(blockedProfileTemp);
    bool failedPetSave = false;
    try { initialPet.Update(dogProfile); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { failedPetSave = true; }
    Check(failedPetSave && initialPet.Current == bunnyProfile &&
          File.ReadAllText(petFile) == originalProfile,
        "disk write failure preserves the last saved pet and choices");
    if (Directory.Exists(blockedProfileTemp)) Directory.Delete(blockedProfileTemp);

    File.WriteAllText(petFile, "malformed profile file");
    var fallbackPet = new PetProfileSettings(petsFolder);
    Check(fallbackPet.Current.Species == PetSpecies.Cat &&
          File.ReadAllText(petFile) == "malformed profile file",
        "malformed profile defaults to cat and original corrupt file is kept");

    Check(new ReminderStore(repeatFolder).Reminders.Count == reminderCountBefore,
        "pet switching and profile corruption never modifies existing reminders");

    var path = Path.Combine(temp, "reminders.json");
    File.WriteAllText(path, "corrupted not-json");
    bool rejected = false;
    try { _ = new ReminderStore(temp); } catch (JsonException) { rejected = true; }
    Check(rejected && File.ReadAllText(path) == "corrupted not-json", "corrupt data is rejected, not silently erased");
    Console.WriteLine($"All {checks} core smoke checks passed.");
}
finally
{
    try { Directory.Delete(temp, recursive: true); } catch (IOException) { }
}

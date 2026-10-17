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

    // Profiles are independent: changing Buddy must not overwrite Milo or Luna.
    initialPet.Select(PetSpecies.Cat);
    Check(initialPet.Current.Species == PetSpecies.Cat &&
          initialPet.Current.Palette == PetPalette.Apricot,
        "switching back to Milo restores his own original coat choices");
    initialPet.Select(PetSpecies.Dog);
    Check(initialPet.Current == dogProfile,
        "switching back to Buddy restores his individually saved scarf/actions");
    initialPet.Select(PetSpecies.Bunny);
    Check(initialPet.Current == bunnyProfile &&
          new PetProfileSettings(petsFolder).Get(PetSpecies.Dog) == dogProfile,
        "switching Luna back preserves Buddy's separate profile across restart");
    bool unknownPetRejected = false;
    try { initialPet.Select((PetSpecies)999); }
    catch (ArgumentException) { unknownPetRejected = true; }
    Check(unknownPetRejected && initialPet.SelectedSpecies == PetSpecies.Bunny,
        "unknown species cannot replace the chosen pet");

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

    // v0.14: each species has its own gait and finite reaction budget. These
    // pure tests protect motion-off/reduced-motion preferences and bound visual
    // effects without starting WPF (which runs in separate Windows CI).
    var catGait = PetAnimationRules.Gait(PetSpecies.Cat, PetPersonality.Playful);
    var dogGait = PetAnimationRules.Gait(PetSpecies.Dog, PetPersonality.Playful);
    var bunnyGait = PetAnimationRules.Gait(PetSpecies.Bunny, PetPersonality.Playful);
    Check(catGait.StepMilliseconds != dogGait.StepMilliseconds &&
          dogGait.StepMilliseconds != bunnyGait.StepMilliseconds,
        "cat, dog and bunny have different actual stepping rhythms");
    Check(dogGait.TailAngle > catGait.TailAngle &&
          bunnyGait.BobHeight > dogGait.BobHeight &&
          bunnyGait.EarWiggleMilliseconds > 0,
        "dog wags wider, bunny hops higher and moves ears");
    var calmCat = PetAnimationRules.Gait(PetSpecies.Cat, PetPersonality.Calm);
    var curiousCat = PetAnimationRules.Gait(PetSpecies.Cat, PetPersonality.Curious);
    Check(calmCat.StepMilliseconds > catGait.StepMilliseconds &&
          calmCat.FootLift < catGait.FootLift,
        "calm personality walks slower and with lighter steps");
    Check(curiousCat.StepMilliseconds < catGait.StepMilliseconds,
        "curious personality increases activity");
    bool badSpeciesGaitRejected = false;
    try { _ = PetAnimationRules.Gait((PetSpecies)999, PetPersonality.Playful); }
    catch (ArgumentOutOfRangeException) { badSpeciesGaitRejected = true; }
    Check(badSpeciesGaitRejected, "animation rejects unsupported species");

    var playableCat = new PetProfile
    {
        Species = PetSpecies.Cat, Effects = PetEffects.Playful, DoneAction = PetDoneAction.Hop
    };
    var playableDog = playableCat with { Species = PetSpecies.Dog };
    var playfulBunny = playableCat with { Species = PetSpecies.Bunny };
    var catReaction = PetAnimationRules.Reaction(playableCat, true, PetMotion.Full);
    var dogReaction = PetAnimationRules.Reaction(playableDog, true, PetMotion.Full);
    var bunnyReaction = PetAnimationRules.Reaction(playfulBunny, true, PetMotion.Full);
    Check(catReaction.ParticleCount == 10 &&
          catReaction.ParticleCount <= PetAnimationRules.MaxParticles,
        "Playful Done celebration uses strictly bounded particles");
    Check(bunnyReaction.HopHeight > dogReaction.HopHeight &&
          dogReaction.HopHeight > catReaction.HopHeight,
        "bunny, puppy and cat receive distinct completed hop heights");
    Check(catReaction.DurationMilliseconds <= PetAnimationRules.MaxReactionMs,
        "reaction duration stays within published finite safety budget");
    var gentleEffects = PetAnimationRules.Reaction(
        playableCat with { Effects = PetEffects.Gentle }, true, PetMotion.Full);
    Check(gentleEffects.ParticleCount == 4,
        "per-pet gentle effects reduce particle count");
    var mutedEffects = PetAnimationRules.Reaction(
        playableCat with { Effects = PetEffects.Off }, true, PetMotion.Full);
    Check(mutedEffects.ParticleCount == 0 &&
          mutedEffects.HopHeight > 0,
        "turning off sparkles does not disable the pet's chosen action");
    var gentleMotion = PetAnimationRules.Reaction(playableCat, true, PetMotion.Gentle);
    Check(gentleMotion.ParticleCount == 0 &&
          gentleMotion.HopHeight > 0 &&
          gentleMotion.DurationMilliseconds < catReaction.DurationMilliseconds,
        "gentle motion keeps short movement but removes particles");
    var motionOff = PetAnimationRules.Reaction(playableCat, true, PetMotion.Off);
    Check(motionOff.ParticleCount == 0 && motionOff.HopHeight == 0 &&
          !motionOff.AllowContinuousIdle,
        "Motion Off eliminates every animated reaction and sparkle");
    var snoozeReaction = PetAnimationRules.Reaction(playableCat, false, PetMotion.Full);
    Check(snoozeReaction.ParticleCount == 5 && snoozeReaction.HopHeight == 0,
        "Snooze has a quieter distinct effect budget than Done");
    bool invalidEffectRejected = false;
    try { PetProfileCatalog.Validate(playableCat with { Effects = (PetEffects)999 }); }
    catch (ArgumentException) { invalidEffectRejected = true; }
    Check(invalidEffectRejected, "unknown visual effect setting rejected");
    var visualPetFolder = Path.Combine(temp, "effect-profile");
    var visualStore = new PetProfileSettings(visualPetFolder);
    var withEffect = visualStore.Current with { Effects = PetEffects.Off };
    visualStore.Update(withEffect);
    Check(new PetProfileSettings(visualPetFolder).Current.Effects == PetEffects.Off,
        "per-pet effects preference persists after application restart");
    var oldRoster = System.Text.Json.Nodes.JsonNode.Parse(
        File.ReadAllText(Path.Combine(visualPetFolder, "pet-profile.json")))!.AsObject();
    foreach (var entry in oldRoster["Profiles"]!.AsArray())
        entry!.AsObject().Remove("Effects");
    var legacyEffectsFolder = Path.Combine(temp, "legacy-effects");
    Directory.CreateDirectory(legacyEffectsFolder);
    File.WriteAllText(Path.Combine(legacyEffectsFolder, "pet-profile.json"),
        oldRoster.ToJsonString());
    Check(new PetProfileSettings(legacyEffectsFolder).Current.Effects == PetEffects.Playful,
        "existing v0.13 pet profiles missing Effects property load safely with defaults");
    Check(new ReminderStore(repeatFolder).Reminders.Count == reminderCountBefore,
        "v0.14 effects configuration leaves reminder data unchanged");

    // R2-B isolated custom recurrence previews. Zero schema / UI migration.
    var r2bAnchor = new DateTime(2026, 10, 12); // Monday
    var r2bNow = new DateTimeOffset(2026, 10, 13, 10, 0, 0, TimeSpan.Zero);
    var monWedFri = new [] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
    var midWeekNext = AdvancedRepeatRules.NextWeekdayOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0), monWedFri, 1, TimeZoneInfo.Utc, r2bNow);
    Check(midWeekNext == new DateTimeOffset(2026, 10, 14, 9, 0, 0, TimeSpan.Zero),
        "custom Mon/Wed/Fri recurrence chooses the next selected local weekday");
    var afterFriday = new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.Zero);
    var nextMonday = AdvancedRepeatRules.NextWeekdayOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0), monWedFri, 1, TimeZoneInfo.Utc, afterFriday);
    Check(nextMonday == new DateTimeOffset(2026, 10, 19, 9, 0, 0, TimeSpan.Zero),
        "custom weekdays wrap correctly from Friday into the next week");
    var everyTwoWeeks = AdvancedRepeatRules.NextWeekdayOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0),
        [DayOfWeek.Monday, DayOfWeek.Wednesday], 2, TimeZoneInfo.Utc, afterFriday);
    Check(everyTwoWeeks == new DateTimeOffset(2026, 10, 26, 9, 0, 0, TimeSpan.Zero),
        "selected days every second week retain the original Monday-week anchor");
    var anchorInsideWeek = AdvancedRepeatRules.NextWeekdayOccurrence(
        new DateTime(2026, 10, 15), new TimeSpan(9, 0, 0),
        [DayOfWeek.Monday, DayOfWeek.Wednesday], 2, TimeZoneInfo.Utc, afterFriday);
    Check(anchorInsideWeek == everyTwoWeeks,
        "custom repeat never schedules a selected weekday before the anchor date");
    var uniqueWeekday = AdvancedRepeatRules.NextWeekdayOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0),
        [DayOfWeek.Wednesday, DayOfWeek.Wednesday], 1, TimeZoneInfo.Utc, r2bNow);
    Check(uniqueWeekday == midWeekNext,
        "duplicate weekday selections are normalized without duplicate schedules");

    var dayThree = AdvancedRepeatRules.NextDayIntervalOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0), 3, TimeZoneInfo.Utc, r2bNow);
    Check(dayThree == new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero),
        "every-three-day schedule advances from its original local anchor");
    var longOffline = new DateTimeOffset(2045, 4, 19, 8, 0, 0, TimeSpan.Zero);
    var jumpedWeek = AdvancedRepeatRules.NextWeekdayOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0), [DayOfWeek.Tuesday], 3,
        TimeZoneInfo.Utc, longOffline);
    var jumpedDay = AdvancedRepeatRules.NextDayIntervalOccurrence(
        r2bAnchor, new TimeSpan(9, 0, 0), 15, TimeZoneInfo.Utc, longOffline);
    Check(jumpedWeek > longOffline && jumpedWeek <= longOffline.AddDays(22) &&
          jumpedDay > longOffline && jumpedDay <= longOffline.AddDays(16),
        "years of downtime jump directly to a bounded future selected occurrence");

    bool missingDayRejected = false, invalidWeekRejected = false,
         invalidDayStepRejected = false, invalidClockRejected = false,
         invalidWeekdayRejected = false;
    try { AdvancedRepeatRules.NextWeekdayOccurrence(r2bAnchor, TimeSpan.FromHours(9),
        Array.Empty<DayOfWeek>(), 1, TimeZoneInfo.Utc, r2bNow); }
    catch (ArgumentException) { missingDayRejected = true; }
    try { AdvancedRepeatRules.NextWeekdayOccurrence(r2bAnchor, TimeSpan.FromHours(9),
        [DayOfWeek.Monday], 53, TimeZoneInfo.Utc, r2bNow); }
    catch (ArgumentOutOfRangeException) { invalidWeekRejected = true; }
    try { AdvancedRepeatRules.NextDayIntervalOccurrence(r2bAnchor,
        TimeSpan.FromHours(9), 0, TimeZoneInfo.Utc, r2bNow); }
    catch (ArgumentOutOfRangeException) { invalidDayStepRejected = true; }
    try { AdvancedRepeatRules.NextDayIntervalOccurrence(r2bAnchor,
        TimeSpan.FromDays(1), 2, TimeZoneInfo.Utc, r2bNow); }
    catch (ArgumentOutOfRangeException) { invalidClockRejected = true; }
    try { AdvancedRepeatRules.NextWeekdayOccurrence(r2bAnchor, TimeSpan.FromHours(9),
        [(DayOfWeek)999], 1, TimeZoneInfo.Utc, r2bNow); }
    catch (ArgumentException) { invalidWeekdayRejected = true; }
    Check(missingDayRejected && invalidWeekRejected && invalidDayStepRejected &&
          invalidClockRejected && invalidWeekdayRejected,
        "custom repeat rejects no weekdays, invalid weekdays, invalid intervals/time");

    var r2bDstStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
        new DateTime(1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday);
    var r2bDstEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
        new DateTime(1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday);
    var r2bRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
        new DateTime(2020, 1, 1), new DateTime(2035, 12, 31),
        TimeSpan.FromHours(1), r2bDstStart, r2bDstEnd);
    var r2bZone = TimeZoneInfo.CreateCustomTimeZone("PetPing-Repeat-DST-Test",
        TimeSpan.FromHours(-5), "Synthetic US timezone", "Winter", "Summer", [r2bRule]);
    var springDue = AdvancedRepeatRules.NextWeekdayOccurrence(
        new DateTime(2026, 3, 1), new TimeSpan(2, 30, 0), [DayOfWeek.Sunday],
        1, r2bZone, new DateTimeOffset(2026, 3, 7, 16, 0, 0, TimeSpan.Zero));
    Check(springDue.Date == new DateTime(2026, 3, 8) &&
          springDue.TimeOfDay == new TimeSpan(3, 0, 0) &&
          springDue.Offset == TimeSpan.FromHours(-4),
        "custom weekly DST spring gap moves just this occurrence to first valid minute");
    var followingSpring = AdvancedRepeatRules.NextWeekdayOccurrence(
        new DateTime(2026, 3, 1), new TimeSpan(2, 30, 0), [DayOfWeek.Sunday],
        1, r2bZone, new DateTimeOffset(2026, 3, 8, 16, 0, 0, TimeSpan.Zero));
    Check(followingSpring.Date == new DateTime(2026, 3, 15) &&
          followingSpring.TimeOfDay == new TimeSpan(2, 30, 0),
        "DST spring recovery restores the originally requested weekly wall time");
    var overlapDue = AdvancedRepeatRules.NextWeekdayOccurrence(
        new DateTime(2026, 10, 25), new TimeSpan(1, 30, 0), [DayOfWeek.Sunday],
        1, r2bZone, new DateTimeOffset(2026, 10, 31, 16, 0, 0, TimeSpan.Zero));
    Check(overlapDue.Date == new DateTime(2026, 11, 1) &&
          overlapDue.Offset == TimeSpan.FromHours(-4),
        "custom recurrence chooses the earlier instant at the fall DST overlap");

    // R2-B persistence safety: opt-in custom series records are additive,
    // never inferred from legacy Daily / Weekly JSON.
    var customRoot = Path.Combine(temp, "advanced-repeat-store");
    var customStore = new ReminderStore(customRoot);
    var customAnchor = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(2),
        DateTimeKind.Unspecified);
    var customDayBit = (byte)(1 << (((int)customAnchor.DayOfWeek + 6) % 7));
    var customOptions = new CustomRepeatOptions
    {
        SchemaVersion = 1,
        Kind = CustomRepeatKind.Weekdays,
        AnchorLocalDate = customAnchor,
        Interval = 1,
        WeekdayMask = customDayBit
    };
    var customFirst = customStore.AddCustom("Every week on chosen days",
        new TimeSpan(9, 0, 0), TimeZoneInfo.Utc.Id, customOptions);
    Check(customFirst.Repeat == RepeatPattern.Custom &&
          customFirst.CustomSchedule == customOptions &&
          customFirst.SeriesId is not null &&
          customFirst.RepeatLocalTime == new TimeSpan(9, 0, 0),
        "custom recurring creation stores versioned anchor, local wall clock and stable series");
    var initialCustomJSON = File.ReadAllText(Path.Combine(customRoot, "reminders.json"));
    var customReloaded = new ReminderStore(customRoot);
    Check(customReloaded.Reminders.Count == 1 &&
          customReloaded.Reminders[0].CustomSchedule == customOptions &&
          customReloaded.Reminders[0].DueAt == customFirst.DueAt,
        "new custom metadata and exact due timestamp survive restart");

    bool customLegacyEditBlocked = false;
    try { customStore.Edit(customFirst, "Unexpected change",
        DateTimeOffset.UtcNow.AddDays(3), RepeatPattern.None); }
    catch (InvalidOperationException) { customLegacyEditBlocked = true; }
    Check(customLegacyEditBlocked && customFirst.Repeat == RepeatPattern.Custom &&
          File.ReadAllText(Path.Combine(customRoot, "reminders.json")) == initialCustomJSON,
        "old reminder editor cannot silently convert a custom series to one-time");

    var customSourceDue = customFirst.OriginalDueAt;
    customStore.Deliver(customFirst, DateTimeOffset.UtcNow);
    var customChild = customStore.Reminders.Single(r =>
        r.SeriesId == customFirst.SeriesId && r.Status == ReminderStatus.Pending);
    Check(customStore.Reminders.Count == 2 &&
          customFirst.RepeatContinuationCreated &&
          customChild.OriginalDueAt > customSourceDue &&
          customChild.CustomSchedule == customOptions &&
          customChild.RepeatLocalTime == customFirst.RepeatLocalTime,
        "atomic custom delivery records one future child with identical recurrence definition");
    customStore.Snooze(customFirst, TimeSpan.FromMinutes(10));
    customStore.Deliver(customFirst, DateTimeOffset.UtcNow);
    Check(customStore.Reminders.Count == 2 &&
          customStore.Reminders.Count(r =>
              r.SeriesId == customFirst.SeriesId && r.Status == ReminderStatus.Pending) == 1 &&
          customChild.Id == customStore.Reminders.Single(r =>
              r.SeriesId == customFirst.SeriesId && r.Status == ReminderStatus.Pending).Id,
        "redelivered snoozed custom occurrence never duplicates the next series event");
    var customReopen = new ReminderStore(customRoot);
    Check(customReopen.Reminders.Count == 2 &&
          customReopen.Reminders.Single(r => r.Id == customFirst.Id).RepeatContinuationCreated &&
          customReopen.Reminders.Single(r => r.Id == customChild.Id).CustomSchedule == customOptions,
        "restart retains idempotent custom continuation flags and versioned child metadata");

    var intervalStore = new ReminderStore(Path.Combine(temp, "advanced-repeat-days"));
    var intervalOptions = new CustomRepeatOptions
    {
        Kind = CustomRepeatKind.EveryNDays, AnchorLocalDate = customAnchor,
        Interval = 3, WeekdayMask = 0
    };
    var intervalFirst = intervalStore.AddCustom("Every three days",
        new TimeSpan(11, 0, 0), TimeZoneInfo.Utc.Id, intervalOptions);
    intervalStore.Deliver(intervalFirst, DateTimeOffset.UtcNow);
    var intervalNext = intervalStore.Reminders.Single(r =>
        r.Id != intervalFirst.Id && r.Status == ReminderStatus.Pending);
    Check(intervalNext.OriginalDueAt == intervalFirst.OriginalDueAt.AddDays(3) &&
          intervalNext.CustomSchedule == intervalOptions,
        "N-day persisted custom recurrence preserves its original day anchor");

    var blockedCustomFolder = Path.Combine(temp, "advanced-repeat-rollback");
    var blockedCustomStore = new ReminderStore(blockedCustomFolder);
    var blockedCustom = blockedCustomStore.AddCustom("No false advance on disk error",
        TimeSpan.FromHours(12), TimeZoneInfo.Utc.Id, customOptions);
    var beforeBadDelivery = File.ReadAllText(
        Path.Combine(blockedCustomFolder, "reminders.json"));
    var blockedCustomTemp = Path.Combine(blockedCustomFolder, "reminders.json.tmp");
    Directory.CreateDirectory(blockedCustomTemp);
    bool customSaveFailed = false;
    try { blockedCustomStore.Deliver(blockedCustom, DateTimeOffset.UtcNow); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    { customSaveFailed = true; }
    Check(customSaveFailed && blockedCustomStore.Reminders.Count == 1 &&
          blockedCustom.Status == ReminderStatus.Pending &&
          !blockedCustom.RepeatContinuationCreated &&
          File.ReadAllText(Path.Combine(blockedCustomFolder, "reminders.json")) ==
              beforeBadDelivery,
        "failed custom recurrence write restores in-memory state and original on-disk file");
    Directory.Delete(blockedCustomTemp);
    blockedCustomStore.Deliver(blockedCustom, DateTimeOffset.UtcNow);
    Check(new ReminderStore(blockedCustomFolder).Reminders.Count == 2,
        "retry after storage recovery writes exactly one custom continuation");

    var corruptCustomFolder = Path.Combine(temp, "advanced-repeat-corrupt");
    Directory.CreateDirectory(corruptCustomFolder);
    var customNode = System.Text.Json.Nodes.JsonNode.Parse(initialCustomJSON)!.AsArray();
    customNode[0]!["CustomSchedule"]!["SchemaVersion"] = 99;
    var corruptCustomFile = Path.Combine(corruptCustomFolder, "reminders.json");
    File.WriteAllText(corruptCustomFile, customNode.ToJsonString());
    var corruptBytes = File.ReadAllText(corruptCustomFile);
    bool corruptCustomRejected = false;
    try { _ = new ReminderStore(corruptCustomFolder); }
    catch (InvalidDataException) { corruptCustomRejected = true; }
    Check(corruptCustomRejected && File.ReadAllText(corruptCustomFile) == corruptBytes,
        "unsupported custom schema is rejected without overwriting user's unreadable data");

    bool emptyDaysRejected = false, extraBitsRejected = false, badIntervalRejected = false;
    try { (customOptions with { WeekdayMask = 0 }).Validate(); }
    catch (InvalidDataException) { emptyDaysRejected = true; }
    try { (customOptions with { WeekdayMask = 0x80 }).Validate(); }
    catch (InvalidDataException) { extraBitsRejected = true; }
    try { (intervalOptions with { Interval = 366 }).Validate(); }
    catch (InvalidDataException) { badIntervalRejected = true; }
    Check(emptyDaysRejected && extraBitsRejected && badIntervalRejected,
        "custom schedule validation rejects missing/unknown weekdays and invalid intervals");

    // A genuine legacy record remains readable when custom metadata does not
    // exist. The new reader does not infer recurrence or change old fields.
    Check(new ReminderStore(repeatFolder).Reminders.Count == reminderCountBefore &&
          new ReminderStore(repeatFolder).Reminders.All(r => r.CustomSchedule is null),
        "preexisting daily/weekly reminder data loads without new custom metadata");

    // R2-B custom editor/domain: create and update only a future unsent
    // occurrence, leave past history and prior-version recovery data alone.
    var editorStoreFolder = Path.Combine(temp, "custom-editor-safety");
    var editorStore = new ReminderStore(editorStoreFolder);
    var legacyBeforeCustom = editorStore.Add("Original daily reminder",
        DateTimeOffset.UtcNow.AddHours(6), RepeatPattern.Daily);
    var initialLegacyBytes = File.ReadAllText(
        Path.Combine(editorStoreFolder, "reminders.json"));
    var editorAnchor = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(4),
        DateTimeKind.Unspecified);
    var editorOptions = new CustomRepeatOptions
    {
        Kind = CustomRepeatKind.EveryNDays,
        AnchorLocalDate = editorAnchor,
        Interval = 3,
        WeekdayMask = 0
    };
    var editTarget = editorStore.AddCustom("Custom learning routine",
        TimeSpan.FromHours(10), TimeZoneInfo.Utc.Id, editorOptions);
    var backupPath = Path.Combine(editorStoreFolder, "reminders.pre-custom-v017.json");
    Check(File.Exists(backupPath) &&
          File.ReadAllText(backupPath) == initialLegacyBytes &&
          new ReminderStore(editorStoreFolder).Reminders.Count == 2,
        "first custom schedule creates a non-overwriting legacy-readable backup");
    var oldSeriesId = editTarget.SeriesId;
    var nextOptions = editorOptions with { Interval = 5 };
    editorStore.EditCustom(editTarget, "Updated custom learning",
        TimeSpan.FromHours(11), TimeZoneInfo.Utc.Id, nextOptions);
    Check(editTarget.Title == "Updated custom learning" &&
          editTarget.CustomSchedule == nextOptions &&
          editTarget.RepeatLocalTime == TimeSpan.FromHours(11) &&
          editTarget.SeriesId == oldSeriesId &&
          editTarget.RepeatContinuationCreated == false &&
          editorStore.Reminders.Count == 2,
        "editing a future unsent custom occurrence updates time/schedule without new series");
    Check(File.ReadAllText(backupPath) == initialLegacyBytes &&
          new ReminderStore(editorStoreFolder).Reminders.Single(r => r.Id == editTarget.Id)
              .CustomSchedule == nextOptions,
        "custom edit persists and keeps the initial pre-custom downgrade backup immutable");

    var beforeRejectedEdit = File.ReadAllText(
        Path.Combine(editorStoreFolder, "reminders.json"));
    bool invalidCustomEditRejected = false;
    try
    {
        editorStore.EditCustom(editTarget, "Not saved",
            TimeSpan.FromHours(12), TimeZoneInfo.Utc.Id,
            nextOptions with { Interval = 366 });
    }
    catch (InvalidDataException) { invalidCustomEditRejected = true; }
    Check(invalidCustomEditRejected &&
          File.ReadAllText(Path.Combine(editorStoreFolder, "reminders.json")) ==
              beforeRejectedEdit && editTarget.Title == "Updated custom learning",
        "invalid edited custom schedule never mutates saved JSON or visible in-memory data");

    var tempBlockedCustom = Path.Combine(editorStoreFolder, "reminders.json.tmp");
    Directory.CreateDirectory(tempBlockedCustom);
    bool customEditRollback = false;
    try
    {
        editorStore.EditCustom(editTarget, "Not persisted during disk fault",
            TimeSpan.FromHours(13), TimeZoneInfo.Utc.Id,
            nextOptions with { Interval = 2 });
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
    { customEditRollback = true; }
    Check(customEditRollback &&
          editTarget.Title == "Updated custom learning" &&
          editTarget.CustomSchedule == nextOptions &&
          File.ReadAllText(Path.Combine(editorStoreFolder, "reminders.json")) ==
              beforeRejectedEdit,
        "custom edit rolls back edited fields when disk save fails");
    Directory.Delete(tempBlockedCustom);

    editorStore.Deliver(editTarget, DateTimeOffset.UtcNow);
    bool historicalEditRejected = false;
    try
    {
        editorStore.EditCustom(editTarget, "Try changing delivered history",
            TimeSpan.FromHours(8), TimeZoneInfo.Utc.Id, nextOptions);
    }
    catch (InvalidOperationException) { historicalEditRejected = true; }
    Check(historicalEditRejected &&
          editTarget.Title == "Updated custom learning" &&
          editorStore.Reminders.Count(r => r.SeriesId == oldSeriesId &&
              r.Status == ReminderStatus.Pending) == 1,
        "once delivered, a custom occurrence's historical schedule cannot be silently edited");

    var corruptBackup = File.ReadAllText(backupPath);
    var subsequentCustom = editorStore.AddCustom("Another custom routine",
        TimeSpan.FromHours(15), TimeZoneInfo.Utc.Id, editorOptions);
    Check(subsequentCustom.Repeat == RepeatPattern.Custom &&
          File.ReadAllText(backupPath) == corruptBackup,
        "additional custom reminder creation never replaces initial pre-custom backup");

    // v0.18 R2-A: bounded, flexible snooze is additive to the legacy
    // one-time / daily / weekly schema; saved history and series anchor stay intact.
    Check(SnoozePolicy.Presets.SequenceEqual([5, 10, 15, 30, 60]),
        "flexible snooze offers five predictable, ascending preset durations");
    Check(SnoozePolicy.FromMinutes(1) == TimeSpan.FromMinutes(1) &&
          SnoozePolicy.FromMinutes(1440) == TimeSpan.FromDays(1),
        "custom snooze accepts the inclusive 1-minute to 24-hour bounds");
    bool negativeSnoozeRejected = false;
    bool zeroSnoozeRejected = false;
    bool overlongSnoozeRejected = false;
    bool fractionalUnderMinimumRejected = false;
    try { SnoozePolicy.FromMinutes(-5); }
    catch (ArgumentOutOfRangeException) { negativeSnoozeRejected = true; }
    try { SnoozePolicy.FromMinutes(0); }
    catch (ArgumentOutOfRangeException) { zeroSnoozeRejected = true; }
    try { SnoozePolicy.FromMinutes(1441); }
    catch (ArgumentOutOfRangeException) { overlongSnoozeRejected = true; }
    try { SnoozePolicy.Validate(TimeSpan.FromSeconds(59)); }
    catch (ArgumentOutOfRangeException) { fractionalUnderMinimumRejected = true; }
    Check(negativeSnoozeRejected && zeroSnoozeRejected &&
          overlongSnoozeRejected && fractionalUnderMinimumRejected,
        "snooze policy rejects negative, zero, under-one-minute and >24h durations");

    var flexibleFolder = Path.Combine(temp, "flexible-snooze");
    var flexibleStore = new ReminderStore(flexibleFolder);
    var flexible = flexibleStore.Add("Reminder with flexible snooze",
        DateTimeOffset.Now.AddMinutes(30), RepeatPattern.Daily);
    flexibleStore.Deliver(flexible, DateTimeOffset.UtcNow);
    var pendingFutureId = flexibleStore.Reminders.Single(r =>
        r.SeriesId == flexible.SeriesId && r.Status == ReminderStatus.Pending).Id;
    var beforeShortSnooze = DateTimeOffset.UtcNow;
    flexibleStore.Snooze(flexible, SnoozePolicy.FromMinutes(5));
    Check(flexible.Status == ReminderStatus.Pending &&
          flexible.SnoozeCount == 1 &&
          flexible.DueAt >= beforeShortSnooze.AddMinutes(5) &&
          flexible.DueAt <= DateTimeOffset.UtcNow.AddMinutes(5) &&
          flexible.OriginalDueAt != flexible.DueAt,
        "5-minute snooze persists future due while keeping original recurrence anchor");
    Check(new ReminderStore(flexibleFolder).Reminders.Count == 2 &&
          flexibleStore.Reminders.Single(r => r.Id == pendingFutureId).Status ==
          ReminderStatus.Pending && flexible.RepeatContinuationCreated,
        "snoozing a delivered daily series does not duplicate its next occurrence");
    flexibleStore.Mark(flexible, ReminderStatus.Missed);
    var beforeLongSnooze = DateTimeOffset.UtcNow;
    flexibleStore.Snooze(flexible, SnoozePolicy.FromMinutes(1440));
    Check(flexible.SnoozeCount == 2 &&
          flexible.DueAt >= beforeLongSnooze.AddDays(1) &&
          flexible.DueAt <= DateTimeOffset.UtcNow.AddDays(1) &&
          flexibleStore.Reminders.Count == 2,
        "24-hour snooze saves the due time and does not generate extra repeats");
    var reopenedFlexible = new ReminderStore(flexibleFolder);
    var matchingFlexible = reopenedFlexible.Reminders.Single(r => r.Id == flexible.Id);
    Check(matchingFlexible.SnoozeCount == 2 &&
          matchingFlexible.OriginalDueAt == flexible.OriginalDueAt &&
          matchingFlexible.RepeatContinuationCreated &&
          reopenedFlexible.Reminders.Count == 2,
        "flexible snooze round-trip preserves occurrence history and series identity");
    // Snooze is permitted only for Delivered/Missed, not already Pending.
    // Recreate a Needs Attention state so this verifies bounds at the store
    // boundary without bypassing its existing status protection.
    reopenedFlexible.Mark(matchingFlexible, ReminderStatus.Missed);
    var stateBeforeInvalidSnooze = matchingFlexible.DueAt;
    bool storeLongRejected = false;
    try { reopenedFlexible.Snooze(matchingFlexible, TimeSpan.FromDays(2)); }
    catch (ArgumentOutOfRangeException) { storeLongRejected = true; }
    Check(storeLongRejected && matchingFlexible.DueAt == stateBeforeInvalidSnooze &&
          new ReminderStore(flexibleFolder).Reminders.Single(r => r.Id == flexible.Id).DueAt
              == stateBeforeInvalidSnooze,
        "storage-layer duration validation rejects oversized snooze without mutation");

    // v0.18 R2-A: the same editor rules govern Create and Edit. No silent
    // conversion of invalid local wall-times or impossible repeat selections.
    Check(ReminderEditorRules.ValidateTitle("  Review storyboard  ") == "Review storyboard",
        "editor trims title whitespace without losing meaningful characters");
    Check(ReminderEditorRules.ValidateTitle(new string('a', 120)).Length == 120,
        "editor permits names exactly at its 120-character boundary");
    bool emptyTitleRejected = false;
    bool overlongTitleRejected = false;
    try { ReminderEditorRules.ValidateTitle("  "); }
    catch (ArgumentException) { emptyTitleRejected = true; }
    try { ReminderEditorRules.ValidateTitle(new string('x', 121)); }
    catch (ArgumentException) { overlongTitleRejected = true; }
    Check(emptyTitleRejected && overlongTitleRejected,
        "editor rejects empty and overlong names without a store write");
    Check(ReminderEditorRules.SelectRepeat(0) == RepeatPattern.None &&
          ReminderEditorRules.SelectRepeat(1) == RepeatPattern.Daily &&
          ReminderEditorRules.SelectRepeat(2) == RepeatPattern.Weekly,
        "editor repeat selection preserves all three existing frequencies");
    bool invalidRepeatRejected = false;
    try { ReminderEditorRules.SelectRepeat(-1); }
    catch (ArgumentException) { invalidRepeatRejected = true; }
    Check(invalidRepeatRejected, "editor rejects an unset or unsupported repeat choice");

    var midnightUtc = new DateTimeOffset(2026, 10, 16, 23, 58, 0, TimeSpan.Zero);
    var midnightDefault = ReminderEditorRules.DefaultLocalDue(midnightUtc, TimeZoneInfo.Utc);
    Check(midnightDefault.Date == new DateTime(2026, 10, 17) &&
          midnightDefault.TimeOfDay == new TimeSpan(0, 3, 0),
        "default reminder date and time advance together across midnight");
    var sampleNow = new DateTimeOffset(2026, 10, 16, 14, 0, 0, TimeSpan.Zero);
    var validDue = ReminderEditorRules.ReadLocalDue(
        new DateTime(2026, 10, 17), "09:30", TimeZoneInfo.Utc, sampleNow);
    Check(validDue == new DateTimeOffset(2026, 10, 17, 9, 30, 0, TimeSpan.Zero),
        "editor parses a future 24-hour local time without silent adjustment");
    bool dateRequired = false, timeRequired = false, timeSyntaxRejected = false,
        pastRejected = false;
    try { ReminderEditorRules.ReadLocalDue(null, "19:00", TimeZoneInfo.Utc, sampleNow); }
    catch (ArgumentException) { dateRequired = true; }
    try { ReminderEditorRules.ReadLocalDue(new DateTime(2026, 10, 17),
        "", TimeZoneInfo.Utc, sampleNow); }
    catch (ArgumentException) { timeRequired = true; }
    try { ReminderEditorRules.ReadLocalDue(new DateTime(2026, 10, 17),
        "24:00", TimeZoneInfo.Utc, sampleNow); }
    catch (ArgumentException) { timeSyntaxRejected = true; }
    try { ReminderEditorRules.ReadLocalDue(new DateTime(2026, 10, 16),
        "13:59", TimeZoneInfo.Utc, sampleNow); }
    catch (ArgumentException) { pastRejected = true; }
    Check(dateRequired && timeRequired && timeSyntaxRejected && pastRejected,
        "editor rejects unset dates, invalid 24-hour clocks, and past timestamps");

    // Use an entirely synthetic timezone to avoid assumptions about Windows
    // registry or ICU aliases on different CI machines.
    var springSwitch = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
        new DateTime(1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday);
    var fallSwitch = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
        new DateTime(1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday);
    var syntheticDSTRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
        new DateTime(2020, 1, 1), new DateTime(2035, 12, 31),
        TimeSpan.FromHours(1), springSwitch, fallSwitch);
    var syntheticZone = TimeZoneInfo.CreateCustomTimeZone("PetPing-Editor-DST-Test",
        TimeSpan.FromHours(-5), "PetPing editor test time zone", "Winter", "Summer",
        [syntheticDSTRule]);
    bool gapRejected = false, repeatedHourRejected = false;
    try { ReminderEditorRules.ReadLocalDue(new DateTime(2026, 3, 8),
        "02:30", syntheticZone, sampleNow.AddMonths(-8)); }
    catch (ArgumentException) { gapRejected = true; }
    try { ReminderEditorRules.ReadLocalDue(new DateTime(2026, 11, 1),
        "01:30", syntheticZone, sampleNow); }
    catch (ArgumentException) { repeatedHourRejected = true; }
    Check(gapRejected && repeatedHourRejected,
        "editor refuses DST gap and ambiguous repeated wall hour instead of guessing");
    var normalSummerDue = ReminderEditorRules.ReadLocalDue(new DateTime(2026, 7, 15),
        "10:00", syntheticZone, sampleNow.AddMonths(-8));
    Check(normalSummerDue.Offset == TimeSpan.FromHours(-4) &&
          normalSummerDue.Hour == 10,
        "valid DST summer reminder retains its intended wall time and UTC offset");

    var invalidEditorStore = new ReminderStore(Path.Combine(temp, "editor-validation"));
    var invalidEditorItem = invalidEditorStore.Add("Unchanged", DateTimeOffset.UtcNow.AddHours(2));
    var diskBeforeInvalidEdit = File.ReadAllText(
        Path.Combine(temp, "editor-validation", "reminders.json"));
    bool invalidEditorEditRejected = false;
    try { invalidEditorStore.Edit(invalidEditorItem, new string('a', 121),
        DateTimeOffset.UtcNow.AddHours(3), RepeatPattern.None); }
    catch (ArgumentException) { invalidEditorEditRejected = true; }
    Check(invalidEditorEditRejected &&
          invalidEditorItem.Title == "Unchanged" &&
          File.ReadAllText(Path.Combine(temp, "editor-validation", "reminders.json"))
            == diskBeforeInvalidEdit,
        "invalid edited name keeps both in-memory occurrence and saved JSON untouched");

    Check(PetPingDataRoot.DefaultDirectory.EndsWith(
        Path.DirectorySeparatorChar + "PetPing-Experimental", StringComparison.Ordinal),
        "experimental preview is compiled with its own local data directory");
    Check(ReminderStore.DefaultDataDirectory == PetPingDataRoot.DefaultDirectory,
        "experimental reminder store never defaults to v0.17 production data");

    // R2 reliability: reconcile a deterministic clock while simulating
    // long scheduler gaps, lock/sleep, reentrant UI and filesystem faults.
    var pulseFolder = Path.Combine(temp, "due-processor");
    var pulseStore = new ReminderStore(pulseFolder);
    var pulseAt = DateTimeOffset.UtcNow.AddHours(1);
    var closePulse = pulseStore.Add("Deliver after delayed tick", pulseAt.AddMinutes(10));
    var missedPulse = pulseStore.Add("Expired during downtime", pulseAt.AddMinutes(11));
    var futurePulse = pulseStore.Add("Not due yet", pulseAt.AddMinutes(12));
    closePulse.DueAt = pulseAt.AddMinutes(-1);
    missedPulse.DueAt = pulseAt.AddMinutes(-20);
    pulseStore.Save();

    var pulse = new ReminderDueProcessor(pulseStore);
    int deliveredPulses = 0, missedPulses = 0, pulseChanged = 0, callbackFaults = 0;
    pulse.ReminderDue += r =>
    {
        deliveredPulses++;
        pulse.CheckNow(pulseAt); // must be ignored rather than nested/duplicated
        throw new InvalidOperationException("UI temporarily unavailable");
    };
    pulse.RemindersMissed += n => missedPulses += n;
    pulse.Changed += () => pulseChanged++;
    pulse.SchedulerFault += (code, error) =>
    {
        if (code == "presentation_failed") callbackFaults++;
    };
    pulse.CheckNow(pulseAt);
    Check(pulse.LastCheckedAt == pulseAt &&
          closePulse.Status == ReminderStatus.Delivered &&
          missedPulse.Status == ReminderStatus.Missed &&
          futurePulse.Status == ReminderStatus.Pending,
        "tick reconciliation commits recent due, flags downtime miss and leaves future untouched");
    Check(deliveredPulses == 1 && missedPulses == 1 &&
          pulseChanged == 2 && callbackFaults == 1,
        "UI presentation exception and recursive tick do not duplicate delivery");
    Check(new ReminderStore(pulseFolder).Reminders.Single(r => r.Id == closePulse.Id).Status
             == ReminderStatus.Delivered &&
          new ReminderStore(pulseFolder).Reminders.Single(r => r.Id == missedPulse.Id).Status
             == ReminderStatus.Missed,
        "due and missed decisions reach disk before callbacks");
    pulse.CheckNow(pulseAt);
    Check(deliveredPulses == 1 && missedPulses == 1 && pulseChanged == 2,
        "repeated tick is idempotent after committed delivery");

    // A presentation callback may remove an item already in the tick's
    // snapshot. The processor must revalidate presence and status.
    var mutationFolder = Path.Combine(temp, "callback-mutation");
    var mutationStore = new ReminderStore(mutationFolder);
    var firstMutation = mutationStore.Add("First popup", pulseAt.AddMinutes(1));
    var secondMutation = mutationStore.Add("Second popup", pulseAt.AddMinutes(2));
    firstMutation.DueAt = pulseAt.AddSeconds(-90);
    secondMutation.DueAt = pulseAt.AddSeconds(-80);
    mutationStore.Save();
    var mutationProcessor = new ReminderDueProcessor(mutationStore);
    int mutationCount = 0;
    mutationProcessor.ReminderDue += _ =>
    {
        mutationCount++;
        mutationStore.Delete(secondMutation);
    };
    mutationProcessor.CheckNow(pulseAt);
    Check(mutationCount == 1 && mutationStore.Reminders.Count == 1 &&
          firstMutation.Status == ReminderStatus.Delivered,
        "due snapshot safely skips reminders deleted by a presentation callback");

    // Storage retry is deterministic and has a 30-second cooldown; in-memory
    // Pending and on-disk JSON are unchanged throughout failed commits.
    var retryFolder = Path.Combine(temp, "due-processor-retry");
    var retryStore = new ReminderStore(retryFolder);
    var retryPending = retryStore.Add("Retry failed disk write", pulseAt.AddMinutes(3));
    retryPending.DueAt = pulseAt.AddMinutes(-2);
    retryStore.Save();
    var retryTempPath = Path.Combine(retryFolder, "reminders.json.tmp");
    var retryBeforeJson = File.ReadAllText(Path.Combine(retryFolder, "reminders.json"));
    Directory.CreateDirectory(retryTempPath);
    var retry = new ReminderDueProcessor(retryStore);
    int retryFaults = 0, retryDeliveries = 0;
    retry.SchedulerFault += (code, error) =>
    {
        if (code == "store_update_failed") retryFaults++;
        throw new Exception("Even error reporting can fail");
    };
    retry.ReminderDue += _ => retryDeliveries++;
    retry.CheckNow(pulseAt);
    Check(retryFaults == 1 && retryDeliveries == 0 &&
          retryPending.Status == ReminderStatus.Pending &&
          retry.RetryAfter == pulseAt.Add(ReminderDueProcessor.StorageRetryDelay) &&
          File.ReadAllText(Path.Combine(retryFolder, "reminders.json")) == retryBeforeJson,
        "disk failure fails closed, rolls back, and schedules bounded retry");
    Directory.Delete(retryTempPath);
    retry.CheckNow(pulseAt.AddSeconds(20));
    Check(retryDeliveries == 0 && retryPending.Status == ReminderStatus.Pending,
        "scheduler honors storage-failure cooldown after disk becomes available");
    retry.CheckNow(pulseAt.AddSeconds(30));
    Check(retryDeliveries == 1 && retryFaults == 1 &&
          retryPending.Status == ReminderStatus.Delivered &&
          new ReminderStore(retryFolder).Reminders.Single().Status ==
              ReminderStatus.Delivered,
        "exact retry boundary delivers once and persists even if fault observer threw");

    // A missed weekly series calculates its successor using the same event
    // instant that established the missed decision, not UtcNow inside Store.
    var offlineFolder = Path.Combine(temp, "due-processor-weekly-offline");
    var offlineStore = new ReminderStore(offlineFolder);
    var offlineFirst = offlineStore.Add("Weekly across long downtime",
        DateTimeOffset.UtcNow.AddHours(3), RepeatPattern.Weekly, TimeZoneInfo.Utc.Id);
    var afterOffline = DateTimeOffset.UtcNow.AddDays(21);
    var offline = new ReminderDueProcessor(offlineStore);
    offline.CheckNow(afterOffline);
    var offlineNext = offlineStore.Reminders.Single(r =>
        r.Id != offlineFirst.Id && r.SeriesId == offlineFirst.SeriesId);
    Check(offlineFirst.Status == ReminderStatus.Missed &&
          offlineFirst.FinishedAt == afterOffline &&
          offlineNext.DueAt > afterOffline &&
          offlineStore.Reminders.Count == 2,
        "missed weekly reminder skips backlog and advances strictly after reconciliation clock");
    offline.CheckNow(afterOffline);
    Check(offlineStore.Reminders.Count == 2 &&
          new ReminderStore(offlineFolder).Reminders.Count == 2,
        "second offline reconciliation preserves one-successor recurrence after restart");

    // User-driven read-only export must never alter source or overwrite
    // previous backups, and refuses corrupt on-disk primary data.
    var exportSourceFolder = Path.Combine(temp, "export-source");
    var exportTargetFolder = Path.Combine(temp, "user-chosen-backups");
    var exportStore = new ReminderStore(exportSourceFolder);
    exportStore.Add("Keep my morning reminder", DateTimeOffset.UtcNow.AddHours(5));
    var currentJson = Path.Combine(exportSourceFolder, "reminders.json");
    var originalJson = File.ReadAllText(currentJson);
    var exported1 = exportStore.ExportVerifiedBackup(exportTargetFolder);
    Check(File.Exists(exported1) && File.ReadAllText(exported1) == originalJson &&
          File.ReadAllText(currentJson) == originalJson,
        "verified export matches the active saved reminders without changing them");
    exportStore.Add("Added after first export", DateTimeOffset.UtcNow.AddHours(6));
    var exported2 = exportStore.ExportVerifiedBackup(exportTargetFolder);
    Check(exported1 != exported2 && File.Exists(exported1) && File.Exists(exported2) &&
          new ReminderStore(exportSourceFolder).Reminders.Count == 2 &&
          System.Text.Json.JsonSerializer.Deserialize<List<Reminder>>(
              File.ReadAllText(exported1), new JsonSerializerOptions
              {
                  Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
              })?.Count == 1 &&
          System.Text.Json.JsonSerializer.Deserialize<List<Reminder>>(
              File.ReadAllText(exported2), new JsonSerializerOptions
              {
                  Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
              })?.Count == 2,
        "repeated exports never overwrite earlier snapshots or delete newer reminders");
    bool sameFolderExportRefused = false;
    try { exportStore.ExportVerifiedBackup(exportSourceFolder); }
    catch (ArgumentException) { sameFolderExportRefused = true; }
    Check(sameFolderExportRefused,
        "backup export refuses the active PetPing data folder as destination");
    var previouslyExported = File.ReadAllText(exported2);
    File.WriteAllText(currentJson, "damaged-json-do-not-export");
    bool corruptExportRefused = false;
    try { exportStore.ExportVerifiedBackup(exportTargetFolder); }
    catch (JsonException) { corruptExportRefused = true; }
    Check(corruptExportRefused && File.ReadAllText(currentJson) ==
          "damaged-json-do-not-export" && File.ReadAllText(exported2) == previouslyExported &&
          Directory.GetFiles(exportTargetFolder, "*.json").Length == 2,
        "corrupt source is not exported or overwritten and prior good backups survive");
    var neverSavedStore = new ReminderStore(Path.Combine(temp, "no-export-yet"));
    bool emptyStoreExportRefused = false;
    try { neverSavedStore.ExportVerifiedBackup(exportTargetFolder); }
    catch (InvalidOperationException) { emptyStoreExportRefused = true; }
    Check(emptyStoreExportRefused,
        "first-run store without a saved JSON file cannot falsely report export success");

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

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

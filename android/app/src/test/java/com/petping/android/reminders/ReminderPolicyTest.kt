package com.petping.android.reminders

import com.petping.android.model.Reminder
import com.petping.android.model.ReminderStatus
import org.junit.Assert.*
import org.junit.Test

class ReminderPolicyTest {
    private val due = 1_800_000_000_000L
    private val reminder = Reminder(id="r1",title="Test",dueAtMillis=due)

    @Test fun futureAndCompletedNeverFire() {
        assertEquals(ReminderPolicy.Decision.NOT_DUE, ReminderPolicy.decision(reminder,due-1))
        assertEquals(ReminderPolicy.Decision.NOT_DUE,
            ReminderPolicy.decision(reminder.copy(status=ReminderStatus.COMPLETED),due+1000))
    }
    @Test fun exactAndFifteenMinutesLateDeliver() {
        assertEquals(ReminderPolicy.Decision.DELIVER,ReminderPolicy.decision(reminder,due))
        assertEquals(ReminderPolicy.Decision.DELIVER,
            ReminderPolicy.decision(reminder,due+ReminderPolicy.CATCH_UP_MS))
    }
    @Test fun longDowntimeCreatesMissed() {
        assertEquals(ReminderPolicy.Decision.MISSED,
            ReminderPolicy.decision(reminder,due+ReminderPolicy.CATCH_UP_MS+1))
    }
    @Test fun snoozeResetsStatusAndMovesFuture() {
        val r=ReminderPolicy.snooze(reminder.copy(status=ReminderStatus.DELIVERED,deliveredAtMillis=due),due+1000)
        assertEquals(ReminderStatus.PENDING,r.status)
        assertEquals(due+1000+ReminderPolicy.DEFAULT_SNOOZE_MS,r.dueAtMillis)
        assertEquals(1,r.snoozeCount)
        assertNull(r.deliveredAtMillis)
    }
    @Test(expected=IllegalArgumentException::class)
    fun cannotSnoozeUnfiredReminder() { ReminderPolicy.snooze(reminder,due) }
}

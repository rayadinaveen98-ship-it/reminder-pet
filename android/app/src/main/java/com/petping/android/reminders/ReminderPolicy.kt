package com.petping.android.reminders
import com.petping.android.model.Reminder
import com.petping.android.model.ReminderStatus
object ReminderPolicy {
    const val CATCH_UP_MS = 15 * 60 * 1000L
    const val DEFAULT_SNOOZE_MS = 10 * 60 * 1000L
    enum class Decision { NOT_DUE, DELIVER, MISSED }
    fun decision(reminder: Reminder, now: Long): Decision {
        if (reminder.status != ReminderStatus.PENDING || reminder.dueAtMillis > now)
            return Decision.NOT_DUE
        return if (now - reminder.dueAtMillis <= CATCH_UP_MS) Decision.DELIVER else Decision.MISSED
    }
    fun snooze(reminder: Reminder, now: Long): Reminder {
        require(reminder.status == ReminderStatus.DELIVERED || reminder.status == ReminderStatus.MISSED)
        return reminder.copy(status = ReminderStatus.PENDING,
            dueAtMillis = now + DEFAULT_SNOOZE_MS, deliveredAtMillis = null,
            snoozeCount = reminder.snoozeCount + 1)
    }
}

package com.petping.android.reminders

import android.content.Context
import android.app.NotificationManager
import com.petping.android.data.ReminderStore
import com.petping.android.model.Reminder
import com.petping.android.model.ReminderStatus

/** Commit statuses before attempting to present notifications. */
object ReminderEngine {
    fun reconcile(context: Context, now: Long = System.currentTimeMillis()) {
        val app = context.applicationContext
        val store = ReminderStore(app)
        val delivered = mutableListOf<Reminder>()
        store.change { current ->
            current.map { reminder ->
                when (ReminderPolicy.decision(reminder, now)) {
                    ReminderPolicy.Decision.NOT_DUE -> reminder
                    ReminderPolicy.Decision.DELIVER -> reminder.copy(
                        status = ReminderStatus.DELIVERED, deliveredAtMillis = now
                    ).also(delivered::add)
                    ReminderPolicy.Decision.MISSED -> reminder.copy(status = ReminderStatus.MISSED)
                }
            }
        }
        ReminderScheduler.reschedule(app)
        delivered.forEach { ReminderNotifications.post(app, it) }
    }

    fun act(context: Context, id: String, snooze: Boolean) {
        val app = context.applicationContext
        val now = System.currentTimeMillis()
        var changed = false
        ReminderStore(app).change { rows -> rows.map { reminder ->
            if (reminder.id != id || reminder.status !in
                    setOf(ReminderStatus.DELIVERED, ReminderStatus.MISSED)) reminder
            else {
                changed = true
                if (snooze) ReminderPolicy.snooze(reminder, now)
                else reminder.copy(status = ReminderStatus.COMPLETED)
            }
        } }
        if (changed) {
            app.getSystemService(NotificationManager::class.java)
                .cancel(ReminderNotifications.notificationId(id))
            ReminderScheduler.reschedule(app)
        }
    }
}

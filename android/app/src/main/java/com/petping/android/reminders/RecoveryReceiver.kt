package com.petping.android.reminders

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent

/** Alarms vanish on reboot or app update. Reconcile pending reminder state. */
class RecoveryReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent?) {
        if (intent?.action !in setOf(Intent.ACTION_BOOT_COMPLETED,
                Intent.ACTION_MY_PACKAGE_REPLACED, Intent.ACTION_TIME_CHANGED,
                Intent.ACTION_TIMEZONE_CHANGED)) return
        ReminderEngine.reconcile(context)
    }
}

class ExactAlarmPermissionReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent?) {
        if (intent?.action == android.app.AlarmManager.ACTION_SCHEDULE_EXACT_ALARM_PERMISSION_STATE_CHANGED &&
            ReminderScheduler.hasExactAccess(context))
            ReminderScheduler.reschedule(context)
    }
}

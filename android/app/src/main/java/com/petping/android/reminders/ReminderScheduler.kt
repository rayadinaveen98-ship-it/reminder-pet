package com.petping.android.reminders

import android.app.AlarmManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.os.Build
import com.petping.android.data.ReminderStore
import com.petping.android.model.ReminderStatus

/** One OS alarm for the earliest pending reminder; re-arm after each change. */
object ReminderScheduler {
    private const val REQUEST_CODE = 6910

    fun hasExactAccess(context: Context): Boolean {
        val manager = context.getSystemService(AlarmManager::class.java)
        return Build.VERSION.SDK_INT < Build.VERSION_CODES.S || manager.canScheduleExactAlarms()
    }

    fun reschedule(context: Context) {
        val app = context.applicationContext
        val manager = app.getSystemService(AlarmManager::class.java)
        val intent = Intent(app, ReminderReceiver::class.java).setAction(ReminderReceiver.ACTION_FIRE)
        val trigger = PendingIntent.getBroadcast(app, REQUEST_CODE, intent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE)
        manager.cancel(trigger)
        val first = ReminderStore(app).all().asSequence()
            .filter { it.status == ReminderStatus.PENDING }
            .minByOrNull { it.dueAtMillis } ?: return
        val whenMillis = maxOf(System.currentTimeMillis() + 1000L, first.dueAtMillis)
        try {
            if (hasExactAccess(app))
                manager.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, whenMillis, trigger)
            else manager.setAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, whenMillis, trigger)
        } catch (_: SecurityException) {
            manager.setAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, whenMillis, trigger)
        }
    }
}

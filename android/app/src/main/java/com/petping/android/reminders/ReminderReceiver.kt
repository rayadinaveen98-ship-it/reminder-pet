package com.petping.android.reminders

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent

class ReminderReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent?) {
        when (intent?.action) {
            ACTION_FIRE -> ReminderEngine.reconcile(context)
            ACTION_DONE, ACTION_SNOOZE -> {
                val id = intent.getStringExtra(EXTRA_ID) ?: return
                ReminderEngine.act(context, id, intent.action == ACTION_SNOOZE)
            }
        }
    }

    companion object {
        const val ACTION_FIRE = "com.petping.android.FIRE"
        const val ACTION_DONE = "com.petping.android.DONE"
        const val ACTION_SNOOZE = "com.petping.android.SNOOZE"
        const val EXTRA_ID = "reminder_id"
    }
}

package com.petping.android.reminders

import android.Manifest
import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.os.Build
import com.petping.android.MainActivity
import com.petping.android.R
import com.petping.android.model.PetKind
import com.petping.android.model.Reminder

/** Ordinary high-priority OS notification, no unsolicited overlay/activity launch. */
object ReminderNotifications {
    private const val CHANNEL_ID = "petping_reminders_v1"
    fun notificationId(id: String): Int = id.hashCode() and Int.MAX_VALUE

    private fun channel(context: Context) {
        context.getSystemService(NotificationManager::class.java)
            .createNotificationChannel(NotificationChannel(CHANNEL_ID, "PetPing reminders",
                NotificationManager.IMPORTANCE_HIGH).apply {
                    description = "Important reminders from your chosen companion"
                    enableVibration(true)
                })
    }

    private fun action(context: Context, id: String, kind: String, label: String): Notification.Action {
        val receiverAction = if (kind == "done") ReminderReceiver.ACTION_DONE
            else ReminderReceiver.ACTION_SNOOZE
        val intent = Intent(context, ReminderReceiver::class.java)
            .setAction(receiverAction).setData(Uri.parse("petping://reminder/$id/$kind"))
            .putExtra(ReminderReceiver.EXTRA_ID, id)
        val pending = PendingIntent.getBroadcast(context, 0, intent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE)
        return Notification.Action.Builder(null, label, pending).build()
    }

    fun post(context: Context, reminder: Reminder) {
        if (Build.VERSION.SDK_INT >= 33 && context.checkSelfPermission(
                Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED) return
        channel(context)
        val pet = context.getSharedPreferences("pet", Context.MODE_PRIVATE)
            .getString("kind", PetKind.CAT.name) ?: PetKind.CAT.name
        val name = when (pet) {
            PetKind.DOG.name -> "Buddy"
            PetKind.BUNNY.name -> "Luna"
            else -> "Milo"
        }
        val open = PendingIntent.getActivity(context, 0,
            Intent(context, MainActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP),
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE)
        val notification = Notification.Builder(context, CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setContentTitle("$name has a reminder for you")
            .setContentText(reminder.title)
            .setStyle(Notification.BigTextStyle().bigText(reminder.title))
            .setCategory(Notification.CATEGORY_REMINDER)
            .setPriority(Notification.PRIORITY_HIGH)
            .setContentIntent(open)
            .setAutoCancel(false)
            .setOnlyAlertOnce(true)
            .addAction(action(context, reminder.id, "done", "Done"))
            .addAction(action(context, reminder.id, "snooze", "Snooze 10m"))
            .build()
        context.getSystemService(NotificationManager::class.java)
            .notify(notificationId(reminder.id), notification)
    }
}

package com.petping.android.data

import android.content.Context
import android.util.AtomicFile
import com.petping.android.model.Reminder
import com.petping.android.model.ReminderStatus
import org.json.JSONArray
import org.json.JSONObject
import java.io.File
import java.io.FileNotFoundException

/** Atomic offline JSON; refuse corrupt data rather than overwriting it. */
class ReminderStore(context: Context) {
    private val file = AtomicFile(File(context.applicationContext.filesDir, "reminders-v1.json"))

    @Synchronized
    fun all(): List<Reminder> {
        val json = try { file.openRead().bufferedReader(Charsets.UTF_8).use { it.readText() } }
                   catch (_: FileNotFoundException) { return emptyList() }
        val root = JSONObject(json)
        check(root.getInt("version") == 1) { "Unsupported reminder format. Original saved data preserved." }
        val rows = root.getJSONArray("reminders")
        val reminders = (0 until rows.length()).map { i ->
            val row = rows.getJSONObject(i)
            Reminder(
                id = row.getString("id"),
                title = row.getString("title"),
                dueAtMillis = row.getLong("dueAtMillis"),
                status = ReminderStatus.valueOf(row.getString("status")),
                snoozeCount = row.getInt("snoozeCount"),
                deliveredAtMillis = if (row.isNull("deliveredAtMillis")) null
                    else row.getLong("deliveredAtMillis")
            )
        }
        check(reminders.map { it.id }.distinct().size == reminders.size) {
            "Duplicate saved reminders; original data preserved."
        }
        return reminders
    }

    @Synchronized
    fun change(transform: (List<Reminder>) -> List<Reminder>): List<Reminder> {
        val next = transform(all())
        require(next.map { it.id }.distinct().size == next.size)
        require(next.all { it.title.isNotBlank() && it.title.length <= 140 && it.dueAtMillis > 0 })
        val json = JSONObject().put("version", 1).put("reminders", JSONArray().also { array ->
            next.forEach { r -> array.put(JSONObject()
                .put("id", r.id).put("title", r.title).put("dueAtMillis", r.dueAtMillis)
                .put("status", r.status.name).put("snoozeCount", r.snoozeCount)
                .put("deliveredAtMillis", r.deliveredAtMillis ?: JSONObject.NULL)) }
        }).toString()
        val stream = file.startWrite()
        try {
            stream.write(json.toByteArray(Charsets.UTF_8))
            file.finishWrite(stream)
        } catch (error: Exception) {
            file.failWrite(stream)
            throw error
        }
        return next
    }
}

package com.petping.android

import android.Manifest
import android.app.AlarmManager
import android.app.DatePickerDialog
import android.app.TimePickerDialog
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.os.Build
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.petping.android.data.ReminderStore
import com.petping.android.model.PetKind
import com.petping.android.model.Reminder
import com.petping.android.model.ReminderStatus
import com.petping.android.reminders.ReminderEngine
import com.petping.android.reminders.ReminderNotifications
import com.petping.android.reminders.ReminderScheduler
import java.text.DateFormat
import java.util.Calendar

private val ink = Color(0xFF24212C)
private val surface = Color(0xFFF9F5F0)
private val lilac = Color(0xFF6B4B9E)

class MainActivity : ComponentActivity() {
    private val repo by lazy { ReminderStore(this) }
    private var reminders by mutableStateOf<List<Reminder>>(emptyList())
    private var pet by mutableStateOf(PetKind.CAT)
    private var permissionStatus by mutableStateOf("")
    private var error by mutableStateOf<String?>(null)
    private val askNotifications = registerForActivityResult(
        ActivityResultContracts.RequestPermission()) { refresh() }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        pet = runCatching { PetKind.valueOf(getSharedPreferences("pet", MODE_PRIVATE)
            .getString("kind", "CAT") ?: "CAT") }.getOrDefault(PetKind.CAT)
        refresh()
        setContent {
            MaterialTheme(colorScheme = lightColorScheme(
                primary = lilac, background = surface, surface = Color.White,
                onBackground = ink, onSurface = ink)) {
                PetPingScreen(
                    reminders = reminders, selected = pet, permissions = permissionStatus,
                    error = error, onPet = { selectPet(it) }, onSave = { title -> pickDateTime(title) },
                    onNotifications = { requestNotificationPermission() },
                    onExact = { requestExactAccess() },
                    onDone = { id -> changeReminder(id, false) },
                    onSnooze = { id -> changeReminder(id, true) },
                    onDelete = { id -> deleteReminder(id) }
                )
            }
        }
    }

    override fun onResume() {
        super.onResume()
        refresh()
        runCatching { ReminderScheduler.reschedule(this) }
    }

    private fun refresh() {
        runCatching {
            reminders = repo.all().sortedBy { it.dueAtMillis }
            val nm = getSystemService(android.app.NotificationManager::class.java)
            val notifications = Build.VERSION.SDK_INT < 33 ||
                checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS) == PackageManager.PERMISSION_GRANTED
            val exact = ReminderScheduler.hasExactAccess(this)
            permissionStatus = "Notifications: ${if (notifications && nm.areNotificationsEnabled()) "on" else "off"}" +
                "   •   Timing: ${if (exact) "precise" else "approximate"}"
            error = null
        }.onFailure { error = "Saved data needs attention: ${it.message}. PetPing will not overwrite it." }
    }

    private fun selectPet(kind: PetKind) {
        pet = kind
        getSharedPreferences("pet", MODE_PRIVATE).edit().putString("kind", kind.name).apply()
    }

    private fun pickDateTime(title: String) {
        val trimmed = title.trim()
        if (trimmed.isBlank() || trimmed.length > 140) {
            error = "Give your reminder a short title (up to 140 characters)."; return
        }
        val now = Calendar.getInstance()
        DatePickerDialog(this, { _, year, month, day ->
            TimePickerDialog(this, { _, hour, minute ->
                val calendar = Calendar.getInstance().apply {
                    set(Calendar.YEAR, year); set(Calendar.MONTH, month)
                    set(Calendar.DAY_OF_MONTH, day); set(Calendar.HOUR_OF_DAY, hour)
                    set(Calendar.MINUTE, minute); set(Calendar.SECOND, 0)
                    set(Calendar.MILLISECOND, 0)
                }
                val due = calendar.timeInMillis
                if (due <= System.currentTimeMillis() + 2000L) {
                    error = "Please choose a future date and time."
                } else {
                    runCatching {
                        repo.change { it + Reminder(title = trimmed, dueAtMillis = due) }
                        ReminderScheduler.reschedule(this)
                        refresh()
                    }.onFailure { error = "Could not save reminder: ${it.message}" }
                }
            }, now.get(Calendar.HOUR_OF_DAY), now.get(Calendar.MINUTE), false).show()
        }, now.get(Calendar.YEAR), now.get(Calendar.MONTH), now.get(Calendar.DAY_OF_MONTH)).apply {
            datePicker.minDate = System.currentTimeMillis() - 1000L
        }.show()
    }

    private fun changeReminder(id: String, snooze: Boolean) {
        runCatching { ReminderEngine.act(this, id, snooze); refresh() }
            .onFailure { error = "Couldn't update reminder: ${it.message}" }
    }

    private fun deleteReminder(id: String) {
        runCatching {
            repo.change { rows -> rows.filterNot { it.id == id } }
            getSystemService(android.app.NotificationManager::class.java)
                .cancel(ReminderNotifications.notificationId(id))
            ReminderScheduler.reschedule(this)
            refresh()
        }.onFailure { error = "Couldn't delete reminder: ${it.message}" }
    }

    private fun requestNotificationPermission() {
        if (Build.VERSION.SDK_INT >= 33 &&
            checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED)
            askNotifications.launch(Manifest.permission.POST_NOTIFICATIONS)
        else refresh()
    }

    private fun requestExactAccess() {
        if (Build.VERSION.SDK_INT >= 31 && !ReminderScheduler.hasExactAccess(this)) {
            runCatching {
                startActivity(Intent(AlarmManager.ACTION_REQUEST_SCHEDULE_EXACT_ALARM,
                    Uri.parse("package:$packageName")))
            }.onFailure { error = "Find Alarms & reminders in Android Settings for PetPing." }
        } else refresh()
    }
}

@Composable
private fun PetPingScreen(reminders: List<Reminder>, selected: PetKind, permissions: String,
    error: String?, onPet: (PetKind) -> Unit, onSave: (String) -> Unit,
    onNotifications: () -> Unit, onExact: () -> Unit,
    onDone: (String) -> Unit, onSnooze: (String) -> Unit, onDelete: (String) -> Unit) {
    var title by remember { mutableStateOf("") }
    Column(Modifier.fillMaxSize().background(surface).verticalScroll(rememberScrollState())
        .padding(horizontal = 20.dp, vertical = 22.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        Text("PetPing", style = MaterialTheme.typography.headlineLarge,
            fontWeight = FontWeight.Bold, color = ink)
        Text("Your little reminder companion", color = Color(0xFF726879))
        Card(shape = RoundedCornerShape(24.dp), colors = CardDefaults.cardColors(containerColor = Color(0xFFF1E9F8))) {
            Column(Modifier.fillMaxWidth().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                Text("Choose your companion", fontWeight = FontWeight.SemiBold)
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    listOf(Triple(PetKind.CAT, "🐱", "Cat"), Triple(PetKind.DOG, "🐶", "Dog"),
                        Triple(PetKind.BUNNY, "🐰", "Bunny")).forEach { (kind, face, label) ->
                        FilterChip(selected = selected == kind, onClick = { onPet(kind) },
                            label = { Text("$face $label") })
                    }
                }
                Text(when (selected) { PetKind.CAT -> "Milo is keeping an eye on your day."
                    PetKind.DOG -> "Buddy is ready when you are."
                    PetKind.BUNNY -> "Luna is here for gentle nudges." }, color = ink)
                Text("Static companions only. No walking or intrusive screen overlays.",
                    style = MaterialTheme.typography.labelMedium, color = Color(0xFF776B80))
            }
        }
        Card(shape = RoundedCornerShape(22.dp)) {
            Column(Modifier.padding(18.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                Text("New reminder", style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold)
                OutlinedTextField(title, onValueChange = { title = it.take(140) }, singleLine = true,
                    label = { Text("What should I remind you?") }, modifier = Modifier.fillMaxWidth())
                Button(onClick = { onSave(title) }, enabled = title.isNotBlank(),
                    modifier = Modifier.fillMaxWidth()) { Text("Choose date & time") }
            }
        }
        Card(shape = RoundedCornerShape(18.dp)) {
            Column(Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Text("Reminder permissions", fontWeight = FontWeight.SemiBold)
                Text(permissions, style = MaterialTheme.typography.bodySmall)
                Text("Precise alarms need your approval. Without it, Android may deliver reminders later.",
                    style = MaterialTheme.typography.bodySmall, color = Color(0xFF6E6670))
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedButton(onClick = onNotifications) { Text("Notifications") }
                    OutlinedButton(onClick = onExact) { Text("Precise alarms") }
                }
            }
        }
        if (error != null) Text(error, color = MaterialTheme.colorScheme.error,
            style = MaterialTheme.typography.bodyMedium)
        Text("Your reminders", style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold)
        if (reminders.isEmpty()) Text("No reminders yet. Add your first one above.", color = Color.Gray)
        reminders.forEach { reminder ->
            Card(shape = RoundedCornerShape(18.dp), modifier = Modifier.fillMaxWidth()) {
                Column(Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(7.dp)) {
                    Text(reminder.title, fontWeight = FontWeight.SemiBold, maxLines = 2,
                        overflow = TextOverflow.Ellipsis)
                    Text(DateFormat.getDateTimeInstance(DateFormat.MEDIUM, DateFormat.SHORT)
                        .format(java.util.Date(reminder.dueAtMillis)),
                        style = MaterialTheme.typography.bodySmall)
                    val label = when(reminder.status) {
                        ReminderStatus.PENDING -> if (reminder.snoozeCount > 0) "Snoozed" else "Upcoming"
                        ReminderStatus.DELIVERED -> "Needs your attention"
                        ReminderStatus.MISSED -> "Missed while unavailable"
                        ReminderStatus.COMPLETED -> "Done"
                    }
                    Text(label, color = lilac, style = MaterialTheme.typography.labelMedium)
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        if (reminder.status in listOf(ReminderStatus.DELIVERED, ReminderStatus.MISSED)) {
                            Button(onClick = { onDone(reminder.id) }) { Text("Done") }
                            OutlinedButton(onClick = { onSnooze(reminder.id) }) { Text("Snooze 10m") }
                        }
                        TextButton(onClick = { onDelete(reminder.id) }) { Text("Remove") }
                    }
                }
            }
        }
        Text("Android alpha · Offline by design · No account required",
            color = Color(0xFF8A8290), style = MaterialTheme.typography.labelSmall)
        Spacer(Modifier.height(24.dp))
    }
}

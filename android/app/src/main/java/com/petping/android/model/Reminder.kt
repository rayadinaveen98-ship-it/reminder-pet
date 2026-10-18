package com.petping.android.model
import java.util.UUID
enum class ReminderStatus { PENDING, DELIVERED, COMPLETED, MISSED }
enum class PetKind { CAT, DOG, BUNNY }
data class Reminder(
    val id: String = UUID.randomUUID().toString(),
    val title: String,
    val dueAtMillis: Long,
    val status: ReminderStatus = ReminderStatus.PENDING,
    val snoozeCount: Int = 0,
    val deliveredAtMillis: Long? = null
)

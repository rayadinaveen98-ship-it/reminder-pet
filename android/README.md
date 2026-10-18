# PetPing Android v0.1 (native alpha)

Open this `android/` directory in Android Studio with Android SDK 36 and Gradle 8.13. Or:

```bash
gradle -p android :app:testDebugUnitTest :app:assembleDebug
```

Debug APK output: `android/app/build/outputs/apk/debug/app-debug.apk`. Alternatively download the `PetPing-Android-v0.1-debug-apk` from the **Android Alpha APK** GitHub Actions run when the workflow succeeds.

**Implemented:** offline one-time reminders, Cat/Dog/Bunny selector (emoji placeholders), local Android notification with Done/Snooze, permission requests, reboot recovery, atomic JSON storage. **No walking or intrusive overlay.**

**Not yet implemented:** imported approved premium cat/dog/bunny artwork, custom recurrence, automatic background speech, aggressive escalation, full-screen pet UI, real phone acceptance or Play release. Details: `../docs/ANDROID_V01_FOUNDATION.md`.

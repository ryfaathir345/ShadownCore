# Tweak List — Kategori Windows

Format: **Nama** — Deskripsi — `Path/Value` — Risk Level

---

1. **Disable Visual Effects/Animations** — Klasik tapi masih efektif, terutama di sistem RAM/CPU terbatas. Windows > Advanced System Settings > Performance → Adjust for best performance (atau custom pilih beberapa efek tetap on). — `HKCU\Control Panel\Desktop\WindowMetrics` + beberapa key terkait — Safe
2. **MenuShowDelay Reduction** — Mengurangi delay sebelum menu muncul (default 400ms, bisa ke 0). — `HKCU\Control Panel\Desktop` → `MenuShowDelay=0` — Safe *(sudah ada di tweak-list-from-user-packs.md)*
3. **AutoEndTasks & Timeout Reduction** — Mempercepat Windows dalam mendeteksi app "not responding" dan auto-close saat shutdown. — `HKCU\Control Panel\Desktop` → `AutoEndTasks=1`, `HungAppTimeout=1000`, `WaitToKillAppTimeout=2000` — Safe *(sudah ada di tweak-list-from-user-packs.md)*
4. **Disable Background Apps (Global)** — Cegah semua UWP app jalan di background tanpa dibuka user. — `HKCU\...\BackgroundAccessApplications` → `GlobalUserDisabled=1` — Safe *(sudah ada di tweak-list-from-user-packs.md)*
5. **Disable Microsoft Edge Pre-Launching** — Edge pre-load di background saat boot; bisa dimatikan kalau tidak dipakai. — `HKLM\SOFTWARE\Wow6432Node\Policies\Microsoft\MicrosoftEdge\Main` → `AllowPrelaunch=0` — Safe *(sudah ada di tweak-list-from-user-packs.md)*
6. **Disable Windows Ads/Suggestions** — Matikan iklan/saran app di Start Menu & lockscreen. — `HKCU\...\ContentDeliveryManager` (beberapa key) — Safe *(sudah ada di tweak-list-from-user-packs.md)*
7. **Disable Windows Search Indexing (selektif per drive)** — Search Indexing berguna untuk pencarian cepat tapi makan disk I/O di background terus-menerus. Bisa dimatikan per-drive (misal drive game) tanpa matikan search sepenuhnya. — Services.msc: `WSearch`, atau per-drive di Indexing Options — Moderate (search jadi lebih lambat)
8. **Disable Game Bar & Game DVR** — Xbox Game Bar dan background recording (Game DVR) konsumsi resource walau tidak dipakai aktif, kadang juga nyebabin overlay conflict dengan game tertentu. — `HKCU\System\GameConfigStore` → `GameDVR_Enabled=0` + Settings > Gaming > Xbox Game Bar Off — Safe
9. **Enable Verbose Boot/Shutdown Status Messages** — Bukan tweak performa murni, tapi berguna untuk diagnosa kalau boot/shutdown lambat (kelihatan lagi stuck di step mana). — `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System` → `VerboseStatus=1` — Safe
10. **Disable Windows Copilot / AI Features** — Fitur AI bawaan Windows 11 2026 (Copilot, Recall, dst) berjalan sebagai proses background yang lumayan berat di beberapa sistem, terutama non-Copilot+ PC yang menjalankannya secara software-only. — `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot` → `TurnOffWindowsCopilot=1` — Safe
11. **Disable Notification Center Sync/Toast Overhead** — Mengurangi background sync notifikasi cross-device (Phone Link, dsb) kalau tidak dipakai. — Settings > Notifications, beberapa key `PushNotifications` — Safe
12. **Disable Widgets Panel** — Widgets (cuaca, berita di taskbar) berjalan sebagai proses terpisah yang selalu fetch data di background. — `HKLM\SOFTWARE\Policies\Microsoft\Dsh` → `AllowNewsAndInterests=0`, atau lewat Settings > Personalization > Taskbar — Safe
13. **Disable Fast Startup (situational)** — Fast Startup (hybrid shutdown) mempercepat boot tapi kadang menyebabkan masalah dengan dual-boot, driver update, atau device tertentu yang butuh cold boot penuh. — Control Panel > Power Options > Choose what power buttons do → uncheck Fast Startup — Safe, situational (trade-off waktu boot)
14. **Group Policy: Disable Telemetry/Diagnostic Data (Enterprise/Pro only)** — Menurunkan level telemetry ke minimum, mengurangi background data collection process. Hanya tersedia penuh di edisi Pro/Enterprise (gpedit). — `gpedit.msc` > Computer Config > Admin Templates > Windows Components > Data Collection — Safe (Home edition perlu lewat registry langsung)

## Catatan Validasi
- No. 10 (Copilot/AI features) relevan banget untuk 2026 karena makin banyak Windows 11 build dengan AI feature aktif default — worth jadi fitur unggulan Debloat/Windows section.
- No. 7 dan 13 tandai sebagai "situational" — bukan yang di-recommend default ON, karena ada trade-off fungsional (bukan cuma performa) yang perlu user sadari.

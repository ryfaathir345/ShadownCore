# Tweak List — Kategori Boot & Power

Format: **Nama** — Deskripsi — Aksi — Risk Level

---

1. **Ultimate Performance Power Plan** — *(overlap dengan CPU category)* — CMD powercfg — Safe
2. **Startup Delay Removal** — Windows sengaja delay ~10 detik sebelum menjalankan startup apps, untuk kasih waktu boot process inti selesai dulu. Bisa di-set ke 0 supaya startup app langsung jalan. — `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Serialize` → `StartupDelayInMSec=0` — Safe *(sudah ada di tweak-list-from-user-packs.md)*
3. **Disable Fast Startup** — *(overlap dengan Windows category)* — Control Panel Power Options — Safe, situational
4. **Startup App Manager (bukan registry, tapi UI feature)** — Task Manager > Startup sudah ada bawaan Windows, tapi app kita bisa kasih versi lebih informatif: startup impact score, kategori (perlu vs tidak), dan history "sejak kapan app ini nambah ke startup". — Task Manager API / registry Run keys — Safe (fitur UI, bukan tweak destructive)
5. **Reduce Boot Menu Timeout (BCD)** — Kalau dual-boot atau ada recovery menu, default timeout 30 detik bisa dipangkas ke 5-10 detik biar boot lebih cepat kalau cuma 1 OS. — `bcdedit /timeout 5` — Safe
6. **Disable Verbose/Detailed Boot Logging (kalau aktif)** — Kebalikan dari tweak diagnostic di kategori Windows — kalau logging boot detail nggak dibutuhkan, matikan untuk sedikit percepat proses boot. — `bcdedit /set {current} bootlog no` — Safe
7. **Disable Hibernation** — *(overlap dengan Storage category)* — `powercfg /hibernate off` — Moderate
8. **USB Selective Suspend Setting** — Power saving untuk port USB yang kadang menyebabkan device (mouse gaming, dsb) delay saat wake dari idle. Disable untuk device tertentu (bukan global) demi responsiveness. — Device Manager > USB Root Hub > Power Management, atau Power Plan Advanced Settings — Safe, per-device
9. **Disable "Turn off hard disk after X minutes"** — Set ke "Never" biar drive tidak spin-down/sleep saat idle, menghindari delay saat drive perlu "wake up" lagi. Lebih relevan untuk HDD, kurang relevan untuk SSD/NVMe modern. — Power Options > Advanced settings > Hard disk — Safe
10. **Fast Boot via BIOS (Guidance)** — Setting BIOS-level yang skip beberapa POST check untuk booting lebih cepat. App cuma kasih instruksi karena ini di luar jangkauan software. — BIOS-level — Guidance only, Safe (trade-off: skip beberapa hardware check saat POST)

## Catatan Validasi
- Kategori ini paling banyak overlap dengan CPU (power plan) dan Windows (fast startup) — pertimbangkan apakah "Boot & Power" perlu jadi kategori sendiri atau digabung jadi sub-section di kategori lain, biar user nggak bingung nyari tweak yang sama di 2 tempat beda.

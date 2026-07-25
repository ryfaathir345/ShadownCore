# Tweak List — Kategori RAM

Format: **Nama** — Deskripsi — `Path/Value` — Risk Level

---

1. **SvcHostSplitThresholdInKB (size-aware)** — Atur threshold RAM sebelum Windows memisahkan svchost.exe jadi proses individual. RAM lebih besar = threshold lebih tinggi = overhead lebih kecil. App bisa auto-detect RAM lalu isi value yang sesuai. — `HKLM\SYSTEM\CurrentControlSet\Control` → `SvcHostSplitThresholdInKB` (KB, mis. 16GB=16777216) — Safe *(sudah ada di tweak-list-from-user-packs.md)*
2. **Disable Superfetch/SysMain** — Mematikan service yang pre-load app yang sering dipakai ke RAM. Bisa membantu di sistem dengan SSD cepat (RAM sudah cepat diisi ulang), tapi kontroversial — sebagian sistem malah lebih lambat. — Services.msc: `SysMain` → Disabled — Moderate, situational (jangan default ON)
3. **Disable Memory Compression** — Mematikan fitur kompresi RAM Windows. Membantu di CPU kuat dengan RAM besar, trade-off pemakaian RAM mentah lebih tinggi. — PowerShell: `Disable-MMAgent -MemoryCompression` — Moderate *(sudah ada di tweak-list-from-user-packs.md)*
4. **Clear Page File at Shutdown** — Menghapus page file setiap shutdown untuk keamanan (mencegah data sensitif tersisa), tapi menambah waktu shutdown. Lebih ke opsi keamanan daripada performa murni. — `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` → `ClearPageFileAtShutdown=1` — Safe (tapi shutdown lebih lambat)
5. **Custom Page File Size (Fixed, bukan System Managed)** — Set ukuran page file tetap (misal 1.5x RAM) alih-alih dikelola otomatis Windows, mengurangi fragmentasi dan overhead resizing dinamis. Kurang relevan kalau RAM sudah 32GB+. — System Properties > Advanced > Performance Settings > Advanced > Virtual Memory — Moderate (nilai keliru bisa sebabkan masalah stabilitas)
6. **Prioritas RAM untuk Foreground Program** — Windows secara default membagi cache RAM antara program dan system cache; bisa dipaksa prioritas ke foreground program. Terkait dengan `Win32PrioritySeparation` di kategori CPU. — `HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl` (overlap dengan CPU tweak) — Moderate
7. **Disable ReadyBoost jika pakai SSD** — ReadyBoost (pakai flashdisk sebagai cache RAM tambahan) hanya berguna untuk HDD lambat; di SSD modern nggak ada manfaat dan cuma nambah proses background. — Services.msc: `ReadyBoost` service — Safe
8. **Standby Memory List Cleaning** — Windows menyimpan "standby list" (RAM yang dipakai app yang udah ditutup, tapi belum dilepas beneran) untuk mempercepat re-open. Kadang bikin RAM "used" kelihatan tinggi padahal bisa dipakai ulang. App bisa kasih tombol manual "Clear Standby List" tanpa perlu tool eksternal. — API: `EmptyStandbyList` (via NtSetSystemInformation atau tool seperti RAMMap) — Safe (aman, RAM standby memang didesain untuk dilepas kapan saja)
9. **XMP/EXPO Enable (Guidance)** — RAM sering jalan di speed default JEDEC (lebih lambat dari rated speed) sampai XMP (Intel)/EXPO (AMD) diaktifkan di BIOS. App hanya bisa deteksi speed aktual vs rated speed lalu kasih instruksi ke BIOS — bukan toggle software. — BIOS-level — Guidance only, Safe
10. **Disable Memory Integrity (Core Isolation)** — (Overlap dengan kategori CPU/Security) Klaim recover 10-15% performa di beberapa game, tapi mengorbankan proteksi VBS. Ditaruh di sini juga karena berkaitan erat dengan manajemen memori virtual. — Settings > Windows Security > Device Security — Advanced, warning keamanan eksplisit

## Catatan Validasi
- No. 2 (Superfetch/SysMain) — jangan dijadikan tweak default "recommended", karena banyak laporan berlawanan (ada yang lebih cepat, ada yang lebih lambat). Cocok jadi fitur A/B test dengan disclaimer jelas.
- No. 8 (Standby List Cleaning) — ini aman tapi sering disalahpahami sebagai "membersihkan RAM kotor" padahal itu justru fitur yang menguntungkan; deskripsi di app harus jelas biar user nggak parno liat RAM "penuh".

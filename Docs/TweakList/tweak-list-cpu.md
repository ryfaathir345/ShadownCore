# Tweak List — Kategori CPU

Status: Draft riset awal dari berbagai sumber komunitas (forum tweaking, blog, YouTube, Reddit-style threads). Perlu validasi ulang sebelum implementasi — beberapa efeknya kontroversial/tergantung hardware.

Format tiap entry: **Nama** — Deskripsi — `Path/Value` (kalau ada) — Risk Level

---

## A. Power Plan & Clock Management

1. **Ultimate Performance Power Plan** — Power plan tersembunyi Windows, menghilangkan batas minimum processor state dan disable core parking otomatis. Klaim komunitas: 5-10% FPS lebih baik di skenario CPU-bound. — CMD: `powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61` — Safe (laptop: matikan lagi saat pakai baterai, boros daya)
2. **Disable Core Parking** — Mencegah Windows "park" (nonaktifkan sementara) core yang idle. Bagus untuk desktop gaming yang selalu colok listrik, kurang cocok untuk laptop. — `HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583` → `ValueMax=0`, plus set core parking min/max ke 100% di power plan aktif — Moderate (idle power naik, cek suhu di laptop)
3. **Processor Performance Increase/Decrease Threshold** — Mempercepat respons CPU saat naik/turun clock speed (default Windows agak lambat/malas naik). — Sama key family di atas, subkey `06cadf0e...` dan `12a0ab44...` → `Attributes=0` (untuk expose opsi di Advanced Power Settings) — Moderate
4. **C-States Limit (via BIOS, bukan software)** — Membatasi deep sleep state CPU (C3/C6) supaya core lebih responsif, trade-off ke power draw & suhu. App cuma bisa kasih instruksi karena ini BIOS-level. — BIOS setting — Guidance only, Advanced
5. **Disable Processor Idle States sepenuhnya (Advanced)** — Extreme version dari core parking disable — paksa semua core di C0 (fully active). Meningkatkan strain & konsumsi daya signifikan, cuma worth dicoba di desktop dengan cooling bagus. — Sama registry family, `ValueMax=0` di semua subkey idle state — Advanced (bukan buat semua orang)

## B. Scheduling & Priority

6. **Win32PrioritySeparation** — Mengatur bagaimana Windows membagi CPU time slice antara foreground vs background app. Nilai 26 (0x1a) umum dipakai untuk gaming (short quantum, prioritas foreground tinggi, fixed). — `HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl` → `Win32PrioritySeparation` (DWORD) = 26 — Moderate
7. **Games Task Priority Profile (lengkap)** — Selain GPU Priority yang udah ada di kategori GPU, task profile "Games" juga punya field CPU-related: `Priority`, `Affinity`, `Background Only`, `Clock Rate`. — `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` — Moderate
8. **System Responsiveness** — (Overlap dengan kategori GPU) Turunkan reservasi CPU untuk background task. Community masih perdebatan soal nilai optimal — 10 (aman) vs 0 (agresif, bisa ganggu audio driver). — `...\Multimedia\SystemProfile` → `SystemResponsiveness` = 10 (moderate) atau 0 (agresif) — Moderate, jadikan slider bukan toggle biner
9. **csrss.exe Priority Boost** — Menaikkan prioritas proses inti Windows (Client/Server Runtime Subsystem). Proses sistem kritikal, resiko kalau app salah setting. — `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions` → `CpuPriorityClass=3` — Advanced (proses sistem kritikal)
10. **CPU Affinity untuk Multi-CCD Ryzen** — Set game process supaya hanya jalan di CCD dengan cache terbanyak (relevan untuk CPU seperti 7950X3D dengan 2 CCD beda karakteristik). **Hanya berguna untuk CPU multi-CCD** — single CCD (5600X, 7600X, 7800X3D) tidak dapat manfaat, jadi app perlu deteksi model CPU dulu. — Per-app affinity via Task Manager atau Process Lasso API — Advanced, CPU-specific

## C. SMT / Hyper-Threading

11. **Disable SMT/Hyper-Threading (situational)** — Mematikan multi-threading per core bisa mengurangi latency/scheduling overhead di game esports kompetitif (Valorant, CS2, Apex), tapi merugikan performa multi-threaded umum. **Perlu warning jelas** karena ini trade-off besar, bukan tweak yang "aman default". — BIOS-level (kadang ada override registry tapi tidak reliable) — Advanced, Guidance (BIOS)

## D. Background Interference

12. **Power Throttling per-App Disable** — Windows "power throttling" (EcoQoS) bisa membatasi CPU time untuk app yang dianggap background meski sedang aktif dipakai. Bisa dikecualikan per-app. — `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Power\PowerThrottling` (per-app melalui Task Manager > Details > Efficiency Mode override, atau registry `PowerThrottlingOff`) — Safe
13. **Disable Intel IME Service Overhead** — Intel Management Engine Interface adalah background service yang selalu jalan, konsumsi sedikit CPU cycle. Disable hanya dari BIOS, bukan dari software (menghapus service Windows-nya saja tidak menghentikan IME firmware-level). — BIOS-level — Guidance only, Advanced (bisa ganggu fitur keamanan platform)
14. **Intel Turbo Boost Max Technology 3.0 Driver** — Untuk CPU Intel i9 (dan sebagian i7) yang mendukung, driver ini mengidentifikasi core tercepat dan prioritaskan single-thread workload ke situ. — Instalasi driver terpisah dari Intel, bukan registry tweak — Guidance, Safe

## E. Security Mitigations (Trade-off Performance vs Keamanan)

15. **Disable Spectre/Meltdown Mitigations** — Mitigasi CPU vulnerability (Spectre/Meltdown/dst) menurunkan performa 5-15% tergantung workload. Disable meningkatkan performa tapi membuka celah keamanan — **wajib ada disclaimer keras & konfirmasi eksplisit**, tidak cocok untuk PC yang browsing/handle data sensitif. — `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` → `FeatureSettingsOverride` & `FeatureSettingsOverrideMask` (nilai spesifik CVE-dependent) — Advanced, butuh warning keamanan eksplisit
16. **Disable Memory Integrity (Core Isolation / HVCI)** — Fitur keamanan virtualization-based security. Community report klaim recover 10-15% FPS di beberapa game saat dimatikan, tapi mengorbankan proteksi terhadap kernel-level malware. — Settings > Windows Security > Device Security > Core Isolation, atau `HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity` → `Enabled=0` — Advanced, butuh warning keamanan eksplisit

## F. Monitoring & Overclocking (Guidance-only, bukan auto-apply)

17. **CPU Overclock Guidance** — App tidak melakukan overclock otomatis (risiko tinggi & sangat hardware-spesifik), tapi bisa kasih link/instruksi ke tool resmi vendor (Intel XTU, AMD Ryzen Master, atau BIOS). — Guidance only — N/A (guidance)
18. **Rekomendasi Tool Monitoring Eksternal** — Untuk user yang mau lihat detail lebih dalam dari yang disediakan Dashboard bawaan, kasih rekomendasi HWiNFO64 (lebih detail dari LibreHardwareMonitor untuk sensor tertentu). — Guidance only, link keluar app — N/A (guidance)

---

## Catatan Validasi
- Tweak No. 8 (SystemResponsiveness) dan No. 6 (Win32PrioritySeparation) adalah yang paling sering direkomendasikan lintas sumber (Windows Forum, XDA, Medium, OptiLag) — confidence tinggi tapi tetap butuh disclaimer "trade-off ke background task".
- No. 15 dan 16 (security mitigations) — ini **kategori paling sensitif**. Rekomendasi: taruh di sub-section terpisah "Security Trade-offs" dengan warning banner merah, bukan dicampur tweak biasa, karena dampaknya ke keamanan bukan cuma performa.
- No. 10 (CPU Affinity multi-CCD) butuh app mendeteksi model CPU spesifik dulu sebelum nampilin opsi ini — kalau CPU single-CCD, sembunyikan tweak ini sama sekali (bukan cuma disable button).
- No. 3, 5 (deep power/idle state tweaks) sumbernya kebanyakan forum lama (2015-2023) yang di-republish ulang di 2026 — mekanismenya masih valid di Windows 11 tapi keyword pencariannya banyak "reused content", perlu extra hati-hati validasi behavior di W11 24H2 terbaru.

## Belum tercakup / bisa digali lebih lanjut
- Tweak spesifik untuk CPU hybrid Intel (P-core/E-core scheduling override, Thread Director tuning)
- BIOS-level power limit (PL1/PL2) tuning guidance untuk laptop vs desktop
- AMD-specific: PBO (Precision Boost Overdrive) guidance

# Tweak List — Kategori GPU

Status: Draft riset awal. Setiap tweak perlu divalidasi ulang di Windows 11 versi terbaru sebelum diimplementasi ke app (beberapa tweak lama sudah di-patch Microsoft/vendor).

Format tiap entry: **Nama** — Deskripsi — `Path/Value` (kalau ada) — Risk Level

---

## A. General (Semua Merk GPU)

1. **Hardware-Accelerated GPU Scheduling (HAGS)** — Memindahkan manajemen memori GPU dari CPU driver ke GPU scheduler, mengurangi latency sisi CPU. Kadang mengganggu software capture/streaming. — Settings > Display > Graphics > HAGS, atau registry `HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\HwSchMode` — Moderate
2. **Disable Fullscreen Optimizations (per-app)** — Memaksa game pakai exclusive fullscreen asli, mengurangi input lag di beberapa game. — Properties app > Compatibility — Safe
3. **GPU Priority untuk Games** — Menaikkan prioritas alokasi GPU untuk proses game. — `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` → `GPU Priority = 8` — Moderate
4. **Scheduling Category untuk Games** — Set task scheduling category game ke High. — key sama seperti di atas → `Scheduling Category = High`, `SFIO Priority = High` — Moderate
5. **System Responsiveness** — Menurunkan alokasi CPU untuk background task saat multimedia/game jalan (nilai valid: 0–10). — `...\Multimedia\SystemProfile` → `SystemResponsiveness = 0000000A` (10) — Moderate
6. **Network Throttling Index Disable** — Menghilangkan throttling bandwidth reservasi untuk proses multimedia. — key sama → `NetworkThrottlingIndex = ffffffff` — Moderate
7. **Set Default High-Performance GPU secara eksplisit (laptop hybrid GPU)** — Pastikan Windows pilih dGPU bukan iGPU untuk game, karena kadang salah deteksi terutama di setup Intel Arc + NVIDIA. — Settings > Display > Graphics > pilih app > High Performance — Safe

## B. NVIDIA

8. **Power Management Mode → Prefer Maximum Performance** — Menghilangkan delay boost clock GPU di awal scene berat, penting untuk 1% lows. — NVIDIA Control Panel > Manage 3D Settings — Safe
9. **Low Latency Mode: Ultra** — Membatasi pre-rendered frames jadi 1, mengurangi input lag dengan trade-off efisiensi GPU sedikit. — NVIDIA Control Panel — Safe
10. **Shader Cache Size: 10 GB** — Mencegah stutter re-compile shader di game open-world besar. — NVIDIA Control Panel — Safe
11. **Texture Filtering Quality: Performance** — Sedikit menurunkan kualitas visual, gain FPS terukur. — NVIDIA Control Panel — Safe
12. **Threaded Optimization: On** — Memungkinkan banyak thread CPU menangani draw call secara paralel. — NVIDIA Control Panel — Safe
13. **Disable NVIDIA Telemetry Service** — Mematikan service telemetry NVIDIA yang berjalan di background. — Services.msc: `NvTelemetryContainer` — Safe
13b. **Disable Dynamic P-State (Desktop only)** — Mengunci GPU di P-State performa maksimum, mencegah down-clock dinamis. HANYA untuk desktop PC (bukan laptop) dan wajib pastikan suhu GPU aman. — `HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\000X` → `DisableDynamicPstate` (DWORD) = 1 — Advanced (risiko overheating kalau cooling kurang)

## C. AMD

14. **Radeon Anti-Lag / Anti-Lag 2: On** — Mengurangi input latency dengan mengatur antrian frame CPU-ke-GPU. Anti-Lag 2 (game-integrated) lebih baik dari versi driver-level kalau game-nya support. — AMD Software: Adrenalin Edition — Safe
15. **Image Sharpening: On @ 80%** — Mengompensasi softness dari FSR/resolution scaling tanpa biaya performa signifikan. — Adrenalin Edition — Safe
16. **GPU Scheduling Priority tweak** — Penyesuaian prioritas scheduling khusus laptop AMD (relevan untuk setup seperti MSI Bravo 15 B7ED). — Registry, device-specific — Advanced
17. **Disable AMD External Events Utility (jika tidak perlu hotkey)** — Mengurangi background process. — Services.msc — Safe
18. **HYPR-RX: On** — Preset satu-klik yang menggabungkan Anti-Lag, Boost, dan frame optimization sekaligus. Klaim performa sampai 2.3x, tapi realistis di kondisi GPU-bound sekitar 1.3–1.8x tergantung skenario. — AMD Software: Adrenalin Edition — Safe
19. **Smart Access Memory (SAM)** — Memberi CPU akses penuh ke VRAM GPU (butuh dukungan motherboard/BIOS + CPU AMD). Gain FPS tanpa ubah setting in-game sama sekali. — BIOS + Adrenalin toggle — Moderate
20. **Radeon Boost** — Menurunkan resolusi render sementara saat ada gerakan cepat di layar untuk menaikkan FPS, otomatis naik lagi saat statis. Trade-off: sedikit penurunan clarity saat gerak cepat. — Adrenalin Edition — Safe
21. **Radeon Chill** — Membatasi FPS secara dinamis berdasar gerakan di layar untuk menghemat daya/panas — lebih cocok buat non-kompetitif, kurang cocok untuk ranked/competitive play. — Adrenalin Edition — Safe (situational)
22. **FreeSync + Frame Cap (di bawah refresh rate)** — Kombinasi ini menghilangkan tearing sekaligus menjaga latency rendah. — Adrenalin Edition + Windows Display Settings — Safe
23. **Wait for Vertical Refresh: Off** — Mematikan V-Sync driver-level demi latency lebih rendah; pakai Enhanced Sync kalau butuh kontrol tearing tanpa nge-cap FPS. — Adrenalin Edition — Safe
24. **Tessellation Mode: AMD Optimized** — Membatasi level tessellation berlebihan yang membebani GPU tanpa banyak dampak visual. — Adrenalin Edition — Safe
25. **Automatic Tuning — Auto Overclock / Rage Mode** — Preset overclock otomatis bawaan driver (tidak perlu tuning manual). Rage Mode khusus tersedia di beberapa seri RX. Void warranty kalau dipakai. — Adrenalin Edition Performance Tuning tab — Advanced
26. **Undervolt otomatis (GPU)** — Menurunkan voltage di clock speed yang sama, biasanya hasilnya clock lebih stabil & suhu lebih rendah (kartu RDNA merespons baik). Butuh testing per-unit karena silicon lottery. — Adrenalin Edition Performance Tuning tab — Advanced
27. **HBCC (High Bandwidth Cache Controller) Memory Size** — Alokasi system memory tambahan untuk GPU yang butuh VRAM lebih dari yang tersedia fisik. Hanya relevan di GPU dengan HBM (mis. Radeon VII), bukan semua kartu AMD. — Adrenalin Edition Global Graphics — Advanced (hardware-specific)
28. **Disable GPU Energy Driver Service** — Mematikan service `GpuEnergyDrv` yang berkaitan dengan monitoring/estimasi energi GPU. — `HKLM\SYSTEM\CurrentControlSet\Services\GpuEnergyDrv` → `Start` = 4 — Moderate

## D. Intel (Arc / iGPU)

18. **Increase iGPU Shared VRAM Allocation** — Alokasi RAM lebih besar ke iGPU, berguna untuk game/emulator lama yang butuh VRAM lebih. — Registry `DedicatedSegmentSize` (GMM) — Advanced (nilai keliru bisa sebabkan instabilitas)
19. **ReBAR (Resizable BAR) Enable** — Wajib untuk performa optimal Arc, gain 30–40% di game yang didukung. Perlu diaktifkan dari BIOS, bukan cuma software — jadi app cuma bisa cek status & kasih instruksi, bukan toggle langsung. — BIOS-level — N/A (guidance only)
20. **Intel Graphics Software — Performance Tuning (Power Limit/Boost)** — Menyesuaikan power limit sebelum tuning in-game. — Intel Graphics Software app — Moderate
21. **HwQueuedRenderPacketGroupLimitPerNode** — Mengatur jumlah render packet yang bisa di-queue per node GPU Arc. Efeknya ke latency tergantung game. Intel sendiri tidak merekomendasikan diubah sembarangan. — Registry, Arc-specific — Advanced (unclear/undocumented official effect)

---

## Catatan Validasi
- Tweak nomor 18–21 (Intel) kurang terdokumentasi resmi dibanding NVIDIA/AMD — sebagian besar dari forum komunitas, bukan dokumentasi Intel resmi. Perlu ditandai "experimental" di app.
- Nomor 5, 6 (SystemResponsiveness, NetworkThrottlingIndex) adalah tweak lama yang masih banyak dipakai tapi efeknya kontroversial di hardware modern — cocok untuk fitur "A/B test sebelum-sesudah" di app.
- ReBAR (19) tidak bisa di-toggle dari software — app hanya bisa mendeteksi status dan kasih instruksi ke BIOS.

## Dikecualikan (kemungkinan placebo — JANGAN dipakai)
- **"AMD Driver Optimization" custom keys** (`DX9Optimized`, `DX11Optimized`, `DX12Optimized`, `DisableCacheFlushOnSubmit`, `EnableLargePageTextureCache`, `EnableHBMPageMigration`, dll di bawah service `amdkmdag`) — ditemukan di salah satu tweak pack komunitas, tapi tidak terdokumentasi resmi AMD di mana pun. Windows/driver kemungkinan besar mengabaikan key custom yang tidak dikenali (tidak merusak, tapi juga tidak ada efek nyata). Jangan diporting ke app supaya kredibilitas tetap terjaga.

## Belum tercakup / bisa digali lebih lanjut
- Overclocking-adjacent tweaks (di luar scope "tweak" murni, lebih ke tuning manual — perlu didiskusikan apakah masuk app atau tidak)
- G-Sync/FreeSync specific driver-level settings
- Multi-GPU / SLI-Crossfire legacy settings (relevan atau tidak tergantung target user)

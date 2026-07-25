# 🚀 WinTweakStudio — Update Roadmap & Priority List

> Dokumen ini menggabungkan **fitur yang perlu diupdate**, **tweak baru dari ShadownTweak.txt**, dan **ide tambahan dari ChatGPT** ke dalam satu roadmap yang terstruktur.

---

## 📊 Score Saat Ini vs Target

| Kondisi | Score | Persentase |
|:---|:---:|:---:|
| ⚠️ **Sekarang** | 3 / 10 | 27% |
| 🎯 **Target Setelah Update** | 8.7 / 10 | 87% |

---

## 🔴 PRIORITY 1 — KRITIS (Harus Ada Sebelum Release)

> Fitur-fitur ini adalah **inti dari aplikasi**. Tanpa ini, WinTweakStudio belum layak dirilis.

### 1. 🔑 Login & Aktivasi VIP Key
- **Status**: ❌ Belum ada UI-nya (logic `LicenseService.cs` sudah ada)
- **Yang perlu dibuat**:
  - Dialog popup input key saat pertama buka
  - Badge role di header: `FREE USER` / `VIP MEMBER` / `OWNER`
  - Lock fitur VIP untuk user Free
  - Simpan key ke `license.dat`

### 2. 🎮 Game Mode
- **Status**: ❌ Belum ada
- **Yang perlu dibuat**:
  - Tombol satu klik "Game Mode ON / OFF"
  - Saat ON: boost CPU/GPU max, kill background apps
  - Background apps yang di-kill: `OneDrive.exe`, `SearchHost.exe`, `GameBarPresenceWriter.exe`, `WidgetService.exe`, `XboxGameSave.exe`
  - Saat OFF: restore ke kondisi normal
  - Sumber: `KentangApp._apply_game_mode_on` / `_apply_game_mode_off`

### 3. 🎯 Performance Profiles
- **Status**: ❌ Belum ada
- **4 profil yang perlu dibuat**:
  - 🎮 **Gaming Mode** — CPU min 80%+, unpark semua core, GPU max
  - 📺 **Streaming Mode** — Encoder priority tinggi, network stabil
  - 💼 **Work Mode** — Prioritas app responsif, RAM dibersihkan
  - 🔋 **Battery Saver** — CPU max 60%, boost off, background minimal

### 4. 🧠 RAM Flush & Auto RAM Cleaner
- **Status**: ❌ Belum ada
- **Yang perlu dibuat**:
  - Tombol "Quick RAM Clean" (bersihkan Standby List)
  - Tombol "Deep RAM Clean" (EmptyWorkingSet background apps)
  - Toggle "Auto RAM Clean setiap 2 jam"
  - Tampilkan RAM freed setelah clean

### 5. 🗑️ Remove Bloat (UWP App Removal)
- **Status**: ❌ Belum ada UI
- **Yang perlu dibuat**:
  - List UWP apps dengan checkbox
  - Tombol "Remove Selected"
  - Uninstall OneDrive script
  - Kode: `Remove-AppxPackage -AllUsers`

---

## 🟡 PRIORITY 2 — PENTING (Dalam 1-2 Minggu)

### 6. 🔧 Troubleshoot & Full Fix
- **Status**: ❌ Belum ada
- **Sub-fitur**:
  - Fix Winsock & TCP (`netsh winsock reset`, `netsh int ip reset`)
  - Fix DNS (`ipconfig /flushdns`)
  - Fix Windows Update (bersihkan cache WU)
  - Fix DirectX / VC++ Redistributable
  - Fix Page File (System Managed)
  - Full System Fix (semua sekaligus)

### 7. 📊 Process Manager
- **Status**: ❌ Belum ada
- **Yang perlu dibuat**:
  - Tampilkan daftar proses aktif + CPU/RAM usage
  - Tombol "Kill" per proses
  - Tombol "Disable Startup" permanen
  - Filter: proses berat > 50MB

### 8. 🏓 Ping Test & Network Monitor
- **Status**: ❌ Belum ada
- **Yang perlu dibuat**:
  - Test ping ke server game: Valorant, CSGO, RDR2, GTA5, dll
  - Tampilkan latency ms real-time
  - Packet loss monitor
  - DNS Benchmark (cari DNS tercepat)

### 9. 🔁 Backup & Restore
- **Status**: ❌ Belum ada
- **Yang perlu dibuat**:
  - Export registry snapshot sebelum tweak
  - Restore ke snapshot manapun
  - Rollback Point Manager
  - Sumber: `Simpan kondisi Windows sebelum tweak bisa rollback kapanpun`

### 10. 🚀 Auto Start Manager
- **Status**: ❌ Belum ada
- **Yang perlu dibuat**:
  - List program yang autostart saat Windows boot
  - Toggle enable/disable per program
  - Tampilkan impact ke waktu boot

---

## 🟢 PRIORITY 3 — ENHANCEMENT (Fitur Tambahan Nilai)

### 11. 🤖 Auto Game Detect
- Deteksi game yang sedang berjalan setiap 5 detik
- Auto-aktifkan Game Mode saat game terdeteksi
- Daftar game yang dideteksi:
  - `gta5.exe`, `GTA5.exe`, `SanAndreas.exe`, `RDR2.exe`
  - `valorant-win64-shipping.exe`, `cs2.exe`, `r5apex.exe`
  - `dota2.exe`, `tslgame.exe` (PUBG), `javaw.exe` (Minecraft)
  - `RobloxPlayerBeta.exe`, `FortniteLauncher.exe`
  - `ModernWarfare2.exe`, `ModernWarfare3.exe`

### 12. 🔄 Driver Update Scanner
- Scan driver GPU, Audio, NIC yang outdated
- Tampilkan versi saat ini vs versi terbaru
- Link download otomatis

### 13. 📈 Live Resource Dashboard Enhancement
- VRAM Pressure Monitor
- NVMe SMART Viewer (kesehatan SSD)
- SSD Temperature Monitor
- Standby Memory Graph
- USB Polling Rate Viewer

---

## 🔵 PRIORITY 4 — TWEAK BARU (Dari ShadownTweak.txt + ChatGPT)

### GPU Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| GPU-SHA-01 | Shader Cache Optimizer | Mengurangi shader stutter DX11/DX12/Vulkan |
| GPU-SHA-02 | DXCache Cleaner | Membersihkan cache shader DirectX |
| GPU-SHA-03 | Vulkan Pipeline Cache Cleaner | Reset cache Vulkan bila corrupt |
| GPU-SHA-04 | Shader Precompile Detection | Deteksi game yang mendukung precompile shader |
| GPU-SHA-05 | Resize BAR Checker | Mengecek apakah Resizable BAR aktif |
| GPU-SHA-06 | Smart Access Memory Checker | Khusus AMD SAM |
| GPU-SHA-07 | VRAM Pressure Monitor | Monitoring penggunaan VRAM |
| GPU-SHA-08 | DXGI Flip Model Detector | Cek game memakai Flip Model modern |
| GPU-SHA-09 | PresentMon Integration | Monitoring frametime & FPS |

### CPU Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| CPU-SHA-01 | CPPC Status Checker | Ryzen Collaborative Processor Performance Control |
| CPU-SHA-02 | SMT Status Detector | Status SMT aktif/tidak |
| CPU-SHA-03 | PBO Detector | Deteksi Precision Boost Overdrive |
| CPU-SHA-04 | Intel TVB Detector | Thermal Velocity Boost |
| CPU-SHA-05 | Intel ABT Detector | Adaptive Boost Technology |
| CPU-SHA-06 | Core Isolation Checker | Cek Memory Integrity / VBS |
| CPU-SHA-07 | Scheduler Information | Tampilkan jenis scheduler Windows |

### Storage Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| STO-SHA-01 | DirectStorage Checker & Enabler | Cek + enable DirectStorage (NVMe + driver GPU update) — **BELUM ADA** |
| STO-SHA-02 | NVMe SMART Viewer | Tampilkan kesehatan SSD NVMe |
| STO-SHA-03 | SSD Temperature Monitor | Monitoring suhu SSD |
| STO-SHA-04 | TRIM Status Checker | Verifikasi TRIM benar-benar aktif |

### RAM Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| RAM-SHA-01 | Compression Memory Status | Status Memory Compression |
| RAM-SHA-02 | Commit Usage Analyzer | Analisis penggunaan commit memory |
| RAM-SHA-03 | Working Set Statistics | Statistik working set proses |
| RAM-SHA-04 | Standby Memory Graph | Grafik Standby Memory real-time |

### Input / Display Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| INP-SHA-01 | USB Polling Rate Viewer | Menampilkan polling rate mouse/keyboard |
| INP-SHA-02 | HID Latency Checker | Mengecek latency perangkat input |
| DSP-SHA-01 | VRR Checker | Cek Variable Refresh Rate aktif |
| DSP-SHA-02 | HDR Status | Status HDR aktif/tidak |
| DSP-SHA-03 | G-Sync/FreeSync Detection | Deteksi VRR vendor |
| DSP-SHA-04 | Refresh Rate Validator | Monitor berjalan di refresh rate maksimum |

### Network Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| NET-SHA-01 | DNS Benchmark | Menguji DNS tercepat otomatis |
| NET-SHA-02 | Ping Monitor | Monitoring latency real-time |
| NET-SHA-03 | Packet Loss Monitor | Monitoring packet loss |
| NET-SHA-04 | MTU Auto Tester | Menentukan MTU optimal |

### System Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| SYS-SHA-01 | Driver Version Checker | Bandingkan versi driver dengan database |
| SYS-SHA-02 | BIOS Version Checker | Informasi BIOS versi & tanggal |
| SYS-SHA-03 | Windows Build Analyzer | Build Windows & fitur yang tersedia |
| SYS-SHA-04 | TPM Status Checker | Informasi TPM 2.0 |

### Gaming Tweaks Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| GAM-SHA-01 | Game Detection Engine | Deteksi game secara otomatis |
| GAM-SHA-02 | Auto Apply Profile | Terapkan profil sesuai game terdeteksi |
| GAM-SHA-03 | Benchmark Before/After | Bandingkan performa sebelum & sesudah tweak |
| GAM-SHA-04 | Frametime Analyzer | Analisis kestabilan frame |
| GAM-SHA-05 | Latency Analyzer | Tampilkan DPC/ISR secara ringkas |

### AI / Smart Features Baru

| ID | Nama Tweak | Manfaat |
|:---|:---|:---|
| AI-SHA-01 | Auto Recommendation | Rekomendasi tweak berdasarkan hardware |
| AI-SHA-02 | Risk Score | Tingkat risiko tiap tweak (Low/Medium/High) |

### UI / UX Improvements Baru

| ID | Nama Fitur | Manfaat |
|:---|:---|:---|
| UI-SHA-01 | Search Tweaks | Pencarian tweak real-time |
| UI-SHA-02 | Favorite Tweaks | Simpan tweak favorit |
| UI-SHA-03 | Category Presets | Preset: Esports, AAA, Streaming, Editing, Battery |
| UI-SHA-04 | Export Hardware Report | Ekspor laporan spesifikasi PC ke PDF/JSON |
| UI-SHA-05 | Rollback Point Manager | Manajemen backup/restore point |
| UI-SHA-06 | Tweak History Advanced | Filter, search, export history |

---

## 📦 RINGKASAN TOTAL TWEAK

| Kondisi | Jumlah Tweak | Fitur Utama |
|:---|:---:|:---:|
| ⚠️ Sekarang | 131 tweak | 3 fitur (apply/history/settings) |
| ✅ Setelah Priority 1-2 | 131 tweak | 14 fitur |
| 🏆 Setelah Priority 1-4 | **183+ tweak** | **20+ fitur** |

---

## 🗓️ Estimasi Pengerjaan

| Priority | Fitur | Estimasi |
|:---|:---|:---:|
| 🔴 P1 | Login Key + Game Mode + Profiles + RAM Flush + Bloat | ~2-3 hari |
| 🟡 P2 | Troubleshoot + Process Mgr + Ping Test + Backup | ~3-4 hari |
| 🟢 P3 | Auto Game Detect + Driver Scanner + Dashboard++ | ~2-3 hari |
| 🔵 P4 | 52 Tweak Baru + AI Features + UI Polish | ~4-5 hari |
| **TOTAL** | **Semua fitur lengkap** | **~2 minggu** |

---

## ✅ STATUS CHECKLIST

### Priority 1 — KRITIS
- [ ] Login & Aktivasi VIP Key UI
- [ ] Game Mode (ON/OFF)
- [ ] Performance Profiles (Gaming/Streaming/Work/Battery)
- [ ] RAM Flush & Auto RAM Cleaner
- [ ] Remove Bloat UI

### Priority 2 — PENTING
- [ ] Troubleshoot & Full Fix
- [ ] Process Manager
- [ ] Ping Test & Network Monitor
- [ ] Backup & Restore
- [ ] Auto Start Manager

### Priority 3 — ENHANCEMENT
- [ ] Auto Game Detect
- [ ] Driver Update Scanner
- [ ] Live Dashboard Enhancement

### Priority 4 — TWEAK BARU (52 tweak)
- [ ] GPU Tweaks Baru (9 tweak)
- [ ] CPU Tweaks Baru (7 tweak)
- [ ] Storage Tweaks Baru (4 tweak)
- [ ] RAM Tweaks Baru (4 tweak)
- [ ] Input/Display Tweaks Baru (6 tweak)
- [ ] Network Tweaks Baru (4 tweak)
- [ ] System Tweaks Baru (4 tweak)
- [ ] Gaming Tweaks Baru (5 tweak)
- [ ] AI Features (2 tweak)
- [ ] UI/UX Improvements (6 fitur)

---

## 🌐 TWEAK TAMBAHAN HASIL RISET WEB (2026)

> Sumber: SageTweaks, Switchblade Gaming, OptiLag — Juli 2026

| # | Nama Tweak | Kategori | Status | Catatan |
|:--|:---|:---|:---:|:---|
| 1 | **Ultimate Performance Power Plan (Unlock)** | Power | ✅ Sudah Ada | Matikan semua CPU power-saving states — lebih agresif dari High Performance |
| 2 | **Optimizations for Windowed Games** | Display | ❌ Belum Ada | Hilangkan gap performa borderless windowed vs exclusive fullscreen |
| 3 | **DirectStorage Checker & Enabler** | Storage/GPU | ❌ Belum Ada | Syarat: NVMe, driver GPU update, game support (Forza H5, Hellblade II) |
| 4 | **Auto Super Resolution (Auto SR)** | Display/GPU | ❌ Belum Ada | AI upscaler level OS, tanpa perlu dukungan developer game — Win11 24H2+ |
| 5 | **CPU Reserve for Games** (SystemResponsiveness=0) | CPU | ✅ Sudah Ada | Default Windows sisihkan 20% CPU untuk system tasks, set 0 = game prioritas max |
| 6 | **NetworkThrottlingIndex** (update warning) | Network | ✅ Sudah Ada | Tambah label ⚠️ **EKSPERIMENTAL** — di sebagian sistem bisa naikkan DPC latency / masalah audio |

### Yang Perlu Ditambahkan ke DbSeeder.cs:
- [ ] `WIN-DSP-XX` — Optimizations for Windowed Games (registry toggle)
- [ ] `STO-DS-XX` — DirectStorage Enabler (`UseStorageCache` + `UseGPUOptimizedMedia`)
- [ ] `WIN-AI-XX` — Auto Super Resolution Toggle (Win11 24H2+ only)
- [ ] Update Description `NetworkThrottlingIndex` → tambah catatan eksperimental + revert option

---

*Dokumen dibuat: 2026-07-24 | Terakhir diupdate: 2026-07-24 | WinTweakStudio Development Roadmap*

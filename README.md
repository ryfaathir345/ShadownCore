<div align="center">

<img src="Icon/logo.png" alt="WinTweakStudio Logo" width="120" height="120"/>

# ⚡ WinTweakStudio

**All-in-One Windows Performance Optimizer & System Tuning Suite**

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![UI](https://img.shields.io/badge/UI-WPF-68217A?style=for-the-badge&logo=microsoft&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/License-Proprietary-FF6B35?style=for-the-badge)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Active%20Development-00E5FF?style=for-the-badge)](https://github.com/Prosnatics/WinTweakStudio)

> 🚀 *Optimalkan Windows Anda seperti profesional — satu klik untuk performa maksimal.*

---

[✨ Fitur Utama](#-fitur-utama) • [📸 Screenshot](#-screenshot) • [⚙️ Instalasi](#️-instalasi) • [🏗️ Arsitektur](#️-arsitektur) • [🛠️ Tech Stack](#️-tech-stack) • [🗺️ Roadmap](#️-roadmap) • [🔑 Lisensi](#-sistem-lisensi)

</div>

---

## 📖 Tentang WinTweakStudio

**WinTweakStudio** adalah aplikasi desktop Windows berbasis WPF (.NET 8) yang dirancang untuk mengoptimalkan performa sistem secara aman, terstruktur, dan dapat di-*revert* kapan saja. Aplikasi ini hadir sebagai solusi all-in-one bagi gamer, streamer, dan pengguna profesional yang ingin memaksimalkan potensi hardware mereka tanpa risiko merusak sistem.

### 🎯 Tujuan Utama

- **Performa Maksimal** — Unlock potensi penuh CPU, GPU, dan RAM melalui tweak sistem yang telah diuji
- **Aman & Reversible** — Setiap tweak dicatat di database SQLite, dan dapat di-*revert* satu per satu atau sekaligus via Restore Point
- **Mudah Digunakan** — UI gelap modern dengan navigasi intuitif, cocok untuk pemula hingga advanced user
- **Hardware-Aware** — Monitoring sensor hardware real-time (suhu, clock, power, VRAM) via LibreHardwareMonitor

---

## ✨ Fitur Utama

### 🎮 Game Mode & Performance Profiles
- **One-Click Game Mode** — Secara otomatis kill background apps (OneDrive, SearchHost, WidgetService, dll.), apply priority registry tweaks GPU/CPU, dan clear standby memory
- **5 Performance Profiles** yang dapat diterapkan:
  | Profil | Deskripsi |
  |:------|:----------|
  | 🎮 **Gaming** | CPU/GPU max priority, kill background apps, Ultimate Performance power plan |
  | 📺 **Streaming** | Network stable, encoder priority tinggi, GPU priority |
  | 💼 **Work** | Balanced plan, CPU efficient, RAM dibersihkan |
  | 🔋 **Battery Saver** | CPU max 50%, boost off, power saver plan |
  | 🖥️ **Standard** | Kembali ke kondisi default |

### 🤖 Auto Game Detection
- Deteksi game otomatis setiap **3 detik** via process scanner
- Support 20+ game populer: CS2, Valorant, GTA V, RDR2, Apex Legends, DOTA 2, PUBG, League of Legends, Roblox, Fortnite, Genshin Impact, dll.
- Auto-aktifkan Game Mode saat game terdeteksi, auto-disable saat game ditutup

### 📊 Live Hardware Monitor (Dashboard)
Real-time monitoring menggunakan **LibreHardwareMonitor** + **WMI fallback**:

| Komponen | Metrics yang Ditampilkan |
|:---------|:------------------------|
| 🔵 **CPU** | Suhu, Usage %, Power (Watt), Clock per-core (hingga 8 core) |
| 🟢 **GPU** | Suhu, Usage %, Power (Watt), VRAM Used/Total, Vendor badge (NVIDIA/AMD/Intel) |
| 🟡 **RAM** | Used GB, Total GB, Usage %, Speed MHz, XMP/EXPO status detector |
| 🔴 **Storage** | Suhu (NVMe/HDD), Usage %, SSD Health % |
| 🔋 **Battery** | Charge %, Wear Level %, Status |
- ⚠️ **XMP/EXPO Warning** — Otomatis mendeteksi jika RAM berjalan di bawah rated speed dan menampilkan peringatan

### 🔧 System Tweaks (130+ Tweak)
Tweak dikategorikan dengan **Risk Level** (Safe / Moderate / Advanced):

| Kategori | Contoh Tweak |
|:---------|:------------|
| 🖥️ **GPU** | HAGS, Disable GPU Energy Driver, NVIDIA/AMD-specific tweaks |
| ⚡ **CPU** | Disable Core Parking, CPU Scheduling, Spectre/Meltdown Mitigations |
| 🧠 **RAM** | Clear PageFile on Shutdown, Disable SysMain/Superfetch, Standby Memory Flush |
| 🌐 **Network** | Disable Network Throttling, Nagle Algorithm, Custom DNS (Cloudflare/Google/Quad9) |
| 🪟 **Windows** | Disable Telemetry, Disable Defender (Advanced), Visual Optimization |
| ⚙️ **Service** | Disable DiagTrack, SysMain, dan unnecessary Windows services |
| 🗑️ **Debloat** | Remove Xbox Game Bar, Disable GameDVR, UWP bloatware removal |
| 💾 **Storage** | Disable NTFS Last Access Update, TRIM optimization |
| 🚀 **Boot/Power** | Ultimate Performance Power Plan, Hibernate disable |

### 📜 History & Restore Point
- Setiap tweak yang diterapkan dicatat **atomically** di SQLite sebelum perubahan sistem dilakukan
- **Restore Point** — Snapshot state sistem, dapat di-rollback sepenuhnya
- Revert individual tweak kapan saja dari halaman History
- View log lengkap: tweak name, category, old value, new value, timestamp

### 🌐 Network Tools
- **Game Ping Tester** — Test latency real-time ke server game populer (Valorant SEA, CS2 Asia, MLBB, DOTA2, Apex Legends, dll.)
- **DNS Changer** — Set DNS Cloudflare (1.1.1.1), Google (8.8.8.8), atau Quad9 (9.9.9.9) via PowerShell
- **Nagle Algorithm Toggle** — Disable/enable per network adapter untuk mengurangi latency TCP
- **Network Info** — IP Address, Subnet Mask, Gateway, DNS per adapter

### 🔍 Driver Scanner
- Scan GPU & Audio driver via WMI (`Win32_PnPSignedDriver`)
- Tampilkan versi driver, tanggal, dan status

### 🛠️ Troubleshoot & Fix
- **Fix Network & Winsock** — `netsh winsock reset` + `netsh int ip reset`
- **Flush DNS Cache** — `ipconfig /flushdns`
- **Fix Windows Update Cache** — Stop WU services → clear SoftwareDistribution → restart services
- **Full System Fix** — Jalankan semua fix sekaligus

### 🔑 Sistem Lisensi (Free / VIP / Owner)
- **Free User** — Akses tweak dasar
- **VIP Member** — Akses semua fitur termasuk fitur premium (Game Mode, Performance Profiles, Network Tools lanjutan)
- **Owner/Developer** — Full access + key generator
- Validasi online via GitHub JSON database dengan fallback offline
- Sesi tersimpan aman dalam file `license.dat` (Base64 encoded)

---

## 📸 Screenshot

> *Screenshot dan demo video akan ditambahkan setelah UI final selesai.*

---

## ⚙️ Instalasi

### Prasyarat

| Requirement | Minimum | Catatan |
|:-----------|:--------|:--------|
| **OS** | Windows 10 64-bit | Windows 11 direkomendasikan |
| **.NET Runtime** | .NET 8.0 | [Download di sini](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Hak Akses** | **Administrator** | **WAJIB** — diperlukan untuk registry & service tweaks |
| **RAM** | 4 GB | 8 GB+ direkomendasikan |

### Cara Install (Binary Release)

```bash
# 1. Download WinTweakStudio.zip dari Releases
# 2. Ekstrak ke folder yang diinginkan (misal: C:\Tools\WinTweakStudio)
# 3. Klik kanan WinTweakStudio.exe → "Run as Administrator"
# 4. Login dengan username atau aktivasi VIP Key
```

> ⚠️ **PENTING**: Aplikasi HARUS dijalankan sebagai Administrator. Tanpa hak admin, tweak registry dan service tidak akan berfungsi.

### Build dari Source

```bash
# Clone repository
git clone https://github.com/Prosnatics/WinTweakStudio.git
cd WinTweakStudio

# Restore dependencies
dotnet restore

# Build (Release)
dotnet build -c Release

# Run
dotnet run --project WinTweakStudio.csproj
```

**Atau gunakan Visual Studio 2022+:**
1. Buka `WinTweakStudio.sln`
2. Set configuration ke `Release`
3. Klik `Start` (F5) — pastikan VS dijalankan sebagai Administrator

---

## 🏗️ Arsitektur

WinTweakStudio menggunakan pola **MVVM (Model-View-ViewModel)** dengan dependency injection manual dan service layer yang terstruktur.

```
WinTweakStudio/
├── 📁 Models/
│   ├── TweakModels.cs          # TweakDefinition, TweakLog, RestorePoint, TweakGroup
│   └── HardwareSensorModels.cs # CpuSensorData, GpuSensorData, RamSensorData, dll.
│
├── 📁 ViewModels/
│   ├── MainViewModel.cs        # Root VM — navigasi, restart PC, logout
│   ├── DashboardViewModel.cs   # Hardware stats, Game Mode, Profiles, Ping test
│   ├── CategoryViewModel.cs    # Tweak list per kategori, apply/revert, filter
│   ├── HistoryViewModel.cs     # Riwayat tweak, restore points
│   └── SettingsViewModel.cs    # Network settings, troubleshoot, driver scan
│
├── 📁 Views/
│   ├── MainWindow.xaml(.cs)    # Shell utama — sidebar navigasi, header
│   ├── DashboardView.xaml(.cs) # Hardware monitoring cards, Game Mode UI
│   ├── CategoryView.xaml(.cs)  # Tweak cards per kategori
│   ├── HistoryView.xaml(.cs)   # Log table & restore point manager
│   ├── SettingsView.xaml(.cs)  # Network, troubleshoot, driver scanner
│   ├── LoginWindow.xaml(.cs)   # Login & VIP key activation
│   └── *Dialog.xaml(.cs)       # Confirmation, DNS Selection, Message dialogs
│
├── 📁 Services/
│   ├── TweakService.cs               # Core: apply/revert tweaks (Registry, Service, Cmd, PS, NvAPI, ADL)
│   ├── HardwareMonitorService.cs     # LibreHardwareMonitor wrapper + WMI fallback
│   ├── OptimizationProfileService.cs # Game Mode & Performance Profiles
│   ├── GameDetectionService.cs       # Auto game detection via process scanner
│   ├── NetworkService.cs             # Ping test, DNS changer, Nagle tweak
│   ├── LicenseService.cs             # Auth: Free/VIP/Owner + online validation
│   ├── NvidiaTweakService.cs         # NVIDIA-specific tweaks via NvAPIWrapper
│   ├── AmdTweakService.cs            # AMD-specific tweaks via ADL
│   ├── DriverScannerService.cs       # WMI driver scan
│   ├── TroubleshootService.cs        # Network fix, DNS flush, WU fix
│   ├── DialogService.cs              # Abstraksi dialog popup
│   └── SoundEffectService.cs         # Audio feedback (Game Mode ON/OFF, profile switch)
│
├── 📁 Data/
│   ├── DatabaseInitializer.cs  # SQLite schema, CRUD operations (TweakLogs, RestorePoints)
│   └── DbSeeder.cs             # 130+ tweak definitions seed data
│
├── 📁 Themes/
│   ├── Colors.xaml             # Design tokens (warna, brush)
│   ├── DarkTheme.xaml          # Global dark theme styles
│   ├── CardStyles.xaml         # Tweak card & hardware card styles
│   └── Styles.xaml             # Button, input, dan komponen UI lainnya
│
├── 📁 Converters/              # WPF value converters
├── 📁 Icon/                    # Application icons & assets
├── 📁 Docs/
│   └── WinTweakStudio_UpdateRoadmap.md
│
├── App.xaml(.cs)               # Application entry point, resource dictionaries
├── app.manifest                # Administrator privilege manifest
└── WinTweakStudio.csproj       # Project configuration
```

### Alur Data Tweak

```
User klik "Apply" 
    → CategoryViewModel.ApplyTweakAsync()
    → TweakService.ApplyTweakAsync()
        → [1] Read current value (GetCurrentValue)
        → [2] Write TweakLog ke SQLite (ATOMIC, sebelum apply)
        → [3] ExecuteApply() berdasarkan TweakType:
              Registry  → WriteRegistryValue()
              Service   → SetServiceStartupType() via Registry
              Command   → cmd.exe /c {command}
              PowerShell → powershell.exe -Command {ps}
              NvApi     → NvidiaTweakService.SetSettingValueByName()
              Adl       → AmdTweakService.SetSettingValue()
    → Update UI state (IsApplied = true)
```

---

## 🛠️ Tech Stack

| Komponen | Library / Framework | Versi |
|:---------|:-------------------|:------|
| **Runtime** | .NET 8.0 (Windows) | 8.0 |
| **UI Framework** | WPF (Windows Presentation Foundation) | .NET 8 |
| **MVVM Toolkit** | CommunityToolkit.Mvvm | 8.4.2 |
| **Hardware Monitoring** | LibreHardwareMonitorLib | 0.9.6 |
| **NVIDIA GPU API** | NvAPIWrapper.Net | 0.8.1.101 |
| **Database** | Microsoft.Data.Sqlite (SQLite) | 10.0.10 |
| **Icons** | Material.Icons.WPF | 3.0.2 |
| **Windows Services** | System.ServiceProcess.ServiceController | 10.0.10 |
| **GPU Compute** | AMD ADL (via P/Invoke) | — |

---

## 🔑 Sistem Lisensi

### User Roles

| Role | Badge | Fitur |
|:-----|:------|:------|
| 🔘 **Free User** | `FREE USER` (Abu-abu) | Dashboard, basic tweaks, history |
| 💎 **VIP Member** | `VIP MEMBER` (Cyan `#00E5FF`) | Semua fitur: Game Mode, Profiles, Network Tools, Advanced Tweaks |
| 👑 **Owner** | `OWNER / DEVELOPER` (Gold `#FFD700`) | Full access + key generator |

### Format License Key
```
VIP Key  : VIP-XXXX-XXXX-XXXX-XXXX
Owner Key: OWNER-XXXX-XXXX-XXXX-XXXX
```

### Cara Aktivasi
1. Buka aplikasi → Login window akan muncul
2. Masukkan username
3. Klik "Activate VIP" → masukkan license key
4. Validasi online via GitHub → fallback offline jika tidak ada koneksi
5. Sesi tersimpan di `license.dat` (di-*encode* Base64)

---

## 🗺️ Roadmap

Lihat dokumen lengkap: [`Docs/WinTweakStudio_UpdateRoadmap.md`](Docs/WinTweakStudio_UpdateRoadmap.md)

### Status Saat Ini

| Priority | Fitur | Status |
|:---------|:------|:------:|
| 🔴 P1 | Login & VIP Activation UI | ✅ Done |
| 🔴 P1 | Game Mode (ON/OFF) | ✅ Done |
| 🔴 P1 | Performance Profiles (5 profil) | ✅ Done |
| 🔴 P1 | RAM Flush & Standby Clear | ✅ Done |
| 🟡 P2 | Troubleshoot & System Fix | ✅ Done |
| 🟡 P2 | Ping Test & Network Monitor | ✅ Done |
| 🟡 P2 | Driver Scanner | ✅ Done |
| 🟢 P3 | Auto Game Detection | ✅ Done |
| 🟢 P3 | Hardware Monitor Dashboard | ✅ Done |
| 🔵 P4 | 52 Tweak Baru (GPU/CPU/Storage/RAM/Network) | 🔄 In Progress |
| 🔵 P4 | AI Recommendation Engine | 📋 Planned |
| 🔵 P4 | Process Manager | 📋 Planned |
| 🔵 P4 | Auto Start Manager | 📋 Planned |
| 🔵 P4 | DirectStorage & VRR Checker | 📋 Planned |

### Target Tweak
| Kondisi | Jumlah Tweak | Fitur |
|:--------|:-----------:|:-----:|
| ⚠️ Saat ini | 131 tweak | 10+ fitur |
| 🏆 Target | **183+ tweak** | **20+ fitur** |

---

## ⚠️ Disclaimer & Safety

> **BACA SEBELUM MENGGUNAKAN:**

- Beberapa tweak kategori **Advanced** (seperti menonaktifkan Spectre mitigations atau Windows Defender) dapat mengurangi keamanan sistem. Gunakan dengan bijak dan hanya jika Anda memahami konsekuensinya.
- **Selalu buat Restore Point** sebelum menerapkan banyak tweak sekaligus.
- Tweak yang diterapkan memerlukan **restart Windows** agar sebagian besar perubahan dapat berjalan penuh.
- Developer tidak bertanggung jawab atas kerusakan sistem akibat penggunaan yang tidak tepat.
- **Aplikasi HARUS dijalankan sebagai Administrator** — tanpa ini, tweak tidak akan berfungsi.

### Risk Level Guide

| Level | Warna | Artinya |
|:------|:------|:--------|
| 🟢 **Safe** | Hijau | Aman untuk semua pengguna, mudah di-revert |
| 🟡 **Moderate** | Kuning | Disarankan untuk pengguna yang paham konsekuensinya |
| 🔴 **Advanced** | Merah | Untuk advanced user saja, dapat mempengaruhi keamanan/stabilitas |

---

## 🤝 Kontribusi

Proyek ini bersifat **proprietary** dan saat ini dalam tahap pengembangan aktif.

Jika Anda menemukan bug atau memiliki saran tweak baru:
1. Buat issue di repository
2. Sertakan: Windows version, hardware specs, dan langkah reproduksi bug

---

## 📞 Kontak & Support

| Platform | Link |
|:---------|:-----|
| 🐙 **GitHub** | [github.com/Prosnatics/WinTweakStudio](https://github.com/Prosnatics/WinTweakStudio) |
| 🔑 **License Auth** | [WinTweakStudio-Auth](https://github.com/Prosnatics/WinTweakStudio-Auth) |

---

<div align="center">

**WinTweakStudio** — *Built with ❤️ untuk komunitas gamer & enthusiast Indonesia*

*Dokumen terakhir diperbarui: Juli 2026*

</div>

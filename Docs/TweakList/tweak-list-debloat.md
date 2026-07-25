# Tweak List — Kategori Debloat

Format: **Nama** — Deskripsi — Aksi — Risk Level

---

1. **Uninstall Bloatware UWP Bawaan** — Xbox app (kalau tidak main game Xbox/Game Pass), Solitaire Collection, Mixed Reality Portal (fitur mati), 3D Viewer, dst. — `Get-AppxPackage | Remove-AppxPackage` per-app (via PowerShell), app kasih checklist bukan hapus semua sekaligus — Safe (per-app, reversible dengan reinstall dari Store)
2. **Remove OneDrive Integration** — OneDrive auto-start dan sync background kalau tidak dipakai. Bisa di-uninstall penuh atau cukup disable auto-start. — Uninstall via Settings > Apps, atau `OneDriveSetup.exe /uninstall` — Safe (opsional, tanya dulu apakah user pakai OneDrive)
3. **Disable Windows Copilot** — *(overlap dengan Windows category)* — Registry Policy — Safe
4. **Disable Cortana Remnants** — Cortana sudah bukan asisten utama lagi tapi beberapa remnant background process masih ada di sistem lama. — Registry/Group Policy — Safe
5. **Remove Widgets** — *(overlap dengan Windows category)* — Uninstall/disable — Safe
6. **Disable Start Menu Ads/Suggested Apps** — *(overlap dengan Windows category tweak No.6)* — Registry — Safe
7. **Uninstall Get Help, Get Started, Feedback Hub** — App bawaan yang jarang dipakai user teknikal. — Remove-AppxPackage — Safe
8. **Disable Teams Consumer Auto-install/Auto-start** — Microsoft Teams (versi consumer) sering ter-install ulang otomatis via Windows Update dan auto-start. — Uninstall + registry block reinstall — Safe
9. **Remove Clipchamp** — Video editor bawaan yang di-bundle di banyak instalasi baru, jarang dipakai kalau user sudah punya editor lain. — Remove-AppxPackage — Safe
10. **Disable "Suggested Actions" (Clipboard AI suggestions)** — Fitur yang mendeteksi teks di clipboard (nomor telepon, tanggal) dan nawarin aksi otomatis — minor background overhead. — Settings > System > Clipboard — Safe
11. **Debloat via Batch Script (Guidance/Referensi)** — Untuk debloat menyeluruh, komunitas biasa pakai script seperti Chris Titus WinUtil atau Sophia Script sebagai referensi daftar app. App kita sebaiknya punya list sendiri yang di-maintain (bukan cuma nyontek 1:1), plus opsi restore per-app (reinstall dari Store) kalau user berubah pikiran. — Guidance/referensi desain, bukan tweak individual — N/A

## Catatan Validasi
- Prinsip penting: **Debloat harus reversible**. Kalau user uninstall app UWP bawaan lalu berubah pikiran, app harus kasih tombol "Reinstall dari Microsoft Store" per item, bukan cuma hapus permanen tanpa jejak.
- Beberapa item di sini (No. 3, 5, 6) sengaja overlap dengan kategori Windows — perlu keputusan desain: taruh di satu tempat dan cross-reference dari tempat lain, biar user nggak apply 2x tanpa sadar.

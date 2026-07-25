# Tweak List — Kategori Storage

Format: **Nama** — Deskripsi — Aksi — Risk Level

---

1. **Delete Windows Update Cache** — Stop service Windows Update, hapus folder `SoftwareDistribution`, buat ulang. Bebasin space dari cache update lama. — Command sequence (net stop wuauserv/UsoSvc + rd + md) — Safe *(sudah ada di tweak-list-from-user-packs.md)*
2. **TRIM Scheduling untuk SSD/NVMe** — Pastikan TRIM aktif dan terjadwal (default Windows biasanya sudah benar, tapi kadang ke-disable oleh tool third-party lain). App bisa verifikasi status & re-enable kalau perlu. — `fsutil behavior query DisableDeleteNotify` (0 = TRIM aktif) — Safe
3. **Disable Disk Indexing per-drive tertentu** — *(overlap dengan Windows category No.7)* Indexing berguna untuk search tapi bikin disk I/O background terus, terutama di drive besar isi file game/media yang jarang dicari by content. — Indexing Options per-drive — Moderate
4. **Storage Sense Auto-cleanup Configuration** — Fitur bawaan Windows untuk otomatis hapus temp file & recycle bin lama. Banyak user belum aktifkan/konfigurasi ini. — Settings > System > Storage > Storage Sense — Safe
5. **Clear Temp Files (%TEMP%, Windows\Temp)** — Pembersihan manual file temporary yang menumpuk dari instalasi app, update, dsb. — Hapus isi folder — Safe
6. **Disk Cleanup — System Restore Points Lama** — Restore point lama (bukan yang baru dibuat app kita) bisa makan puluhan GB kalau tidak pernah dibersihkan. — `vssadmin` atau Disk Cleanup > Clean up system files — Moderate (jangan hapus restore point yang masih relevan)
7. **Disable 8.3 Filename Creation (NTFS)** — Fitur legacy compatibility NTFS yang jarang dibutuhkan lagi di software modern, disable bisa sedikit mempercepat operasi file di direktori dengan banyak file. — `fsutil 8dot3name set 1` — Moderate (bisa break beberapa software lawas yang masih bergantung ke short filename)
8. **Disable Last Access Timestamp Update (NTFS)** — Mengurangi disk write overhead kecil setiap kali file dibaca (Windows update "last accessed" timestamp by default). — `fsutil behavior set disablelastaccess 1` — Safe
9. **Hibernation File Removal (`hiberfil.sys`)** — *(overlap dengan CPU/Boot category)* Menghapus file hibernasi (seukuran RAM) untuk bebasin space, tapi hilangkan fitur Fast Startup/Hibernate. — `powercfg /hibernate off` — Moderate (trade-off fitur)
10. **Storage Health Check (SMART Data)** — Bukan tweak, tapi fitur monitoring: tampilkan remaining lifespan SSD/NVMe (dari LibreHardwareMonitor), warning kalau health mulai menurun. — Guidance/dashboard feature — N/A (informational)

## Catatan Validasi
- No. 7 (8.3 filename) — worth double-check dulu, karena beberapa software instalasi lama (terutama enterprise/legacy) masih bergantung ke ini. Kasih warning jelas di app.
- No. 6 — hati-hati bedain "restore point Windows System Restore" (fitur bawaan OS) dengan "restore point kita sendiri" (RestorePoints table di app) — dua hal yang beda tapi gampang bikin user bingung kalau UI-nya nggak jelas.

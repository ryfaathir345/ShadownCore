# Tweak List — Kategori Service

Format: **Nama Service** — Deskripsi — Rekomendasi — Risk Level

Prinsip: app TIDAK uninstall/hapus service, hanya ubah StartType (Automatic → Manual/Disabled), dan WAJIB simpan StartType lama ke TweakLogs sebelum diubah supaya reversible.

---

1. **Print Spooler (`Spooler`)** — Servis untuk printer. Bisa di-disable kalau user nggak punya/pakai printer sama sekali. — Disabled jika tidak ada printer — Safe (tapi cek dulu ada printer fisik/virtual atau tidak)
2. **Fax Service (`Fax`)** — Hampir tidak ada yang pakai fax modern. — Disabled — Safe
3. **Windows Search (`WSearch`)** — Indexing untuk pencarian file cepat. Disable menghemat disk I/O tapi search jadi lambat. — Manual/Disabled (situational) — Moderate
4. **Superfetch/SysMain (`SysMain`)** — *(overlap dengan RAM tweak)* — Situational — Moderate
5. **Remote Registry (`RemoteRegistry`)** — Memungkinkan edit registry dari jarak jauh; jarang dipakai user rumahan, sekaligus celah keamanan kalau aktif tanpa perlu. — Disabled — Safe (malah improvement keamanan)
6. **Windows Insider Service (`wisvc`)** — Cuma perlu kalau ikut Windows Insider Program. — Disabled jika tidak ikut Insider — Safe
7. **Downloaded Maps Manager (`MapsBroker`)** — Terkait fitur offline maps yang jarang dipakai di desktop/laptop non-Surface. — Disabled — Safe
8. **Retail Demo Service (`RetailDemo`)** — Untuk mode demo di toko retail, sama sekali tidak relevan untuk PC pribadi. — Disabled — Safe
9. **Touch Keyboard and Handwriting Panel Service (`TabletInputService`)** — Kalau device tidak punya touchscreen/stylus. — Disabled jika non-touch device — Safe
10. **Bluetooth Support Service (`bthserv`)** — Kalau device tidak pakai Bluetooth sama sekali (desktop tanpa adapter BT). — Disabled jika tidak ada hardware Bluetooth — Safe (app perlu deteksi hardware dulu)
11. **NVIDIA Telemetry Container (`NvTelemetryContainer`)** — *(sudah ada di GPU list)* — Disabled — Safe
12. **AMD External Events Utility** — *(sudah ada di GPU list)* — Disabled jika hotkey AMD tidak dipakai — Safe
13. **Program Compatibility Assistant Service (`PcaSvc`)** — Mendeteksi masalah kompatibilitas app lama, background overhead kecil tapi ada. Matikan kalau jarang install software lawas. — Manual — Safe
14. **Connected User Experiences and Telemetry (`DiagTrack`)** — Salah satu servis telemetry utama Windows, ngirim data diagnostic ke Microsoft secara berkala. — Disabled — Moderate (beberapa fitur troubleshooting Windows bisa kurang optimal)
15. **Windows Error Reporting Service (`WerSvc`)** — Kirim crash report ke Microsoft. Disable kalau tidak butuh. — Manual/Disabled — Safe

## Catatan Validasi
- Banyak dari list ini **overlap konsep dengan Debloat** — perlu keputusan desain: Service tab fokus ke "matikan proses background service level", sementara Debloat fokus ke "uninstall aplikasi/fitur UWP". Kalau ada tumpang tindih (misal DiagTrack juga relevan di Privacy), pilih taruh di satu tempat aja dan cross-reference.
- No. 5, 8, 9, 10 — sebelum apply, app idealnya deteksi dulu apakah relevan (misal jangan tawarin disable Bluetooth service kalau device emang gak punya adapter BT) — pola yang sama seperti hardware-specific check di kategori GPU/CPU.

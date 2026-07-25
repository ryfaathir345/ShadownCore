# Tweak List — Kategori Network

Format: **Nama** — Deskripsi — `Path/Value` — Risk Level

---

1. **Disable Nagle's Algorithm** — Mencegah TCP membundle paket kecil sebelum kirim, mengurangi latency 5-40ms. **Catatan penting:** cuma efektif untuk game berbasis TCP (MMO seperti WoW/FFXIV); game kompetitif modern (CS2, Valorant, Apex, Fortnite) pakai UDP jadi tweak ini TIDAK ada efek untuk mereka. App wajib kasih disclaimer ini biar user nggak salah ekspektasi. — Per-adapter di `HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{adapter-id}` → `TcpAckFrequency=1`, `TCPNoDelay=1` — Safe (tapi situational, cek dulu game-nya TCP atau UDP)
2. **Disable Network Throttling Index** — Menghilangkan cap bandwidth reservasi untuk proses multimedia/game. — `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile` → `NetworkThrottlingIndex=0xffffffff` — Moderate *(overlap dengan GPU tweak general, sama key)*
3. **Custom DNS (Cloudflare/Google)** — Ganti DNS default ISP (sering lebih lambat) dengan Cloudflare (1.1.1.1) atau Google (8.8.8.8) untuk resolusi domain lebih cepat. App bisa kasih 1-klik switch dengan beberapa pilihan provider. — Network Adapter Settings → DNS server addresses — Safe
4. **TCP Auto-Tuning Level** — Windows 11 modern sudah punya autotuning yang adaptif; JANGAN disable sepenuhnya (dulu populer di era Windows 7/8, sekarang justru merugikan). Kalau mau tuning, gunakan level "normal" bukan "disabled". — `netsh int tcp set global autotuninglevel=normal` — Safe (tapi app harus warning: JANGAN pilih "disabled")
5. **Set Network Adapter ke Maximum Performance** — Beberapa adapter punya power-saving mode yang bisa nambah micro-latency. Set ke performa maksimum di Device Manager. — Device Manager > Network Adapter > Advanced — Safe
6. **Disable Large Send Offload (LSO)** — Fitur adapter yang kadang menyebabkan latency spike/jitter di beberapa kombinasi driver+game, meski defaultnya untuk efisiensi CPU. — Device Manager > Adapter Advanced Settings → Large Send Offload = Disabled — Moderate (situational, tes dulu)
7. **Disable Network Adapter Power Saving** — Mencegah Windows mematikan sebagian adapter buat hemat daya, yang bisa nambah latency saat wake-up. — Device Manager > Adapter Properties > Power Management → uncheck "Allow the computer to turn off this device" — Safe
8. **QoS Packet Scheduler Reserve Bandwidth Limit** — Windows secara default reserve 20% bandwidth untuk QoS; bisa diturunkan ke 0% biar semua available untuk user traffic. Group Policy based, bukan pure registry. — `gpedit.msc` > Computer Config > Admin Templates > Network > QoS Packet Scheduler → Limit reservable bandwidth = 0% — Safe (butuh Pro/Enterprise edition untuk gpedit)
9. **Disable Windows Update P2P Delivery Optimization** — Windows Update bisa share/download update dari PC lain di jaringan lokal/internet ("Delivery Optimization"), yang bisa makan bandwidth di background tanpa disadari. — Settings > Windows Update > Delivery Optimization → Off — Safe
10. **Static IP alih-alih DHCP (Guidance)** — Mengurangi delay negosiasi DHCP saat reconnect, relevan untuk koneksi yang sering putus-nyambung. App kasih instruksi, bukan otomatis set (butuh input manual dari user sesuai jaringan mereka). — Guidance only — Safe, tapi user-specific

## Catatan Validasi
- No. 1 (Nagle's Algorithm) — paling sering disalahpahami. WAJIB ada UI yang jelasin "cek dulu game kamu pakai TCP atau UDP" sebelum apply, kalau nggak user bakal kecewa karena nggak ada efek.
- No. 4 — historically banyak guide lama (Windows 7 era) yang rekomendasiin disable autotuning sepenuhnya; di Windows 11 modern itu kontraproduktif. Pastikan app TIDAK reuse tweak lama ini secara verbatim.

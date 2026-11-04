<p align="center">
  <h1 align="center">⚔️ MD-Sync</h1>
  <p align="center">
    <strong>Công cụ đồng bộ dữ liệu đa tài khoản Yu-Gi-Oh! Master Duel</strong><br>
    Dùng chung dữ liệu tải về giữa nhiều tài khoản Steam mà không tốn thêm 1 MB dung lượng ổ cứng.
  </p>
  <p align="center">
    <a href="README_VI.md">🇻🇳 Tiếng Việt</a> | 
    <a href="README.md">🇬🇧 English</a>
  </p>
</p>

---

## 📌 Vấn đề & Giải pháp

- **Vấn đề:** Mỗi khi bạn đổi hoặc tạo tài khoản Steam mới để chơi *Yu-Gi-Oh! Master Duel* (reroll, clone, cày gem mở pack...), game lại bắt tải lại toàn bộ ~10 GB – 14 GB dữ liệu game (`0000`) vào thư mục riêng `LocalData\<Mã_Acc>\`.
- **Giải pháp:** **MD-Sync** giải quyết triệt để vấn đề này bằng tính năng **NTFS Directory Junction** (`mklink /J`) chính chủ của Windows. Ứng dụng sẽ liên kết thư mục dữ liệu của tài khoản mới trực tiếp vào dữ liệu của tài khoản chính.

### 🌟 Ưu điểm vượt trội:
- 🚀 **0 MB dung lượng phát sinh:** Dù bạn có 10 hay 50 tài khoản thì toàn bộ ổ cứng cũng chỉ tốn đúng dung lượng của 1 tài khoản gốc (~10GB - 13GB).
- 🔄 **Cập nhật 1 lần cho tất cả:** Khi game có update mới (pack bài mới, event mới), bạn chỉ cần dùng bất kỳ acc nào tải xong là tất cả các acc còn lại đều nhận update luôn, không cần tải lại dù chỉ 1 MB.
- ⚙️ **Dùng chung Cài đặt (`localsave`):** Tự động liên kết cả thư mục cấu hình cá nhân (độ phân giải màn hình, âm lượng, tốc độ duel), không bao giờ phải tốn công chỉnh lại setting mỗi khi vào acc mới.
- 🔍 **Tự động dò đường dẫn (Auto-detect):** Quét Registry Steam và các thư viện ổ đĩa phụ (`libraryfolders.vdf` ở ổ C, D, E...) để tự tìm thư mục cài đặt game.
- 🛡️ **An toàn & Sạch sẽ:** Không can thiệp bộ nhớ RAM, không hook DLL, không sửa file `.exe` của game. Không mã hóa, không dùng packer, an toàn 100% trước Windows Defender.

---

## 🚀 Hướng dẫn sử dụng

1. Tải file **`MD-Sync.exe`** mới nhất từ mục [Releases](../../releases).
2. Đăng nhập tài khoản Steam mới, mở game Master Duel lên cho đến màn hình Title (mục đích để game tự tạo thư mục tài khoản mới trong `LocalData`), sau đó tắt game.
3. Mở **`MD-Sync.exe`** lên. Tool sẽ tự động tìm thấy đường dẫn game và hiển thị danh sách tài khoản.
4. Bấm nút **`Sync All`**.
5. Mở lại game trên acc mới -> Thanh tải dữ liệu sẽ nhảy thẳng 100% và vào game ngay lập tức!

---

## 🖥️ Giao diện ứng dụng

Phần mềm được thiết kế theo phong cách tối giản, chuẩn Native Windows Utility:
- **Đường dẫn Master Duel:** Tự động nhận diện, có nút "Chọn..." để duyệt thủ công nếu cần.
- **Danh sách tài khoản:** Hiển thị rõ mã thư mục, dung lượng thực tế, và trạng thái (*Chính / Đã liên kết / Chưa liên kết*).
- **Thao tác 1-Click:** Tự động dọn dẹp data rác/dở dang của acc phụ và tạo liên kết Junction chỉ trong vòng 1-2 giây.

---

## 🛠️ Hướng dẫn tự Build từ mã nguồn

### Yêu cầu
- Đã cài [.NET 10.0 SDK](https://dotnet.microsoft.com/) trở lên.

### Lệnh biên dịch
```bash
# 1. Clone mã nguồn
git clone https://github.com/nyanomise/MD-Sync.git
cd MD-Sync

# 2. Xuất bản 1 file thực thi duy nhất (Self-Contained)
dotnet publish -c Release -o ./publish
```

File `.exe` độc lập hoàn chỉnh sẽ nằm trong thư mục:
`./publish/MD-Sync.exe`

---

## 📄 Giấy phép (License)

Dự án được phân phối dưới giấy phép [MIT License](LICENSE). 
Hoàn toàn miễn phí và mã nguồn mở.

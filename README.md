## 🧑‍🎓 Trải nghiệm người dùng và các chức năng
---

## 📑 Thông tin môn học

| Mục | Thông tin |
| :--- | :--- |
| **Môn học** | Hệ Thống Nhúng Mạng Không Dây |
| **Lớp** | NT131.Q24 |
| **Giảng viên** | ThS. Lê Minh Khánh Hội |
| **Nhóm thực hiện** | Nhóm 13 |
| **Nhóm trưởng** | Phạm Đức Tài |

---

## 👨‍💻 Thành viên Nhóm 12

| STT | Họ và tên | MSSV | Vai trò chính |
| :--- | :--- | :--- | :--- |
| 1 | **Phạm Đức Tài** | 24521557 | Nhóm trưởng / Backend |
| 2 | Huỳnh Vũ Khánh Hưng | 24520592 | Thành viên / Android App |
| 3 | Trần Sơn | 24521538 | Thành viên / Firmware ESP32 |
| 4 | Trần Thế Hiệp | 24520485 | Thành viên / Tích hợp SignalR WebSocket |

---
### 1. Tài khoản và bảo mật
Hệ thống cung cấp đầy đủ các chức năng quản lý người dùng:
* **Đăng ký tài khoản:** Xác thực định dạng email, kiểm tra trùng lặp `username`.
* **Bảo mật:** Mật khẩu được mã hóa một chiều bằng thuật toán **SHA-256** trước khi lưu vào SSMS.
* **Duy trì đăng nhập:** Sử dụng **JWT (JSON Web Token)**, lưu trữ an toàn tại `SharedPreferences` giúp người dùng không cần đăng nhập lại nhiều lần.

---

### 2. Dashboard — Màn hình chính
Nơi giám sát thời gian thực với giao diện trực quan:
* **Độ ẩm đất:** Hiển thị dạng biểu đồ.
* **Môi trường:** Theo dõi nhiệt độ và độ ẩm không khí từ cảm biến DHT.
* **Điều khiển nhanh:** Nút Toggle máy bơm ngay trên màn hình chính.
* **Kết nối:** Chỉ báo trạng thái **ESP32 Online/Offline** thời gian thực.
* **Công nghệ:** Cập nhật dữ liệu tức thời qua **SignalR (WebSocket)**, độ trễ cực thấp.

---

### 3. Chế độ điều khiển Máy bơm
Hệ thống hỗ trợ song song hai cơ chế điều khiển:

#### A. Điều khiển thủ công (Manual)
* Gửi lệnh trực tiếp từ App qua **REST API** xuống Backend và Forward tới ESP32.
* Tính năng **An toàn:** Tự động đếm thời gian bơm và hiển thị cảnh báo nếu bơm chạy liên tục quá 30 phút (tránh cháy bơm hoặc ngập úng).

#### B. Tưới tự động theo ngưỡng (Auto Threshold)
* Người dùng tự cấu hình **Ngưỡng dưới** (Kích hoạt) và **Ngưỡng trên** (Dừng).
* Backend xử lý logic tự động: Khi dữ liệu cảm biến gửi lên vi phạm ngưỡng, hệ thống tự ra lệnh cho ESP32 mà không cần mở App.
* Lưu nhật ký (Log) chi tiết cho các sự kiện tự động.

---

### 4. Lịch sử & Phân tích xu hướng
* **History:** Lưu trữ chi tiết mọi phiên tưới (Thời gian, thời lượng, độ ẩm lúc đó, hình thức tưới).
* **Analytics:** Biểu đồ đường (**Line Chart**) sử dụng thư viện `MPAndroidChart`, hỗ trợ lọc dữ liệu theo ngày/tuần/tháng.
* **Export:** Cho phép xuất báo cáo định dạng **CSV** để phục vụ nghiên cứu hoặc báo cáo đồ án.

---

## 🛠 Công nghệ sử dụng

| Tầng hệ thống | Công nghệ / Thiết bị |
| :--- | :--- |
| **Phần cứng** | ESP32, Soil Moisture Sensor, DHT11/22, Relay Module |
| **Firmware** | C++, Arduino Framework |
| **Backend** | ASP.NET Core 8.0 (C#), EF Core |
| **Database** | SQL Server (SSMS) |
| **Real-time** | SignalR (WebSocket) |
| **Mobile App** | Android (Kotlin/Java), Retrofit2, MPAndroidChart |
| **Thông báo** | Firebase Cloud Messaging (FCM) |

---

## 🚀 Hướng dẫn triển khai nhanh

1.  **Phần cứng:** Kết nối cảm biến và Relay vào ESP32. Cấu hình WiFi trong file config.h.
2.  **Database:** Chỉnh sửa ConnectionStrings trong appsettings.json của Backend, sau đó chạy lệnh `dotnet ef database update.
3.  **API:** Chạy Backend và copy địa chỉ IP/URL dán vào Constants của App Android.
4.  **Khởi động:** Nạp code cho ESP32 và cài đặt APK lên điện thoại để bắt đầu giám sát.

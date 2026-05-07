---

## 🧑‍🎓 Trải nghiệm người dùng và các chức năng

### 1. Tài khoản và bảo mật

Hệ thống cung cấp đầy đủ các chức năng về tài khoản:

- **Đăng ký tài khoản mới**
  - Nhập email, tên đăng nhập và mật khẩu
  - Hệ thống kiểm tra trùng email / tên đăng nhập
  - Tài khoản được lưu vào SQL Server với mật khẩu băm SHA-256

- **Đăng nhập**
  - Xác thực bằng tên đăng nhập và mật khẩu
  - Nhận JWT Token, lưu vào SharedPreferences để tự động đăng nhập lần sau

---

### 2. Dashboard — màn hình chính

Sau khi đăng nhập, người dùng được đưa đến **Dashboard**, nơi:

- Hiển thị **độ ẩm đất** dạng gauge tròn với màu sắc thay đổi theo mức độ
- Hiển thị **nhiệt độ và độ ẩm không khí** với icon trực quan
- Nút **toggle bật/tắt máy bơm** ngay tại Dashboard
- Chỉ báo trạng thái kết nối ESP32 (Online / Offline)
- Dữ liệu cập nhật **tự động trong vài giây** nhờ SignalR WebSocket — không cần reload

---

### 3. Điều khiển máy bơm thủ công

Chức năng dành cho người dùng muốn **can thiệp trực tiếp**:

- Nhấn nút **BẬT / TẮT bơm** — lệnh gửi qua REST API đến backend và xuống ESP32
- Hiển thị **thời gian bơm đã chạy** trong phiên hiện tại (đếm giây)
- Cảnh báo màu đỏ nếu bơm chạy liên tục quá 30 phút

---

### 4. Tưới tự động theo ngưỡng

Đối với những ai muốn **không cần can thiệp thủ công**:

- Người dùng cài đặt **ngưỡng độ ẩm thấp** (ví dụ 30%) để kích hoạt bơm
- Cài đặt **ngưỡng độ ẩm cao** (ví dụ 70%) để dừng bơm
- Backend tự động so sánh dữ liệu cảm biến với ngưỡng mỗi lần ESP32 gửi dữ liệu
- Mọi sự kiện tưới tự động được ghi log với lý do `auto_threshold`

---

### 5. Lịch sử tưới (History)

Phần **lịch sử** giúp người dùng xem lại:

- Danh sách các lần tưới theo thời gian mới nhất lên đầu
- Mỗi mục hiển thị: thời gian bắt đầu, thời lượng, loại tưới, độ ẩm lúc kích hoạt
- Badge phân biệt **Thủ công** (xanh) và **Tự động** (cam)
- Thống kê tổng: số lần tưới trong tháng, tổng thời gian hoạt động bơm

---

### 6. Biểu đồ & phân tích độ ẩm

Người dùng có thể **phân tích xu hướng** theo thời gian:

- Biểu đồ **Line Chart** trục X là thời gian, trục Y là phần trăm độ ẩm
- Lọc theo: **Hôm nay / 7 ngày / 30 ngày / Tùy chỉnh**
- Xuất dữ liệu dạng **CSV** để phân tích offline

---

### 7. Cài đặt ngưỡng & thiết bị

Người dùng tùy chỉnh hoạt động của hệ thống:

- **Slider** trực quan để cài ngưỡng thấp / cao
- Cài đặt **chu kỳ đọc cảm biến**: 10s / 30s / 60s / 5 phút
- Toggle bật/tắt **chế độ tưới tự động**
- Cài đặt được lưu trên server và đồng bộ xuống ESP32 trong lần giao tiếp tiếp theo

---

## 🛠 Công nghệ sử dụng

- **Phần cứng:** ESP32, cảm biến Soil Moisture, DHT11/DHT22, Module Relay
- **Firmware:** Arduino Framework (C++)
- **Backend:** ASP.NET Core 7.0, C#
- **ORM:** Entity Framework Core
- **Cơ sở dữ liệu:** SQL Server
- **Real-time:** SignalR WebSocket
- **Ứng dụng di động:** Android (Java/Kotlin)
- **HTTP Client (Android):** Retrofit2
- **Xác thực:** JWT Token
- **Bảo mật mật khẩu:** SHA-256

Ngoài ra, nhóm còn sử dụng:

- **[Firebase Cloud Messaging (FCM)](https://firebase.google.com/)** để gửi push notification
- **MPAndroidChart** để vẽ biểu đồ Line Chart trên Android
- **GitHub Actions** để tự động build và kiểm thử khi push code

---

## 🚀 Hướng dẫn triển khai từ source

Phần này dành cho những ai muốn **tự chạy toàn bộ hệ thống từ mã nguồn GitHub**.

### Yêu cầu môi trường

| Thành phần | Yêu cầu |
| --- | --- |
| ESP32 Firmware | Arduino IDE 2.x hoặc PlatformIO |
| Backend | .NET 7 SDK, SQL Server 2019+ |
| Android App | Android Studio, Android SDK 29+ |

### Bước 1 — Chuẩn bị phần cứng

- Đấu nối ESP32 với cảm biến độ ẩm đất (chân ADC) và DHT11 (chân GPIO)
- Kết nối relay với GPIO điều khiển máy bơm
- Nạp firmware từ thư mục `/firmware` vào ESP32
- Cập nhật `WIFI_SSID`, `WIFI_PASSWORD` và `SERVER_URL` trong file `config.h`

### Bước 2 — Chạy Backend

```bash
cd backend
# Cập nhật connection string trong appsettings.json
dotnet ef database update   # Tạo database và migration
dotnet run                  # Chạy server tại https://localhost:5001
```

### Bước 3 — Chạy ứng dụng Android

- Mở thư mục `/android` trong Android Studio
- Cập nhật `BASE_URL` trong `Constants.kt` thành địa chỉ server
- Build và cài đặt APK lên thiết bị hoặc máy ảo Android

### Bước 4 — Thử nghiệm

- Cấp nguồn ESP32 — kiểm tra log gửi dữ liệu mỗi 30 giây
- Mở app Android, đăng ký / đăng nhập
- Dashboard hiển thị dữ liệu cảm biến cập nhật tự động
- Thử bật/tắt bơm, cài đặt ngưỡng và xem lịch sử tưới

---

## 🎯 Mục đích & ý nghĩa đồ án

Đồ án **"Hệ thống tưới cây kết hợp phần mềm giám sát"** giúp nhóm:

- Áp dụng kiến thức **hệ thống nhúng và mạng không dây** vào bài toán IoT thực tế
- Hiểu cách thiết kế và triển khai **hệ thống IoT ba lớp** hoàn chỉnh
- Kết hợp nhiều mảng kiến thức:
  - Lập trình nhúng (ESP32, Arduino)
  - Truyền thông Wi-Fi và giao thức HTTP/WebSocket
  - Backend và cơ sở dữ liệu (ASP.NET Core, SQL Server)
  - Phát triển ứng dụng di động (Android)
- Rèn luyện kỹ năng **làm việc nhóm, phân chia công việc, tích hợp và kiểm thử**

Sản phẩm được thực hiện với mục đích **học tập** trong khuôn khổ môn học
*Hệ thống nhúng mạng không dây – NT131.Q24*.  
Nhóm rất mong nhận được góp ý từ giảng viên và các bạn để hệ thống ngày càng hoàn thiện hơn.

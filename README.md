<div align="center">

# 🏠 Unistay - Nền Tảng Tư Vấn Tìm Trọ Thông Minh

Ứng dụng web giúp người dùng tìm phòng trọ nhanh chóng, chính xác và tiết kiệm chi phí, kết hợp các tính năng tư vấn tự động, ghép bạn cùng ở, quản lý chuyển trọ, và kết nối các dịch vụ liên quan.

[Trang Chủ](#) • [Tính Năng](#-tính-năng) • [Cài Đặt](#-cài-đặt) • [Đóng Góp](#-đóng-góp)

</div>

---

## 📋 Mục Lục

- [Giới Thiệu](#-giới-thiệu)
- [Tính Năng](#-tính-năng)
- [Công Nghệ](#-công-nghệ)
- [Yêu Cầu Hệ Thống](#-yêu-cầu-hệ-thống)
- [Cài Đặt](#-cài-đặt)
- [Chạy Ứng Dụng](#-chạy-ứng-dụng)
- [Cấu Trúc Dự Án](#-cấu-trúc-dự-án)
- [API Endpoints](#-api-endpoints)
- [Đóng Góp](#-đóng-góp)
- [License](#-license)

---

## 💡 Giới Thiệu

**Unistay** là một nền tảng web toàn diện dành cho những người đang tìm kiếm phòng trọ. Với các tính năng thông minh, người dùng có thể:

- 🔍 Tìm kiếm phòng trọ theo tiêu chí cụ thể
- 💬 Giao tiếp trực tiếp với chủ nhà và những người ở chung
- 🤝 Được kết nối với những người bạn cùng sở thích để ở chung
- 📊 Quản lý các yêu cầu chuyển trọ và thanh lý đồ
- 🛒 Sử dụng marketplace để mua/bán đồ cũ
- ⭐ Xem đánh giá và bình luận từ những người khác

---

## ✨ Tính Năng

### Tìm Kiếm & Khám Phá

- ✅ Tìm kiếm phòng trọ nâng cao với bộ lọc đa chiều
- ✅ Xem chi tiết phòng trọ với hình ảnh và thông tin chủ nhà
- ✅ Lưu phòng yêu thích cho lần xem lại sau

### Ghép Bạn Ở Chung

- ✅ Tạo hồ sơ bạn cùng ở để tìm những người phù hợp
- ✅ Duyệt hồ sơ bạn cùng ở dựa trên tiêu chí
- ✅ Hệ thống gợi ý tự động dựa trên thông tin cá nhân

### Giao Tiếp & Đặt Phòng

- ✅ Chat trực tiếp với chủ nhà và bạn ở chung
- ✅ Lịch hẹn xem phòng
- ✅ Hệ thống đặt phòng online

### Dịch Vụ Bổ Sung

- ✅ Marketplace: Mua bán đồ cũ/liên quan đến ở trọ
- ✅ Dịch vụ chuyển trọ và thanh lý đồ
- ✅ Quản lý hồ sơ cá nhân và lịch sử hoạt động

### Tính Năng Quản Trị

- ✅ Dashboard cho quản trị viên quản lý nội dung
- ✅ Hệ thống báo cáo và xử lý vi phạm

---

## 🛠️ Công Nghệ

### Backend

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server
- **Authentication**:
  - ASP.NET Core Identity
  - JWT Bearer Token
  - Google OAuth 2.0
- **ORM**: Entity Framework Core 8.0

### Frontend

- **HTML/CSS/JavaScript**
- **Responsive Design**

### Khác

- **Email Service**: SendGrid (hoặc SMTP tương thích)
- **Version Control**: Git

---

## 📦 Yêu Cầu Hệ Thống

### Cần Cài Đặt

- ✅ **.NET 8.0 SDK** trở lên ([Tải tại đây](https://dotnet.microsoft.com/download/dotnet/8.0))
- ✅ **SQL Server 2019** hoặc cao hơn
  - Hoặc: SQL Server Express (miễn phí)
  - Hoặc: LocalDB (đã kèm với Visual Studio)
- ✅ **Visual Studio 2022** (được khuyên dùng) hoặc **VS Code**

### Tùy Chọn

- Git để clone repository
- Postman hoặc Insomnia để test API

---

## ⚙️ Cài Đặt

### 1. Clone Repository

```bash
git clone https://github.com/nhotungdo/unistay-web.git
cd unistay-web
```

### 2. Restore Dependencies

```bash
cd "Unistay Web/Unistay Web"
dotnet restore
```

### 3. Cấu Hình Database

**Sử dụng SQL Server Express (Khuyên dùng):**

Mở file `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UniStayDb;Trusted_Connection=true;"
  }
}
```

Hoặc cho SQL Server đầy đủ:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=UniStayDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### 4. Khởi Tạo Database

```bash
dotnet ef database update
```

Lệnh này sẽ:

- Tạo database `UniStayDb`
- Chạy tất cả migrations
- Khởi tạo schema

### 5. Cấu Hình Google OAuth (Tùy Chọn)

Chạy script PowerShell để cấu hình:

```powershell
.\setup-google-auth.ps1
```

Hoặc cấu hình thủ công trong `appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
  }
}
```

---

## 🚀 Chạy Ứng Dụng

### Sử dụng dotnet CLI

```bash
cd "Unistay Web/Unistay Web"
dotnet run
```

Ứng dụng sẽ khởi động tại `https://localhost:5001` hoặc `http://localhost:5000`

### Sử dụng Visual Studio 2022

1. Mở file `Unistay Web.sln`
2. Nhấn `F5` hoặc chọn **Debug → Start Debugging**

### Sử dụng VS Code

1. Mở thư mục `Unistay Web/Unistay Web`
2. Chạy lệnh:

```bash
dotnet run
```

---

## 📁 Cấu Trúc Dự Án

```
Unistay Web/
├── Controllers/              # Controllers MVC
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── RoomsController.cs
│   ├── BookingController.cs
│   ├── ChatController.cs
│   ├── AdminController.cs
│   └── ...
├── Models/                   # Entity Models
│   ├── User/
│   ├── Room/
│   ├── Booking/
│   ├── Chat/
│   ├── Marketplace/
│   └── ...
├── Views/                    # Razor Views
│   ├── Home/
│   ├── Account/
│   ├── Rooms/
│   ├── Shared/
│   └── ...
├── Data/                     # Database Context
│   └── ApplicationDbContext.cs
├── Services/                 # Business Logic
│   ├── EmailService.cs
│   └── IEmailService.cs
├── ViewModels/               # View Models
├── Migrations/               # EF Core Migrations
├── wwwroot/                  # Static Files
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json          # Cấu hình ứng dụng
├── appsettings.Development.json
├── Program.cs                # Startup configuration
└── Unistay Web.csproj        # Project file
```

---

## 🔌 API Endpoints

### Authentication

- `POST /api/account/register` - Đăng ký tài khoản
- `POST /api/account/login` - Đăng nhập
- `POST /api/account/logout` - Đăng xuất

### Rooms

- `GET /api/rooms` - Danh sách phòng trọ
- `GET /api/rooms/{id}` - Chi tiết phòng
- `POST /api/rooms` - Tạo phòng (chỉ chủ nhà)
- `PUT /api/rooms/{id}` - Cập nhật phòng
- `DELETE /api/rooms/{id}` - Xóa phòng

### Chat

- `GET /api/chat/{userId}` - Lấy tin nhắn
- `POST /api/chat/send` - Gửi tin nhắn
- `GET /api/chat/conversations` - Danh sách cuộc hội thoại

### Bookings

- `POST /api/bookings` - Tạo yêu cầu đặt phòng
- `GET /api/bookings/{id}` - Chi tiết đặt phòng
- `PUT /api/bookings/{id}/status` - Cập nhật trạng thái

### Roommates

- `GET /api/roommates` - Danh sách bạn cùng ở
- `POST /api/roommates` - Tạo hồ sơ bạn cùng ở
- `GET /api/roommates/recommendations` - Gợi ý bạn ở chung

---

## 🤝 Đóng Góp

Chúng tôi rất hân hạnh tiếp nhận đóng góp từ cộng đồng!

### Hướng Dẫn Đóng Góp

1. **Fork** repository này
2. **Tạo branch** cho tính năng mới (`git checkout -b feature/AmazingFeature`)
3. **Commit** thay đổi (`git commit -m 'Add some AmazingFeature'`)
4. **Push** lên branch (`git push origin feature/AmazingFeature`)
5. **Mở Pull Request** và mô tả những thay đổi của bạn

### Quy Tắc Đóng Góp

- Hãy đảm bảo code của bạn tuân theo convention của project
- Viết test cho các tính năng mới
- Cập nhật documentation nếu cần
- Hãy yêu quý và tôn trọng các contributor khác

---

## 📝 License

Dự án này được cấp phép dưới **MIT License** - xem file [LICENSE](LICENSE) để biết chi tiết.

---

## 📞 Liên Hệ & Hỗ Trợ

- 📧 Email: [support@unistay.com]
- 💬 Issues: [Mở một issue mới](https://github.com/nhotungdo/unistay-web/issues)
- 🐦 Twitter: [@UniStay]

---

## 🙏 Cảm Ơn

Cảm ơn bạn đã quan tâm đến dự án **Unistay**! Hãy ⭐ star repository này nếu bạn thấy hữu ích.

---

<div align="center">

**[⬆ Lên đầu](#-unistay---nền-tảng-tư-vấn-tìm-trọ-thông-minh)**

Made with ❤️ by the Unistay Team

</div>

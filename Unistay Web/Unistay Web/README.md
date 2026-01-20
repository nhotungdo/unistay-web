# Unistay Web - Student Accommodation Platform

## Tổng quan dự án
Unistay là nền tảng tìm trọ và kết nối sinh viên toàn diện, bao gồm các module chính:

### Các module đã triển khai:

1. **Tài khoản & Xác thực** (Account)
   - Đăng ký/Đăng nhập
   - Quản lý hồ sơ người dùng
   - Xác thực OTP, Email

2. **Tìm trọ** (Rooms)
   - Đăng phòng trọ
   - Tìm kiếm & lọc phòng
   - Đánh giá & phản hồi
   - Đặt lịch xem phòng

3. **Ghép bạn trọ** (Roommates)
   - Tạo hồ sơ tìm roommate
   - Matching & kết nối
   - Đánh giá độ phù hợp

4. **Chợ đồ cũ** (Marketplace)
   - Đăng bán đồ thanh lý
   - Tìm kiếm & mua đồ

5. **Chuyển trọ** (Moving)
   - Yêu cầu chuyển đồ
   - Kết nối đơn vị vận chuyển

6. **Tìm trọ hộ** (RoomFinder)
   - Yêu cầu tìm trọ hộ
   - Quản lý dịch vụ

7. **Chat & Thông báo** (Chat)
   - Nhắn tin 1-1
   - Thông báo realtime

8. **Quản trị** (Admin)
   - Quản lý người dùng
   - Quản lý nội dung
   - Báo cáo & thống kê

## Cấu trúc thư mục

```
Unistay Web/
├── Controllers/          # MVC Controllers
├── Models/              # Data models
│   ├── User/
│   ├── Room/
│   ├── Roommate/
│   ├── Marketplace/
│   ├── Moving/
│   ├── Service/
│   ├── Chat/
│   ├── Payment/
│   └── Report/
├── Views/               # Razor views
├── Data/                # DbContext
└── wwwroot/            # Static files
```

## Cài đặt

1. Restore packages:
```bash
dotnet restore
```

2. Update database connection string trong `appsettings.json`

3. Tạo database:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. Chạy ứng dụng:
```bash
dotnet run
```

## Công nghệ sử dụng

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- SQL Server
- Bootstrap 5
- jQuery

## Các tính năng cần triển khai tiếp

- [ ] Tích hợp Identity cho authentication
- [ ] Tích hợp SignalR cho chat realtime
- [ ] Tích hợp payment gateway
- [ ] Tích hợp Google Maps API
- [ ] Tích hợp AI cho matching & recommendation
- [ ] Upload ảnh/video
- [ ] Email service
- [ ] SMS OTP service
- [ ] Push notification

## License
Copyright © 2026 Unistay

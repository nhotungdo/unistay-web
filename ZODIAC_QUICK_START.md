# 🌟 Chức Năng Cung Hoàng Đạo - Quick Start

## 🚀 Chạy Ứng Dụng

```bash
cd "f:\EXE101-REVIEW\unistay-web\Unistay Web\Unistay Web"
dotnet build
dotnet run
```

## 📍 Truy Cập

### Giao Diện Web:
- **Trang chủ 12 cung:** https://localhost:7198/cung-hoang-dao
- **Tìm cung của bạn:** https://localhost:7198/cung-hoang-dao/tim-kiem
- **Chi tiết cung:** https://localhost:7198/cung-hoang-dao/chi-tiet/1
- **Tử vi hàng ngày:** https://localhost:7198/cung-hoang-dao/tu-vi-hang-ngay

### REST API:
- **Tất cả cung:** `GET /api/ZodiacApi`
- **Theo ID:** `GET /api/ZodiacApi/1`
- **Theo tên:** `GET /api/ZodiacApi/name/Aries`
- **Theo ngày sinh:** `GET /api/ZodiacApi/date/2000-03-25`
- **Theo yếu tố:** `GET /api/ZodiacApi/element/Fire`
- **Tử vi ngày:** `GET /api/ZodiacApi/horoscope/1`
- **Kiểm tra tương thích:** `GET /api/ZodiacApi/compatible/Aries/Leo`

## ✨ Tính Năng

- ✅ 12 cung hoàng đạo với thông tin đầy đủ
- ✅ Tìm kiếm cung theo ngày sinh
- ✅ Tử vi hàng ngày với điểm số tình yêu, sự nghiệp, sức khỏe, tài chính
- ✅ Kiểm tra độ tương thích
- ✅ Lọc theo yếu tố (Lửa, Đất, Khí, Nước)
- ✅ Giao diện đẹp, responsive với animations
- ✅ Memory caching cho hiệu suất cao
- ✅ RESTful API hoàn chỉnh
- ✅ Error handling và loading states

## 📚 Xem Thêm

Chi tiết đầy đủ: [ZODIAC_FEATURE_GUIDE.md](./ZODIAC_FEATURE_GUIDE.md)

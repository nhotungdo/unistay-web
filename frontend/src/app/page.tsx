"use client";

import { useState, useEffect, useCallback } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import {
  Search,
  MapPin,
  Filter,
  Ruler,
  Users,
  Eye,
  SearchX,
  ShieldCheck,
  Sparkles,
  Zap,
  ArrowRight,
} from 'lucide-react';
import { Spinner, Skeleton, AlertBanner, RoomImage } from '@/components/ui';
import {
  fetchRooms,
  resolveImageUrl,
  formatPrice,
  formatArea,
  ApiRoomFilter,
  RoomWithDetails,
} from '@/lib/rooms';
import './home.css';

const PRICE_OPTIONS = [
  { value: '', label: 'Mọi mức giá' },
  { value: 'under2m', label: 'Dưới 2 triệu' },
  { value: '2m-4m', label: '2 - 4 triệu' },
  { value: 'above4m', label: 'Trên 4 triệu' },
];

export default function Home() {
  const router = useRouter();
  const [location, setLocation] = useState('');
  const [price, setPrice] = useState('');
  const [rooms, setRooms] = useState<RoomWithDetails[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadRooms = useCallback(async (filter: ApiRoomFilter) => {
    try {
      const data = await fetchRooms(filter);
      setRooms(data.slice(0, 6));
    } catch {
      setError('Không thể tải danh sách phòng. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await fetchRooms({});
        if (!cancelled) setRooms(data.slice(0, 6));
      } catch {
        if (!cancelled) {
          setError('Không thể tải danh sách phòng. Vui lòng thử lại sau.');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (location.trim()) params.set('location', location.trim());
    if (price) params.set('price', price);
    router.push(`/rooms${params.size > 0 ? `?${params.toString()}` : ''}`);
  };

  const handleRetry = () => {
    setLoading(true);
    loadRooms({});
  };

  return (
    <div className="home-container">
      {/* Hero Section */}
      <section className="hero">
        <div className="hero-content">
          <h1>
            Tìm Không Gian Sống <span className="hero-highlight">Tuyệt Vời</span> Của Bạn
          </h1>
          <p>
            Khám phá hàng ngàn phòng trọ chất lượng và tìm người ở ghép phù hợp nhất
            với bạn ngay hôm nay.
          </p>

          <form className="search-box" onSubmit={handleSearch} role="search">
            <div className="search-input-group">
              <MapPin className="search-icon" size={20} />
              <input
                type="text"
                placeholder="Bạn muốn thuê phòng ở đâu?"
                className="search-input"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                aria-label="Tìm theo vị trí"
              />
            </div>
            <div className="search-input-group">
              <Filter className="search-icon" size={20} />
              <select
                className="search-input"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                aria-label="Lọc theo giá"
              >
                {PRICE_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
            <button type="submit" className="btn-primary search-btn">
              <Search size={20} />
              <span>Tìm kiếm</span>
            </button>
          </form>

          <div className="hero-stats">
            <div className="hero-stat">
              <strong>1.000+</strong>
              <span>Phòng trọ</span>
            </div>
            <div className="hero-stat">
              <strong>500+</strong>
              <span>Sinh viên kết nối</span>
            </div>
            <div className="hero-stat">
              <strong>24/7</strong>
              <span>Hỗ trợ</span>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features container">
        <h2>Tại sao chọn Unistay?</h2>
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">
              <Search size={28} />
            </div>
            <h3>Tìm Kiếm Dễ Dàng</h3>
            <p>
              Bộ lọc thông minh giúp bạn tìm được phòng ưng ý với giá cả và vị trí
              phù hợp nhất.
            </p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">
              <Sparkles size={28} />
            </div>
            <h3>Ở Ghép Phù Hợp</h3>
            <p>
              Hệ thống AI phân tích tính cách, thói quen và cung hoàng đạo để đề
              xuất người ở ghép hoàn hảo.
            </p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">
              <ShieldCheck size={28} />
            </div>
            <h3>An Toàn &amp; Bảo Mật</h3>
            <p>
              Mọi tin đăng và người dùng đều được xác thực để đảm bảo môi trường
              sống an toàn.
            </p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">
              <Zap size={28} />
            </div>
            <h3>Kết Nối Nhanh Chóng</h3>
            <p>
              Nhắn tin trực tiếp với chủ phòng hoặc người ở ghép tiềm năng trong
              vài giây.
            </p>
          </div>
        </div>
      </section>

      {/* Recommended Rooms */}
      <section className="recommended container">
        <div className="section-header">
          <h2>Phòng Trọ Nổi Bật</h2>
          <Link href="/rooms" className="view-all">
            Xem tất cả <ArrowRight size={16} />
          </Link>
        </div>

        {loading ? (
          <div className="rooms-grid">
            {[1, 2, 3].map((i) => (
              <div key={i} className="room-card">
                <Skeleton className="room-image-skeleton" />
                <div className="room-info">
                  <Skeleton className="skeleton-line w-40" />
                  <Skeleton className="skeleton-line" />
                  <Skeleton className="skeleton-line w-60" />
                </div>
              </div>
            ))}
          </div>
        ) : error ? (
          <AlertBanner variant="error">
            <span>{error}</span>
            <button type="button" className="alert-retry" onClick={handleRetry}>
              <Spinner size={14} /> Thử lại
            </button>
          </AlertBanner>
        ) : rooms.length === 0 ? (
          <div className="empty-state">
            <SearchX size={40} />
            <h3>Chưa có phòng nào</h3>
            <p>Hãy là người đầu tiên đăng tin cho thuê trên Unistay!</p>
            <Link href="/post" className="btn-primary">
              Đăng tin ngay
            </Link>
          </div>
        ) : (
          <div className="rooms-grid">
            {rooms.map(({ room, images }) => {
              const primary =
                images.find((img) => img.isPrimary) ??
                images.sort((a, b) => a.displayOrder - b.displayOrder)[0];
              return (
                <Link key={room.id} href={`/rooms/${room.id}`} className="room-card">
                  <div className="room-image-placeholder">
                    <RoomImage
                      src={resolveImageUrl(primary?.imageUrl)}
                      alt={room.title}
                      className="room-image"
                    />
                    {room.isVIP && <span className="badge vip">VIP</span>}
                    {room.isFeatured && !room.isVIP && (
                      <span className="badge">Nổi bật</span>
                    )}
                  </div>
                  <div className="room-info">
                    <div className="room-price">
                      {formatPrice(room.price)}
                      <span>/tháng</span>
                    </div>
                    <h3 className="room-title">{room.title}</h3>
                    <div className="room-location">
                      <MapPin size={14} />
                      <span>{room.address}</span>
                    </div>
                    <div className="room-meta">
                      <span>
                        <Ruler size={14} /> {formatArea(room.area)}
                      </span>
                      <span>
                        <Users size={14} /> Tối đa {room.maxOccupants} người
                      </span>
                      <span>
                        <Eye size={14} /> {room.viewCount}
                      </span>
                    </div>
                  </div>
                </Link>
              );
            })}
          </div>
        )}
      </section>

      {/* CTA Section */}
      <section className="cta-section">
        <div className="container cta-content">
          <h2>Bạn có phòng muốn cho thuê?</h2>
          <p>
            Đăng tin miễn phí trên Unistay để tiếp cận hàng ngàn người thuê tiềm
            năng.
          </p>
          <Link href="/post" className="btn-primary btn-lg">
            Đăng tin ngay <ArrowRight size={18} />
          </Link>
        </div>
      </section>
    </div>
  );
}

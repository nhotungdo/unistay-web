"use client";

import { useState, useEffect, useCallback, Suspense } from 'react';
import Link from 'next/link';
import { useSearchParams, useRouter } from 'next/navigation';
import { Search, MapPin, Filter, Ruler, Users, Eye, SearchX } from 'lucide-react';
import { Spinner, Skeleton, AlertBanner, RoomImage } from '@/components/ui';
import { fetchRooms, resolveImageUrl, formatPrice, formatArea, RoomWithDetails } from '@/lib/rooms';
import '../home.css';
import './rooms.css';

const PRICE_OPTIONS = [
  { value: '', label: 'Mọi mức giá' },
  { value: 'under2m', label: 'Dưới 2 triệu' },
  { value: '2m-4m', label: '2 - 4 triệu' },
  { value: 'above4m', label: 'Trên 4 triệu' },
];

function RoomsContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  
  const [rooms, setRooms] = useState<RoomWithDetails[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  const [location, setLocation] = useState(searchParams.get('location') || '');
  const [price, setPrice] = useState(searchParams.get('price') || '');

  const loadRooms = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await fetchRooms({
        location: location.trim(),
        price: price
      });
      setRooms(data);
    } catch {
      setError('Không thể tải danh sách phòng. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  }, [location, price]);

  useEffect(() => {
     
    loadRooms();
  }, [loadRooms]);

  const handleApplyFilter = () => {
    const params = new URLSearchParams();
    if (location.trim()) params.set('location', location.trim());
    if (price) params.set('price', price);
    router.push(`/rooms${params.size > 0 ? `?${params.toString()}` : ''}`);
  };

  const handleResetFilter = () => {
    setLocation('');
    setPrice('');
    router.push('/rooms');
  };

  return (
    <div className="container rooms-page-container fade-in">
      <div className="rooms-layout">
        
        {/* Filters Sidebar */}
        <aside className="filter-sidebar">
          <div className="filter-header">
            <h2 className="filter-title"><Filter size={20} /> Lọc kết quả</h2>
            <button onClick={handleResetFilter} className="reset-filter-btn">Xóa lọc</button>
          </div>
          
          <div className="filter-section">
            <h3 className="filter-section-title">Vị trí</h3>
            <div className="input-icon-wrap">
              <MapPin size={18} className="input-icon" />
              <input
                type="text"
                className="form-input has-icon"
                placeholder="Nhập khu vực, tên đường..."
                value={location}
                onChange={(e) => setLocation(e.target.value)}
              />
            </div>
          </div>
          
          <div className="filter-section">
            <h3 className="filter-section-title">Mức giá</h3>
            <select
              className="form-input"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
            >
              {PRICE_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
          
          <button onClick={handleApplyFilter} className="btn-primary w-full mt-4">
            Áp dụng
          </button>
        </aside>

        {/* Results */}
        <div className="rooms-results">
          <div className="rooms-results-header">
            <h1 className="results-count">
              {loading ? 'Đang tìm kiếm...' : `Tìm thấy ${rooms.length} phòng trọ`}
            </h1>
          </div>
          
          {loading ? (
            <div className="rooms-grid-large">
              {[1, 2, 3, 4, 5, 6].map((i) => (
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
              <button type="button" className="alert-retry" onClick={loadRooms}>
                Thử lại
              </button>
            </AlertBanner>
          ) : rooms.length === 0 ? (
            <div className="empty-state">
              <SearchX size={48} />
              <h3>Không tìm thấy phòng nào phù hợp</h3>
              <p>Hãy thử thay đổi điều kiện tìm kiếm hoặc xóa bộ lọc.</p>
              <button onClick={handleResetFilter} className="btn-outline mt-4">Xóa bộ lọc</button>
            </div>
          ) : (
            <div className="rooms-grid-large">
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
        </div>
      </div>
    </div>
  );
}

export default function RoomsPage() {
  return (
    <Suspense fallback={<div className="container" style={{ padding: '5rem', textAlign: 'center' }}><Spinner size={40} /></div>}>
      <RoomsContent />
    </Suspense>
  );
}

"use client";

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { 
  MapPin, Ruler, Users, Eye, CheckCircle2, 
  MessageSquare, Calendar, Heart, Share2
} from 'lucide-react';
import api from '@/lib/api';
import { resolveImageUrl, formatPrice, formatArea, RoomWithDetails } from '@/lib/rooms';
import { Spinner, AlertBanner } from '@/components/ui';
import './room-detail.css';

export default function RoomDetailPage() {
  const { id } = useParams();
  const router = useRouter();
  
  const [data, setData] = useState<RoomWithDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) return;
    
    api.get(`/Rooms/${id}`)
      .then(res => {
        setData(res.data);
      })
      .catch(err => {
        setError('Không thể tải thông tin phòng hoặc phòng không tồn tại.');
      })
      .finally(() => {
        setLoading(false);
      });
  }, [id]);

  if (loading) {
    return <div className="container" style={{ padding: '5rem', textAlign: 'center' }}><Spinner size={40} /></div>;
  }

  if (error || !data) {
    return (
      <div className="container" style={{ padding: '5rem' }}>
        <AlertBanner variant="error">{error}</AlertBanner>
        <button className="btn-outline mt-4" onClick={() => router.push('/rooms')}>Quay lại danh sách</button>
      </div>
    );
  }

  const { room, images, owner } = data;
  
  // Sort images so primary is first
  const sortedImages = [...images].sort((a, b) => {
    if (a.isPrimary) return -1;
    if (b.isPrimary) return 1;
    return a.displayOrder - b.displayOrder;
  });

  const primaryImage = sortedImages[0];
  const otherImages = sortedImages.slice(1, 3); // Get up to 2 other images

  const amenitiesList = room.amenities ? room.amenities.split(',') : [];

  return (
    <div className="container room-detail-container fade-in">
      
      {/* Image Gallery */}
      <div className="gallery-section">
        {primaryImage ? (
          <img 
            src={resolveImageUrl(primaryImage.imageUrl) || ''} 
            alt={room.title} 
            className="gallery-main"
          />
        ) : (
          <div className="gallery-main img-fallback"></div>
        )}
        
        <div className="gallery-side">
          {otherImages.map((img, idx) => (
            <img 
              key={idx}
              src={resolveImageUrl(img.imageUrl) || ''} 
              alt={`${room.title} - ${idx + 2}`} 
              className="gallery-side-img"
            />
          ))}
          {/* Fill remaining slots with fallbacks if less than 2 extra images */}
          {Array.from({ length: Math.max(0, 2 - otherImages.length) }).map((_, idx) => (
            <div key={`fallback-${idx}`} className="gallery-side-img img-fallback"></div>
          ))}
        </div>
      </div>

      <div className="room-content-layout">
        
        {/* Main Info */}
        <div className="room-main-info">
          <div className="room-header">
            <h1 className="room-title-large">{room.title}</h1>
            <div className="room-price-large">{formatPrice(room.price)} / tháng</div>
            <div className="room-location-large">
              <MapPin size={18} /> {room.address}
            </div>
          </div>

          <div className="room-specs">
            <div className="spec-item">
              <Ruler size={18} className="text-primary" />
              <span>{formatArea(room.area)}</span>
            </div>
            <div className="spec-item">
              <Users size={18} className="text-primary" />
              <span>Tối đa {room.maxOccupants} người</span>
            </div>
            {room.deposit && (
              <div className="spec-item">
                <CheckCircle2 size={18} className="text-primary" />
                <span>Cọc: {formatPrice(room.deposit)}</span>
              </div>
            )}
            <div className="spec-item">
              <Eye size={18} className="text-primary" />
              <span>{room.viewCount} lượt xem</span>
            </div>
          </div>

          <div className="room-section">
            <h3>Mô tả chi tiết</h3>
            <div className="room-description">{room.description}</div>
          </div>

          {amenitiesList.length > 0 && (
            <div className="room-section">
              <h3>Tiện ích có sẵn</h3>
              <div className="amenities-grid">
                {amenitiesList.map((am, idx) => (
                  <div key={idx} className="amenity-tag">
                    <CheckCircle2 size={16} /> {am.trim()}
                  </div>
                ))}
              </div>
            </div>
          )}

          {room.rules && (
            <div className="room-section">
              <h3>Nội quy phòng</h3>
              <div className="room-description">{room.rules}</div>
            </div>
          )}
        </div>

        {/* Sidebar */}
        <aside className="owner-sidebar">
          <h3>Thông tin liên hệ</h3>
          
          <div className="owner-profile mt-4">
            <img 
              src={owner?.avatarUrl ? resolveImageUrl(owner.avatarUrl)! : '/default-avatar.svg'} 
              alt="Owner" 
              className="owner-avatar"
            />
            <div className="owner-info">
              <h4>{owner?.fullName || 'Người dùng Unistay'}</h4>
              <p>Chủ phòng</p>
            </div>
          </div>
          
          <div className="action-buttons">
            <button className="btn-primary w-full">
              <MessageSquare size={18} /> Nhắn tin ngay
            </button>
            <button className="btn-secondary w-full">
              <Calendar size={18} /> Đặt lịch xem phòng
            </button>
            
            <div className="form-grid mt-4">
              <button className="btn-outline w-full" onClick={() => alert('Đã lưu vào Yêu thích!')}>
                <Heart size={18} /> Lưu phòng
              </button>
              <button className="btn-outline w-full" onClick={() => navigator.clipboard.writeText(window.location.href)}>
                <Share2 size={18} /> Chia sẻ
              </button>
            </div>
          </div>
        </aside>
        
      </div>
    </div>
  );
}

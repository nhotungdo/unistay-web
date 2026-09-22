"use client";

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { Search, MapPin, SlidersHorizontal, UserPlus } from 'lucide-react';
import api from '@/lib/api';
import { formatPrice } from '@/lib/rooms';
import { Spinner, AlertBanner } from '@/components/ui';
import './roommates.css';

interface Roommate {
  id: number;
  userId: string;
  fullName: string;
  avatarUrl?: string;
  age: number;
  gender: string;
  occupation: string;
  budget: number;
  preferredArea: string;
  habits: string[];
  matchPercentage: number;
}

export default function RoommatesPage() {
  const [roommates, setRoommates] = useState<Roommate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Filters
  const [searchString, setSearchString] = useState('');
  const [gender, setGender] = useState('All');
  const [budgetRange, setBudgetRange] = useState('All');

  const fetchRoommates = async () => {
    setLoading(true);
    setError('');
    try {
      const params = new URLSearchParams();
      if (searchString) params.append('searchString', searchString);
      if (gender !== 'All') params.append('gender', gender);
      if (budgetRange !== 'All') params.append('budgetRange', budgetRange);

      const res = await api.get(`/Roommates?${params.toString()}`);
      setRoommates(res.data);
    } catch (err) {
      setError('Lỗi tải danh sách người tìm phòng.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRoommates();
     
  }, [gender, budgetRange]); // re-fetch when these change

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchRoommates();
  };

  const handleConnect = async (userId: string) => {
    try {
      await api.post('/Connections/request', { targetUserId: userId });
      alert('Đã gửi lời mời kết nối thành công!');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Có lỗi xảy ra');
    }
  };

  return (
    <div className="container roommates-container fade-in">
      <div className="roommates-header">
        <h1 className="roommates-title">Tìm bạn ở ghép lý tưởng</h1>
        <p className="roommates-desc">
          Kết nối với những người có chung lối sống, sở thích và cùng ngân sách để chia sẻ không gian sống tuyệt vời.
        </p>
      </div>

      <div className="roommates-filters">
        <form onSubmit={handleSearch} className="filter-group" style={{ flex: '1 1 250px' }}>
          <div className="input-icon-wrap w-full">
            <Search size={18} className="input-icon" />
            <input 
              type="text" 
              className="form-input has-icon" 
              placeholder="Tìm theo khu vực..." 
              value={searchString}
              onChange={e => setSearchString(e.target.value)}
            />
          </div>
        </form>

        <div className="filter-group">
          <label className="filter-label">Giới tính:</label>
          <select className="form-input" value={gender} onChange={e => setGender(e.target.value)}>
            <option value="All">Tất cả</option>
            <option value="Male">Nam</option>
            <option value="Female">Nữ</option>
          </select>
        </div>

        <div className="filter-group">
          <label className="filter-label">Ngân sách:</label>
          <select className="form-input" value={budgetRange} onChange={e => setBudgetRange(e.target.value)}>
            <option value="All">Mọi mức giá</option>
            <option value="low">Dưới 2 triệu</option>
            <option value="medium">2 - 4 triệu</option>
            <option value="high">Trên 4 triệu</option>
          </select>
        </div>

        <button className="btn-primary" onClick={fetchRoommates}>
          <SlidersHorizontal size={18} /> Lọc
        </button>
      </div>

      {loading ? (
        <div className="text-center" style={{ padding: '3rem' }}>
          <Spinner size={40} />
        </div>
      ) : error ? (
        <AlertBanner variant="error">{error}</AlertBanner>
      ) : roommates.length === 0 ? (
        <div className="empty-state">
          <UserPlus size={48} className="text-muted mb-4" />
          <h3>Không tìm thấy kết quả phù hợp</h3>
          <p>Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm.</p>
        </div>
      ) : (
        <div className="roommate-grid">
          {roommates.map(rm => (
            <div key={rm.id} className="roommate-card">
              <Link href={`/roommates/${rm.id}`} style={{ display: 'contents' }}>
                <div className="rm-card-header">
                  <div className="rm-avatar-wrapper">
                    <img 
                      src={rm.avatarUrl ? `http://localhost:8386${rm.avatarUrl}` : '/default-avatar.svg'} 
                      alt={rm.fullName} 
                      className="rm-avatar" 
                      onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                    />
                  </div>
                </div>
                
                <div className="rm-card-body">
                  <h3 className="rm-name">{rm.fullName}, {rm.age}</h3>
                  <div className="rm-info">{rm.occupation || 'Chưa cập nhật nghề nghiệp'}</div>
                  
                  <div className="rm-tags">
                    {rm.habits.slice(0, 3).map((habit, idx) => (
                      <span key={idx} className="rm-tag">{habit}</span>
                    ))}
                    {rm.habits.length > 3 && <span className="rm-tag">+{rm.habits.length - 3}</span>}
                  </div>
                  
                  <div className="rm-stats">
                    <div className="rm-stat-item">
                      <span className="rm-stat-label">Khu vực</span>
                      <span className="rm-stat-value"><MapPin size={14} style={{display:'inline', marginBottom:'-2px'}}/> {rm.preferredArea || 'Bất kỳ'}</span>
                    </div>
                    <div className="rm-stat-item">
                      <span className="rm-stat-label">Ngân sách</span>
                      <span className="rm-stat-value highlight">{formatPrice(rm.budget)}</span>
                    </div>
                  </div>
                </div>
              </Link>
              
              <div className="rm-card-footer">
                <button 
                  className="btn-connect" 
                  onClick={(e) => {
                    e.preventDefault(); // Prevent navigating to detail
                    handleConnect(rm.userId);
                  }}
                >
                  <UserPlus size={18} /> Kết nối ngay
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

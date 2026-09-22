"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Sparkles, UserPlus, FileText } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { formatPrice } from '@/lib/rooms';
import { Spinner, AlertBanner } from '@/components/ui';
import './match.css';

interface MatchResult {
  id: number;
  userId: string;
  fullName: string;
  avatarUrl?: string;
  age: number;
  gender: string;
  occupation: string;
  budget: number;
  preferredArea: string;
  matchPercentage: number;
}

export default function MatchmakingPage() {
  const router = useRouter();
  const { user, loading } = useAuth();
  
  const [matches, setMatches] = useState<MatchResult[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchMatches = async () => {
    setIsLoading(true);
    try {
      const res = await api.get('/Roommates/Match');
      setMatches(res.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Lỗi khi phân tích dữ liệu AI.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login?redirect=/roommates/match');
    } else if (user) {
      fetchMatches();
    }
  }, [user, loading, router]);

  const handleConnect = async (userId: string) => {
    try {
      await api.post('/Connections/request', { targetUserId: userId });
      alert('Đã gửi lời mời kết nối thành công!');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Có lỗi xảy ra');
    }
  };

  const viewDetail = (id: number) => {
    router.push(`/roommates/${id}`);
  };

  if (loading || isLoading) {
    return (
      <div className="container match-container">
        <div className="match-header fade-in">
          <div className="magic-circle">
            <div className="magic-circle-inner">
              <Sparkles size={48} />
            </div>
          </div>
          <h1 className="match-title" style={{fontSize: '2rem'}}>AI đang phân tích dữ liệu...</h1>
          <p className="match-subtitle">Hệ thống đang đối chiếu Cung Hoàng Đạo, thói quen và ngân sách của bạn với hàng ngàn người dùng khác.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container match-container fade-in">
      <div className="match-header">
        <h1 className="match-title">Kết quả Ghép đôi từ AI</h1>
        <p className="match-subtitle">
          Dưới đây là những người dùng có độ tương thích cao nhất với bạn dựa trên các tiêu chí sinh hoạt và tử vi.
        </p>
      </div>

      {error ? (
        <AlertBanner variant="error">{error}</AlertBanner>
      ) : matches.length === 0 ? (
        <div className="empty-state">
          <Sparkles size={48} className="text-muted mb-4" />
          <h3>Chưa tìm thấy người phù hợp</h3>
          <p>Hãy cập nhật thêm thói quen và sở thích trong hồ sơ để AI gợi ý tốt hơn.</p>
          <button className="btn-primary mt-4" onClick={() => router.push('/profile')}>Cập nhật hồ sơ</button>
        </div>
      ) : (
        <div className="match-grid">
          {matches.map(match => (
            <div key={match.id} className="match-card">
              <div className="match-score-badge">
                {match.matchPercentage}%
                <span>Tương hợp</span>
              </div>
              
              <div className="match-profile">
                <img 
                  src={match.avatarUrl ? `http://localhost:8386${match.avatarUrl}` : '/default-avatar.svg'} 
                  alt={match.fullName} 
                  className="match-avatar"
                  onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                />
                <div>
                  <h3 className="match-name">{match.fullName}</h3>
                  <div className="match-info">{match.age} tuổi • {match.gender === 'Male' ? 'Nam' : match.gender === 'Female' ? 'Nữ' : 'Khác'}</div>
                </div>
              </div>

              <div className="match-stats">
                <div className="match-stat-row">
                  <span className="match-stat-label">Khu vực:</span>
                  <span className="match-stat-value">{match.preferredArea || 'Bất kỳ'}</span>
                </div>
                <div className="match-stat-row">
                  <span className="match-stat-label">Ngân sách:</span>
                  <span className="match-stat-value" style={{color: 'var(--primary)'}}>{formatPrice(match.budget)}</span>
                </div>
                <div className="match-stat-row">
                  <span className="match-stat-label">Nghề nghiệp:</span>
                  <span className="match-stat-value">{match.occupation || 'Chưa cập nhật'}</span>
                </div>
              </div>

              <div className="match-actions">
                <button className="btn-ai-view" onClick={() => viewDetail(match.id)}>
                  <FileText size={18} style={{marginRight: 6}}/> Xem chi tiết
                </button>
                <button 
                  className="btn-primary" 
                  style={{display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6, border: 'none'}}
                  onClick={() => handleConnect(match.userId)}
                >
                  <UserPlus size={18} /> Kết nối
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

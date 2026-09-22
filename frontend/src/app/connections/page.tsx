"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { 
  Users, UserPlus, Lightbulb, 
  Check, X, MessageCircle
} from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner } from '@/components/ui';
import './connections.css';

type Tab = 'suggestions' | 'pending' | 'friends';

export default function ConnectionsPage() {
  const router = useRouter();
  const { user, loading } = useAuth();
  
  const [activeTab, setActiveTab] = useState<Tab>('suggestions');
  
  const [suggestions, setSuggestions] = useState<any[]>([]);
  const [pending, setPending] = useState<any[]>([]);
  const [friends, setFriends] = useState<any[]>([]);
  
  const [isLoading, setIsLoading] = useState(true);

  const fetchAllData = async () => {
    setIsLoading(true);
    try {
      const [sugRes, penRes, friRes] = await Promise.all([
        api.get('/Connections/suggestions'),
        api.get('/Connections/pending'),
        api.get('/Connections/friends')
      ]);
      setSuggestions(sugRes.data);
      setPending(penRes.data);
      setFriends(friRes.data);
    } catch (err) {
      console.error("Lỗi khi tải dữ liệu kết nối:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login?redirect=/connections');
    } else if (user) {
      fetchAllData();
    }
  }, [user, loading, router]);

  const handleRequest = async (userId: string) => {
    try {
      await api.post('/Connections/request', { targetUserId: userId });
      alert('Đã gửi lời mời!');
      fetchAllData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Lỗi gửi yêu cầu');
    }
  };

  const handleRespond = async (connectionId: number, action: 'accept' | 'reject') => {
    try {
      await api.post('/Connections/respond', { connectionId, action });
      fetchAllData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Lỗi phản hồi');
    }
  };

  if (loading || isLoading) {
    return <div className="container" style={{ padding: '5rem', textAlign: 'center' }}><Spinner size={40} /></div>;
  }

  return (
    <div className="container connections-container fade-in">
      <div className="connections-layout">
        
        {/* Sidebar */}
        <aside className="connections-sidebar">
          <h2 className="sidebar-title">Kết nối</h2>
          <nav className="conn-nav">
            <div 
              className={`conn-nav-item ${activeTab === 'suggestions' ? 'active' : ''}`}
              onClick={() => setActiveTab('suggestions')}
            >
              <div className="conn-nav-label"><Lightbulb size={18} /> Gợi ý kết bạn</div>
            </div>
            
            <div 
              className={`conn-nav-item ${activeTab === 'pending' ? 'active' : ''}`}
              onClick={() => setActiveTab('pending')}
            >
              <div className="conn-nav-label"><UserPlus size={18} /> Lời mời chờ duyệt</div>
              {pending.length > 0 && <span className="conn-badge">{pending.length}</span>}
            </div>
            
            <div 
              className={`conn-nav-item ${activeTab === 'friends' ? 'active' : ''}`}
              onClick={() => setActiveTab('friends')}
            >
              <div className="conn-nav-label"><Users size={18} /> Bạn bè</div>
              <span>{friends.length}</span>
            </div>
          </nav>
        </aside>

        {/* Content */}
        <div className="connections-content">
          
          {activeTab === 'suggestions' && (
            <div className="fade-in">
              <h3 className="text-xl font-bold mb-6">Có thể bạn quen</h3>
              
              {suggestions.length === 0 ? (
                <div className="empty-state">
                  <Lightbulb size={48} className="text-muted mb-4" />
                  <p>Không có gợi ý mới lúc này.</p>
                </div>
              ) : (
                <div className="conn-grid">
                  {suggestions.map(s => (
                    <div key={s.id} className="conn-card">
                      <img 
                        src={s.avatarUrl ? `http://localhost:8386${s.avatarUrl}` : '/default-avatar.svg'} 
                        alt={s.name} 
                        className="conn-avatar"
                        onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                      />
                      <h4 className="conn-name">{s.name}</h4>
                      <p className="conn-meta">{s.mutualInfo}</p>
                      <button className="btn-primary w-full btn-sm" onClick={() => handleRequest(s.id)}>
                        Thêm bạn bè
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'pending' && (
            <div className="fade-in">
              <h3 className="text-xl font-bold mb-6">Lời mời chờ duyệt</h3>
              
              {pending.length === 0 ? (
                <div className="empty-state">
                  <UserPlus size={48} className="text-muted mb-4" />
                  <p>Không có lời mời kết bạn nào.</p>
                </div>
              ) : (
                <div className="conn-grid">
                  {pending.map(p => (
                    <div key={p.connectionId} className="conn-card">
                      <img 
                        src={p.requesterAvatar ? `http://localhost:8386${p.requesterAvatar}` : '/default-avatar.svg'} 
                        alt={p.requesterName} 
                        className="conn-avatar"
                        onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                      />
                      <h4 className="conn-name">{p.requesterName}</h4>
                      <p className="conn-meta">Đã gửi {new Date(p.sentAt).toLocaleDateString('vi-VN')}</p>
                      
                      <div className="conn-actions">
                        <button className="conn-btn btn-accept" onClick={() => handleRespond(p.connectionId, 'accept')}>
                          <Check size={16} /> Đồng ý
                        </button>
                        <button className="conn-btn btn-decline" onClick={() => handleRespond(p.connectionId, 'reject')}>
                          <X size={16} /> Xóa
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'friends' && (
            <div className="fade-in">
              <h3 className="text-xl font-bold mb-6">Tất cả bạn bè ({friends.length})</h3>
              
              {friends.length === 0 ? (
                <div className="empty-state">
                  <Users size={48} className="text-muted mb-4" />
                  <p>Bạn chưa có người bạn nào trên Unistay.</p>
                </div>
              ) : (
                <div className="friend-list">
                  {friends.map(f => (
                    <div key={f.friendId} className="friend-item">
                      <div className="friend-info-left">
                        <img 
                          src={f.friendAvatar ? `http://localhost:8386${f.friendAvatar}` : '/default-avatar.svg'} 
                          alt={f.friendName} 
                          className="friend-avatar"
                          onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                        />
                        <div>
                          <h4 className="friend-name">{f.friendName}</h4>
                          <span className="friend-since">Kết bạn từ {new Date(f.connectedSince).toLocaleDateString('vi-VN')}</span>
                        </div>
                      </div>
                      
                      <button className="conn-btn btn-message" style={{width: 'auto', padding: '0.5rem 1rem'}}>
                        <MessageCircle size={16} /> Nhắn tin
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

        </div>
      </div>
    </div>
  );
}

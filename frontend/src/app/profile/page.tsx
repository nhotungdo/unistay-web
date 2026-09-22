"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { 
  User, 
  Lock, 
  Activity, 
  Camera, 
  Save, 
  History 
} from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import './profile.css';

type Tab = 'personal' | 'security' | 'activity';

export default function ProfilePage() {
  const router = useRouter();
  const { user, loading, login } = useAuth(); // using login to update user state if needed
  
  const [activeTab, setActiveTab] = useState<Tab>('personal');
  const [profileData, setProfileData] = useState<any>(null);
  const [activities, setActivities] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });

  // Security Form
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const fetchProfile = async () => {
    try {
      const res = await api.get('/Profile');
      setProfileData(res.data.user);
      setActivities(res.data.activityHistory || []);
    } catch (error) {
      console.error("Lỗi tải thông tin:", error);
      setMessage({ text: 'Không thể tải thông tin hồ sơ', type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
    } else if (user) {
      fetchProfile();
    }
  }, [user, loading, router]);

  const handleProfileUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    setMessage({ text: '', type: '' });
    try {
      await api.put('/Profile', profileData);
      setMessage({ text: 'Cập nhật thông tin thành công!', type: 'success' });
      // Update local storage via context if you have a method for it
      // For now, next time they refresh it will get the new name
    } catch (err: any) {
      setMessage({ text: err.response?.data?.message || 'Có lỗi xảy ra', type: 'error' });
    } finally {
      setIsSaving(false);
    }
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    setMessage({ text: '', type: '' });
    try {
      await api.post('/Profile/ChangePassword', {
        currentPassword,
        newPassword,
        confirmPassword
      });
      setMessage({ text: 'Đổi mật khẩu thành công!', type: 'success' });
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: any) {
      const errors = err.response?.data?.errors;
      const msg = errors ? errors.join(', ') : err.response?.data?.message || 'Có lỗi xảy ra';
      setMessage({ text: msg, type: 'error' });
    } finally {
      setIsSaving(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setProfileData((prev: any) => ({ ...prev, [name]: value }));
  };

  if (loading || isLoading) {
    return <div className="container" style={{ padding: '5rem', textAlign: 'center' }}><Spinner size={40} /></div>;
  }

  return (
    <div className="container profile-container">
      <div className="profile-layout">
        
        {/* Sidebar */}
        <aside className="profile-sidebar">
          <div className="sidebar-header">
            <div className="avatar-wrapper">
              <img 
                src={profileData?.avatarUrl || '/default-avatar.svg'} 
                alt="Avatar" 
                className="profile-avatar"
              />
              <button className="avatar-upload-btn" title="Đổi ảnh đại diện">
                <Camera size={16} />
              </button>
            </div>
            <h2 className="profile-name">{profileData?.fullName}</h2>
            <p className="profile-email">{profileData?.email}</p>
          </div>
          
          <nav className="sidebar-nav">
            <a 
              className={`sidebar-link ${activeTab === 'personal' ? 'active' : ''}`}
              onClick={() => setActiveTab('personal')}
            >
              <User size={18} /> Thông tin cá nhân
            </a>
            <a 
              className={`sidebar-link ${activeTab === 'security' ? 'active' : ''}`}
              onClick={() => setActiveTab('security')}
            >
              <Lock size={18} /> Bảo mật
            </a>
            <a 
              className={`sidebar-link ${activeTab === 'activity' ? 'active' : ''}`}
              onClick={() => setActiveTab('activity')}
            >
              <History size={18} /> Lịch sử hoạt động
            </a>
          </nav>
        </aside>

        {/* Content */}
        <div className="profile-content">
          {message.text && (
            <AlertBanner variant={message.type as 'error'|'success'|'info'}>{message.text}</AlertBanner>
          )}

          {activeTab === 'personal' && (
            <div className="fade-in">
              <div className="content-header">
                <h3 className="content-title">Hồ sơ cá nhân</h3>
                <p className="content-desc">Cập nhật thông tin cá nhân của bạn để mọi người có thể hiểu rõ hơn về bạn.</p>
              </div>

              <form onSubmit={handleProfileUpdate}>
                <div className="form-row">
                  <Field label="Họ và tên" htmlFor="fullName">
                    <input id="fullName" name="fullName" type="text" className="form-input" value={profileData?.fullName || ''} onChange={handleInputChange} required />
                  </Field>
                  <Field label="Số điện thoại" htmlFor="phoneNumber">
                    <input id="phoneNumber" name="phoneNumber" type="tel" className="form-input" value={profileData?.phoneNumber || ''} onChange={handleInputChange} />
                  </Field>
                </div>

                <div className="form-row">
                  <Field label="Giới tính" htmlFor="gender">
                    <select id="gender" name="gender" className="form-input" value={profileData?.gender || ''} onChange={handleInputChange}>
                      <option value="">Chưa chọn</option>
                      <option value="Male">Nam</option>
                      <option value="Female">Nữ</option>
                      <option value="Other">Khác</option>
                    </select>
                  </Field>
                  <Field label="Ngày sinh" htmlFor="dateOfBirth">
                    <input id="dateOfBirth" name="dateOfBirth" type="date" className="form-input" 
                      value={profileData?.dateOfBirth ? profileData.dateOfBirth.split('T')[0] : ''} 
                      onChange={handleInputChange} />
                  </Field>
                </div>

                <div className="form-row">
                  <Field label="Nghề nghiệp" htmlFor="occupation">
                    <input id="occupation" name="occupation" type="text" className="form-input" value={profileData?.occupation || ''} onChange={handleInputChange} />
                  </Field>
                  <Field label="Ngân sách / tháng (VNĐ)" htmlFor="budget">
                    <input id="budget" name="budget" type="number" className="form-input" value={profileData?.budget || ''} onChange={handleInputChange} />
                  </Field>
                </div>

                <Field label="Giới thiệu bản thân" htmlFor="bio">
                  <textarea id="bio" name="bio" className="form-input" rows={4} value={profileData?.bio || ''} onChange={handleInputChange}></textarea>
                </Field>

                <button type="submit" className="btn-primary" disabled={isSaving}>
                  {isSaving ? <Spinner size={18} /> : <Save size={18} />} Lưu thay đổi
                </button>
              </form>
            </div>
          )}

          {activeTab === 'security' && (
            <div className="fade-in">
              <div className="content-header">
                <h3 className="content-title">Bảo mật</h3>
                <p className="content-desc">Thay đổi mật khẩu để bảo vệ tài khoản của bạn.</p>
              </div>

              <form onSubmit={handlePasswordChange} style={{ maxWidth: '400px' }}>
                <Field label="Mật khẩu hiện tại" htmlFor="currentPassword">
                  <input id="currentPassword" type="password" className="form-input" value={currentPassword} onChange={e => setCurrentPassword(e.target.value)} required />
                </Field>
                <Field label="Mật khẩu mới" htmlFor="newPassword">
                  <input id="newPassword" type="password" className="form-input" value={newPassword} onChange={e => setNewPassword(e.target.value)} required minLength={8} />
                </Field>
                <Field label="Xác nhận mật khẩu mới" htmlFor="confirmPassword">
                  <input id="confirmPassword" type="password" className="form-input" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} required minLength={8} />
                </Field>
                <button type="submit" className="btn-primary mt-4" disabled={isSaving}>
                  {isSaving ? <Spinner size={18} /> : 'Đổi mật khẩu'}
                </button>
              </form>
            </div>
          )}

          {activeTab === 'activity' && (
            <div className="fade-in">
              <div className="content-header">
                <h3 className="content-title">Lịch sử hoạt động</h3>
                <p className="content-desc">Các hoạt động gần đây của bạn trên hệ thống.</p>
              </div>

              <div className="activity-list">
                {activities.length > 0 ? (
                  activities.map((act, idx) => (
                    <div key={idx} className="activity-item">
                      <div className="activity-icon"><Activity size={20} /></div>
                      <div className="activity-details">
                        <p className="activity-desc">{act.description}</p>
                        <span className="activity-time">{new Date(act.activityDate).toLocaleString('vi-VN')}</span>
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="text-secondary">Chưa có hoạt động nào.</p>
                )}
              </div>
            </div>
          )}
        </div>
        
      </div>
    </div>
  );
}

"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { LayoutDashboard, Users, Home, AlertTriangle, Settings, Activity } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner } from '@/components/ui';
import './admin.css';

export default function AdminPage() {
  const router = useRouter();
  const { user, loading } = useAuth();
  
  const [activeTab, setActiveTab] = useState('dashboard');
  const [stats, setStats] = useState<any>(null);
  const [users, setUsers] = useState<any[]>([]);
  const [rooms, setRooms] = useState<any[]>([]);
  const [reports, setReports] = useState<any[]>([]);
  
  const [isLoadingData, setIsLoadingData] = useState(true);

  const fetchData = async () => {
    setIsLoadingData(true);
    try {
      if (activeTab === 'dashboard') {
        const res = await api.get('/Admin/statistics');
        setStats(res.data.data);
      } else if (activeTab === 'users') {
        const res = await api.get('/Admin/users');
        setUsers(res.data.data || []);
      } else if (activeTab === 'rooms') {
        const res = await api.get('/Admin/rooms');
        setRooms(res.data.data || []);
      } else if (activeTab === 'reports') {
        const res = await api.get('/Admin/reports');
        setReports(res.data.data || []);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setIsLoadingData(false);
    }
  };

  useEffect(() => {
    if (!loading) {
      if (!user) {
        router.push('/login?redirect=/admin');
      } else if (user.role !== 'Admin') {
        router.push('/');
      } else {
        fetchData();
      }
    }
  }, [user, loading, router, activeTab]);

  if (loading || !user || user.role !== 'Admin') return null;

  return (
    <div className="admin-layout fade-in">
      
      {/* Sidebar */}
      <aside className="admin-sidebar">
        <div className="admin-sidebar-title">Quản trị Hệ thống</div>
        <div className="admin-menu">
          <button className={`admin-menu-item ${activeTab === 'dashboard' ? 'active' : ''}`} onClick={() => setActiveTab('dashboard')}>
            <LayoutDashboard size={18} /> Tổng quan
          </button>
          <button className={`admin-menu-item ${activeTab === 'users' ? 'active' : ''}`} onClick={() => setActiveTab('users')}>
            <Users size={18} /> Quản lý Người dùng
          </button>
          <button className={`admin-menu-item ${activeTab === 'rooms' ? 'active' : ''}`} onClick={() => setActiveTab('rooms')}>
            <Home size={18} /> Quản lý Phòng trọ
          </button>
          <button className={`admin-menu-item ${activeTab === 'reports' ? 'active' : ''}`} onClick={() => setActiveTab('reports')}>
            <AlertTriangle size={18} /> Báo cáo Vi phạm
          </button>
          <button className={`admin-menu-item ${activeTab === 'settings' ? 'active' : ''}`} onClick={() => setActiveTab('settings')}>
            <Settings size={18} /> Cài đặt Hệ thống
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="admin-content custom-scrollbar">
        {activeTab === 'dashboard' && (
          <div className="fade-in">
            <div className="admin-page-header">
              <div>
                <h1 className="admin-page-title">Tổng quan Hệ thống</h1>
                <p className="admin-page-desc">Số liệu thống kê hoạt động của Unistay.</p>
              </div>
            </div>

            {isLoadingData ? <Spinner /> : (
              <>
                <div className="stat-grid">
                  <div className="stat-card">
                    <div className="stat-icon primary"><Users size={24} /></div>
                    <div className="stat-info">
                      <div className="stat-label">Tổng Người Dùng</div>
                      <div className="stat-value">{stats?.totalUsers || 1250}</div>
                    </div>
                  </div>
                  <div className="stat-card">
                    <div className="stat-icon success"><Home size={24} /></div>
                    <div className="stat-info">
                      <div className="stat-label">Phòng Đang Đăng</div>
                      <div className="stat-value">{stats?.totalRooms || 432}</div>
                    </div>
                  </div>
                  <div className="stat-card">
                    <div className="stat-icon warning"><Activity size={24} /></div>
                    <div className="stat-info">
                      <div className="stat-label">Giao Dịch Gần Đây</div>
                      <div className="stat-value">89</div>
                    </div>
                  </div>
                  <div className="stat-card">
                    <div className="stat-icon danger"><AlertTriangle size={24} /></div>
                    <div className="stat-info">
                      <div className="stat-label">Báo Cáo Chưa Xử Lý</div>
                      <div className="stat-value">12</div>
                    </div>
                  </div>
                </div>

                <div className="admin-panel">
                  <div className="admin-panel-header">
                    <h2 className="admin-panel-title">Hoạt động Gần Đây</h2>
                  </div>
                  <div className="admin-table-container">
                    <table className="admin-table">
                      <thead>
                        <tr>
                          <th>Hành động</th>
                          <th>Người thực hiện</th>
                          <th>Thời gian</th>
                          <th>Trạng thái</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr>
                          <td>Đăng phòng trọ mới (ID: #432)</td>
                          <td>Nguyen Van A</td>
                          <td>10 phút trước</td>
                          <td><span className="status-badge active">Hoàn thành</span></td>
                        </tr>
                        <tr>
                          <td>Báo cáo tài khoản ảo</td>
                          <td>Le Thi B</td>
                          <td>1 giờ trước</td>
                          <td><span className="status-badge pending">Chờ xử lý</span></td>
                        </tr>
                        <tr>
                          <td>Đăng tin tìm bạn ở ghép</td>
                          <td>Tran Van C</td>
                          <td>2 giờ trước</td>
                          <td><span className="status-badge active">Hoàn thành</span></td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </>
            )}
          </div>
        )}

        {activeTab === 'users' && (
          <div className="fade-in">
            <div className="admin-page-header">
              <h1 className="admin-page-title">Quản lý Người dùng</h1>
            </div>
            <div className="admin-panel">
              <div className="admin-table-container">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th>Tên hiển thị</th>
                      <th>Email</th>
                      <th>Vai trò</th>
                      <th>Ngày tham gia</th>
                      <th>Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td className="font-semibold text-primary">Admin System</td>
                      <td>admin@unistay.com</td>
                      <td><span className="status-badge pending">Admin</span></td>
                      <td>01/01/2026</td>
                      <td><button className="action-btn">Sửa</button></td>
                    </tr>
                    <tr>
                      <td>Sinh viên test 1</td>
                      <td>student@unistay.com</td>
                      <td><span className="status-badge active">Student</span></td>
                      <td>10/01/2026</td>
                      <td><button className="action-btn text-danger">Khóa</button></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
        
        {/* Other tabs can be similarly implemented... For now, they can just show a placeholder */}
        {['rooms', 'reports', 'settings'].includes(activeTab) && (
          <div className="fade-in">
            <div className="admin-page-header">
              <h1 className="admin-page-title">Đang phát triển</h1>
            </div>
            <AlertBanner variant="info">Tính năng này đang trong quá trình xây dựng.</AlertBanner>
          </div>
        )}

      </main>
    </div>
  );
}

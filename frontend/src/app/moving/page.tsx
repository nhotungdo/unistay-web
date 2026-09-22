"use client";

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Truck, MapPin, Calendar, Package, FileText, Check } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import './moving.css';

export default function MovingPage() {
  const router = useRouter();
  const { user } = useAuth();

  const [formData, setFormData] = useState({
    fromAddress: '',
    toAddress: '',
    preferredDate: '',
    itemsList: '',
    notes: ''
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) {
      router.push('/login?redirect=/moving');
      return;
    }

    setIsSubmitting(true);
    setError('');

    try {
      await api.post('/Moving', {
        ...formData,
        userId: user.id
      });
      setSuccess(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Có lỗi xảy ra khi gửi yêu cầu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (success) {
    return (
      <div className="container moving-container fade-in">
        <div className="moving-card moving-success">
          <div className="moving-success-icon">
            <Check size={40} />
          </div>
          <h2 className="moving-success-title">Đã gửi yêu cầu thành công!</h2>
          <p className="text-muted mb-6">Chúng tôi sẽ liên hệ lại với bạn qua số điện thoại để báo giá trong thời gian sớm nhất.</p>
          <button className="btn-primary" onClick={() => router.push('/')}>Trở về Trang chủ</button>
        </div>
      </div>
    );
  }

  return (
    <div className="container moving-container fade-in">
      <div className="moving-header">
        <h1 className="moving-title">Dịch vụ Chuyển Trọ Nhanh</h1>
        <p className="moving-desc">Hỗ trợ sinh viên chuyển đồ đạc an toàn, tiện lợi với chi phí ưu đãi.</p>
      </div>

      <div className="moving-card">
        <div className="moving-truck-illustration">
          <Truck size={64} color="var(--primary)" />
        </div>

        {error && <AlertBanner variant="error" className="mb-4">{error}</AlertBanner>}

        <form onSubmit={handleSubmit}>

          <div className="moving-form-section">
            <h3 className="moving-form-title"><MapPin size={20} /> Điểm đi và Điểm đến</h3>
            <div className="form-grid">
              <Field label="Địa chỉ chuyển đi" htmlFor="from">
                <input
                  id="from"
                  type="text"
                  className="form-input"
                  required
                  placeholder="Ví dụ: KTX Khu B, ĐHQG"
                  value={formData.fromAddress}
                  onChange={e => setFormData({ ...formData, fromAddress: e.target.value })}
                />
              </Field>
              <Field label="Địa chỉ chuyển đến" htmlFor="to">
                <input
                  id="to"
                  type="text"
                  className="form-input"
                  required
                  placeholder="Ví dụ: 123 Đường Linh Trung..."
                  value={formData.toAddress}
                  onChange={e => setFormData({ ...formData, toAddress: e.target.value })}
                />
              </Field>
            </div>
          </div>

          <div className="moving-form-section">
            <h3 className="moving-form-title"><Calendar size={20} /> Thời gian dự kiến</h3>
            <Field label="Ngày chuyển" htmlFor="date">
              <input
                id="date"
                type="date"
                className="form-input"
                required
                value={formData.preferredDate}
                onChange={e => setFormData({ ...formData, preferredDate: e.target.value })}
              />
            </Field>
          </div>

          <div className="moving-form-section">
            <h3 className="moving-form-title"><Package size={20} /> Khối lượng đồ đạc</h3>
            <Field label="Liệt kê các đồ vật lớn (Tủ lạnh, Máy giặt, Nệm...)" htmlFor="items">
              <textarea
                id="items"
                className="form-input"
                rows={3}
                placeholder="Giúp chúng tôi ước tính loại xe phù hợp..."
                value={formData.itemsList}
                onChange={e => setFormData({ ...formData, itemsList: e.target.value })}
              ></textarea>
            </Field>
          </div>

          <div className="moving-form-section">
            <h3 className="moving-form-title"><FileText size={20} /> Ghi chú thêm</h3>
            <Field label="Lưu ý cho tài xế (Ngõ hẹp, Lầu cao...)" htmlFor="notes">
              <textarea
                id="notes"
                className="form-input"
                rows={2}
                value={formData.notes}
                onChange={e => setFormData({ ...formData, notes: e.target.value })}
              ></textarea>
            </Field>
          </div>

          <button type="submit" className="btn-primary btn-lg w-full" disabled={isSubmitting}>
            {isSubmitting ? <Spinner size={20} /> : 'Nhận báo giá ngay'}
          </button>
        </form>
      </div>
    </div>
  );
}

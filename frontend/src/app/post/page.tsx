"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { 
  Building2, MapPin, DollarSign, FileText, Image as ImageIcon,
  Check, X
} from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import './post.css';

const AVAILABLE_AMENITIES = [
  'Điều hòa', 'Nóng lạnh', 'Tủ lạnh', 'Máy giặt', 
  'Giường', 'Tủ quần áo', 'Bàn ghế', 'Ban công',
  'Chỗ để xe', 'Bảo vệ 24/7', 'Tự do giờ giấc', 'Nuôi thú cưng'
];

export default function PostRoomPage() {
  const router = useRouter();
  const { user, loading } = useAuth();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    price: '',
    deposit: '',
    area: '',
    address: '',
    maxOccupants: '',
    rules: '',
  });

  const [selectedAmenities, setSelectedAmenities] = useState<string[]>([]);
  const [imageFiles, setImageFiles] = useState<File[]>([]);
  const [imagePreviews, setImagePreviews] = useState<string[]>([]);
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login?redirect=/post');
    }
  }, [user, loading, router]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const toggleAmenity = (amenity: string) => {
    setSelectedAmenities(prev => 
      prev.includes(amenity) 
        ? prev.filter(a => a !== amenity)
        : [...prev, amenity]
    );
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const files = Array.from(e.target.files);
      
      // Limit to 6 images total
      const newFiles = [...imageFiles, ...files].slice(0, 6);
      setImageFiles(newFiles);
      
      // Create previews
      const previews = newFiles.map(file => URL.createObjectURL(file));
      setImagePreviews(previews);
    }
  };

  const removeImage = (index: number) => {
    const newFiles = [...imageFiles];
    newFiles.splice(index, 1);
    setImageFiles(newFiles);
    
    const newPreviews = [...imagePreviews];
    URL.revokeObjectURL(newPreviews[index]);
    newPreviews.splice(index, 1);
    setImagePreviews(newPreviews);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setMessage({ text: '', type: '' });

    try {
      const formDataToSend = new FormData();
      formDataToSend.append('Title', formData.title);
      formDataToSend.append('Description', formData.description);
      formDataToSend.append('Price', formData.price);
      formDataToSend.append('Deposit', formData.deposit);
      formDataToSend.append('Area', formData.area);
      formDataToSend.append('Address', formData.address);
      formDataToSend.append('MaxOccupants', formData.maxOccupants);
      formDataToSend.append('Rules', formData.rules);
      
      selectedAmenities.forEach(am => {
        formDataToSend.append('SelectedAmenities', am);
      });

      imageFiles.forEach(file => {
        formDataToSend.append('ImageFiles', file);
      });

      const res = await api.post('/Rooms', formDataToSend, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      setMessage({ text: 'Đăng tin thành công!', type: 'success' });
      setTimeout(() => {
        router.push(`/rooms/${res.data.id}`);
      }, 1500);
      
    } catch (err: any) {
      setMessage({ 
        text: err.response?.data?.message || 'Có lỗi xảy ra khi đăng tin. Vui lòng kiểm tra lại thông tin.', 
        type: 'error' 
      });
      setIsSubmitting(false);
    }
  };

  if (loading || !user) {
    return <div className="container" style={{ padding: '5rem', textAlign: 'center' }}><Spinner size={40} /></div>;
  }

  return (
    <div className="container post-room-container fade-in">
      <div className="post-room-header">
        <h1 className="post-room-title">Đăng tin cho thuê phòng mới</h1>
        <p className="post-room-desc">Điền thông tin chi tiết để thu hút người thuê phù hợp nhất.</p>
      </div>

      <div className="post-room-card">
        {message.text && (
          <AlertBanner variant={message.type as 'error' | 'success'} className="mb-4">
            {message.text}
          </AlertBanner>
        )}

        <form onSubmit={handleSubmit}>
          
          <div className="form-section">
            <h2 className="section-title"><Building2 size={20} /> Thông tin cơ bản</h2>
            
            <Field label="Tiêu đề bài đăng" htmlFor="title" hint="Ví dụ: Phòng trọ khép kín, rộng rãi gần Đại học FPT">
              <input id="title" name="title" type="text" className="form-input" value={formData.title} onChange={handleChange} required />
            </Field>

            <div className="form-grid">
              <Field label="Giá cho thuê (VNĐ/tháng)" htmlFor="price">
                <div className="input-icon-wrap">
                  <DollarSign size={18} className="input-icon" />
                  <input id="price" name="price" type="number" className="form-input has-icon" value={formData.price} onChange={handleChange} required min={0} />
                </div>
              </Field>
              <Field label="Tiền cọc (VNĐ)" htmlFor="deposit">
                <div className="input-icon-wrap">
                  <DollarSign size={18} className="input-icon" />
                  <input id="deposit" name="deposit" type="number" className="form-input has-icon" value={formData.deposit} onChange={handleChange} required min={0} />
                </div>
              </Field>
            </div>

            <div className="form-grid">
              <Field label="Diện tích (m²)" htmlFor="area">
                <input id="area" name="area" type="number" className="form-input" value={formData.area} onChange={handleChange} required min={0} />
              </Field>
              <Field label="Số người ở tối đa" htmlFor="maxOccupants">
                <input id="maxOccupants" name="maxOccupants" type="number" className="form-input" value={formData.maxOccupants} onChange={handleChange} required min={1} />
              </Field>
            </div>
          </div>

          <div className="form-section">
            <h2 className="section-title"><MapPin size={20} /> Vị trí</h2>
            <Field label="Địa chỉ chính xác" htmlFor="address">
              <div className="input-icon-wrap">
                <MapPin size={18} className="input-icon" />
                <input id="address" name="address" type="text" className="form-input has-icon" value={formData.address} onChange={handleChange} required />
              </div>
            </Field>
          </div>

          <div className="form-section">
            <h2 className="section-title"><FileText size={20} /> Mô tả chi tiết</h2>
            <Field label="Mô tả phòng" htmlFor="description">
              <textarea id="description" name="description" className="form-input" rows={6} value={formData.description} onChange={handleChange} required></textarea>
            </Field>
            <Field label="Nội quy phòng (Không bắt buộc)" htmlFor="rules">
              <textarea id="rules" name="rules" className="form-input" rows={3} value={formData.rules} onChange={handleChange}></textarea>
            </Field>
          </div>

          <div className="form-section">
            <h2 className="section-title"><Check size={20} /> Tiện ích có sẵn</h2>
            <div className="amenities-grid">
              {AVAILABLE_AMENITIES.map(amenity => {
                const isSelected = selectedAmenities.includes(amenity);
                return (
                  <label key={amenity} className={`amenity-checkbox-label ${isSelected ? 'selected' : ''}`}>
                    <input 
                      type="checkbox" 
                      className="hidden-checkbox"
                      checked={isSelected}
                      onChange={() => toggleAmenity(amenity)}
                    />
                    <div className="checkbox-icon">
                      {isSelected ? <Check size={16} /> : <div style={{width: 16, height: 16, border: '1px solid var(--border)', borderRadius: 3}}></div>}
                    </div>
                    <span>{amenity}</span>
                  </label>
                )
              })}
            </div>
          </div>

          <div className="form-section">
            <h2 className="section-title"><ImageIcon size={20} /> Hình ảnh (Tối đa 6 ảnh)</h2>
            <div className="image-upload-area">
              <input 
                type="file" 
                multiple 
                accept="image/*" 
                className="file-input" 
                onChange={handleImageChange}
                disabled={imageFiles.length >= 6}
              />
              <ImageIcon size={32} className="upload-icon" />
              <div className="upload-text">Nhấn hoặc kéo thả ảnh vào đây</div>
              <div className="upload-subtext">Hỗ trợ JPG, PNG (Tối đa 5MB)</div>
            </div>

            {imagePreviews.length > 0 && (
              <div className="image-preview-grid">
                {imagePreviews.map((src, idx) => (
                  <div key={idx} className="image-preview-item">
                    <img src={src} alt="Preview" className="image-preview-img" />
                    <button type="button" className="remove-image-btn" onClick={() => removeImage(idx)}>
                      <X size={14} />
                    </button>
                    {idx === 0 && <div style={{position: 'absolute', bottom: 0, left: 0, right: 0, background: 'var(--primary)', color: 'white', fontSize: '0.7rem', textAlign: 'center', padding: '2px 0'}}>Ảnh bìa</div>}
                  </div>
                ))}
              </div>
            )}
          </div>

          <button type="submit" className="btn-primary btn-lg w-full" disabled={isSubmitting}>
            {isSubmitting ? <Spinner size={20} /> : 'Đăng tin ngay'}
          </button>
        </form>
      </div>
    </div>
  );
}

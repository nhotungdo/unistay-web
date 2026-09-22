"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { ShoppingBag, Plus, MapPin, Tag, Package, X, Image as ImageIcon } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import { formatPrice } from '@/lib/rooms';
import './marketplace.css';

interface MarketplaceItem {
  id: number;
  title: string;
  description: string;
  price: number;
  condition: string;
  location: string;
  category: string;
  imageUrls: string;
  sellerId: string;
  createdAt: string;
}

export default function MarketplacePage() {
  const router = useRouter();
  const { user } = useAuth();
  
  const [items, setItems] = useState<MarketplaceItem[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Post Item Modal state
  const [showModal, setShowModal] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [postError, setPostError] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState('');
  
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    price: '',
    condition: 'Mới 100%',
    location: '',
    category: 'Đồ điện tử'
  });

  const fetchItems = async () => {
    setLoading(true);
    try {
      const res = await api.get('/Marketplace');
      setItems(res.data);
    } catch (err) {
      console.error("Lỗi tải danh sách chợ", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchItems();
  }, []);

  const handleOpenModal = () => {
    if (!user) {
      router.push('/login?redirect=/marketplace');
      return;
    }
    setShowModal(true);
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setImageFile(file);
      setImagePreview(URL.createObjectURL(file));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setPostError('');

    try {
      const submitData = new FormData();
      submitData.append('Title', formData.title);
      submitData.append('Description', formData.description);
      submitData.append('Price', formData.price);
      submitData.append('Condition', formData.condition);
      submitData.append('Location', formData.location);
      submitData.append('Category', formData.category);
      
      if (imageFile) {
        submitData.append('ImageFile', imageFile);
      }

      await api.post('/Marketplace', submitData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      
      setShowModal(false);
      fetchItems(); // refresh list
      
      // Reset form
      setFormData({
        title: '',
        description: '',
        price: '',
        condition: 'Mới 100%',
        location: '',
        category: 'Đồ điện tử'
      });
      setImageFile(null);
      setImagePreview('');
      
    } catch (err: any) {
      setPostError(err.response?.data?.message || 'Có lỗi xảy ra khi đăng bán.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container marketplace-container fade-in">
      <div className="marketplace-header">
        <div>
          <h1 className="marketplace-title">Chợ Sinh Viên</h1>
          <p className="marketplace-desc">Mua bán, trao đổi đồ dùng học tập, nội thất phòng trọ giá tốt.</p>
        </div>
        <button className="btn-primary" onClick={handleOpenModal} style={{display: 'flex', alignItems: 'center', gap: '0.5rem'}}>
          <Plus size={18} /> Đăng bán đồ
        </button>
      </div>

      {loading ? (
        <div style={{padding: '4rem', textAlign: 'center'}}>
          <Spinner size={40} />
        </div>
      ) : items.length === 0 ? (
        <div className="empty-state">
          <ShoppingBag size={48} className="text-muted mb-4" />
          <h3>Chưa có sản phẩm nào</h3>
          <p>Hãy là người đầu tiên đăng bán đồ trên Chợ Sinh Viên.</p>
        </div>
      ) : (
        <div className="marketplace-grid">
          {items.map(item => (
            <div key={item.id} className="market-card">
              <div className="market-image-wrapper">
                <span className="market-category">{item.category}</span>
                <img 
                  src={item.imageUrls ? `http://localhost:8386${item.imageUrls}` : '/placeholder.jpg'} 
                  alt={item.title} 
                  className="market-image"
                  onError={(e) => { (e.target as HTMLImageElement).src = '/placeholder.jpg' }}
                />
              </div>
              
              <div className="market-content">
                <h3 className="market-title">{item.title}</h3>
                <div className="market-price">{formatPrice(item.price)}</div>
                
                <div className="market-meta">
                  <div className="market-meta-item">
                    <Package size={14} /> <span>{item.condition}</span>
                  </div>
                  <div className="market-meta-item">
                    <MapPin size={14} /> <span>{item.location}</span>
                  </div>
                </div>
                
                <button 
                  className="btn-primary w-full mt-4" 
                  onClick={() => user ? router.push(`/messages`) : router.push('/login')}
                  style={{padding: '0.5rem'}}
                >
                  Liên hệ người bán
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Post Modal */}
      {showModal && (
        <div className="market-modal-overlay">
          <div className="market-modal fade-in" style={{animationDuration: '0.2s'}}>
            <button className="modal-close" onClick={() => setShowModal(false)}>
              <X size={20} />
            </button>
            
            <div className="modal-header">
              <h2 className="modal-title">Đăng bán sản phẩm</h2>
            </div>
            
            <div className="modal-body">
              {postError && <AlertBanner variant="error" className="mb-4">{postError}</AlertBanner>}
              
              <form onSubmit={handleSubmit}>
                <Field label="Tên sản phẩm" htmlFor="title">
                  <input id="title" type="text" className="form-input" required value={formData.title} onChange={e => setFormData({...formData, title: e.target.value})} />
                </Field>
                
                <div className="form-grid">
                  <Field label="Mức giá (VNĐ)" htmlFor="price">
                    <input id="price" type="number" min="0" className="form-input" required value={formData.price} onChange={e => setFormData({...formData, price: e.target.value})} />
                  </Field>
                  <Field label="Danh mục" htmlFor="category">
                    <select id="category" className="form-input" value={formData.category} onChange={e => setFormData({...formData, category: e.target.value})}>
                      <option value="Đồ điện tử">Đồ điện tử</option>
                      <option value="Nội thất">Nội thất</option>
                      <option value="Đồ dùng học tập">Đồ dùng học tập</option>
                      <option value="Phương tiện">Phương tiện đi lại</option>
                      <option value="Khác">Khác</option>
                    </select>
                  </Field>
                </div>
                
                <div className="form-grid">
                  <Field label="Tình trạng" htmlFor="condition">
                    <select id="condition" className="form-input" value={formData.condition} onChange={e => setFormData({...formData, condition: e.target.value})}>
                      <option value="Mới 100%">Mới 100%</option>
                      <option value="Như mới (99%)">Như mới (99%)</option>
                      <option value="Tốt (90-95%)">Tốt (90-95%)</option>
                      <option value="Khá (Cũ)">Khá (Cũ)</option>
                    </select>
                  </Field>
                  <Field label="Khu vực giao dịch" htmlFor="location">
                    <input id="location" type="text" className="form-input" required value={formData.location} onChange={e => setFormData({...formData, location: e.target.value})} />
                  </Field>
                </div>
                
                <Field label="Mô tả chi tiết" htmlFor="desc">
                  <textarea id="desc" className="form-input" rows={4} required value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})}></textarea>
                </Field>
                
                <Field label="Hình ảnh sản phẩm" htmlFor="image">
                  <input id="image" type="file" accept="image/*" className="form-input" onChange={handleImageChange} />
                  {imagePreview && (
                    <img src={imagePreview} alt="Preview" className="image-preview" />
                  )}
                </Field>
                
                <button type="submit" className="btn-primary w-full btn-lg mt-4" disabled={isSubmitting}>
                  {isSubmitting ? <Spinner size={20} /> : 'Đăng bán ngay'}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}

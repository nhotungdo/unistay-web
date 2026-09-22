"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { UserRound, Building2, ChevronRight, CheckCircle2 } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import './onboarding.css';

type Role = 'Student' | 'Landlord' | null;

export default function OnboardingPage() {
  const router = useRouter();
  const { user, loading } = useAuth();
  
  const [step, setStep] = useState(1);
  const [role, setRole] = useState<Role>(null);
  
  // Student fields
  const [dob, setDob] = useState('');
  const [preferences, setPreferences] = useState('');
  
  // Landlord fields
  const [phone, setPhone] = useState('');
  const [city, setCity] = useState('');
  const [district, setDistrict] = useState('');
  const [ward, setWard] = useState('');
  const [streetName, setStreetName] = useState('');
  const [houseNumber, setHouseNumber] = useState('');
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [analysis, setAnalysis] = useState('');

  useEffect(() => {
    // If not loading and no user, or already completed, redirect
    if (!loading && !user) {
      router.push('/login');
    } else if (user?.isOnboardingComplete) {
      router.push('/');
    } else {
      // Fetch status to know which step we are on
      api.get('/onboarding/status').then(res => {
        if (res.data.isOnboardingComplete) {
          router.push('/');
        } else if (res.data.onboardingStep > 1) {
          setStep(res.data.onboardingStep);
        }
      }).catch(console.error);
    }
  }, [user, loading, router]);

  const handleRoleSelect = async (selectedRole: Role) => {
    setRole(selectedRole);
    setError('');
    setIsSubmitting(true);
    try {
      const res = await api.post('/onboarding/select-role', { role: selectedRole });
      setStep(res.data.nextStep);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Có lỗi xảy ra');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleProfileSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    try {
      if (role === 'Student') {
        const res = await api.post('/onboarding/seeker-profile', {
          dateOfBirth: dob,
          preferences
        });
        setAnalysis(res.data.analysis);
        setStep(res.data.nextStep);
      } else {
        const address = `${houseNumber} ${streetName}, ${ward}, ${district}, ${city}`;
        const res = await api.post('/onboarding/landlord-verify', {
          phone, address, houseNumber, streetName, city, district, ward
        });
        setStep(res.data.nextStep);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Có lỗi xảy ra');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleComplete = async () => {
    setIsSubmitting(true);
    try {
      await api.post('/onboarding/complete');
      // Update local storage user manually or just force reload
      window.location.assign('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Có lỗi xảy ra');
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return <div className="onboarding-loading"><Spinner size={40} /></div>;
  }

  return (
    <div className="onboarding-container">
      <div className="onboarding-card">
        <div className="onboarding-progress">
          <div className={`progress-step ${step >= 1 ? 'active' : ''}`}>1</div>
          <div className={`progress-line ${step >= 2 ? 'active' : ''}`}></div>
          <div className={`progress-step ${step >= 2 ? 'active' : ''}`}>2</div>
          <div className={`progress-line ${step >= 3 ? 'active' : ''}`}></div>
          <div className={`progress-step ${step >= 3 ? 'active' : ''}`}>3</div>
        </div>

        {error && <AlertBanner variant="error">{error}</AlertBanner>}

        {step === 1 && (
          <div className="onboarding-step fade-in">
            <h1 className="step-title">Bạn là ai?</h1>
            <p className="step-desc">Hãy chọn vai trò để Unistay mang lại trải nghiệm tốt nhất cho bạn.</p>
            
            <div className="role-options">
              <button 
                className={`role-card ${role === 'Student' ? 'selected' : ''}`}
                onClick={() => handleRoleSelect('Student')}
                disabled={isSubmitting}
              >
                <div className="role-icon"><UserRound size={32} /></div>
                <h3>Người đi thuê</h3>
                <p>Tôi muốn tìm phòng trọ hoặc người ở ghép</p>
              </button>
              
              <button 
                className={`role-card ${role === 'Landlord' ? 'selected' : ''}`}
                onClick={() => handleRoleSelect('Landlord')}
                disabled={isSubmitting}
              >
                <div className="role-icon"><Building2 size={32} /></div>
                <h3>Chủ trọ</h3>
                <p>Tôi có phòng trống muốn cho thuê</p>
              </button>
            </div>
            {isSubmitting && <div className="loading-overlay"><Spinner /></div>}
          </div>
        )}

        {step === 2 && role === 'Student' && (
          <div className="onboarding-step fade-in">
            <h1 className="step-title">Hồ sơ cá nhân</h1>
            <p className="step-desc">Điền thông tin để hệ thống AI phân tích và gợi ý người ở ghép phù hợp với cung hoàng đạo của bạn.</p>
            
            <form onSubmit={handleProfileSubmit} className="onboarding-form">
              <Field label="Ngày tháng năm sinh" htmlFor="dob">
                <input
                  id="dob"
                  type="date"
                  className="form-input"
                  value={dob}
                  onChange={e => setDob(e.target.value)}
                  required
                />
              </Field>
              <Field label="Sở thích & Thói quen" htmlFor="prefs" hint="Ví dụ: Gọn gàng, hay thức khuya, không hút thuốc, thích nuôi mèo...">
                <textarea
                  id="prefs"
                  className="form-input"
                  rows={4}
                  value={preferences}
                  onChange={e => setPreferences(e.target.value)}
                  required
                  placeholder="Mô tả thói quen sinh hoạt của bạn..."
                />
              </Field>
              
              <button type="submit" className="btn-primary w-full mt-4" disabled={isSubmitting}>
                {isSubmitting ? <Spinner size={18} /> : 'Tiếp tục'}
              </button>
            </form>
          </div>
        )}

        {step === 2 && role === 'Landlord' && (
          <div className="onboarding-step fade-in">
            <h1 className="step-title">Xác thực chủ trọ</h1>
            <p className="step-desc">Cung cấp thông tin liên hệ và địa chỉ để đăng tin cho thuê.</p>
            
            <form onSubmit={handleProfileSubmit} className="onboarding-form">
              <Field label="Số điện thoại liên hệ" htmlFor="phone">
                <input id="phone" type="tel" className="form-input" value={phone} onChange={e => setPhone(e.target.value)} required />
              </Field>
              
              <div className="form-grid">
                <Field label="Tỉnh / Thành phố" htmlFor="city">
                  <input id="city" type="text" className="form-input" value={city} onChange={e => setCity(e.target.value)} required />
                </Field>
                <Field label="Quận / Huyện" htmlFor="district">
                  <input id="district" type="text" className="form-input" value={district} onChange={e => setDistrict(e.target.value)} required />
                </Field>
              </div>
              
              <div className="form-grid">
                <Field label="Phường / Xã" htmlFor="ward">
                  <input id="ward" type="text" className="form-input" value={ward} onChange={e => setWard(e.target.value)} required />
                </Field>
                <Field label="Tên đường" htmlFor="street">
                  <input id="street" type="text" className="form-input" value={streetName} onChange={e => setStreetName(e.target.value)} required />
                </Field>
              </div>
              
              <Field label="Số nhà" htmlFor="house">
                <input id="house" type="text" className="form-input" value={houseNumber} onChange={e => setHouseNumber(e.target.value)} required />
              </Field>
              
              <button type="submit" className="btn-primary w-full mt-4" disabled={isSubmitting}>
                {isSubmitting ? <Spinner size={18} /> : 'Tiếp tục'}
              </button>
            </form>
          </div>
        )}

        {step === 3 && (
          <div className="onboarding-step fade-in text-center">
            <div className="success-icon-wrap">
              <CheckCircle2 size={64} className="text-success" />
            </div>
            <h1 className="step-title">Hoàn tất!</h1>
            <p className="step-desc">Hồ sơ của bạn đã được thiết lập thành công.</p>
            
            {analysis && (
              <div className="ai-analysis-box">
                <h4>🔮 Phân tích từ AI Unistay</h4>
                <p>{analysis}</p>
              </div>
            )}
            
            <button onClick={handleComplete} className="btn-primary btn-lg mt-6" disabled={isSubmitting}>
              {isSubmitting ? <Spinner size={18} /> : (
                <>Khám phá Unistay ngay <ChevronRight size={20} /></>
              )}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

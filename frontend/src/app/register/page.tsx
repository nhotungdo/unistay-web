"use client";

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { AxiosError } from 'axios';
import { Mail, Lock, Eye, EyeOff, UserRound } from 'lucide-react';
import api from '@/lib/api';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import '../login/login.css';

interface FormErrors {
  fullName?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
}

function getPasswordScore(password: string): number {
  if (!password) return 0;
  let score = 0;
  if (password.length >= 8) score++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
  if (/\d/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;
  return score; // 0..4
}

const strengthLabels = ['Rất yếu', 'Yếu', 'Trung bình', 'Tốt', 'Mạnh'];

export default function RegisterPage() {
  const router = useRouter();

  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [errors, setErrors] = useState<FormErrors>({});
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const strength = getPasswordScore(formData.password);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
    // Clear the field-level error as the user fixes it
    if (errors[e.target.name as keyof FormErrors]) {
      setErrors({ ...errors, [e.target.name]: undefined });
    }
  };

  const validate = (): boolean => {
    const next: FormErrors = {};
    if (formData.fullName.trim().length < 2) {
      next.fullName = 'Vui lòng nhập họ tên đầy đủ.';
    }
    if (!/^\S+@\S+\.\S+$/.test(formData.email)) {
      next.email = 'Email không hợp lệ.';
    }
    if (formData.password.length < 6) {
      next.password = 'Mật khẩu phải có ít nhất 6 ký tự.';
    }
    if (formData.confirmPassword !== formData.password) {
      next.confirmPassword = 'Mật khẩu xác nhận không khớp.';
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!validate()) return;

    setIsLoading(true);

    try {
      await api.post('/account/register', {
        fullName: formData.fullName,
        email: formData.email,
        password: formData.password,
      });

      setSuccess('Đăng ký thành công! Đang chuyển hướng đến trang đăng nhập...');
      setTimeout(() => {
        router.push('/login');
      }, 2000);
    } catch (err) {
      const error = err as AxiosError<{ message?: string }>;
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError('Đăng ký thất bại. Vui lòng thử lại.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <Link href="/" className="auth-logo">
            Unistay
          </Link>
          <h1>Đăng Ký</h1>
          <p>Tạo tài khoản Unistay mới</p>
        </div>

        {error && <AlertBanner variant="error">{error}</AlertBanner>}
        {success && <AlertBanner variant="success">{success}</AlertBanner>}

        <form onSubmit={handleSubmit} className="auth-form" noValidate>
          <Field label="Họ và tên" htmlFor="reg-name" error={errors.fullName}>
            <div className="input-icon-wrap">
              <UserRound size={18} className="input-icon" aria-hidden="true" />
              <input
                id="reg-name"
                type="text"
                name="fullName"
                className={`form-input has-icon ${errors.fullName ? 'input-invalid' : ''}`}
                value={formData.fullName}
                onChange={handleChange}
                placeholder="VD: Nguyễn Văn A"
                autoComplete="name"
                required
              />
            </div>
          </Field>

          <Field label="Email" htmlFor="reg-email" error={errors.email}>
            <div className="input-icon-wrap">
              <Mail size={18} className="input-icon" aria-hidden="true" />
              <input
                id="reg-email"
                type="email"
                name="email"
                className={`form-input has-icon ${errors.email ? 'input-invalid' : ''}`}
                value={formData.email}
                onChange={handleChange}
                placeholder="Nhập email của bạn"
                autoComplete="email"
                required
              />
            </div>
          </Field>

          <Field
            label="Mật khẩu"
            htmlFor="reg-password"
            error={errors.password}
            hint="Tối thiểu 6 ký tự. Dùng chữ hoa, chữ thường và số để mạnh hơn."
          >
            <div className="input-icon-wrap input-password-wrap">
              <Lock size={18} className="input-icon" aria-hidden="true" />
              <input
                id="reg-password"
                type={showPassword ? 'text' : 'password'}
                name="password"
                className={`form-input has-icon ${errors.password ? 'input-invalid' : ''}`}
                value={formData.password}
                onChange={handleChange}
                placeholder="Tạo mật khẩu"
                autoComplete="new-password"
                required
                minLength={6}
              />
              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowPassword((v) => !v)}
                aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {formData.password && (
              <div
                className="password-strength"
                role="status"
                aria-label="Độ mạnh mật khẩu"
              >
                <div className="strength-bars">
                  {[1, 2, 3, 4].map((lvl) => (
                    <span
                      key={lvl}
                      className={`strength-bar ${strength >= lvl ? `level-${strength}` : ''}`}
                    />
                  ))}
                </div>
                <span className={`strength-label level-${strength}`}>
                  {strengthLabels[strength]}
                </span>
              </div>
            )}
          </Field>

          <Field
            label="Xác nhận mật khẩu"
            htmlFor="reg-confirm"
            error={errors.confirmPassword}
          >
            <div className="input-icon-wrap input-password-wrap">
              <Lock size={18} className="input-icon" aria-hidden="true" />
              <input
                id="reg-confirm"
                type={showConfirm ? 'text' : 'password'}
                name="confirmPassword"
                className={`form-input has-icon ${errors.confirmPassword ? 'input-invalid' : ''}`}
                value={formData.confirmPassword}
                onChange={handleChange}
                placeholder="Nhập lại mật khẩu"
                autoComplete="new-password"
                required
                minLength={6}
              />
              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowConfirm((v) => !v)}
                aria-label={showConfirm ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
              >
                {showConfirm ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </Field>

          <button
            type="submit"
            className="btn-primary auth-submit"
            disabled={isLoading || !!success}
          >
            {isLoading ? (
              <>
                <Spinner size={18} /> Đang xử lý...
              </>
            ) : (
              'Đăng Ký'
            )}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Đã có tài khoản?{' '}
            <Link href="/login" className="auth-link">
              Đăng nhập ngay
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

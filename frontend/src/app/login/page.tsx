"use client";

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { AxiosError } from 'axios';
import { Mail, Lock, Eye, EyeOff } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner, AlertBanner, Field } from '@/components/ui';
import './login.css';

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const res = await api.post('/account/login', {
        email,
        password,
        rememberMe,
      });

      if (res.data.token) {
        login(res.data.token, res.data.user);

        if (res.data.user.isOnboardingComplete === false) {
          router.push('/onboarding');
        } else {
          router.push('/');
        }
      } else {
        setError('Đăng nhập thất bại. Vui lòng thử lại.');
      }
    } catch (err) {
      const error = err as AxiosError<{ message?: string }>;
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError('Sai email hoặc mật khẩu.');
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
          <h1>Đăng Nhập</h1>
          <p>Chào mừng trở lại với Unistay</p>
        </div>

        {error && <AlertBanner variant="error">{error}</AlertBanner>}

        <form onSubmit={handleSubmit} className="auth-form" noValidate>
          <Field label="Email" htmlFor="login-email">
            <div className="input-icon-wrap">
              <Mail size={18} className="input-icon" aria-hidden="true" />
              <input
                id="login-email"
                type="email"
                className="form-input has-icon"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Nhập email của bạn"
                autoComplete="email"
                required
              />
            </div>
          </Field>

          <Field label="Mật khẩu" htmlFor="login-password">
            <div className="input-icon-wrap input-password-wrap">
              <Lock size={18} className="input-icon" aria-hidden="true" />
              <input
                id="login-password"
                type={showPassword ? 'text' : 'password'}
                className="form-input has-icon"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Nhập mật khẩu"
                autoComplete="current-password"
                required
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
          </Field>

          <div className="form-options">
            <label className="remember-me">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
              />
              <span>Ghi nhớ đăng nhập</span>
            </label>
            <Link href="/forgot-password" className="forgot-password">
              Quên mật khẩu?
            </Link>
          </div>

          <button
            type="submit"
            className="btn-primary auth-submit"
            disabled={isLoading}
          >
            {isLoading ? (
              <>
                <Spinner size={18} /> Đang xử lý...
              </>
            ) : (
              'Đăng Nhập'
            )}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Chưa có tài khoản?{' '}
            <Link href="/register" className="auth-link">
              Đăng ký ngay
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

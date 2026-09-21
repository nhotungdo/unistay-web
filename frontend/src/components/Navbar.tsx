"use client";

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  useEffect,
  useState,
} from 'react';
import {
  Home,
  MessageSquare,
  PlusSquare,
  Search,
  User as UserIcon,
  LogOut,
  FileText,
  Menu,
  X,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { useClickOutside, Skeleton } from '@/components/ui';

const NAV_LINKS = [
  { href: '/', label: 'Trang chủ', Icon: Home, match: (p: string) => p === '/' },
  {
    href: '/rooms',
    label: 'Tìm phòng',
    Icon: Search,
    match: (p: string) => p.startsWith('/rooms'),
  },
  {
    href: '/roommates',
    label: 'Tìm người ở ghép',
    Icon: UserIcon,
    match: (p: string) => p.startsWith('/roommates'),
  },
  {
    href: '/rental-advice',
    label: 'Kinh nghiệm',
    Icon: FileText,
    match: (p: string) => p.startsWith('/rental-advice'),
  },
];

export default function Navbar() {
  const pathname = usePathname();
  const { user, loading, logout } = useAuth();
  const [isScrolled, setIsScrolled] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  const menuRef = useClickOutside<HTMLDivElement>(() => setShowDropdown(false));

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 10);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Close the mobile menu with Escape
  useEffect(() => {
    if (!mobileOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMobileOpen(false);
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [mobileOpen]);

  const closeMobileMenu = () => setMobileOpen(false);

  const links = (
    <>
      {NAV_LINKS.map(({ href, label, Icon, match }) => (
        <Link
          key={href}
          href={href}
          className={`nav-link ${match(pathname) ? 'active' : ''}`}
          onClick={closeMobileMenu}
        >
          <Icon size={18} />
          <span>{label}</span>
        </Link>
      ))}
    </>
  );

  return (
    <nav className={`navbar ${isScrolled ? 'scrolled' : ''}`}>
      <div className="navbar-container">
        <Link href="/" className="navbar-logo" aria-label="Unistay - Trang chủ">
          <span className="logo-text">Unistay</span>
        </Link>

        <div className="navbar-links" role="navigation" aria-label="Điều hướng chính">
          {links}
        </div>

        <div className="navbar-actions">
          {loading ? (
            <Skeleton className="nav-avatar-skeleton" />
          ) : user ? (
            <>
              <Link
                href="/messages"
                className="nav-icon-btn"
                aria-label="Tin nhắn"
              >
                <MessageSquare size={20} />
              </Link>
              <Link href="/post" className="btn-primary-sm">
                <PlusSquare size={16} />
                <span>Đăng tin</span>
              </Link>
              <div
                className="user-menu"
                ref={menuRef}
                onMouseLeave={() => setShowDropdown(false)}
              >
                <button
                  className="user-avatar-btn"
                  onClick={() => setShowDropdown((v) => !v)}
                  aria-label="Menu tài khoản"
                  aria-expanded={showDropdown}
                  aria-haspopup="menu"
                >
                  <img
                    src={user.avatarUrl || '/default-avatar.svg'}
                    alt={user.fullName}
                    className="avatar-img"
                  />
                </button>

                {showDropdown && (
                  <div className="dropdown-menu" role="menu">
                    <div className="dropdown-header">
                      <p className="user-name">{user.fullName}</p>
                      <p className="user-email">{user.email}</p>
                    </div>
                    <div className="dropdown-divider" />
                    <Link
                      href="/profile"
                      className="dropdown-item"
                      role="menuitem"
                      onClick={() => setShowDropdown(false)}
                    >
                      Hồ sơ cá nhân
                    </Link>
                    <Link
                      href="/my-rooms"
                      className="dropdown-item"
                      role="menuitem"
                      onClick={() => setShowDropdown(false)}
                    >
                      Phòng của tôi
                    </Link>
                    <Link
                      href="/favorites"
                      className="dropdown-item"
                      role="menuitem"
                      onClick={() => setShowDropdown(false)}
                    >
                      Đã lưu
                    </Link>
                    <div className="dropdown-divider" />
                    <button
                      onClick={() => {
                        setShowDropdown(false);
                        logout();
                      }}
                      className="dropdown-item text-danger"
                      role="menuitem"
                    >
                      <LogOut size={16} />
                      <span>Đăng xuất</span>
                    </button>
                  </div>
                )}
              </div>
            </>
          ) : (
            <div className="auth-buttons">
              <Link href="/login" className="btn-outline btn-outline-sm">
                Đăng nhập
              </Link>
              <Link href="/register" className="btn-primary-sm">
                Đăng ký
              </Link>
            </div>
          )}

          {/* Mobile menu toggle */}
          <button
            className="mobile-menu-btn"
            onClick={() => setMobileOpen((v) => !v)}
            aria-label={mobileOpen ? 'Đóng menu' : 'Mở menu'}
            aria-expanded={mobileOpen}
          >
            {mobileOpen ? <X size={24} /> : <Menu size={24} />}
          </button>
        </div>
      </div>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="mobile-menu">
          <div className="mobile-links">{links}</div>
          {!loading && !user && (
            <div className="mobile-auth">
              <Link href="/login" className="btn-outline">
                Đăng nhập
              </Link>
              <Link href="/register" className="btn-primary">
                Đăng ký
              </Link>
            </div>
          )}
        </div>
      )}
    </nav>
  );
}

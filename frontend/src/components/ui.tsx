"use client";

import { useState, useEffect, useRef } from 'react';
import { CircleAlert, CircleCheck, Info, LoaderCircle } from 'lucide-react';

export function Spinner({ size = 20 }: { size?: number }) {
  return <LoaderCircle size={size} className="spinner" aria-hidden="true" />;
}

export function Skeleton({ className = '' }: { className?: string }) {
  return <div className={`skeleton ${className}`} aria-hidden="true" />;
}

type BannerVariant = 'error' | 'success' | 'info';

const bannerConfig: Record<BannerVariant, { className: string; Icon: typeof Info; role: string }> = {
  error: { className: 'alert-banner error', Icon: CircleAlert, role: 'alert' },
  success: { className: 'alert-banner success', Icon: CircleCheck, role: 'status' },
  info: { className: 'alert-banner info', Icon: Info, role: 'status' },
};

export function AlertBanner({ variant, children }: { variant: BannerVariant; children: React.ReactNode }) {
  const { className, Icon, role } = bannerConfig[variant];
  return (
    <div className={className} role={role}>
      <Icon size={18} className="alert-banner-icon" aria-hidden="true" />
      <span>{children}</span>
    </div>
  );
}

export function Field({
  label,
  htmlFor,
  hint,
  error,
  children,
}: {
  label: string;
  htmlFor?: string;
  hint?: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="form-group">
      <label className="form-label" htmlFor={htmlFor}>
        {label}
      </label>
      {children}
      {hint && !error && <p className="field-hint">{hint}</p>}
      {error && (
        <p className="field-error" role="alert">
          <CircleAlert size={14} aria-hidden="true" /> {error}
        </p>
      )}
    </div>
  );
}

/**
 * Image that resolves API-relative URLs and falls back to a placeholder
 * (with an optional spinner) while loading or when it fails.
 */
export function RoomImage({
  src,
  alt,
  className = '',
}: {
  src: string | null | undefined;
  alt: string;
  className?: string;
}) {
  const [failed, setFailed] = useState(false);

  if (!src || failed) {
    return (
      <div
        className={`img-fallback ${className}`}
        role="img"
        aria-label={alt}
      />
    );
  }

  return (
    <img
      src={src}
      alt={alt}
      className={className}
      loading="lazy"
      onError={() => setFailed(true)}
    />
  );
}

/** Run a callback when clicking outside the referenced element. */
export function useClickOutside<T extends HTMLElement>(
  onOutside: () => void
): React.RefObject<T | null> {
  const ref = useRef<T | null>(null);
  const cbRef = useRef(onOutside);

  useEffect(() => {
    cbRef.current = onOutside;
  });

  useEffect(() => {
    const handler = (e: MouseEvent | TouchEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        cbRef.current();
      }
    };
    document.addEventListener('mousedown', handler);
    document.addEventListener('touchstart', handler);
    return () => {
      document.removeEventListener('mousedown', handler);
      document.removeEventListener('touchstart', handler);
    };
  }, []);

  return ref;
}

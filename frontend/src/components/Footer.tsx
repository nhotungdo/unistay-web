import Link from 'next/link';
import { Globe, MessageCircle, Share2, Mail, MapPin, Phone } from 'lucide-react';

export default function Footer() {
  return (
    <footer className="footer">
      <div className="container footer-container">
        <div className="footer-grid">
          <div className="footer-col">
            <Link href="/" className="footer-logo">
              Unistay
            </Link>
            <p className="footer-desc">
              Nền tảng tìm kiếm phòng trọ và người ở ghép thông minh, kết nối sinh viên nhanh chóng và an toàn.
            </p>
            <div className="social-links">
              <a href="#" aria-label="Facebook"><Globe size={20} /></a>
              <a href="#" aria-label="Instagram"><MessageCircle size={20} /></a>
              <a href="#" aria-label="Twitter"><Share2 size={20} /></a>
            </div>
          </div>
          
          <div className="footer-col">
            <h4 className="footer-heading">Khám phá</h4>
            <Link href="/rooms" className="footer-link">Tìm phòng trọ</Link>
            <Link href="/roommates" className="footer-link">Tìm người ở ghép</Link>
            <Link href="/rental-advice" className="footer-link">Kinh nghiệm thuê</Link>
            <Link href="/marketplace" className="footer-link">Chợ sinh viên</Link>
          </div>
          
          <div className="footer-col">
            <h4 className="footer-heading">Hỗ trợ</h4>
            <Link href="/help" className="footer-link">Trung tâm trợ giúp</Link>
            <Link href="/terms" className="footer-link">Điều khoản sử dụng</Link>
            <Link href="/privacy" className="footer-link">Chính sách bảo mật</Link>
            <Link href="/contact" className="footer-link">Liên hệ</Link>
          </div>
          
          <div className="footer-col">
            <h4 className="footer-heading">Liên hệ</h4>
            <div className="footer-contact-item">
              <MapPin size={16} />
              <span>Đại học FPT, TP. HCM</span>
            </div>
            <div className="footer-contact-item">
              <Phone size={16} />
              <span>1900 1234</span>
            </div>
            <div className="footer-contact-item">
              <Mail size={16} />
              <span>support@unistay.vn</span>
            </div>
          </div>
        </div>
        
        <div className="footer-bottom">
          <p>&copy; {new Date().getFullYear()} Unistay. All rights reserved.</p>
        </div>
      </div>
    </footer>
  );
}

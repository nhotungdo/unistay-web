"use client";

import { useState, useEffect } from 'react';
import { Moon, Star, X } from 'lucide-react';
import api from '@/lib/api';
import { Spinner, AlertBanner } from '@/components/ui';
import './zodiac.css';

interface ZodiacSign {
  id: number;
  englishName: string;
  vietnameseName: string;
  startDate: string;
  endDate: string;
  element: string;
  symbol: string;
  rulingPlanet: string;
  traits: string;
  iconUrl: string;
}

interface Horoscope {
  id: number;
  zodiacSignId: number;
  date: string;
  content: string;
  luckyNumber: string;
  luckyColor: string;
}

export default function ZodiacPage() {
  const [signs, setSigns] = useState<ZodiacSign[]>([]);
  const [loading, setLoading] = useState(true);
  
  const [selectedSign, setSelectedSign] = useState<ZodiacSign | null>(null);
  const [horoscope, setHoroscope] = useState<Horoscope | null>(null);
  const [horoscopeLoading, setHoroscopeLoading] = useState(false);

  const fetchSigns = async () => {
    try {
      const res = await api.get('/Zodiac');
      setSigns(res.data.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSigns();
  }, []);

  const openHoroscope = async (sign: ZodiacSign) => {
    setSelectedSign(sign);
    setHoroscopeLoading(true);
    setHoroscope(null);
    
    try {
      // In real scenario, dateString is optional and defaults to today on API side
      const res = await api.get(`/Zodiac/horoscope/${sign.id}`);
      setHoroscope(res.data.data);
    } catch (err) {
      console.error(err);
    } finally {
      setHoroscopeLoading(false);
    }
  };

  if (loading) {
    return <div className="zodiac-container" style={{display:'flex', alignItems:'center', justifyContent:'center'}}><Spinner size={50} color="white" /></div>;
  }

  return (
    <div className="zodiac-container fade-in">
      <div className="container">
        <div className="zodiac-header">
          <Moon size={48} className="mx-auto mb-4" color="#FDE047" />
          <h1 className="zodiac-title">Tử vi Hàng ngày</h1>
          <p className="zodiac-desc">Khám phá năng lượng của các chòm sao và gợi ý để có một ngày hoàn hảo.</p>
        </div>

        <div className="zodiac-grid">
          {signs.map(sign => (
            <div key={sign.id} className="zodiac-card" onClick={() => openHoroscope(sign)}>
              <div className="zodiac-icon-wrapper">
                <Star size={32} color="#FDE047" />
                {/* Normally we'd use iconUrl but for demo we use Star */}
              </div>
              <h3 className="zodiac-name">{sign.vietnameseName}</h3>
              <p className="zodiac-dates">{sign.englishName} ({sign.startDate} - {sign.endDate})</p>
              <span className={`zodiac-element ${sign.element.toLowerCase()}`}>{sign.element}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Horoscope Modal */}
      {selectedSign && (
        <div className="zodiac-modal-overlay">
          <div className="zodiac-modal fade-in" style={{animationDuration: '0.3s'}}>
            <button className="zodiac-modal-close" onClick={() => setSelectedSign(null)}>
              <X size={24} />
            </button>
            
            <div className="zodiac-modal-header">
              <div className="zodiac-icon-wrapper" style={{width: 60, height: 60, marginBottom: '1rem'}}>
                <Star size={24} color="#FDE047" />
              </div>
              <h2 className="zodiac-name mb-1">{selectedSign.vietnameseName}</h2>
              <div className="zodiac-horoscope-date">
                Dự báo ngày {new Date().toLocaleDateString('vi-VN')}
              </div>
            </div>

            <div className="zodiac-modal-body">
              {horoscopeLoading ? (
                <div className="text-center py-8"><Spinner size={40} color="#FDE047" /></div>
              ) : horoscope ? (
                <>
                  <div className="horoscope-content">
                    {horoscope.content}
                  </div>
                  <div className="lucky-stats">
                    <div className="lucky-item">
                      <span className="lucky-label">Số may mắn</span>
                      <span className="lucky-value">{horoscope.luckyNumber}</span>
                    </div>
                    <div className="lucky-item">
                      <span className="lucky-label">Màu sắc</span>
                      <span className="lucky-value" style={{color: horoscope.luckyColor !== 'Bất kỳ' ? horoscope.luckyColor : '#FDE047'}}>{horoscope.luckyColor}</span>
                    </div>
                  </div>
                </>
              ) : (
                <AlertBanner variant="error">Chưa có dữ liệu tử vi cho hôm nay.</AlertBanner>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

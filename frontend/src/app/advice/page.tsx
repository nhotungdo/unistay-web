"use client";

import { useState, useRef, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Sparkles, Send, User, MessageCircle, MapPin, TrendingUp } from 'lucide-react';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import './advice.css';

interface Message {
  id: string;
  type: 'user' | 'ai';
  content: string;
}

const SUGGESTIONS = [
  "Làm sao để nhận biết phòng trọ lừa đảo?",
  "Giá thuê trung bình ở khu vực Quận 1 hiện nay?",
  "Nên hỏi chủ trọ những gì khi xem phòng?",
  "Cần lưu ý gì trong hợp đồng thuê nhà?"
];

export default function AdvicePage() {
  const router = useRouter();
  const { user, loading } = useAuth();
  
  const [messages, setMessages] = useState<Message[]>([
    {
      id: '1',
      type: 'ai',
      content: 'Chào bạn! Mình là Trợ lý AI của Unistay. Mình có thể giúp bạn tìm hiểu giá cả thị trường, đánh giá rủi ro phòng trọ, hoặc đưa ra lời khuyên khi ký hợp đồng. Bạn cần hỗ trợ gì?'
    }
  ]);
  const [input, setInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login?redirect=/advice');
    }
  }, [user, loading, router]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isTyping]);

  const handleSend = async (text: string) => {
    if (!text.trim() || isTyping) return;

    // Add user message
    const userMsg: Message = { id: Date.now().toString(), type: 'user', content: text };
    setMessages(prev => [...prev, userMsg]);
    setInput('');
    setIsTyping(true);

    try {
      const res = await api.post('/RentalAdvice/chat', { message: text });
      
      const aiMsg: Message = { 
        id: (Date.now() + 1).toString(), 
        type: 'ai', 
        content: res.data.response 
      };
      
      setMessages(prev => [...prev, aiMsg]);
    } catch (err) {
      console.error(err);
      setMessages(prev => [...prev, { 
        id: (Date.now() + 1).toString(), 
        type: 'ai', 
        content: 'Xin lỗi, hệ thống AI đang gặp chút sự cố. Bạn thử lại sau nhé!' 
      }]);
    } finally {
      setIsTyping(false);
    }
  };

  const onSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleSend(input);
  };

  if (loading || !user) return null;

  return (
    <div className="advice-container fade-in">
      
      {/* Sidebar for History / Tools */}
      <aside className="advice-sidebar">
        <div className="advice-sidebar-header">
          <h2 className="advice-title"><Sparkles size={20} /> Tiện ích AI</h2>
        </div>
        
        <div className="advice-history custom-scrollbar">
          <div className="history-item active">
            <MessageCircle size={16} />
            <span className="history-item-text">Trò chuyện mới</span>
          </div>
          
          <h3 className="text-muted text-xs font-bold uppercase mt-6 mb-2 px-2">Công cụ phân tích</h3>
          
          <div className="history-item" onClick={() => handleSend("Phân tích giá thuê hiện tại")}>
            <TrendingUp size={16} />
            <span className="history-item-text">Phân tích giá thị trường</span>
          </div>
          <div className="history-item" onClick={() => handleSend("Gợi ý khu vực tốt nhất")}>
            <MapPin size={16} />
            <span className="history-item-text">Gợi ý khu vực an ninh</span>
          </div>
        </div>
      </aside>

      {/* Chat Area */}
      <main className="advice-chat-area">
        <div className="advice-chat-header">
          <div className="ai-avatar-wrapper">
            <div className="ai-avatar">
              <Sparkles size={20} />
            </div>
            <div>
              <div className="font-bold">Trợ lý AI Unistay</div>
              <div className="ai-status">Luôn sẵn sàng hỗ trợ</div>
            </div>
          </div>
        </div>

        <div className="advice-messages custom-scrollbar">
          {messages.map(msg => (
            <div key={msg.id} className={`advice-msg ${msg.type}`}>
              <div className={`msg-icon ${msg.type}`}>
                {msg.type === 'ai' ? <Sparkles size={16} /> : <User size={16} />}
              </div>
              <div className="msg-content">
                {msg.content.split('\n').map((line, i) => (
                  <span key={i}>{line}<br/></span>
                ))}
              </div>
            </div>
          ))}
          
          {isTyping && (
            <div className="advice-msg ai">
              <div className="msg-icon ai"><Sparkles size={16} /></div>
              <div className="msg-content msg-typing">
                <div className="dot"></div>
                <div className="dot"></div>
                <div className="dot"></div>
              </div>
            </div>
          )}
          
          <div ref={messagesEndRef} />
        </div>

        <div className="advice-input-container">
          {messages.length === 1 && (
            <div className="suggestions mb-4">
              {SUGGESTIONS.map((s, idx) => (
                <button key={idx} className="suggestion-pill" onClick={() => handleSend(s)}>
                  {s}
                </button>
              ))}
            </div>
          )}
          
          <form className="advice-input-wrapper" onSubmit={onSubmit}>
            <input 
              type="text" 
              className="advice-input" 
              placeholder="Nhập câu hỏi của bạn..." 
              value={input}
              onChange={e => setInput(e.target.value)}
              disabled={isTyping}
            />
            <button type="submit" className="btn-send-ai" disabled={!input.trim() || isTyping}>
              <Send size={18} />
            </button>
          </form>
        </div>
      </main>

    </div>
  );
}

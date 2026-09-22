"use client";

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { 
  Search, Send, Paperclip, Image as ImageIcon, 
  MoreVertical, Phone, Video, CalendarClock, MessageSquare
} from 'lucide-react';
import * as signalR from '@microsoft/signalr';
import api from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import { Spinner } from '@/components/ui';
import './messages.css';

interface Conversation {
  userId?: string;
  groupId?: number;
  isGroup: boolean;
  userName: string;
  userAvatar: string;
  lastMessage?: {
    content: string;
    createdAt: string;
    isSent: boolean;
  };
  unreadCount: number;
  isOnline?: boolean;
}

interface Message {
  id: number;
  content: string;
  senderId: string;
  senderAvatar: string;
  createdAt: string;
  isSent: boolean;
  status: string;
}

export default function MessagesPage() {
  const router = useRouter();
  const { user, loading } = useAuth();
  
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeChat, setActiveChat] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  
  const [messageInput, setMessageInput] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [hubConnection, setHubConnection] = useState<signalR.HubConnection | null>(null);
  
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const fetchConversations = async () => {
    try {
      const res = await api.get('/messages/conversations');
      setConversations(res.data);
    } catch (err) {
      console.error("Lỗi khi tải danh sách trò chuyện", err);
    }
  };

  const setupSignalR = () => {
    const token = localStorage.getItem('token');
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:8386/chatHub", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveMessage", (message: any) => {
      // If message belongs to active chat, add it
      setActiveChat(currentActive => {
        if (currentActive && 
           ((!currentActive.isGroup && (message.senderId === currentActive.userId || message.receiverId === currentActive.userId)) ||
            (currentActive.isGroup && message.groupId === currentActive.groupId))) {
          
          setMessages(prev => {
            // Avoid duplicate
            if (prev.find(m => m.id === message.id)) return prev;
            
            return [...prev, {
              id: message.id,
              content: message.content,
              senderId: message.senderId,
              senderAvatar: message.senderAvatar,
              createdAt: message.createdAt,
              isSent: message.senderId === user?.id,
              status: message.status
            }];
          });
          
          setTimeout(() => scrollToBottom(), 100);
        }
        return currentActive;
      });

      // Also refresh conversations to update last message & unread count
      fetchConversations();
    });

    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch(err => console.error("SignalR Connection Error: ", err));

    setHubConnection(connection);
  };

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login?redirect=/messages');
    } else if (user) {
      fetchConversations();
      setupSignalR();
    }
    
    return () => {
      if (hubConnection) {
        hubConnection.stop();
      }
    };
     
  }, [user, loading, router]);

  const loadChat = async (conv: Conversation) => {
    setActiveChat(conv);
    setMessages([]); // clear current messages
    try {
      const url = conv.isGroup ? `/messages/group/${conv.groupId}` : `/messages/${conv.userId}`;
      const res = await api.get(url);
      setMessages(res.data.data.reverse()); // data is returned desc, we need asc for display
      setTimeout(() => scrollToBottom(), 100);
    } catch (err) {
      console.error("Lỗi khi tải tin nhắn", err);
    }
  };

  const sendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!messageInput.trim() || !activeChat || isSending) return;

    setIsSending(true);
    const textToSend = messageInput;
    setMessageInput(''); // Optimistic clear

    try {
      const payload = {
        receiverId: activeChat.isGroup ? null : activeChat.userId,
        groupId: activeChat.isGroup ? activeChat.groupId : null,
        content: textToSend,
        type: 'Text',
        isEncrypted: false
      };

      await api.post('/messages/send', payload);
      // We don't manually append to state here because SignalR will broadcast it back to us, 
      // preventing duplicate messages. (In a real robust app, we might add an optimistic UI message and replace it).
      
    } catch (err) {
      console.error("Lỗi khi gửi tin nhắn", err);
      setMessageInput(textToSend); // Restore on fail
    } finally {
      setIsSending(false);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const formatTime = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  };

  if (loading || !user) {
    return <div className="container" style={{ padding: '5rem', textAlign: 'center' }}><Spinner size={40} /></div>;
  }

  return (
    <div className="messages-layout fade-in">
      
      {/* Sidebar */}
      <aside className="messages-sidebar">
        <div className="messages-header">
          <h2 className="messages-title">Đoạn chat</h2>
          <div className="chat-actions">
            <button className="chat-action-btn" title="Tin nhắn mới"><MessageSquare size={18} /></button>
          </div>
        </div>
        
        <div className="search-chat">
          <div className="relative">
            <Search size={16} className="search-chat-icon" />
            <input type="text" className="search-chat-input" placeholder="Tìm kiếm tin nhắn..." />
          </div>
        </div>

        <div className="conversation-list custom-scrollbar">
          {conversations.length === 0 ? (
            <div style={{padding: '2rem', textAlign: 'center', color: 'var(--text-muted)'}}>
              Chưa có cuộc trò chuyện nào.
            </div>
          ) : (
            conversations.map((c, idx) => {
              const isActive = (activeChat?.userId === c.userId && !c.isGroup) || (activeChat?.groupId === c.groupId && c.isGroup);
              
              return (
                <div 
                  key={idx} 
                  className={`conversation-item ${isActive ? 'active' : ''}`}
                  onClick={() => loadChat(c)}
                >
                  <div className="conv-avatar-wrapper">
                    <img 
                      src={c.userAvatar ? `http://localhost:8386${c.userAvatar}` : '/default-avatar.svg'} 
                      alt={c.userName} 
                      className="conv-avatar" 
                      onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                    />
                    {c.isOnline && <div className="conv-online-dot"></div>}
                  </div>
                  <div className="conv-info">
                    <div className="conv-header">
                      <span className="conv-name">{c.userName}</span>
                      {c.lastMessage && <span className="conv-time">{formatTime(c.lastMessage.createdAt)}</span>}
                    </div>
                    <div className="conv-last-msg">
                      <span className="conv-text">
                        {c.lastMessage ? (c.lastMessage.isSent ? `Bạn: ${c.lastMessage.content}` : c.lastMessage.content) : 'Nhấn để trò chuyện'}
                      </span>
                      {c.unreadCount > 0 && <span className="conv-unread">{c.unreadCount}</span>}
                    </div>
                  </div>
                </div>
              );
            })
          )}
        </div>
      </aside>

      {/* Chat Area */}
      {activeChat ? (
        <div className="chat-area">
          <div className="chat-header">
            <div className="chat-header-info">
              <img 
                src={activeChat.userAvatar ? `http://localhost:8386${activeChat.userAvatar}` : '/default-avatar.svg'} 
                alt={activeChat.userName} 
                className="conv-avatar" 
                style={{width: '40px', height: '40px'}}
                onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
              />
              <div>
                <div className="chat-header-name">{activeChat.userName}</div>
                <div className="chat-header-status">Đang hoạt động</div>
              </div>
            </div>
            
            <div className="chat-actions">
              <button className="chat-action-btn" title="Gọi thoại"><Phone size={18} /></button>
              <button className="chat-action-btn" title="Gọi video"><Video size={18} /></button>
              <button className="chat-action-btn" title="Đặt lịch xem phòng"><CalendarClock size={18} /></button>
              <button className="chat-action-btn" title="Tùy chọn khác"><MoreVertical size={18} /></button>
            </div>
          </div>

          <div className="chat-messages custom-scrollbar">
            {messages.length === 0 ? (
              <div className="text-center text-muted" style={{marginTop: 'auto', marginBottom: 'auto'}}>
                Hãy bắt đầu cuộc trò chuyện.
              </div>
            ) : (
              messages.map(msg => (
                <div key={msg.id} className={`msg-wrapper ${msg.isSent ? 'sent' : 'received'}`}>
                  {!msg.isSent && (
                    <img 
                      src={msg.senderAvatar ? `http://localhost:8386${msg.senderAvatar}` : '/default-avatar.svg'} 
                      alt="Avatar" 
                      className="msg-avatar" 
                      onError={(e) => { (e.target as HTMLImageElement).src = '/default-avatar.svg' }}
                    />
                  )}
                  <div style={{display: 'flex', flexDirection: 'column'}}>
                    <div className="msg-bubble">{msg.content}</div>
                    <div className="msg-time">
                      {formatTime(msg.createdAt)}
                      {msg.isSent && <span className="msg-status"> {msg.status === 'Seen' ? '• Đã xem' : '• Đã nhận'}</span>}
                    </div>
                  </div>
                </div>
              ))
            )}
            <div ref={messagesEndRef} />
          </div>

          <form className="chat-input-area" onSubmit={sendMessage}>
            <button type="button" className="chat-action-btn" title="Đính kèm file"><Paperclip size={20} /></button>
            <button type="button" className="chat-action-btn" title="Gửi ảnh"><ImageIcon size={20} /></button>
            
            <div className="chat-input-wrapper">
              <input 
                type="text" 
                className="chat-input" 
                placeholder="Nhập tin nhắn..." 
                value={messageInput}
                onChange={e => setMessageInput(e.target.value)}
                autoFocus
              />
            </div>
            
            <button 
              type="submit" 
              className="chat-send-btn" 
              disabled={!messageInput.trim() || isSending}
            >
              <Send size={18} />
            </button>
          </form>
        </div>
      ) : (
        <div className="chat-empty">
          <MessageSquare size={64} className="chat-empty-icon" />
          <h3>Tin nhắn của bạn</h3>
          <p>Chọn một cuộc trò chuyện để bắt đầu nhắn tin.</p>
        </div>
      )}
    </div>
  );
}

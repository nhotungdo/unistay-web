"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

let currentConversationId = null;
let currentReceiverId = null;
let conversations = [];

// DOM Elements
const conversationList = document.getElementById('conversation-list');
const messagesContainer = document.getElementById('messages-container');
const messageInput = document.getElementById('message-input');
const sendBtn = document.getElementById('send-btn');
const headerName = document.getElementById('header-name');
const headerAvatar = document.getElementById('header-avatar');
const headerStatus = document.getElementById('header-status-text');
const headerStatusIndicator = document.getElementById('header-status-indicator');
const chatInterface = document.getElementById('chat-interface');
const chatPlaceholder = document.getElementById('chat-placeholder');
const searchInput = document.getElementById('conversation-search');
const userSearchInput = document.getElementById('user-search-input');
const searchResultsList = document.getElementById('search-results-list');
const messengerApp = document.getElementById('messenger-app');
const mobileBackBtn = document.getElementById('mobile-back-btn');

// Initialize
document.addEventListener('DOMContentLoaded', async () => {
    await startConnection();
    await loadConversations();

    // Check if URL has conversation id
    const urlParams = new URLSearchParams(window.location.search);
    const chatId = urlParams.get('chatId');
    // If chatId exists, logic to open it directly would go here
});

async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");

        connection.on("ReceiveMessage", (message) => {
            handleIncomingMessage(message);
        });

        connection.on("MessageSent", (message) => {
            if (currentConversationId && parseInt(currentConversationId) === parseInt(message.conversationId)) {
                appendMessage(message, true);
                scrollToBottom();
            }
            updateConversationPreview(message);
        });

        connection.on("MessagesRead", (conversationId) => {
            if (currentConversationId && parseInt(conversationId) === parseInt(currentConversationId)) {
                // Update UI to show 'Seen' logic if implemented
            }
        });

    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(startConnection, 5000);
    }
}

async function loadConversations() {
    try {
        const response = await fetch('/Chat/GetConversations');
        conversations = await response.json();
        renderConversations(conversations);
    } catch (error) {
        console.error("Error loading conversations:", error);
    }
}

function renderConversations(list) {
    conversationList.innerHTML = '';

    if (list.length === 0) {
        conversationList.innerHTML = '<div class="text-center mt-4 text-muted small">Chưa có tin nhắn nào.</div>';
        return;
    }

    list.forEach(c => {
        const div = document.createElement('div');
        div.className = `conversation-item ${c.unread > 0 ? 'unread' : ''}`;
        div.dataset.id = c.id;
        div.dataset.otherUserId = c.otherUserId;
        div.onclick = () => selectConversation(c);

        div.innerHTML = `
            <div class="avatar-container">
                <img src="${c.avatar}" class="user-avatar" loading="lazy">
                <div class="status-indicator"></div>
            </div>
            <div class="conv-details">
                <div class="conv-top">
                    <div class="conv-name">${c.name}</div>
                    <div class="conv-time">${c.time}</div>
                </div>
                <div class="conv-message">
                    ${c.unread > 0 ? '<strong>' + c.lastMessage + '</strong>' : c.lastMessage}
                </div>
            </div>
        `;
        conversationList.appendChild(div);
    });
}

async function selectConversation(conv) {
    currentConversationId = conv.id;
    currentReceiverId = conv.otherUserId;

    // UI Updates
    chatPlaceholder.style.display = 'none';
    chatInterface.style.display = 'flex';
    messengerApp.classList.add('chat-sub-open'); // For mobile

    // Highlight
    document.querySelectorAll('.conversation-item').forEach(el => el.classList.remove('active'));
    const activeEl = document.querySelector(`.conversation-item[data-id="${conv.id}"]`);
    if (activeEl) {
        activeEl.classList.add('active');
        activeEl.classList.remove('unread');
        // Reset preview boldness
        const msgEl = activeEl.querySelector('.conv-message');
        msgEl.innerHTML = msgEl.textContent;
    }

    // Header
    headerName.textContent = conv.name;
    headerAvatar.src = conv.avatar;

    // Load History
    messagesContainer.innerHTML = '<div class="text-center mt-5"><div class="spinner-border spinner-border-sm text-secondary"></div></div>';

    try {
        const res = await fetch(`/Chat/GetHistory?conversationId=${conv.id}`);
        const messages = await res.json();
        renderMessages(messages);

        // Mark as read
        await connection.invoke("MarkAsRead", parseInt(conv.id));

    } catch (err) {
        console.error("Error loading history:", err);
    }
}

function renderMessages(messages) {
    messagesContainer.innerHTML = '';
    messages.forEach(msg => appendMessage(msg));
    scrollToBottom();
}

function appendMessage(msg, isNew = false) {
    // Determine class
    const isMe = msg.isMe;

    const div = document.createElement('div');
    div.className = `message-row ${isMe ? 'me' : 'other'}`;

    let contentHtml = `<div class="msg-bubble">${msg.content}<span class="msg-meta">${new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span></div>`;

    if (msg.type === 'Image') {
        contentHtml = `
            <div class="msg-bubble" style="background:transparent; padding:0; border:none;">
                <img src="${msg.attachmentUrl}" style="max-width: 250px; border-radius: 12px; cursor: pointer;" onclick="window.open(this.src)">
            </div>`;
    }

    div.innerHTML = `
        ${!isMe ? `<img src="${msg.senderAvatar}" class="msg-avatar">` : ''}
        ${contentHtml}
    `;

    messagesContainer.appendChild(div);
}

function scrollToBottom() {
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

function handleIncomingMessage(msg) {
    if (currentConversationId && parseInt(msg.conversationId) === parseInt(currentConversationId)) {
        msg.isMe = false;
        appendMessage(msg, true);
        scrollToBottom();
        connection.invoke("MarkAsRead", parseInt(msg.conversationId));
    } else {
        updateConversationPreview(msg, true);
    }
}

function updateConversationPreview(msg, isUnread = false) {
    loadConversations(); // Simplest way to re-sort and update
}

// Sending
sendBtn.addEventListener('click', sendMessage);
messageInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') sendMessage();
});

async function sendMessage() {
    const text = messageInput.value.trim();
    const fileInput = document.getElementById('file-upload');

    if (!text && !fileInput.files.length) return;

    let attachmentUrl = null;
    let type = "Text";

    if (fileInput.files.length > 0) {
        // Upload logic
        const formData = new FormData();
        formData.append('file', fileInput.files[0]);

        try {
            const uploadRes = await fetch('/Chat/UploadAttachment', {
                method: 'POST',
                body: formData
            });
            const data = await uploadRes.json();
            if (data.success) {
                attachmentUrl = data.url;
                type = data.type;
            }
        } catch (err) {
            console.error(err);
            return;
        }
    }

    try {
        await connection.invoke("SendMessage", currentReceiverId, text, attachmentUrl, type);
        messageInput.value = '';
        fileInput.value = '';
    } catch (err) {
        console.error(err);
    }
}

// Search local
searchInput.addEventListener('input', (e) => {
    const term = e.target.value.toLowerCase();
    const items = document.querySelectorAll('.conversation-item');
    items.forEach(item => {
        const name = item.querySelector('.conv-name').textContent.toLowerCase();
        item.style.display = name.includes(term) ? 'flex' : 'none';
    });
});

// Mobile Back
mobileBackBtn.addEventListener('click', () => {
    messengerApp.classList.remove('chat-sub-open');
    setTimeout(() => {
        chatPlaceholder.style.display = 'flex';
        chatInterface.style.display = 'none';
        currentConversationId = null;
    }, 300);
});

// Global user search
let searchTimeout;
userSearchInput.addEventListener('input', (e) => {
    const val = e.target.value;
    clearTimeout(searchTimeout);
    if (val.length < 2) return;

    searchTimeout = setTimeout(async () => {
        const res = await fetch(`/Chat/SearchUsers?query=${val}`);
        const users = await res.json();

        searchResultsList.innerHTML = '';
        users.forEach(u => {
            const a = document.createElement('a');
            a.className = 'list-group-item list-group-item-action bg-transparent text-white border-secondary d-flex align-items-center';
            a.href = '#';
            a.innerHTML = `<img src="${u.avatar}" class="rounded-circle me-2" width="30"> ${u.name}`;
            a.onclick = async (ev) => {
                ev.preventDefault();
                const cRes = await fetch(`/Chat/GetConversationId?targetUserId=${u.id}`);
                const data = await cRes.json();

                const modal = bootstrap.Modal.getInstance(document.getElementById('newChatModal'));
                modal.hide();

                if (data.conversationId) {
                    await loadConversations();
                    // find and click
                    setTimeout(() => {
                        const item = document.querySelector(`.conversation-item[data-id="${data.conversationId}"]`);
                        if (item) item.click();
                    }, 500);
                } else {
                    // Prepare new
                    currentReceiverId = u.id;
                    currentConversationId = null;
                    chatPlaceholder.style.display = 'none';
                    chatInterface.style.display = 'flex';
                    headerName.textContent = u.name;
                    headerAvatar.src = u.avatar;
                    messagesContainer.innerHTML = '<div class="text-center text-muted mt-5">Bắt đầu cuộc trò chuyện mới</div>';
                }
            };
            searchResultsList.appendChild(a);
        });
    }, 400);
});

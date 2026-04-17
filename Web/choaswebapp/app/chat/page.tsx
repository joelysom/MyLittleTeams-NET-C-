'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '../../lib/useAuth';
import { ChatServiceTS, ChatMessage, Conversation } from '../../lib/chatService';
import {
  MessageCircle,
  Phone,
  Video,
  Info,
  MoreVertical,
  Paperclip,
  Send,
  Search,
  Plus,
  Filter,
  Loader,
} from 'lucide-react';

export default function ChatPage() {
  const user = useAuth();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [selectedConversation, setSelectedConversation] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [messageInput, setMessageInput] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [chatService, setChatService] = useState<ChatServiceTS | null>(null);

  // Load conversations and messages
  useEffect(() => {
    if (!user) return;

    const loadData = async () => {
      try {
        setLoading(true);
        const service = new ChatServiceTS(user.uid, user.uid);
        setChatService(service);

        const loadedConversations = await service.loadConversationsAsync();
        setConversations(loadedConversations);

        if (loadedConversations.length > 0) {
          setSelectedConversation(loadedConversations[0]);
          const contactId =
            loadedConversations[0].userAId === user.uid
              ? loadedConversations[0].userBId
              : loadedConversations[0].userAId;
          const msgs = await service.loadMessagesAsync(contactId);
          setMessages(msgs);
        }
      } catch (error) {
        console.error('Erro ao carregar chat:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [user]);

  const handleSelectConversation = async (conversation: Conversation) => {
    setSelectedConversation(conversation);
    if (chatService && user) {
      const contactId = conversation.userAId === user.uid ? conversation.userBId : conversation.userAId;
      const msgs = await chatService.loadMessagesAsync(contactId);
      setMessages(msgs);
    }
  };

  const handleSendMessage = async () => {
    if (!messageInput.trim() || !selectedConversation || !chatService || !user) return;

    try {
      setSending(true);
      const contactId =
        selectedConversation.userAId === user.uid
          ? selectedConversation.userBId
          : selectedConversation.userAId;

      const contactName =
        selectedConversation.userAId === user.uid
          ? selectedConversation.userBName
          : selectedConversation.userAName;

      const result = await chatService.sendMessageAsync(
        contactId,
        contactName,
        user.displayName || 'Usuário',
        messageInput,
        'text'
      );

      if (result.success) {
        const newMessage: ChatMessage = {
          messageId: `msg_${Date.now()}`,
          documentId: `doc_${Date.now()}`,
          senderId: user.uid,
          senderName: user.displayName || 'Usuário',
          content: messageInput,
          messageType: 'text',
          timestamp: new Date(),
          isOwn: true,
        };

        setMessages([...messages, newMessage]);
        setMessageInput('');

        // Reload conversations to update last message
        const updatedConversations = await chatService.loadConversationsAsync();
        setConversations(updatedConversations);
      }
    } catch (error) {
      console.error('Erro ao enviar mensagem:', error);
    } finally {
      setSending(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const getContactName = (conversation: Conversation): string => {
    return conversation.userAId === user?.uid ? conversation.userBName : conversation.userAName;
  };

  const filteredConversations = conversations.filter((conv) => {
    const name = getContactName(conv);
    return name.toLowerCase().includes(searchInput.toLowerCase());
  });

  if (loading) {
    return (
      <div className="h-screen flex items-center justify-center">
        <div className="text-center">
          <Loader className="mx-auto animate-spin text-blue-600 mb-4" size={48} />
          <p className="text-slate-600">Carregando mensagens...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col">
      {/* Top Bar */}
      <div className="bg-white border-b border-slate-200 h-20 flex items-center px-8 shadow-sm">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-full bg-gradient-to-br from-blue-600 to-indigo-600 flex items-center justify-center text-white text-lg font-bold">
            💬
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">Conversas</h1>
            <p className="text-sm text-slate-500">Chat direto, histórico e presença do workspace</p>
          </div>
        </div>
      </div>

      {/* Main Chat Area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Sidebar - Conversations List */}
        <div className="w-80 bg-white border-r border-slate-200 flex flex-col">
          {/* Header with Add Button */}
          <div className="border-b border-slate-200 p-4">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-slate-900">💬 Conversas</h2>
              <div className="flex gap-2">
                <button className="p-2 hover:bg-slate-100 rounded-lg transition text-blue-600">
                  <Plus size={18} />
                </button>
                <button className="p-2 hover:bg-slate-100 rounded-lg transition">
                  <Filter size={18} />
                </button>
              </div>
            </div>

            {/* Search Bar */}
            <div className="relative">
              <Search className="absolute left-3 top-3 text-slate-400" size={18} />
              <input
                type="text"
                placeholder="🔍 Pesquisar conversas..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                className="w-full pl-10 pr-4 py-2 bg-slate-100 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-600"
              />
            </div>
          </div>

          {/* Filters */}
          <div className="border-b border-slate-200 px-4 py-3 flex gap-2">
            <button className="px-4 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-semibold">
              Favoritos
            </button>
            <button className="px-4 py-1 bg-slate-100 text-slate-700 rounded-full text-sm font-semibold hover:bg-slate-200">
              Chats
            </button>
            <span className="ml-auto text-xs text-slate-500 font-semibold">Personalizada</span>
          </div>

          {/* Conversations List */}
          <div className="flex-1 overflow-y-auto">
            {filteredConversations.length === 0 ? (
              <div className="p-4 text-center text-slate-500">
                <MessageCircle size={32} className="mx-auto mb-2 opacity-50" />
                <p className="text-sm">Nenhuma conversa encontrada</p>
              </div>
            ) : (
              filteredConversations.map((conversation) => {
                const contactName = getContactName(conversation);
                const initial = contactName.charAt(0).toUpperCase();

                return (
                  <button
                    key={conversation.conversationId}
                    onClick={() => handleSelectConversation(conversation)}
                    className={`w-full text-left px-4 py-3 border-b border-slate-100 hover:bg-slate-50 transition ${
                      selectedConversation?.conversationId === conversation.conversationId
                        ? 'bg-blue-50'
                        : ''
                    }`}
                  >
                    <div className="flex items-start gap-3">
                      <div className="relative flex-shrink-0">
                        <div className="w-12 h-12 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-white font-semibold text-sm">
                          {initial}
                        </div>
                        <div className="absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-white bg-green-500" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-semibold text-slate-900">{contactName}</p>
                        <p className="text-xs text-slate-500 truncate">{conversation.lastMessage}</p>
                      </div>
                    </div>
                  </button>
                );
              })
            )}
          </div>
        </div>

        {/* Chat Area */}
        {selectedConversation ? (
          <div className="flex-1 flex flex-col bg-gradient-to-br from-slate-50 to-blue-50">
            {/* Top Bar - Contact Info */}
            <div className="bg-white border-b border-slate-200 px-8 py-4 flex items-center justify-between shadow-sm">
              <div className="flex items-center gap-4">
                <div className="relative">
                  <div className="w-12 h-12 rounded-full bg-gradient-to-br from-blue-600 to-indigo-600 flex items-center justify-center text-white font-semibold">
                    {getContactName(selectedConversation).charAt(0).toUpperCase()}
                  </div>
                  <div className="absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-white bg-green-500" />
                </div>
                <div>
                  <p className="font-semibold text-slate-900">{getContactName(selectedConversation)}</p>
                  <p className="text-xs text-green-600 font-medium">Online</p>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex items-center gap-2">
                <button className="p-2 hover:bg-slate-100 rounded-lg transition text-blue-600">
                  <Phone size={18} />
                </button>
                <button className="p-2 hover:bg-slate-100 rounded-lg transition text-blue-600">
                  <Video size={18} />
                </button>
                <button className="p-2 hover:bg-slate-100 rounded-lg transition text-blue-600">
                  <Info size={18} />
                </button>
                <button className="p-2 hover:bg-slate-100 rounded-lg transition">
                  <MoreVertical size={18} />
                </button>
              </div>
            </div>

            {/* Messages Area */}
            <div className="flex-1 overflow-y-auto px-8 py-6 space-y-4">
              {messages.length === 0 ? (
                <div className="h-full flex items-center justify-center">
                  <div className="text-center">
                    <MessageCircle size={48} className="mx-auto text-slate-400 mb-4" />
                    <p className="text-slate-600">Nenhuma mensagem ainda. Comece uma conversa!</p>
                  </div>
                </div>
              ) : (
                messages.map((msg) => (
                  <div key={msg.documentId} className={`flex ${msg.isOwn ? 'justify-end' : 'justify-start'}`}>
                    <div
                      className={`max-w-xs lg:max-w-md px-4 py-3 rounded-2xl ${
                        msg.isOwn
                          ? 'bg-blue-600 text-white rounded-br-none'
                          : 'bg-white text-slate-900 border border-slate-200 rounded-bl-none'
                      }`}
                    >
                      <p className="text-sm leading-relaxed">{msg.content}</p>
                      <p
                        className={`text-xs mt-1 ${
                          msg.isOwn ? 'text-blue-100' : 'text-slate-500'
                        }`}
                      >
                        {msg.timestamp.toLocaleTimeString('pt-BR', {
                          hour: '2-digit',
                          minute: '2-digit',
                        })}
                      </p>
                    </div>
                  </div>
                ))
              )}
            </div>

            {/* Input Area */}
            <div className="bg-white border-t border-slate-200 px-8 py-4">
              <div className="flex gap-3">
                <div className="flex-1 flex items-center gap-2 bg-slate-100 border border-slate-200 rounded-2xl px-4 py-2">
                  <input
                    type="text"
                    placeholder="Escreva uma mensagem..."
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    onKeyPress={handleKeyPress}
                    disabled={sending}
                    className="flex-1 bg-transparent text-slate-900 placeholder-slate-500 focus:outline-none text-sm disabled:opacity-50"
                  />
                </div>
                <button className="p-2 hover:bg-slate-100 rounded-lg transition text-blue-600">
                  <Paperclip size={18} />
                </button>
                <button
                  onClick={handleSendMessage}
                  disabled={sending || !messageInput.trim()}
                  className="p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50"
                >
                  {sending ? <Loader size={18} className="animate-spin" /> : <Send size={18} />}
                </button>
              </div>
            </div>
          </div>
        ) : (
          <div className="flex-1 flex items-center justify-center bg-gradient-to-br from-slate-50 to-blue-50">
            <div className="text-center">
              <MessageCircle size={64} className="mx-auto text-slate-300 mb-4" />
              <p className="text-slate-500 text-lg">Selecione uma conversa para começar</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

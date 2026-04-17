'use client';

import { useAuth } from '../../../lib/useAuth';
import { Search, Plus, Send, MoreVertical } from 'lucide-react';
import { useState } from 'react';

interface Conversation {
  id: string;
  contactName: string;
  lastMessage: string;
  timestamp: string;
  unread: boolean;
  avatar: string;
}

export default function ChatsPage() {
  const user = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedChat, setSelectedChat] = useState<string | null>(null);
  const [messageInput, setMessageInput] = useState('');
  const [conversations, setConversations] = useState<Conversation[]>([
    {
      id: '1',
      contactName: 'Ana Silva',
      lastMessage: 'Vai ficar pronto para segunda?',
      timestamp: 'há 2h',
      unread: true,
      avatar: 'A',
    },
    {
      id: '2',
      contactName: 'Bruno Costa',
      lastMessage: 'Vi a apresentação, ficou ótimo!',
      timestamp: 'há 5h',
      unread: false,
      avatar: 'B',
    },
    {
      id: '3',
      contactName: 'Camila Rocha',
      lastMessage: 'Vamos refinar o código hoje?',
      timestamp: 'ontem',
      unread: false,
      avatar: 'C',
    },
  ]);

  const [messages, setMessages] = useState<
    Array<{ id: string; sender: string; text: string; timestamp: string }>
  >([
    {
      id: '1',
      sender: 'Ana Silva',
      text: 'Oi! Tudo bem?',
      timestamp: '10:30',
    },
    {
      id: '2',
      sender: 'Você',
      text: 'Oi! Tudo certo por aqui',
      timestamp: '10:31',
    },
    {
      id: '3',
      sender: 'Ana Silva',
      text: 'Legal! Vai ficar pronto para segunda?',
      timestamp: '10:35',
    },
  ]);

  const handleSendMessage = (e: React.FormEvent) => {
    e.preventDefault();
    if (!messageInput.trim() || !selectedChat) return;

    // Add message
    setMessages([
      ...messages,
      {
        id: String(messages.length + 1),
        sender: 'Você',
        text: messageInput,
        timestamp: new Date().toLocaleTimeString('pt-BR', {
          hour: '2-digit',
          minute: '2-digit',
        }),
      },
    ]);

    // Update conversation
    const updatedConversations = conversations.map((conv) => {
      if (conv.id === selectedChat) {
        return {
          ...conv,
          lastMessage: messageInput,
          unread: false,
        };
      }
      return conv;
    });
    setConversations(updatedConversations);
    setMessageInput('');
  };

  const filteredConversations = conversations.filter((conv) =>
    conv.contactName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (!user) {
    return <div>Carregando...</div>;
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold text-slate-900">Chats</h2>
          <p className="text-slate-600 mt-1">Converse com seus colegas e professores</p>
        </div>
        <button className="flex items-center gap-2 bg-blue-600 text-white font-semibold px-6 py-3 rounded-lg hover:bg-blue-700 transition">
          <Plus size={20} />
          Nova Conversa
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-[600px]">
        {/* Chat List */}
        <div className="lg:col-span-1 border border-slate-200 rounded-xl overflow-hidden bg-white flex flex-col">
          <div className="p-4 border-b border-slate-200">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
              <input
                type="text"
                placeholder="Buscar conversas..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 text-sm"
              />
            </div>
          </div>

          <div className="flex-1 overflow-y-auto">
            {filteredConversations.map((conv) => (
              <button
                key={conv.id}
                onClick={() => setSelectedChat(conv.id)}
                className={`w-full text-left p-4 border-b border-slate-100 hover:bg-slate-50 transition ${
                  selectedChat === conv.id ? 'bg-blue-50 border-b-blue-200' : ''
                }`}
              >
                <div className="flex items-start gap-3">
                  <div className="w-12 h-12 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-white font-bold flex-shrink-0">
                    {conv.avatar}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <h4 className="font-semibold text-slate-900 text-sm">{conv.contactName}</h4>
                      <span className="text-xs text-slate-500">{conv.timestamp}</span>
                    </div>
                    <p
                      className={`text-xs mt-1 truncate ${
                        conv.unread
                          ? 'font-semibold text-slate-900'
                          : 'text-slate-600'
                      }`}
                    >
                      {conv.lastMessage}
                    </p>
                  </div>
                  {conv.unread && (
                    <div className="w-2 h-2 rounded-full bg-blue-600 flex-shrink-0" />
                  )}
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Chat Window */}
        <div className="lg:col-span-2 border border-slate-200 rounded-xl overflow-hidden bg-white flex flex-col">
          {selectedChat ? (
            <>
              {/* Header */}
              <div className="p-4 border-b border-slate-200 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-white font-bold">
                    {conversations.find((c) => c.id === selectedChat)?.avatar}
                  </div>
                  <div>
                    <h4 className="font-semibold text-slate-900 text-sm">
                      {conversations.find((c) => c.id === selectedChat)?.contactName}
                    </h4>
                    <p className="text-xs text-slate-500">Online</p>
                  </div>
                </div>
                <button className="p-2 hover:bg-slate-100 rounded-lg transition">
                  <MoreVertical size={18} className="text-slate-600" />
                </button>
              </div>

              {/* Messages */}
              <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-slate-50">
                {messages.map((msg) => (
                  <div
                    key={msg.id}
                    className={`flex ${
                      msg.sender === 'Você' ? 'justify-end' : 'justify-start'
                    }`}
                  >
                    <div
                      className={`max-w-xs px-4 py-2 rounded-lg ${
                        msg.sender === 'Você'
                          ? 'bg-blue-600 text-white'
                          : 'bg-white border border-slate-200 text-slate-900'
                      }`}
                    >
                      <p className="text-sm">{msg.text}</p>
                      <p
                        className={`text-xs mt-1 ${
                          msg.sender === 'Você'
                            ? 'text-blue-100'
                            : 'text-slate-500'
                        }`}
                      >
                        {msg.timestamp}
                      </p>
                    </div>
                  </div>
                ))}
              </div>

              {/* Input */}
              <form onSubmit={handleSendMessage} className="p-4 border-t border-slate-200">
                <div className="flex gap-3">
                  <input
                    type="text"
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    placeholder="Digite sua mensagem..."
                    className="flex-1 px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 text-sm"
                  />
                  <button
                    type="submit"
                    className="p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                  >
                    <Send size={18} />
                  </button>
                </div>
              </form>
            </>
          ) : (
            <div className="flex items-center justify-center h-full">
              <div className="text-center">
                <MessageCircle size={48} className="mx-auto text-slate-400 mb-4" />
                <p className="text-slate-600">Selecione uma conversa para começar</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

import { MessageCircle } from 'lucide-react';

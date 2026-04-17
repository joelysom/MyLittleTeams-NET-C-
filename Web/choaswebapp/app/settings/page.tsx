'use client';

import { useState } from 'react';
import { useAuth } from '../../lib/useAuth';
import { useRouter } from 'next/navigation';
import { auth } from '../../lib/firebase';
import { Bell, Lock, Eye, Accessibility, LogOut, ChevronRight, Moon, Smartphone } from 'lucide-react';

export default function SettingsPage() {
  const user = useAuth();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState('account');
  const [settings, setSettings] = useState({
    emailNotifications: true,
    pushNotifications: false,
    privateProfile: false,
    twoFactorAuth: false,
    darkMode: false,
    reducedMotion: false,
  });

  const handleLogout = async () => {
    try {
      await auth.signOut();
      router.push('/login');
    } catch (error) {
      console.error('Erro ao fazer logout:', error);
    }
  };

  const handleSettingChange = (key: string, value: boolean) => {
    setSettings({
      ...settings,
      [key]: value,
    });
  };

  const settingsTabs = [
    { id: 'account', label: 'Conta', icon: '👤' },
    { id: 'notifications', label: 'Notificações', icon: '🔔' },
    { id: 'privacy', label: 'Privacidade', icon: '🔒' },
    { id: 'accessibility', label: 'Acessibilidade', icon: '♿' },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50">
      {/* Top Bar */}
      <div className="bg-white border-b border-slate-200 h-20 flex items-center px-8 shadow-sm">
        <h1 className="text-2xl font-bold text-slate-900">Configurações</h1>
      </div>

      {/* Settings Container */}
      <div className="max-w-5xl mx-auto px-8 py-8">
        <div className="flex gap-6">
          {/* Sidebar Navigation */}
          <div className="w-56">
            <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm">
              {settingsTabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`w-full text-left px-6 py-4 flex items-center gap-3 border-b border-slate-100 last:border-b-0 transition-colors ${
                    activeTab === tab.id
                      ? 'bg-blue-50 text-blue-700 font-semibold'
                      : 'text-slate-700 hover:bg-slate-50'
                  }`}
                >
                  <span className="text-xl">{tab.icon}</span>
                  {tab.label}
                  {activeTab === tab.id && <ChevronRight size={18} className="ml-auto" />}
                </button>
              ))}
            </div>
          </div>

          {/* Content Area */}
          <div className="flex-1">
            {/* Account Settings */}
            {activeTab === 'account' && (
              <div className="space-y-6">
                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-xl font-bold text-slate-900 mb-6">Informações da Conta</h2>

                  <div className="space-y-6">
                    <div>
                      <label className="block text-sm font-semibold text-slate-700 mb-2">Email</label>
                      <input
                        type="email"
                        value={user?.email || ''}
                        disabled
                        className="w-full px-4 py-2 bg-slate-100 text-slate-700 rounded-lg border border-slate-200 cursor-not-allowed"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-slate-700 mb-2">Nome Completo</label>
                      <input
                        type="text"
                        value={user?.displayName || ''}
                        className="w-full px-4 py-2 text-slate-900 rounded-lg border border-slate-200 focus:border-blue-600 focus:ring-2 focus:ring-blue-100 outline-none transition"
                      />
                    </div>

                    <div>
                      <label className="flex items-center gap-3">
                        <input
                          type="checkbox"
                          checked={settings.twoFactorAuth}
                          onChange={(e) => handleSettingChange('twoFactorAuth', e.target.checked)}
                          className="w-4 h-4 text-blue-600 rounded"
                        />
                        <span className="text-slate-700 font-semibold">Autenticação de dois fatores</span>
                      </label>
                      <p className="text-xs text-slate-500 ml-7 mt-1">Adicione uma camada extra de segurança à sua conta</p>
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-xl font-bold text-slate-900 mb-4">Opções Gerais</h2>

                  <button
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-4 py-3 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition border border-red-200 font-semibold"
                  >
                    <LogOut size={18} />
                    Sair de Todas as Contas
                  </button>
                </div>
              </div>
            )}

            {/* Notification Settings */}
            {activeTab === 'notifications' && (
              <div className="space-y-6">
                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-xl font-bold text-slate-900 mb-6 flex items-center gap-3">
                    <Bell size={24} className="text-blue-600" />
                    Preferências de Notificação
                  </h2>

                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900">Notificações por Email</p>
                        <p className="text-sm text-slate-500">Receba atualizações importantes por email</p>
                      </div>
                      <input
                        type="checkbox"
                        checked={settings.emailNotifications}
                        onChange={(e) => handleSettingChange('emailNotifications', e.target.checked)}
                        className="w-5 h-5 text-blue-600 rounded cursor-pointer"
                      />
                    </div>

                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900">Notificações Push</p>
                        <p className="text-sm text-slate-500">Receba notificações em tempo real no navegador</p>
                      </div>
                      <input
                        type="checkbox"
                        checked={settings.pushNotifications}
                        onChange={(e) => handleSettingChange('pushNotifications', e.target.checked)}
                        className="w-5 h-5 text-blue-600 rounded cursor-pointer"
                      />
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-lg font-bold text-slate-900 mb-4">Notificações de Atividade</h2>
                  <div className="space-y-3 text-sm text-slate-600">
                    <label className="flex items-center gap-2">
                      <input type="checkbox" defaultChecked className="w-4 h-4 text-blue-600 rounded" />
                      Novas mensagens
                    </label>
                    <label className="flex items-center gap-2">
                      <input type="checkbox" defaultChecked className="w-4 h-4 text-blue-600 rounded" />
                      Convites de equipe
                    </label>
                    <label className="flex items-center gap-2">
                      <input type="checkbox" defaultChecked className="w-4 h-4 text-blue-600 rounded" />
                      Atualizações de projeto
                    </label>
                  </div>
                </div>
              </div>
            )}

            {/* Privacy Settings */}
            {activeTab === 'privacy' && (
              <div className="space-y-6">
                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-xl font-bold text-slate-900 mb-6 flex items-center gap-3">
                    <Lock size={24} className="text-blue-600" />
                    Controle de Privacidade
                  </h2>

                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900">Perfil Privado</p>
                        <p className="text-sm text-slate-500">Apenas pessoas que você aprova podem ver seu perfil</p>
                      </div>
                      <input
                        type="checkbox"
                        checked={settings.privateProfile}
                        onChange={(e) => handleSettingChange('privateProfile', e.target.checked)}
                        className="w-5 h-5 text-blue-600 rounded cursor-pointer"
                      />
                    </div>

                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900">Status Online Visível</p>
                        <p className="text-sm text-slate-500">Mostre quando você está online para outros usuários</p>
                      </div>
                      <input
                        type="checkbox"
                        defaultChecked
                        className="w-5 h-5 text-blue-600 rounded cursor-pointer"
                      />
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-lg font-bold text-slate-900 mb-4">Dados e Armazenamento</h2>
                  <button className="px-4 py-2 text-red-600 border border-red-200 rounded-lg hover:bg-red-50 transition font-semibold">
                    Baixar Meus Dados
                  </button>
                </div>
              </div>
            )}

            {/* Accessibility Settings */}
            {activeTab === 'accessibility' && (
              <div className="space-y-6">
                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-xl font-bold text-slate-900 mb-6 flex items-center gap-3">
                    <Accessibility size={24} className="text-blue-600" />
                    Acessibilidade e Tema
                  </h2>

                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900 flex items-center gap-2">
                          <Moon size={18} /> Modo Escuro
                        </p>
                        <p className="text-sm text-slate-500">Use uma paleta mais escura para reduzir a fadiga ocular</p>
                      </div>
                      <input
                        type="checkbox"
                        checked={settings.darkMode}
                        onChange={(e) => handleSettingChange('darkMode', e.target.checked)}
                        className="w-5 h-5 text-blue-600 rounded cursor-pointer"
                      />
                    </div>

                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900">Aumentar Contraste</p>
                        <p className="text-sm text-slate-500">Reforça o contraste para melhor legibilidade</p>
                      </div>
                      <input type="checkbox" className="w-5 h-5 text-blue-600 rounded cursor-pointer" />
                    </div>

                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg">
                      <div>
                        <p className="font-semibold text-slate-900 flex items-center gap-2">
                          <Smartphone size={18} /> Reduzir Animações
                        </p>
                        <p className="text-sm text-slate-500">Diminua movimentos rápidos e distrações</p>
                      </div>
                      <input
                        type="checkbox"
                        checked={settings.reducedMotion}
                        onChange={(e) => handleSettingChange('reducedMotion', e.target.checked)}
                        className="w-5 h-5 text-blue-600 rounded cursor-pointer"
                      />
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h2 className="text-lg font-bold text-slate-900 mb-4">Tamanho do Texto</h2>
                  <div className="flex items-center gap-4">
                    <button className="px-3 py-2 border border-slate-200 rounded-lg hover:bg-slate-100">
                      -
                    </button>
                    <span className="text-slate-700 font-semibold">100%</span>
                    <button className="px-3 py-2 border border-slate-200 rounded-lg hover:bg-slate-100">
                      +
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

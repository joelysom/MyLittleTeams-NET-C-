'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { onAuthStateChanged } from 'firebase/auth';
import { auth } from '../../lib/firebase';
import {
  MessageCircle,
  Users,
  Briefcase,
  BookOpen,
  Calendar,
  FileText,
  Settings,
  Home,
  LogOut,
  Menu,
  X,
  ChevronDown
} from 'lucide-react';

interface User {
  uid: string;
  email?: string;
  displayName?: string;
}

const navItems = [
  { label: 'Visão Geral', icon: Home, id: 'overview' },
  { label: 'Chats', icon: MessageCircle, id: 'chats' },
  { label: 'Conexões', icon: Users, id: 'connections' },
  { label: 'Equipes', icon: Briefcase, id: 'teams' },
  { label: 'Docência', icon: BookOpen, id: 'teaching' },
  { label: 'Calendário', icon: Calendar, id: 'calendar' },
  { label: 'Arquivos', icon: FileText, id: 'files' },
  { label: 'Configurações', icon: Settings, id: 'settings' },
];

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [activeNav, setActiveNav] = useState('overview');

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, (currentUser) => {
      if (currentUser) {
        setUser({
          uid: currentUser.uid,
          email: currentUser.email || '',
          displayName: currentUser.displayName || 'Usuário',
        });
        setLoading(false);
      } else {
        router.push('/login');
      }
    });

    return () => unsubscribe();
  }, [router]);

  const handleLogout = async () => {
    try {
      await auth.signOut();
      router.push('/login');
    } catch (error) {
      console.error('Erro ao fazer logout:', error);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-50 flex items-center justify-center">
        <div className="text-center">
          <div className="mb-4 inline-block">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
          <p className="text-slate-600">Carregando...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-50">
      {/* Sidebar */}
      <aside
        className={`fixed left-0 top-0 h-screen bg-white border-r border-slate-200 transition-all duration-300 ${
          sidebarOpen ? 'w-72' : 'w-24'
        } flex flex-col z-40`}
      >
        {/* Logo/Header */}
        <div className="h-20 flex items-center justify-between px-6 border-b border-slate-200">
          {sidebarOpen && (
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-blue-600 to-indigo-600 flex items-center justify-center">
                <span className="text-white font-bold text-lg">C</span>
              </div>
              <span className="font-bold text-lg text-slate-900">Choas</span>
            </div>
          )}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-2 hover:bg-slate-100 rounded-lg transition"
          >
            {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto px-3 py-6">
          <div className="space-y-2">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <button
                  key={item.id}
                  onClick={() => setActiveNav(item.id)}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                    activeNav === item.id
                      ? 'bg-blue-100 text-blue-700 font-semibold'
                      : 'text-slate-700 hover:bg-slate-100'
                  }`}
                >
                  <Icon size={20} className="flex-shrink-0" />
                  {sidebarOpen && <span className="text-sm">{item.label}</span>}
                </button>
              );
            })}
          </div>
        </nav>

        {/* User Profile Section */}
        <div className="border-t border-slate-200 p-4">
          <div className="relative">
            <button
              onClick={() => setUserMenuOpen(!userMenuOpen)}
              className="w-full flex items-center gap-3 p-3 hover:bg-slate-100 rounded-lg transition"
            >
              <div className="w-10 h-10 rounded-full bg-gradient-to-br from-cyan-400 to-blue-600 flex items-center justify-center text-white font-semibold flex-shrink-0">
                {user?.displayName?.charAt(0).toUpperCase() || 'U'}
              </div>
              {sidebarOpen && (
                <div className="flex-1 text-left min-w-0">
                  <p className="text-sm font-semibold text-slate-900 truncate">
                    {user?.displayName || 'Usuário'}
                  </p>
                  <p className="text-xs text-slate-500 truncate">{user?.email}</p>
                </div>
              )}
              {sidebarOpen && (
                <ChevronDown
                  size={16}
                  className={`transition-transform ${userMenuOpen ? 'rotate-180' : ''}`}
                />
              )}
            </button>

            {/* User Menu Dropdown */}
            {userMenuOpen && sidebarOpen && (
              <div className="absolute bottom-full left-0 right-0 mb-2 bg-white border border-slate-200 rounded-lg shadow-lg overflow-hidden">
                <button
                  onClick={() => router.push('/profile')}
                  className="w-full text-left px-4 py-2 text-sm text-slate-700 hover:bg-slate-50 transition"
                >
                  Meu Perfil
                </button>
                <button
                  onClick={() => setActiveNav('settings')}
                  className="w-full text-left px-4 py-2 text-sm text-slate-700 hover:bg-slate-50 transition"
                >
                  Configurações
                </button>
                <div className="h-px bg-slate-200" />
                <button
                  onClick={handleLogout}
                  className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition flex items-center gap-2"
                >
                  <LogOut size={16} />
                  Sair
                </button>
              </div>
            )}
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className={`transition-all duration-300 ${sidebarOpen ? 'ml-72' : 'ml-24'}`}>
        {/* Top Bar */}
        <div className="bg-white border-b border-slate-200 h-20 flex items-center px-8 shadow-sm">
          <h1 className="text-2xl font-bold text-slate-900">
            {navItems.find((item) => item.id === activeNav)?.label || 'Dashboard'}
          </h1>
        </div>

        {/* Content Area */}
        <div className="p-8">
          {children}
        </div>
      </main>
    </div>
  );
}

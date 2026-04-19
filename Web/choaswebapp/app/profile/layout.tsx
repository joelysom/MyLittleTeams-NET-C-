'use client';

import { useEffect, useState } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { onAuthStateChanged, signOut } from 'firebase/auth';
import { auth } from '../../lib/firebase';
import AvatarDisplay from '../../components/AvatarDisplay';
import { DEFAULT_AVATAR, type AvatarComponents } from '../../lib/avatarService';
import { getUserProfileService } from '../../lib/userProfileService';
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
  ChevronDown,
} from 'lucide-react';

interface User {
  uid: string;
  email?: string;
  displayName?: string;
  avatar: AvatarComponents;
  profilePhotoSource?: string;
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

const routeByNavId: Record<string, string> = {
  overview: '/dashboard',
  chats: '/chat',
  connections: '/dashboard/connections',
  teams: '/dashboard/teams',
  teaching: '/dashboard/teaching',
  calendar: '/dashboard/calendar',
  files: '/dashboard/files',
  profile: '/profile',
  settings: '/settings',
};

function getNavIdFromPath(pathname: string): string {
  if (pathname.startsWith('/chat')) return 'chats';
  if (pathname.startsWith('/dashboard/teams')) return 'teams';
  if (pathname.startsWith('/dashboard/teaching')) return 'teaching';
  if (pathname.startsWith('/dashboard/calendar')) return 'calendar';
  if (pathname.startsWith('/dashboard/files')) return 'files';
  if (pathname.startsWith('/dashboard/profile') || pathname === '/profile') return 'profile';
  if (pathname.startsWith('/settings')) return 'settings';
  if (pathname.startsWith('/dashboard')) return 'overview';
  return 'overview';
}

export default function ProfileLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [activeNav, setActiveNav] = useState(getNavIdFromPath(pathname));

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, (currentUser) => {
      if (currentUser) {
        setUser({
          uid: currentUser.uid,
          email: currentUser.email || '',
          displayName: currentUser.displayName || currentUser.email?.split('@')[0] || 'Usuário',
          avatar: DEFAULT_AVATAR,
          profilePhotoSource: '',
        });
        setLoading(false);

        void (async () => {
          try {
            const profile = await getUserProfileService().getUserProfile(currentUser.uid);
            if (!profile) {
              return;
            }

            setUser({
              uid: currentUser.uid,
              email: profile.email || currentUser.email || '',
              displayName: profile.displayName || currentUser.displayName || currentUser.email?.split('@')[0] || 'Usuário',
              avatar: profile.avatar || DEFAULT_AVATAR,
              profilePhotoSource: profile.profilePhotoSource || profile.profilePhoto || '',
            });
          } catch (error) {
            console.error('Erro ao carregar perfil do usuário:', error);
          }
        })();
      } else {
        router.push('/login');
      }
    });

    return () => unsubscribe();
  }, [router]);

  useEffect(() => {
    setActiveNav(getNavIdFromPath(pathname));
  }, [pathname]);

  const handleLogout = async () => {
    try {
      await signOut(auth);
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

  const headerTitle = pathname.startsWith('/profile') ? 'Meu Perfil' : navItems.find((item) => item.id === activeNav)?.label || 'Dashboard';

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-50">
      <aside
        className={`fixed left-0 top-0 z-40 flex h-screen flex-col border-r border-slate-200 bg-white transition-all duration-300 ${
          sidebarOpen ? 'w-72' : 'w-24'
        }`}
      >
        <div className="flex h-20 items-center justify-between border-b border-slate-200 px-6">
          {sidebarOpen && (
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br from-blue-600 to-indigo-600">
                <span className="text-lg font-bold text-white">C</span>
              </div>
              <span className="text-lg font-bold text-slate-900">Choas</span>
            </div>
          )}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="rounded-lg p-2 transition hover:bg-slate-100"
          >
            {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>

        <nav className="flex-1 overflow-y-auto px-3 py-6">
          <div className="space-y-2">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <button
                  key={item.id}
                  onClick={() => {
                    setActiveNav(item.id);
                    const target = routeByNavId[item.id];
                    if (target) {
                      router.push(target);
                    }
                  }}
                  className={`w-full rounded-xl px-4 py-3 text-left transition-all ${
                    activeNav === item.id
                      ? 'bg-blue-100 font-semibold text-blue-700'
                      : 'text-slate-700 hover:bg-slate-100'
                  } flex items-center gap-3`}
                >
                  <Icon size={20} className="flex-shrink-0" />
                  {sidebarOpen && <span className="text-sm">{item.label}</span>}
                </button>
              );
            })}
          </div>
        </nav>

        <div className="border-t border-slate-200 p-4">
          <div className="relative">
            <button
              onClick={() => setUserMenuOpen(!userMenuOpen)}
              className="flex w-full items-center gap-3 rounded-lg p-3 transition hover:bg-slate-100"
            >
              <AvatarDisplay
                avatar={user?.avatar || DEFAULT_AVATAR}
                imageSrc={user?.profilePhotoSource || ''}
                size="sm"
                fallback={user?.displayName?.charAt(0).toUpperCase() || 'U'}
              />
              {sidebarOpen && (
                <div className="min-w-0 flex-1 text-left">
                  <p className="truncate text-sm font-semibold text-slate-900">{user?.displayName || 'Usuário'}</p>
                  <p className="truncate text-xs text-slate-500">{user?.email}</p>
                </div>
              )}
              {sidebarOpen && (
                <ChevronDown size={16} className={`transition-transform ${userMenuOpen ? 'rotate-180' : ''}`} />
              )}
            </button>

            {userMenuOpen && sidebarOpen && (
              <div className="absolute bottom-full left-0 right-0 mb-2 overflow-hidden rounded-lg border border-slate-200 bg-white shadow-lg">
                <button
                  onClick={() => router.push('/profile')}
                  className="w-full px-4 py-2 text-left text-sm text-slate-700 transition hover:bg-slate-50"
                >
                  Meu Perfil
                </button>
                <button
                  onClick={() => router.push('/settings')}
                  className="w-full px-4 py-2 text-left text-sm text-slate-700 transition hover:bg-slate-50"
                >
                  Configurações
                </button>
                <div className="h-px bg-slate-200" />
                <button
                  onClick={handleLogout}
                  className="flex w-full items-center gap-2 px-4 py-2 text-left text-sm text-red-600 transition hover:bg-red-50"
                >
                  <LogOut size={16} />
                  Sair
                </button>
              </div>
            )}
          </div>
        </div>
      </aside>

      <main className={`transition-all duration-300 ${sidebarOpen ? 'ml-72' : 'ml-24'}`}>
        <div className="flex h-20 items-center border-b border-slate-200 bg-white px-8 shadow-sm">
          <h1 className="text-2xl font-bold text-slate-900">{headerTitle}</h1>
        </div>
        <div className="p-8">{children}</div>
      </main>
    </div>
  );
}

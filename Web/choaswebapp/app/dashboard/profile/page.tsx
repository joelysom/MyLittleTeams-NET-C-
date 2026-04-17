'use client';

import { useAuth } from '../../../lib/useAuth';
import { Send, Phone, MapPin, Mail } from 'lucide-react';
import { useState } from 'react';

export default function ProfilePage() {
  const user = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [profileData, setProfileData] = useState({
    displayName: user?.displayName || '',
    email: user?.email || '',
    phone: '',
    bio: '',
    title: '',
  });

  if (!user) {
    return <div>Carregando...</div>;
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold text-slate-900">Meu Perfil</h2>
          <p className="text-slate-600 mt-1">Visualize e edite suas informações</p>
        </div>
        <button
          onClick={() => setIsEditing(!isEditing)}
          className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition"
        >
          {isEditing ? 'Cancelar' : 'Editar Perfil'}
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Profile Card */}
        <div className="lg:col-span-1">
          <div className="bg-white rounded-xl border border-slate-200 p-6 sticky top-8">
            <div className="flex flex-col items-center text-center">
              <div className="w-32 h-32 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-white mb-4">
                <span className="text-5xl font-bold">
                  {(user?.displayName || 'U').charAt(0).toUpperCase()}
                </span>
              </div>
              <h3 className="text-xl font-bold text-slate-900 mb-1">
                {user?.displayName || 'Usuário'}
              </h3>
              <p className="text-sm text-slate-600 mb-6">{user?.email}</p>

              <div className="w-full space-y-3 text-left">
                <div className="flex items-center gap-3 text-slate-600 text-sm">
                  <Mail size={16} className="text-slate-400" />
                  <span>{user?.email}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Edit Form */}
        {isEditing && (
          <div className="lg:col-span-2">
            <div className="bg-white rounded-xl border border-slate-200 p-8">
              <h3 className="text-xl font-bold text-slate-900 mb-6">Editar Informações</h3>
              <form className="space-y-6">
                <div>
                  <label className="block text-sm font-semibold text-slate-700 mb-2">
                    Nome Completo
                  </label>
                  <input
                    type="text"
                    value={profileData.displayName}
                    onChange={(e) =>
                      setProfileData({ ...profileData, displayName: e.target.value })
                    }
                    className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold text-slate-700 mb-2">
                    Título Profissional
                  </label>
                  <input
                    type="text"
                    value={profileData.title}
                    onChange={(e) =>
                      setProfileData({ ...profileData, title: e.target.value })
                    }
                    placeholder="Ex: Analista de Sistemas"
                    className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold text-slate-700 mb-2">
                    Telefone
                  </label>
                  <input
                    type="tel"
                    value={profileData.phone}
                    onChange={(e) =>
                      setProfileData({ ...profileData, phone: e.target.value })
                    }
                    placeholder="(81) 9 8887-2515"
                    className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold text-slate-700 mb-2">
                    Bio
                  </label>
                  <textarea
                    value={profileData.bio}
                    onChange={(e) =>
                      setProfileData({ ...profileData, bio: e.target.value })
                    }
                    placeholder="Conte um pouco sobre você e suas habilidades..."
                    rows={4}
                    className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 resize-none"
                  />
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => setIsEditing(false)}
                    className="flex-1 px-4 py-2 border border-slate-300 text-slate-700 font-semibold rounded-lg hover:bg-slate-100 transition"
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="flex-1 px-4 py-2 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition flex items-center justify-center gap-2"
                  >
                    <Send size={16} />
                    Salvar Alterações
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Profile Details */}
        {!isEditing && (
          <div className="lg:col-span-2 space-y-6">
            <div className="bg-white rounded-xl border border-slate-200 p-8">
              <h3 className="text-xl font-bold text-slate-900 mb-6">Informações Pessoais</h3>
              <div className="space-y-4">
                <div className="pb-4 border-b border-slate-200">
                  <label className="text-xs font-semibold text-slate-500 uppercase tracking-widest">
                    Email
                  </label>
                  <p className="text-slate-900 mt-1">{user?.email || 'Não informado'}</p>
                </div>
                <div>
                  <label className="text-xs font-semibold text-slate-500 uppercase tracking-widest">
                    ID do Usuário
                  </label>
                  <p className="text-slate-900 text-sm mt-1 font-mono break-all">{user?.uid}</p>
                </div>
              </div>
            </div>

            <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-xl border border-blue-200 p-8">
              <h3 className="text-lg font-bold text-slate-900 mb-4">Estatísticas</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-2xl font-bold text-blue-600">0</p>
                  <p className="text-sm text-slate-600">Equipes Criadas</p>
                </div>
                <div>
                  <p className="text-2xl font-bold text-indigo-600">0</p>
                  <p className="text-sm text-slate-600">Projetos Completados</p>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

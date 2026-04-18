'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '../../lib/useAuth';
import { getUserProfileService } from '../../lib/userProfileService';
import { AvatarComponents, DEFAULT_AVATAR } from '../../lib/avatarService';
import AvatarDisplay from '../../components/AvatarDisplay';
import AvatarSelector from '../../components/AvatarSelector';
import { Edit3, Mail, Phone, GraduationCap, Briefcase } from 'lucide-react';

interface UserProfile {
  uid: string;
  displayName: string;
  email: string;
  avatar: AvatarComponents;
  photoURL?: string;
  headline?: string;
  course?: string;
  registration?: string;
  languages?: string;
  professionalSummary?: string;
  phoneNumber?: string;
  department?: string;
  academicFocus?: string;
}

export default function ProfilePage() {
  const user = useAuth();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [isEditingAvatar, setIsEditingAvatar] = useState(false);
  const [tempAvatar, setTempAvatar] = useState<AvatarComponents>(DEFAULT_AVATAR);

  useEffect(() => {
    const loadProfile = async () => {
      if (!user) return;

      try {
        const userProfileService = getUserProfileService();
        const firebaseProfile = await userProfileService.getUserProfile(user.uid);

        if (firebaseProfile) {
          setProfile({
            uid: user.uid,
            displayName: firebaseProfile.displayName,
            email: firebaseProfile.email,
            avatar: firebaseProfile.avatar,
          } as UserProfile);
        } else {
          setProfile({
            uid: user.uid,
            displayName: user.displayName || '',
            email: user.email || '',
            avatar: DEFAULT_AVATAR,
          });
        }
      } catch (error) {
        console.error('Erro ao carregar perfil:', error);
      } finally {
        setLoading(false);
      }
    };

    loadProfile();
  }, [user]);

  const handleSaveAvatar = async () => {
    if (!profile) return;

    try {
      const userProfileService = getUserProfileService();
      await userProfileService.updateUserAvatar(profile.uid, tempAvatar);
      setProfile({ ...profile, avatar: tempAvatar });
      setIsEditingAvatar(false);
    } catch (error) {
      console.error('Erro ao salvar avatar:', error);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="mb-4 inline-block">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
          <p className="text-slate-600">Carregando perfil...</p>
        </div>
      </div>
    );
  }

  if (!profile) {
    return <div className="text-center text-slate-500 py-12">Perfil não encontrado</div>;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50">
      {/* Top Bar */}
      <div className="bg-white border-b border-slate-200 h-20 flex items-center px-8 shadow-sm">
        <h1 className="text-2xl font-bold text-slate-900">Perfil acadêmico</h1>
      </div>

      {/* Content */}
      <div className="max-w-6xl mx-auto px-8 py-8">
        {/* Profile Header Card */}
        <div className="bg-white rounded-3xl border border-slate-200 p-8 mb-6 shadow-sm">
          <div className="flex items-start gap-8">
            {/* Avatar */}
            <div className="relative">
              {isEditingAvatar ? (
                <div className="w-32 h-32 rounded-2xl overflow-hidden">
                  <AvatarDisplay avatar={tempAvatar} size="lg" />
                </div>
              ) : (
                <div className="w-32 h-32 rounded-2xl overflow-hidden">
                  <AvatarDisplay avatar={profile.avatar} size="lg" fallback={profile.displayName?.charAt(0)} />
                </div>
              )}
              <button
                onClick={() => {
                  if (isEditingAvatar) {
                    setIsEditingAvatar(false);
                  } else {
                    setTempAvatar(profile.avatar);
                    setIsEditingAvatar(true);
                  }
                }}
                className="absolute bottom-2 right-2 p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
              >
                <Edit3 size={18} />
              </button>
            </div>

            {/* Profile Info */}
            <div className="flex-1">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h2 className="text-3xl font-bold text-slate-900">{profile.displayName}</h2>
                  {profile.headline && (
                    <p className="text-lg text-slate-600 mt-2">{profile.headline}</p>
                  )}
                </div>
                <button
                  onClick={() => setIsEditing(!isEditing)}
                  className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                >
                  <Edit3 size={18} />
                  {isEditing ? 'Salvar' : 'Editar'}
                </button>
              </div>

              {/* Badges */}
              <div className="flex flex-wrap gap-3 mt-4">
                {profile.course && (
                  <div className="px-4 py-2 bg-blue-100 text-blue-700 rounded-full text-sm font-semibold">
                    {profile.course}
                  </div>
                )}
                {profile.registration && (
                  <div className="px-4 py-2 bg-slate-100 text-slate-700 rounded-full text-sm font-semibold">
                    {profile.registration}
                  </div>
                )}
                {profile.languages && (
                  <div className="px-4 py-2 bg-green-100 text-green-700 rounded-full text-sm font-semibold">
                    {profile.languages}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Avatar Editor Modal */}
        {isEditingAvatar && (
          <div className="bg-white rounded-2xl border border-slate-200 p-0 mb-6 shadow-lg overflow-hidden">
            <div className="flex items-center justify-between bg-gradient-to-r from-blue-600 to-blue-700 p-6">
              <h3 className="text-2xl font-bold text-white">✨ Personalize seu Avatar</h3>
              <button
                onClick={() => setIsEditingAvatar(false)}
                className="p-2 hover:bg-blue-500 rounded-lg transition text-white"
              >
                ✕
              </button>
            </div>

            <div className="h-96 overflow-hidden">
              <AvatarSelector
                value={tempAvatar}
                onChange={setTempAvatar}
                onClose={() => {
                  handleSaveAvatar();
                }}
                showSuggestions={true}
              />
            </div>
          </div>
        )}

        {/* Professional Summary */}
        {profile.professionalSummary && !isEditingAvatar && (
          <div className="bg-white rounded-2xl border border-slate-200 p-6 mb-6 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-4">Resumo profissional</h3>
            <p className="text-slate-600 leading-relaxed">{profile.professionalSummary}</p>
          </div>
        )}

        {/* Two Column Section */}
        {!isEditingAvatar && (
          <div className="grid grid-cols-2 gap-6 mb-6">
            {/* Contact Info */}
            <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
              <h3 className="text-xl font-bold text-slate-900 mb-6">Contato</h3>
              <div className="space-y-6">
                <div>
                  <p className="text-sm text-slate-500 font-semibold mb-2 flex items-center gap-2">
                    <Mail size={16} /> Email
                  </p>
                  <p className="text-slate-900">{profile.email}</p>
                </div>
                {profile.phoneNumber && (
                  <div>
                    <p className="text-sm text-slate-500 font-semibold mb-2 flex items-center gap-2">
                      <Phone size={16} /> Telefone
                    </p>
                    <p className="text-slate-900">{profile.phoneNumber}</p>
                  </div>
                )}
              </div>
            </div>

            {/* Academic Info */}
            <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
              <h3 className="text-xl font-bold text-slate-900 mb-6">Acadêmico</h3>
              <div className="space-y-6">
                {profile.course && (
                  <div>
                    <p className="text-sm text-slate-500 font-semibold mb-2 flex items-center gap-2">
                      <GraduationCap size={16} /> Curso
                    </p>
                    <p className="text-slate-900">{profile.course}</p>
                  </div>
                )}
                {profile.registration && (
                  <div>
                    <p className="text-sm text-slate-500 font-semibold mb-2 flex items-center gap-2">
                      <Briefcase size={16} /> Matrícula
                    </p>
                    <p className="text-slate-900">{profile.registration}</p>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Additional Info for Professors */}
        {profile.department && !isEditingAvatar && (
          <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-6">Informações de Docência</h3>
            <div className="grid grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-slate-500 font-semibold mb-2">Departamento</p>
                <p className="text-slate-900">{profile.department}</p>
              </div>
              {profile.academicFocus && (
                <div>
                  <p className="text-sm text-slate-500 font-semibold mb-2">Foco Acadêmico</p>
                  <p className="text-slate-900">{profile.academicFocus}</p>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

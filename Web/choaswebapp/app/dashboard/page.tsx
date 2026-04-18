'use client';

import { useEffect, useState } from 'react';
import { getFirestore, collection, query, where, getDocs } from 'firebase/firestore';
import { useAuth } from '../../lib/useAuth';
import { auth } from '../../lib/firebase';
import AvatarDisplay from '../../components/AvatarDisplay';
import { AvatarComponents } from '../../lib/avatarService';
import {
  Users,
  Calendar,
  TrendingUp,
  Clock,
  CheckCircle2,
  AlertCircle,
  Plus,
  ArrowRight,
  BookOpen,
  Trophy,
  Briefcase,
} from 'lucide-react';

interface TeamWorkspace {
  teamId: string;
  teamName: string;
  course: string;
  className: string;
  projectProgress: number;
  projectStatus: string;
  projectDeadline?: string;
  members: Array<{ userId: string; name: string; email: string; avatar?: AvatarComponents }>;
  createdAt: string;
  updatedAt: string;
}

interface Stats {
  totalTeams: number;
  activeProjects: number;
  upcomingDeadlines: number;
  completedMilestones: number;
}

export default function DashboardPage() {
  const user = useAuth();
  const [loading, setLoading] = useState(true);
  const [teams, setTeams] = useState<TeamWorkspace[]>([]);
  const [stats, setStats] = useState<Stats>({
    totalTeams: 0,
    activeProjects: 0,
    upcomingDeadlines: 0,
    completedMilestones: 0,
  });

  useEffect(() => {
    if (!user) return;

    const loadTeams = async () => {
      try {
        const db = getFirestore();
        const userTeamsRef = collection(db, 'userTeams');
        
        // Query all teams for this user
        const q = query(userTeamsRef, where('userId', '==', user.uid));
        const snapshot = await getDocs(q);

        const loadedTeams: TeamWorkspace[] = [];
        
        for (const doc of snapshot.docs) {
          const data = doc.data();
          // Load the full team data from the teams collection
          const teamRef = collection(db, 'teams');
          const teamQuery = query(teamRef, where('teamId', '==', data.teamId));
          const teamSnapshot = await getDocs(teamQuery);
          
          if (!teamSnapshot.empty) {
            const teamData = teamSnapshot.docs[0].data();
            loadedTeams.push({
              teamId: teamData.teamId || '',
              teamName: teamData.teamName || 'Sem Nome',
              course: teamData.course || 'Não informado',
              className: teamData.className || '',
              projectProgress: teamData.projectProgress || 0,
              projectStatus: teamData.projectStatus || 'Planejamento',
              projectDeadline: teamData.projectDeadline,
              members: teamData.members || [],
              createdAt: teamData.createdAt || new Date().toISOString(),
              updatedAt: teamData.updatedAt || new Date().toISOString(),
            });
          }
        }

        setTeams(loadedTeams);

        // Calculate stats
        const upcomingCount = loadedTeams.filter((t) => {
          if (!t.projectDeadline) return false;
          const deadline = new Date(t.projectDeadline);
          const today = new Date();
          const daysUntil = Math.ceil((deadline.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
          return daysUntil > 0 && daysUntil <= 7;
        }).length;

        setStats({
          totalTeams: loadedTeams.length,
          activeProjects: loadedTeams.filter((t) => t.projectProgress < 100).length,
          upcomingDeadlines: upcomingCount,
          completedMilestones: loadedTeams.filter((t) => t.projectProgress === 100).length,
        });
      } catch (error) {
        console.error('Erro ao carregar equipes:', error);
      } finally {
        setLoading(false);
      }
    };

    loadTeams();
  }, [user]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="mb-4 inline-block">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
          <p className="text-slate-600">Carregando suas equipes...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Welcome Section */}
      <div className="bg-gradient-to-r from-blue-600 via-blue-500 to-indigo-600 rounded-2xl p-8 text-white shadow-lg">
        <h2 className="text-3xl font-bold mb-2">
          Bem-vindo, {user?.displayName || 'Colega'}! 👋
        </h2>
        <p className="text-blue-100 mb-6">
          Você está acompanhando {stats.totalTeams} {stats.totalTeams === 1 ? 'equipe' : 'equipes'} de projeto.
        </p>
        <button className="flex items-center gap-2 bg-white text-blue-600 font-semibold px-6 py-3 rounded-lg hover:bg-blue-50 transition">
          <Plus size={20} />
          Criar Nova Equipe
        </button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {[
          {
            icon: Briefcase,
            label: 'Equipes Totais',
            value: stats.totalTeams,
            color: 'from-blue-500 to-blue-600',
          },
          {
            icon: TrendingUp,
            label: 'Projetos em Andamento',
            value: stats.activeProjects,
            color: 'from-purple-500 to-purple-600',
          },
          {
            icon: Clock,
            label: 'Prazos Próximos',
            value: stats.upcomingDeadlines,
            color: 'from-orange-500 to-orange-600',
          },
          {
            icon: Trophy,
            label: 'Projetos Concluídos',
            value: stats.completedMilestones,
            color: 'from-green-500 to-green-600',
          },
        ].map((stat, index) => {
          const Icon = stat.icon;
          return (
            <div
              key={index}
              className="bg-white rounded-xl p-6 shadow-sm border border-slate-200 hover:shadow-md transition"
            >
              <div className={`w-12 h-12 rounded-lg bg-gradient-to-br ${stat.color} flex items-center justify-center mb-4`}>
                <Icon size={24} className="text-white" />
              </div>
              <p className="text-slate-600 text-sm font-medium mb-1">{stat.label}</p>
              <p className="text-3xl font-bold text-slate-900">{stat.value}</p>
            </div>
          );
        })}
      </div>

      {/* Teams Section */}
      <div>
        <div className="flex items-center justify-between mb-6">
          <h3 className="text-2xl font-bold text-slate-900">Suas Equipes</h3>
          <a
            href="/dashboard/teams"
            className="text-blue-600 hover:text-blue-700 font-semibold text-sm flex items-center gap-2"
          >
            Ver Todas
            <ArrowRight size={16} />
          </a>
        </div>

        {teams.length === 0 ? (
          <div className="bg-white rounded-xl border-2 border-dashed border-slate-300 p-12 text-center">
            <BookOpen size={48} className="mx-auto text-slate-400 mb-4" />
            <h4 className="text-lg font-semibold text-slate-900 mb-2">Nenhuma equipe ainda</h4>
            <p className="text-slate-600 mb-6">Crie sua primeira equipe para começar a colaborar.</p>
            <button className="inline-flex items-center gap-2 bg-blue-600 text-white font-semibold px-6 py-3 rounded-lg hover:bg-blue-700 transition">
              <Plus size={20} />
              Criar Equipe
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {teams.map((team) => {
              const deadline = team.projectDeadline
                ? new Date(team.projectDeadline).toLocaleDateString('pt-BR')
                : 'Sem prazo';

              return (
                <div
                  key={team.teamId}
                  className="bg-white rounded-xl border border-slate-200 overflow-hidden hover:shadow-lg transition cursor-pointer group"
                >
                  {/* Header */}
                  <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b border-slate-200 p-6">
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <h4 className="text-lg font-bold text-slate-900 group-hover:text-blue-600 transition">
                          {team.teamName}
                        </h4>
                        <p className="text-sm text-slate-600">{team.course}</p>
                      </div>
                      <div className={`px-3 py-1 rounded-full text-xs font-semibold ${
                        team.projectStatus === 'Concluído'
                          ? 'bg-green-100 text-green-700'
                          : team.projectStatus === 'Em Andamento'
                          ? 'bg-blue-100 text-blue-700'
                          : 'bg-amber-100 text-amber-700'
                      }`}>
                        {team.projectStatus}
                      </div>
                    </div>
                  </div>

                  {/* Content */}
                  <div className="p-6 space-y-4">
                    {/* Progress */}
                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-semibold text-slate-700">Progresso</span>
                        <span className="text-sm font-bold text-slate-900">{team.projectProgress}%</span>
                      </div>
                      <div className="w-full h-2 bg-slate-200 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-gradient-to-r from-blue-500 to-indigo-600 transition-all"
                          style={{ width: `${team.projectProgress}%` }}
                        />
                      </div>
                    </div>

                    {/* Members */}
                    <div className="flex items-center gap-2 text-sm text-slate-600">
                      <Users size={16} className="text-slate-400" />
                      <span>{team.members.length} {team.members.length === 1 ? 'membro' : 'membros'}</span>
                    </div>

                    {/* Deadline */}
                    <div className="flex items-center gap-2 text-sm text-slate-600">
                      <Calendar size={16} className="text-slate-400" />
                      <span>Prazo: {deadline}</span>
                    </div>

                    {/* Members Avatars Preview */}
                    {team.members.length > 0 && (
                      <div className="flex -space-x-2">
                        {team.members.slice(0, 3).map((member, idx) => (
                          <div key={idx} title={member.name}>
                            {member.avatar ? (
                              <div className="w-8 h-8 rounded-full overflow-hidden border border-white">
                                <AvatarDisplay avatar={member.avatar} size="sm" fallback={member.name.charAt(0)} />
                              </div>
                            ) : (
                              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-white text-xs font-bold border border-white">
                                {member.name.charAt(0).toUpperCase()}
                              </div>
                            )}
                          </div>
                        ))}
                        {team.members.length > 3 && (
                          <div className="w-8 h-8 rounded-full bg-slate-300 flex items-center justify-center text-slate-700 text-xs font-bold border border-white">
                            +{team.members.length - 3}
                          </div>
                        )}
                      </div>
                    )}
                  </div>

                  {/* Footer Action */}
                  <div className="border-t border-slate-200 p-4 bg-slate-50">
                    <button className="w-full flex items-center justify-center gap-2 text-blue-600 font-semibold hover:text-blue-700 transition py-2">
                      Abrir Projeto
                      <ArrowRight size={16} />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Recent Activity */}
      <div className="bg-white rounded-xl border border-slate-200 p-6">
        <h3 className="text-lg font-bold text-slate-900 mb-6">Atividades Recentes</h3>
        <div className="space-y-4">
          {teams.slice(0, 3).map((team) => (
            <div key={team.teamId} className="flex items-start gap-4 pb-4 border-b border-slate-100 last:pb-0 last:border-b-0">
              <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center flex-shrink-0">
                <Briefcase size={20} className="text-white" />
              </div>
              <div className="flex-1">
                <p className="text-sm font-semibold text-slate-900">
                  {team.teamName}
                </p>
                <p className="text-xs text-slate-600">
                  Atualizado: {new Date(team.updatedAt).toLocaleDateString('pt-BR')}
                </p>
              </div>
              <div className="text-right">
                <div className="text-sm font-semibold text-slate-900">{team.projectProgress}%</div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

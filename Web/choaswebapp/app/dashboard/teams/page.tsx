'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '../../../lib/useAuth';
import { getFirestore, collection, query, where, getDocs, addDoc } from 'firebase/firestore';
import { Plus, Search, Users, Calendar, TrendingUp } from 'lucide-react';

interface TeamWorkspace {
  teamId: string;
  teamName: string;
  course: string;
  className: string;
  projectProgress: number;
  projectStatus: string;
  projectDeadline?: string;
  members: Array<{ userId: string; name: string; email: string }>;
  createdAt: string;
}

export default function TeamsPage() {
  const user = useAuth();
  const [loading, setLoading] = useState(true);
  const [teams, setTeams] = useState<TeamWorkspace[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [isCreatingTeam, setIsCreatingTeam] = useState(false);
  const [newTeamData, setNewTeamData] = useState({
    teamName: '',
    course: '',
    className: '',
  });

  useEffect(() => {
    if (!user) return;

    const loadTeams = async () => {
      try {
        const db = getFirestore();
        const userTeamsRef = collection(db, 'userTeams');
        const q = query(userTeamsRef, where('userId', '==', user.uid));
        const snapshot = await getDocs(q);

        const loadedTeams: TeamWorkspace[] = [];
        
        for (const doc of snapshot.docs) {
          const data = doc.data();
          const teamRef = collection(db, 'teams');
          const teamQuery = query(teamRef, where('teamId', '==', data.teamId));
          const teamSnapshot = await getDocs(teamQuery);
          
          if (!teamSnapshot.empty) {
            const teamData = teamSnapshot.docs[0].data();
            loadedTeams.push({
              teamId: teamData.teamId || '',
              teamName: teamData.teamName || 'Sem Nome',
              course: teamData.course || '',
              className: teamData.className || '',
              projectProgress: teamData.projectProgress || 0,
              projectStatus: teamData.projectStatus || 'Planejamento',
              projectDeadline: teamData.projectDeadline,
              members: teamData.members || [],
              createdAt: teamData.createdAt || new Date().toISOString(),
            });
          }
        }

        setTeams(loadedTeams);
      } catch (error) {
        console.error('Erro ao carregar equipes:', error);
      } finally {
        setLoading(false);
      }
    };

    loadTeams();
  }, [user]);

  const handleCreateTeam = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !newTeamData.teamName.trim()) return;

    try {
      const db = getFirestore();
      const teamId = `${user.uid}_${Date.now()}`;
      
      // Criar documento da equipe
      await addDoc(collection(db, 'teams'), {
        teamId,
        teamName: newTeamData.teamName,
        course: newTeamData.course,
        className: newTeamData.className,
        projectProgress: 0,
        projectStatus: 'Planejamento',
        members: [{ userId: user.uid, name: user.displayName || 'Usuário', email: user.email || '' }],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        createdBy: user.uid,
      });

      // Criar referência do usuário
      await addDoc(collection(db, 'userTeams'), {
        userId: user.uid,
        teamId,
        teamName: newTeamData.teamName,
        addedAt: new Date().toISOString(),
      });

      // Reset form
      setNewTeamData({ teamName: '', course: '', className: '' });
      setIsCreatingTeam(false);

      // Reload teams
      const userTeamsRef = collection(db, 'userTeams');
      const q = query(userTeamsRef, where('userId', '==', user.uid));
      const snapshot = await getDocs(q);

      const loadedTeams: TeamWorkspace[] = [];
      for (const doc of snapshot.docs) {
        const data = doc.data();
        const teamRef = collection(db, 'teams');
        const teamQuery = query(teamRef, where('teamId', '==', data.teamId));
        const teamSnapshot = await getDocs(teamQuery);
        
        if (!teamSnapshot.empty) {
          const teamData = teamSnapshot.docs[0].data();
          loadedTeams.push({
            teamId: teamData.teamId || '',
            teamName: teamData.teamName || '',
            course: teamData.course || '',
            className: teamData.className || '',
            projectProgress: teamData.projectProgress || 0,
            projectStatus: teamData.projectStatus || 'Planejamento',
            projectDeadline: teamData.projectDeadline,
            members: teamData.members || [],
            createdAt: teamData.createdAt || new Date().toISOString(),
          });
        }
      }
      setTeams(loadedTeams);
    } catch (error) {
      console.error('Erro ao criar equipe:', error);
    }
  };

  const filteredTeams = teams.filter((team) => {
    const matchesSearch =
      team.teamName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      team.course.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = filterStatus === 'all' || team.projectStatus === filterStatus;
    return matchesSearch && matchesStatus;
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="mb-4 inline-block">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
          <p className="text-slate-600">Carregando equipes...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold text-slate-900">Gerenciar Equipes</h2>
          <p className="text-slate-600 mt-1">Visualize, crie e gerencie suas equipes de projeto</p>
        </div>
        <button
          onClick={() => setIsCreatingTeam(true)}
          className="flex items-center gap-2 bg-blue-600 text-white font-semibold px-6 py-3 rounded-lg hover:bg-blue-700 transition"
        >
          <Plus size={20} />
          Nova Equipe
        </button>
      </div>

      {/* Create Team Modal */}
      {isCreatingTeam && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-2xl p-8 max-w-md w-full mx-4">
            <h3 className="text-2xl font-bold text-slate-900 mb-6">Criar Nova Equipe</h3>
            <form onSubmit={handleCreateTeam} className="space-y-4">
              <div>
                <label className="block text-sm font-semibold text-slate-700 mb-2">
                  Nome da Equipe
                </label>
                <input
                  type="text"
                  value={newTeamData.teamName}
                  onChange={(e) =>
                    setNewTeamData({ ...newTeamData, teamName: e.target.value })
                  }
                  placeholder="Ex: Projeto XYZ"
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-slate-700 mb-2">
                  Curso
                </label>
                <input
                  type="text"
                  value={newTeamData.course}
                  onChange={(e) =>
                    setNewTeamData({ ...newTeamData, course: e.target.value })
                  }
                  placeholder="Ex: Análise e Desenvolvimento de Sistemas"
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-slate-700 mb-2">
                  Turma
                </label>
                <input
                  type="text"
                  value={newTeamData.className}
                  onChange={(e) =>
                    setNewTeamData({ ...newTeamData, className: e.target.value })
                  }
                  placeholder="Ex: Turma A - Manhã"
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
                />
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setIsCreatingTeam(false)}
                  className="flex-1 px-4 py-2 border border-slate-300 text-slate-700 font-semibold rounded-lg hover:bg-slate-100 transition"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="flex-1 px-4 py-2 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition"
                >
                  Criar Equipe
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Search and Filter */}
      <div className="space-y-4">
        <div className="relative">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
          <input
            type="text"
            placeholder="Buscar por nome ou curso..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-12 pr-4 py-3 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600"
          />
        </div>

        <div className="flex gap-2">
          {['all', 'Planejamento', 'Em Andamento', 'Concluído'].map((status) => (
            <button
              key={status}
              onClick={() => setFilterStatus(status)}
              className={`px-4 py-2 rounded-lg font-semibold transition ${
                filterStatus === status
                  ? 'bg-blue-600 text-white'
                  : 'bg-slate-200 text-slate-700 hover:bg-slate-300'
              }`}
            >
              {status === 'all' ? 'Todas' : status}
            </button>
          ))}
        </div>
      </div>

      {/* Teams Grid */}
      {filteredTeams.length === 0 ? (
        <div className="bg-white rounded-xl border-2 border-dashed border-slate-300 p-12 text-center">
          <Users size={48} className="mx-auto text-slate-400 mb-4" />
          <h4 className="text-lg font-semibold text-slate-900 mb-2">Nenhuma equipe encontrada</h4>
          <p className="text-slate-600">
            {searchQuery ? 'Tente refinar sua busca' : 'Crie sua primeira equipe para começar'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {filteredTeams.map((team) => (
            <div
              key={team.teamId}
              className="bg-white rounded-xl border border-slate-200 overflow-hidden hover:shadow-lg transition cursor-pointer group"
            >
              <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b border-slate-200 p-6">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <h4 className="text-lg font-bold text-slate-900 group-hover:text-blue-600 transition">
                      {team.teamName}
                    </h4>
                    <p className="text-sm text-slate-600 mt-1">{team.course}</p>
                    {team.className && (
                      <p className="text-xs text-slate-500 mt-1">{team.className}</p>
                    )}
                  </div>
                  <div
                    className={`px-3 py-1 rounded-full text-xs font-semibold whitespace-nowrap ml-4 ${
                      team.projectStatus === 'Concluído'
                        ? 'bg-green-100 text-green-700'
                        : team.projectStatus === 'Em Andamento'
                        ? 'bg-blue-100 text-blue-700'
                        : 'bg-amber-100 text-amber-700'
                    }`}
                  >
                    {team.projectStatus}
                  </div>
                </div>
              </div>

              <div className="p-6 space-y-4">
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-semibold text-slate-700">Progresso</span>
                    <span className="text-sm font-bold text-slate-900">{team.projectProgress}%</span>
                  </div>
                  <div className="w-full h-2 bg-slate-200 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-gradient-to-r from-blue-500 to-indigo-600"
                      style={{ width: `${team.projectProgress}%` }}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div className="flex items-center gap-2 text-slate-600">
                    <Users size={16} className="text-slate-400" />
                    <span>{team.members.length} membros</span>
                  </div>
                  {team.projectDeadline && (
                    <div className="flex items-center gap-2 text-slate-600">
                      <Calendar size={16} className="text-slate-400" />
                      <span>{new Date(team.projectDeadline).toLocaleDateString('pt-BR')}</span>
                    </div>
                  )}
                </div>

                {team.members.length > 0 && (
                  <div className="flex -space-x-2 pt-2">
                    {team.members.slice(0, 3).map((member, idx) => (
                      <div
                        key={idx}
                        className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-white text-xs font-bold border border-white"
                        title={member.name}
                      >
                        {member.name.charAt(0).toUpperCase()}
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

              <div className="border-t border-slate-200 p-4 bg-slate-50">
                <button className="w-full text-blue-600 font-semibold hover:text-blue-700 transition py-2">
                  Visualizar Detalhes
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

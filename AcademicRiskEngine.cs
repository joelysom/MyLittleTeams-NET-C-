using System;
using System.Collections.Generic;
using System.Linq;

namespace MeuApp
{
    public sealed class TeamRiskSnapshot
    {
        public string TeamId { get; init; } = string.Empty;
        public string TeamName { get; init; } = string.Empty;
        public string ClassName { get; init; } = string.Empty;
        public string Course { get; init; } = string.Empty;
        public string Level { get; init; } = "Baixo";
        public int Score { get; init; }
        public int OverdueItems { get; init; }
        public int UpcomingSevenDays { get; init; }
        public int UnassignedTasks { get; init; }
        public int PendingMilestones { get; init; }
        public string Summary { get; init; } = string.Empty;
        public string Recommendation { get; init; } = string.Empty;
    }

    public sealed class MemberWorkloadSnapshot
    {
        public string UserId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public int AssignedTasks { get; init; }
        public int OverdueTasks { get; init; }
        public int UpcomingTasks { get; init; }
        public int EstimatedHours { get; init; }
        public int WorkloadPoints { get; init; }
        public string Level { get; init; } = "Leve";
        public string Summary { get; init; } = string.Empty;
    }

    public sealed class ProfessorDashboardSnapshot
    {
        public List<TeamRiskSnapshot> Teams { get; init; } = new List<TeamRiskSnapshot>();
        public int HighRiskTeams { get; init; }
        public int ModerateRiskTeams { get; init; }
        public int OverdueItems { get; init; }
        public int UpcomingItems { get; init; }
    }

    public static class AcademicRiskEngine
    {
        public static TeamRiskSnapshot EvaluateTeam(TeamWorkspaceInfo team)
        {
            var overdueTasks = team.TaskColumns
                .Where(column => !column.Title.Contains("Conclu", StringComparison.OrdinalIgnoreCase))
                .SelectMany(column => column.Cards)
                .Count(card => card.DueDate.HasValue && card.DueDate.Value.Date < DateTime.Today);

            var unassignedTasks = team.TaskColumns
                .Where(column => !column.Title.Contains("Conclu", StringComparison.OrdinalIgnoreCase))
                .SelectMany(column => column.Cards)
                .Count(card => card.AssignedUserIds.Count == 0);

            var upcomingItems = team.TaskColumns
                .SelectMany(column => column.Cards)
                .Count(card => card.DueDate.HasValue && card.DueDate.Value.Date >= DateTime.Today && card.DueDate.Value.Date <= DateTime.Today.AddDays(7));

            var pendingMilestones = team.Milestones.Count(milestone => !string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase));
            var score = (overdueTasks * 4) + (unassignedTasks * 2) + Math.Min(4, upcomingItems) + (pendingMilestones == 0 ? 2 : 0);

            var level = score >= 10
                ? "Alto"
                : score >= 5
                    ? "Moderado"
                    : "Baixo";

            var summary = level switch
            {
                "Alto" => $"{team.TeamName} concentra atrasos, tarefas sem dono ou checkpoints insuficientes para a janela atual.",
                "Moderado" => $"{team.TeamName} entra na semana com carga relevante e precisa de leitura próxima do board.",
                _ => $"{team.TeamName} está estável, com espaço para revisão de qualidade e preparação da próxima entrega."
            };

            var recommendation = level switch
            {
                "Alto" => "Priorize revisão com o professor, redistribua responsáveis e reancore a equipe em um checkpoint de curto prazo.",
                "Moderado" => "Monitore a próxima janela de sete dias, confirme responsáveis e revise dependências do board.",
                _ => "Use o fôlego para fechar documentação, qualidade e narrativa da entrega."
            };

            return new TeamRiskSnapshot
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                ClassName = team.ClassName,
                Course = team.Course,
                Level = level,
                Score = score,
                OverdueItems = overdueTasks,
                UpcomingSevenDays = upcomingItems,
                UnassignedTasks = unassignedTasks,
                PendingMilestones = pendingMilestones,
                Summary = summary,
                Recommendation = recommendation
            };
        }

        public static List<MemberWorkloadSnapshot> BuildMemberWorkload(TeamWorkspaceInfo team)
        {
            return team.Members
                .Where(member => TeamPermissionService.CanBeTaskAssignee(member.Role))
                .Select(member =>
                {
                    var assignedCards = team.TaskColumns
                        .SelectMany(column => column.Cards)
                        .Where(card => card.AssignedUserIds.Contains(member.UserId))
                        .ToList();

                    var overdue = assignedCards.Count(card => card.DueDate.HasValue && card.DueDate.Value.Date < DateTime.Today);
                    var upcoming = assignedCards.Count(card => card.DueDate.HasValue && card.DueDate.Value.Date >= DateTime.Today && card.DueDate.Value.Date <= DateTime.Today.AddDays(7));
                    var estimatedHours = assignedCards.Sum(card => Math.Max(1, card.EstimatedHours));
                    var workloadPoints = assignedCards.Sum(card => Math.Max(1, card.WorkloadPoints == 0 ? 2 : card.WorkloadPoints));
                    var level = workloadPoints >= 16
                        ? "Alta"
                        : workloadPoints >= 8
                            ? "Media"
                            : "Leve";

                    return new MemberWorkloadSnapshot
                    {
                        UserId = member.UserId,
                        Name = member.Name,
                        Role = TeamPermissionService.GetRoleLabel(member.Role),
                        AssignedTasks = assignedCards.Count,
                        OverdueTasks = overdue,
                        UpcomingTasks = upcoming,
                        EstimatedHours = estimatedHours,
                        WorkloadPoints = workloadPoints,
                        Level = level,
                        Summary = assignedCards.Count == 0
                            ? "Sem tarefas atribuídas no momento."
                            : $"{assignedCards.Count} tarefa(s), {upcoming} na janela de 7 dias e {overdue} em atraso."
                    };
                })
                .OrderByDescending(item => item.WorkloadPoints)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public static ProfessorDashboardSnapshot BuildProfessorDashboard(IEnumerable<TeamWorkspaceInfo> teams)
        {
            var snapshots = (teams ?? Enumerable.Empty<TeamWorkspaceInfo>())
                .Select(EvaluateTeam)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.TeamName)
                .ToList();

            return new ProfessorDashboardSnapshot
            {
                Teams = snapshots,
                HighRiskTeams = snapshots.Count(item => string.Equals(item.Level, "Alto", StringComparison.OrdinalIgnoreCase)),
                ModerateRiskTeams = snapshots.Count(item => string.Equals(item.Level, "Moderado", StringComparison.OrdinalIgnoreCase)),
                OverdueItems = snapshots.Sum(item => item.OverdueItems),
                UpcomingItems = snapshots.Sum(item => item.UpcomingSevenDays)
            };
        }

        public static string BuildAcademicAssistantBrief(TeamWorkspaceInfo team)
        {
            var risk = EvaluateTeam(team);
            var workload = BuildMemberWorkload(team).Take(2).ToList();
            var workloadText = workload.Count == 0
                ? "A equipe ainda não distribuiu tarefas suficientes para medir carga com confiança."
                : string.Join(" ", workload.Select(item => $"{item.Name} está com carga {item.Level.ToLowerInvariant()} ({item.AssignedTasks} tarefa(s))."));

            return $"{risk.Summary} {risk.Recommendation} {workloadText}".Trim();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace MeuApp
{
    /// <summary>
    /// Informações de uma equipe de projeto
    /// </summary>
    public class TeamWorkspaceInfo
    {
        public string TeamId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProjectProgress { get; set; }
        public DateTime? ProjectDeadline { get; set; }
        public string ProjectStatus { get; set; } = "Planejamento";
        public List<UserInfo> Members { get; set; } = new List<UserInfo>();
        public List<string> Ucs { get; set; } = new List<string>();
        public List<TeamMilestoneInfo> Milestones { get; set; } = new List<TeamMilestoneInfo>();
        public List<TeamAssetInfo> Assets { get; set; } = new List<TeamAssetInfo>();
        public List<TeamTaskColumnInfo> TaskColumns { get; set; } = new List<TeamTaskColumnInfo>();
        public List<TeamNotificationInfo> Notifications { get; set; } = new List<TeamNotificationInfo>();
        public List<TeamChatMessageInfo> ChatMessages { get; set; } = new List<TeamChatMessageInfo>();
        public TeamCsdBoardInfo CsdBoard { get; set; } = new TeamCsdBoardInfo();
    }

    /// <summary>
    /// Marco academico ou entrega planejada da equipe
    /// </summary>
    public class TeamMilestoneInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Planejada";
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Informações de um ativo (arquivo) de uma equipe
    /// </summary>
    public class TeamAssetInfo
    {
        public string Category { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }

    /// <summary>
    /// Coluna de tarefas no quadro Trello
    /// </summary>
    public class TeamTaskColumnInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public Color AccentColor { get; set; }
        public List<TeamTaskCardInfo> Cards { get; set; } = new List<TeamTaskCardInfo>();
    }

    /// <summary>
    /// Cartão de tarefa no quadro Trello
    /// </summary>
    public class TeamTaskCardInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Media";
        public DateTime? DueDate { get; set; }
        public List<string> AssignedUserIds { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Notificação da equipe
    /// </summary>
    public class TeamNotificationInfo
    {
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Mensagem do chat em grupo da equipe
    /// </summary>
    public class TeamChatMessageInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Quadro CSD (Certainties, Strengths, Doubts)
    /// </summary>
    public class TeamCsdBoardInfo
    {
        public List<string> Certainties { get; set; } = new List<string>();
        public List<string> Assumptions { get; set; } = new List<string>();
        public List<string> Doubts { get; set; } = new List<string>();
    }
}

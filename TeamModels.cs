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
        public string AcademicTerm { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastRealtimeSyncAt { get; set; }
        public int ProjectProgress { get; set; }
        public DateTime? ProjectDeadline { get; set; }
        public string ProjectStatus { get; set; } = "Planejamento";
        public string TeacherNotes { get; set; } = string.Empty;
        public string FocalProfessorUserId { get; set; } = string.Empty;
        public string FocalProfessorName { get; set; } = string.Empty;
        public List<string> ProfessorSupervisorUserIds { get; set; } = new List<string>();
        public List<string> ProfessorSupervisorNames { get; set; } = new List<string>();
        public string DefaultFilePermissionScope { get; set; } = "team";
        public List<UserInfo> Members { get; set; } = new List<UserInfo>();
        public List<string> Ucs { get; set; } = new List<string>();
        public List<TeamTimelineItemInfo> SemesterTimeline { get; set; } = new List<TeamTimelineItemInfo>();
        public List<TeamAccessRuleInfo> AccessRules { get; set; } = new List<TeamAccessRuleInfo>();
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
        public string CreatedByUserId { get; set; } = string.Empty;
        public string OwnerUserId { get; set; } = string.Empty;
        public bool RequiresProfessorReview { get; set; }
        public List<string> MentionedUserIds { get; set; } = new List<string>();
        public List<TeamCommentInfo> Comments { get; set; } = new List<TeamCommentInfo>();
        public List<TeamAttachmentInfo> Attachments { get; set; } = new List<TeamAttachmentInfo>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedByUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Informações de um ativo (arquivo) de uma equipe
    /// </summary>
    public class TeamAssetInfo
    {
        public string AssetId { get; set; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string PreviewImageDataUri { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string PermissionScope { get; set; } = "team";
        public string StorageKind { get; set; } = "firestore-preview";
        public string StorageReference { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public int Version { get; set; } = 1;
        public string AddedByUserId { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public List<TeamAssetVersionInfo> VersionHistory { get; set; } = new List<TeamAssetVersionInfo>();
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
        public string ColumnId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Media";
        public DateTime? DueDate { get; set; }
        public int EstimatedHours { get; set; }
        public int WorkloadPoints { get; set; }
        public string RequiredRole { get; set; } = "student";
        public bool RequiresProfessorReview { get; set; }
        public List<string> AssignedUserIds { get; set; } = new List<string>();
        public List<string> MentionedUserIds { get; set; } = new List<string>();
        public List<TeamCommentInfo> Comments { get; set; } = new List<TeamCommentInfo>();
        public List<TeamAttachmentInfo> Attachments { get; set; } = new List<TeamAttachmentInfo>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string UpdatedByUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Notificação da equipe
    /// </summary>
    public class TeamNotificationInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public string Audience { get; set; } = "team";
        public string RelatedEntityId { get; set; } = string.Empty;
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

    public class TeamTimelineItemInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = "Planejado";
        public string OwnerUserId { get; set; } = string.Empty;
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
    }

    public class TeamAccessRuleInfo
    {
        public string Role { get; set; } = "student";
        public bool CanAddMembers { get; set; }
        public bool CanManageMembers { get; set; }
        public bool CanAssignLeadership { get; set; }
        public bool CanEditProjectSettings { get; set; }
        public bool CanDeleteTeam { get; set; }
        public bool CanComment { get; set; } = true;
        public bool CanUploadFiles { get; set; } = true;
        public bool CanExportAgenda { get; set; }
        public bool CanViewProfessorDashboard { get; set; }
        public bool CanReviewDeliverables { get; set; }
    }

    public class TeamCommentInfo
    {
        public string CommentId { get; set; } = Guid.NewGuid().ToString();
        public string AuthorUserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> MentionedUserIds { get; set; } = new List<string>();
        public List<string> AttachmentFileNames { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class TeamAttachmentInfo
    {
        public string AttachmentId { get; set; } = Guid.NewGuid().ToString();
        public string AssetId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string PreviewImageDataUri { get; set; } = string.Empty;
        public string PermissionScope { get; set; } = "team";
        public int Version { get; set; } = 1;
        public string AddedByUserId { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }

    public class TeamAssetVersionInfo
    {
        public int VersionNumber { get; set; } = 1;
        public string ChangedByUserId { get; set; } = string.Empty;
        public string ChangeSummary { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string PermissionScope { get; set; } = "team";
        public string StorageKind { get; set; } = "firestore-document";
        public string StorageReference { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace MeuApp
{
    public sealed class AcademicMilestoneBlueprint
    {
        public string Title { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
        public int OffsetDays { get; init; }
        public bool RequiresProfessorReview { get; init; }
    }

    public sealed class AcademicTimelineBlueprint
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int StartOffsetDays { get; init; }
        public int EndOffsetDays { get; init; }
    }

    public sealed class AcademicProjectTemplateInfo
    {
        public string TemplateId { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Course { get; init; } = string.Empty;
        public string Discipline { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string ProfessorGuidance { get; init; } = string.Empty;
        public string RecommendedProjectStatus { get; init; } = "Planejamento";
        public IReadOnlyList<string> SuggestedUcs { get; init; } = Array.Empty<string>();
        public IReadOnlyList<AcademicMilestoneBlueprint> Milestones { get; init; } = Array.Empty<AcademicMilestoneBlueprint>();
        public IReadOnlyList<AcademicTimelineBlueprint> Timeline { get; init; } = Array.Empty<AcademicTimelineBlueprint>();
    }

    public static class AcademicProjectTemplateCatalog
    {
        public static IReadOnlyList<AcademicProjectTemplateInfo> GetAll()
        {
            return new List<AcademicProjectTemplateInfo>
            {
                new AcademicProjectTemplateInfo
                {
                    TemplateId = "software-integrador",
                    Title = "Produto de software integrador",
                    Course = "Analise e Desenvolvimento de Sistemas",
                    Discipline = "Projeto Integrador",
                    Description = "Fluxo orientado a descoberta, prototipação, sprint técnico, validação e banca.",
                    ProfessorGuidance = "Use checkpoints quinzenais e exija revisão funcional antes da banca.",
                    RecommendedProjectStatus = "Planejamento",
                    SuggestedUcs = new[] { "Projeto Integrador", "Arquitetura de Software", "Qualidade de Software", "Experiencia do Usuario" },
                    Milestones = new[]
                    {
                        new AcademicMilestoneBlueprint { Title = "Definição do problema", Notes = "Delimitar contexto, dor e recorte do projeto.", OffsetDays = 7, RequiresProfessorReview = true },
                        new AcademicMilestoneBlueprint { Title = "Protótipo navegável", Notes = "Validar fluxo e linguagem com a turma ou público alvo.", OffsetDays = 21, RequiresProfessorReview = true },
                        new AcademicMilestoneBlueprint { Title = "Sprint técnico validado", Notes = "Entregar a primeira versão operacional com backlog priorizado.", OffsetDays = 40, RequiresProfessorReview = false },
                        new AcademicMilestoneBlueprint { Title = "Pré-banca", Notes = "Fechar narrativa, resultados e evidências antes da apresentação final.", OffsetDays = 70, RequiresProfessorReview = true }
                    },
                    Timeline = new[]
                    {
                        new AcademicTimelineBlueprint { Title = "Imersão", Description = "Mapeamento do problema e coleta de insumos.", Category = "descoberta", StartOffsetDays = 0, EndOffsetDays = 10 },
                        new AcademicTimelineBlueprint { Title = "Definição", Description = "Escopo, arquitetura e metas do semestre.", Category = "planejamento", StartOffsetDays = 11, EndOffsetDays = 24 },
                        new AcademicTimelineBlueprint { Title = "Construção", Description = "Execução do backlog e ciclos curtos de revisão.", Category = "execucao", StartOffsetDays = 25, EndOffsetDays = 58 },
                        new AcademicTimelineBlueprint { Title = "Banca", Description = "Fechamento, apresentação e refinamento final.", Category = "fechamento", StartOffsetDays = 59, EndOffsetDays = 84 }
                    }
                },
                new AcademicProjectTemplateInfo
                {
                    TemplateId = "pesquisa-aplicada",
                    Title = "Pesquisa aplicada orientada",
                    Course = "Ciencia da Computacao",
                    Discipline = "Metodologia Cientifica",
                    Description = "Modelo mais forte em leitura crítica, experimento, relatório e orientação acadêmica.",
                    ProfessorGuidance = "Reforce leituras, hipótese e checkpoints documentais a cada entrega.",
                    RecommendedProjectStatus = "Planejamento",
                    SuggestedUcs = new[] { "Projeto Integrador", "Banco de Dados", "Gestao de Projetos" },
                    Milestones = new[]
                    {
                        new AcademicMilestoneBlueprint { Title = "Problema e hipótese", Notes = "Definir pergunta de pesquisa e objetivo geral.", OffsetDays = 10, RequiresProfessorReview = true },
                        new AcademicMilestoneBlueprint { Title = "Referencial consolidado", Notes = "Mapa bibliográfico e recorte do estado da arte.", OffsetDays = 28, RequiresProfessorReview = true },
                        new AcademicMilestoneBlueprint { Title = "Experimento executado", Notes = "Resultados iniciais com instrumentos e coleta fechados.", OffsetDays = 52, RequiresProfessorReview = false },
                        new AcademicMilestoneBlueprint { Title = "Artigo ou relatório final", Notes = "Versão final pronta para banca ou submissão.", OffsetDays = 78, RequiresProfessorReview = true }
                    },
                    Timeline = new[]
                    {
                        new AcademicTimelineBlueprint { Title = "Pergunta", Description = "Construção do problema e hipótese.", Category = "descoberta", StartOffsetDays = 0, EndOffsetDays = 12 },
                        new AcademicTimelineBlueprint { Title = "Referencial", Description = "Leituras, revisão e estrutura do método.", Category = "planejamento", StartOffsetDays = 13, EndOffsetDays = 30 },
                        new AcademicTimelineBlueprint { Title = "Execução", Description = "Experimento, coleta e análise dos dados.", Category = "execucao", StartOffsetDays = 31, EndOffsetDays = 62 },
                        new AcademicTimelineBlueprint { Title = "Síntese", Description = "Relatório, defesa e ajustes finais.", Category = "fechamento", StartOffsetDays = 63, EndOffsetDays = 84 }
                    }
                },
                new AcademicProjectTemplateInfo
                {
                    TemplateId = "dados-e-indicadores",
                    Title = "Painel de dados e indicadores",
                    Course = "Sistemas de Informacao",
                    Discipline = "Analise de Dados",
                    Description = "Focado em coleta, modelagem, dashboard e validação com stakeholders.",
                    ProfessorGuidance = "Exija evidências de qualidade dos dados e narrativa do dashboard em cada checkpoint.",
                    RecommendedProjectStatus = "Planejamento",
                    SuggestedUcs = new[] { "Banco de Dados", "Programacao Web", "Gestao de Projetos" },
                    Milestones = new[]
                    {
                        new AcademicMilestoneBlueprint { Title = "Mapa de fontes", Notes = "Identificar dados, responsáveis e critérios de qualidade.", OffsetDays = 8, RequiresProfessorReview = true },
                        new AcademicMilestoneBlueprint { Title = "Modelo analítico", Notes = "Estruturar métricas, dimensões e pipeline inicial.", OffsetDays = 24, RequiresProfessorReview = true },
                        new AcademicMilestoneBlueprint { Title = "Dashboard funcional", Notes = "Entregar versão navegável com indicadores principais.", OffsetDays = 46, RequiresProfessorReview = false },
                        new AcademicMilestoneBlueprint { Title = "Validação executiva", Notes = "Apresentar leitura de negócio e plano de continuidade.", OffsetDays = 74, RequiresProfessorReview = true }
                    },
                    Timeline = new[]
                    {
                        new AcademicTimelineBlueprint { Title = "Fontes", Description = "Descoberta de dados e requisitos.", Category = "descoberta", StartOffsetDays = 0, EndOffsetDays = 9 },
                        new AcademicTimelineBlueprint { Title = "Modelagem", Description = "Tratamento, estrutura e validação inicial.", Category = "planejamento", StartOffsetDays = 10, EndOffsetDays = 27 },
                        new AcademicTimelineBlueprint { Title = "Dashboard", Description = "Construção visual e regras de negócio.", Category = "execucao", StartOffsetDays = 28, EndOffsetDays = 60 },
                        new AcademicTimelineBlueprint { Title = "Validação", Description = "Revisão final com stakeholders e banca.", Category = "fechamento", StartOffsetDays = 61, EndOffsetDays = 84 }
                    }
                }
            };
        }

        public static AcademicProjectTemplateInfo? GetById(string? templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return null;
            }

            return GetAll().FirstOrDefault(item => string.Equals(item.TemplateId, templateId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static AcademicProjectTemplateInfo? FindBestMatch(string? course, IEnumerable<string>? ucs)
        {
            var normalizedCourse = (course ?? string.Empty).Trim();
            var ucSet = new HashSet<string>((ucs ?? Enumerable.Empty<string>()).Where(item => !string.IsNullOrWhiteSpace(item)), StringComparer.OrdinalIgnoreCase);

            return GetAll()
                .OrderByDescending(template => string.Equals(template.Course, normalizedCourse, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(template => template.SuggestedUcs.Count(uc => ucSet.Contains(uc)))
                .FirstOrDefault();
        }

        public static void ApplyToTeam(TeamWorkspaceInfo team, AcademicProjectTemplateInfo template, string actorUserId)
        {
            if (team == null || template == null)
            {
                return;
            }

            var startDate = DateTime.Today;
            team.TemplateId = template.TemplateId;
            team.TemplateName = template.Title;
            team.ProjectStatus = string.IsNullOrWhiteSpace(team.ProjectStatus) ? template.RecommendedProjectStatus : team.ProjectStatus;
            team.TeacherNotes = string.IsNullOrWhiteSpace(team.TeacherNotes) ? template.ProfessorGuidance : team.TeacherNotes;
            team.AccessRules = team.AccessRules.Count == 0 ? TeamPermissionService.CreateDefaultAccessRules() : team.AccessRules;

            team.Ucs = team.Ucs
                .Concat(template.SuggestedUcs)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item)
                .ToList();

            if (team.Milestones.Count == 0)
            {
                team.Milestones = template.Milestones
                    .Select(item => new TeamMilestoneInfo
                    {
                        Title = item.Title,
                        Notes = item.Notes,
                        Status = "Planejada",
                        DueDate = startDate.AddDays(item.OffsetDays),
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = actorUserId,
                        RequiresProfessorReview = item.RequiresProfessorReview
                    })
                    .ToList();
            }

            if (team.SemesterTimeline.Count == 0)
            {
                team.SemesterTimeline = template.Timeline
                    .Select(item => new TeamTimelineItemInfo
                    {
                        Title = item.Title,
                        Description = item.Description,
                        Category = item.Category,
                        Status = "Planejado",
                        StartsAt = startDate.AddDays(item.StartOffsetDays),
                        EndsAt = startDate.AddDays(item.EndOffsetDays)
                    })
                    .ToList();
            }

            if (!team.ProjectDeadline.HasValue)
            {
                var lastTimeline = team.SemesterTimeline
                    .Where(item => item.EndsAt.HasValue)
                    .OrderByDescending(item => item.EndsAt)
                    .FirstOrDefault();
                team.ProjectDeadline = lastTimeline?.EndsAt;
            }
        }
    }
}
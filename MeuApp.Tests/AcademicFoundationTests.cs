using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MeuApp.Tests;

[TestClass]
public class TeamPermissionServiceTests
{
    [TestMethod]
    public void NormalizeRole_MapsProfessorAliases()
    {
        Assert.AreEqual("professor", TeamPermissionService.NormalizeRole("orientador"));
        Assert.AreEqual("professor", TeamPermissionService.NormalizeRole("teacher"));
        Assert.AreEqual("leader", TeamPermissionService.NormalizeRole("owner"));
        Assert.AreEqual("student", TeamPermissionService.NormalizeRole("membro"));
    }

    [TestMethod]
    public void DefaultAccessRules_GrantProfessorDashboardAndExport()
    {
        var team = new TeamWorkspaceInfo
        {
            AccessRules = TeamPermissionService.CreateDefaultAccessRules()
        };

        Assert.IsTrue(TeamPermissionService.CanManageMembers(team, "professor"));
        Assert.IsTrue(TeamPermissionService.CanAssignLeadership(team, "professor"));
        Assert.IsTrue(TeamPermissionService.CanExportAgenda(team, "professor"));
        Assert.IsTrue(TeamPermissionService.ResolveAccessRule(team, "professor").CanViewProfessorDashboard);
        Assert.IsFalse(TeamPermissionService.CanDeleteTeam(team, "professor", "other-user"));
        Assert.IsTrue(TeamPermissionService.CanUseProfessorDashboard(new UserProfile { Role = "coordinator" }));
    }

    [TestMethod]
    public void DefaultAccessRules_GrantCoordinatorManagementWithoutDelete()
    {
        var team = new TeamWorkspaceInfo
        {
            AccessRules = TeamPermissionService.CreateDefaultAccessRules()
        };

        var coordinatorRule = TeamPermissionService.ResolveAccessRule(team, "coordinator");

        Assert.IsTrue(coordinatorRule.CanAddMembers);
        Assert.IsTrue(coordinatorRule.CanManageMembers);
        Assert.IsTrue(coordinatorRule.CanAssignLeadership);
        Assert.IsTrue(coordinatorRule.CanEditProjectSettings);
        Assert.IsTrue(coordinatorRule.CanUploadFiles);
        Assert.IsTrue(coordinatorRule.CanReviewDeliverables);
        Assert.IsFalse(coordinatorRule.CanDeleteTeam);
    }

    [TestMethod]
    public void DefaultAccessRules_KeepLeadershipDiscenteOperationalButNotGovernanceOnly()
    {
        var team = new TeamWorkspaceInfo
        {
            AccessRules = TeamPermissionService.CreateDefaultAccessRules()
        };

        var leaderRule = TeamPermissionService.ResolveAccessRule(team, "leader");

        Assert.IsTrue(leaderRule.CanAddMembers);
        Assert.IsFalse(leaderRule.CanManageMembers);
        Assert.IsFalse(leaderRule.CanAssignLeadership);
        Assert.IsTrue(leaderRule.CanEditProjectSettings);
        Assert.IsFalse(leaderRule.CanDeleteTeam);
    }

    [TestMethod]
    public void TeamCreator_CanAddMembersWithoutFacultyGovernance()
    {
        var team = new TeamWorkspaceInfo
        {
            CreatedBy = "student-creator",
            AccessRules = TeamPermissionService.CreateDefaultAccessRules()
        };

        Assert.IsTrue(TeamPermissionService.CanAddMembers(team, "student", "student-creator"));
        Assert.IsFalse(TeamPermissionService.CanManageMembers(team, "student"));
    }

    [TestMethod]
    public void CanViewAsset_RespectsLeadershipAndPrivateScopes()
    {
        var team = new TeamWorkspaceInfo
        {
            CreatedBy = "owner-01",
            AccessRules = TeamPermissionService.CreateDefaultAccessRules()
        };

        Assert.IsFalse(TeamPermissionService.CanViewAsset(team, "student", "leadership", "student-01", "student-01"));
        Assert.IsTrue(TeamPermissionService.CanViewAsset(team, "leader", "leadership", "leader-01", "student-01"));
        Assert.IsTrue(TeamPermissionService.CanViewAsset(team, "student", "private", "student-01", "student-01"));
        Assert.IsFalse(TeamPermissionService.CanViewAsset(team, "student", "private", "student-02", "student-01"));
    }
}

[TestClass]
public class AcademicRiskEngineTests
{
    [TestMethod]
    public void EvaluateTeam_ReturnsHighRiskWhenOverdueAndUnassigned()
    {
        var team = new TeamWorkspaceInfo
        {
            TeamId = "team-01",
            TeamName = "Equipe Delta",
            Course = "ADS",
            ClassName = "Turma A",
            Milestones =
            {
                new TeamMilestoneInfo { Title = "Pré-banca", Status = "Planejada", DueDate = DateTime.Today.AddDays(2) }
            },
            TaskColumns =
            {
                new TeamTaskColumnInfo
                {
                    Title = "Em andamento",
                    Cards =
                    {
                        new TeamTaskCardInfo { Title = "Backend", DueDate = DateTime.Today.AddDays(-1) },
                        new TeamTaskCardInfo { Title = "Frontend", DueDate = DateTime.Today.AddDays(-2) }
                    }
                }
            }
        };

        var snapshot = AcademicRiskEngine.EvaluateTeam(team);

        Assert.AreEqual("Alto", snapshot.Level);
        Assert.IsTrue(snapshot.OverdueItems > 0);
        Assert.IsTrue(snapshot.UnassignedTasks > 0);
    }

    [TestMethod]
    public void BuildMemberWorkload_SummarizesHoursAndPoints()
    {
        var member = new UserInfo { UserId = "user-01", Name = "Ana", Role = "leader" };
        var team = new TeamWorkspaceInfo
        {
            Members = { member },
            TaskColumns =
            {
                new TeamTaskColumnInfo
                {
                    Title = "Sprint",
                    Cards =
                    {
                        new TeamTaskCardInfo
                        {
                            Title = "Protótipo",
                            AssignedUserIds = { member.UserId },
                            EstimatedHours = 6,
                            WorkloadPoints = 5,
                            DueDate = DateTime.Today.AddDays(2)
                        },
                        new TeamTaskCardInfo
                        {
                            Title = "Apresentação",
                            AssignedUserIds = { member.UserId },
                            EstimatedHours = 4,
                            WorkloadPoints = 4,
                            DueDate = DateTime.Today.AddDays(5)
                        }
                    }
                }
            }
        };

        var workload = AcademicRiskEngine.BuildMemberWorkload(team).Single();

        Assert.AreEqual(2, workload.AssignedTasks);
        Assert.AreEqual(10, workload.EstimatedHours);
        Assert.AreEqual(9, workload.WorkloadPoints);
        Assert.AreEqual("Media", workload.Level);
    }

    [TestMethod]
    public void BuildMemberWorkload_ExcludesFacultyFromExecutionLoad()
    {
        var team = new TeamWorkspaceInfo
        {
            Members =
            {
                new UserInfo { UserId = "student-01", Name = "Lia", Role = "student" },
                new UserInfo { UserId = "prof-01", Name = "Prof. Caio", Role = "professor" }
            },
            TaskColumns =
            {
                new TeamTaskColumnInfo
                {
                    Title = "Sprint",
                    Cards =
                    {
                        new TeamTaskCardInfo
                        {
                            Title = "Documento-base",
                            AssignedUserIds = { "student-01", "prof-01" },
                            EstimatedHours = 3,
                            WorkloadPoints = 2,
                            DueDate = DateTime.Today.AddDays(2)
                        }
                    }
                }
            }
        };

        var workload = AcademicRiskEngine.BuildMemberWorkload(team);

        Assert.AreEqual(1, workload.Count);
        Assert.AreEqual("student-01", workload.Single().UserId);
    }
}

[TestClass]
public class AcademicProjectTemplateCatalogTests
{
    [TestMethod]
    public void ApplyToTeam_PopulatesMilestonesTimelineAndDeadline()
    {
        var template = AcademicProjectTemplateCatalog.GetById("software-integrador");
        Assert.IsNotNull(template);

        var team = new TeamWorkspaceInfo
        {
            TeamName = "Equipe Prisma",
            Course = "Analise e Desenvolvimento de Sistemas",
            Ucs = { "Projeto Integrador" }
        };

        AcademicProjectTemplateCatalog.ApplyToTeam(team, template!, "prof-01");

        Assert.AreEqual(template.TemplateId, team.TemplateId);
        Assert.AreEqual(template.Title, team.TemplateName);
        Assert.IsTrue(team.Milestones.Count > 0);
        Assert.IsTrue(team.SemesterTimeline.Count > 0);
        Assert.IsTrue(team.ProjectDeadline.HasValue);
        Assert.AreEqual("prof-01", team.Milestones.First().CreatedByUserId);
    }
}

[TestClass]
public class AppConfigTests
{
    [TestMethod]
    public void FirestoreHelpers_BuildExpectedUrls()
    {
        var documentUrl = AppConfig.BuildFirestoreDocumentUrl("users/test-user");
        var queryUrl = AppConfig.BuildFirestoreRunQueryUrl();

        StringAssert.Contains(documentUrl, "/documents/users/test-user");
        StringAssert.Contains(documentUrl, AppConfig.FirebaseProjectId);
        StringAssert.Contains(queryUrl, "/documents:runQuery");
    }
}
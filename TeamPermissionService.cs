using System;
using System.Collections.Generic;
using System.Linq;

namespace MeuApp
{
    public static class TeamPermissionService
    {
        private static readonly HashSet<string> ProfessorRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "professor",
            "orientador",
            "advisor",
            "teacher"
        };

        private static readonly HashSet<string> LeaderRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "leader",
            "lider",
            "owner",
            "creator"
        };

        private static readonly HashSet<string> CoordinatorRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "coordinator",
            "coordenador",
            "course-coordinator",
            "academic-coordinator"
        };

        private static readonly HashSet<string> RestrictedAssetScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "leadership",
            "private"
        };

        public static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return "student";
            }

            var normalized = role.Trim().ToLowerInvariant();
            if (ProfessorRoles.Contains(normalized))
            {
                return "professor";
            }

            if (LeaderRoles.Contains(normalized))
            {
                return "leader";
            }

            if (CoordinatorRoles.Contains(normalized))
            {
                return "coordinator";
            }

            return normalized switch
            {
                "student" => "student",
                "aluno" => "student",
                "member" => "student",
                "membro" => "student",
                _ => "student"
            };
        }

        public static bool IsProfessorLike(string? role)
        {
            return string.Equals(NormalizeRole(role), "professor", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLeaderLike(string? role)
        {
            return string.Equals(NormalizeRole(role), "leader", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCoordinatorLike(string? role)
        {
            return string.Equals(NormalizeRole(role), "coordinator", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetRoleLabel(string? role)
        {
            return NormalizeRole(role) switch
            {
                "professor" => "Professor orientador",
                "leader" => "Líder discente",
                "coordinator" => "Coordenador",
                _ => "Aluno"
            };
        }

        public static bool IsFacultyRole(string? role)
        {
            return IsProfessorLike(role) || IsCoordinatorLike(role);
        }

        public static bool CanBeTaskAssignee(string? role)
        {
            var normalizedRole = NormalizeRole(role);
            return string.Equals(normalizedRole, "student", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedRole, "leader", StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeExecutionRole(string? role)
        {
            return IsLeaderLike(role) ? "leader" : "student";
        }

        public static List<TeamAccessRuleInfo> CreateDefaultAccessRules()
        {
            return new List<TeamAccessRuleInfo>
            {
                new TeamAccessRuleInfo
                {
                    Role = "leader",
                    CanAddMembers = true,
                    CanManageMembers = false,
                    CanAssignLeadership = false,
                    CanEditProjectSettings = true,
                    CanDeleteTeam = false,
                    CanComment = true,
                    CanUploadFiles = true,
                    CanExportAgenda = true,
                    CanReviewDeliverables = true
                },
                new TeamAccessRuleInfo
                {
                    Role = "professor",
                    CanAddMembers = true,
                    CanManageMembers = true,
                    CanAssignLeadership = true,
                    CanEditProjectSettings = true,
                    CanDeleteTeam = false,
                    CanComment = true,
                    CanUploadFiles = true,
                    CanExportAgenda = true,
                    CanViewProfessorDashboard = true,
                    CanReviewDeliverables = true
                },
                new TeamAccessRuleInfo
                {
                    Role = "coordinator",
                    CanAddMembers = true,
                    CanManageMembers = true,
                    CanAssignLeadership = true,
                    CanEditProjectSettings = true,
                    CanDeleteTeam = false,
                    CanComment = true,
                    CanUploadFiles = true,
                    CanExportAgenda = true,
                    CanViewProfessorDashboard = true,
                    CanReviewDeliverables = true
                },
                new TeamAccessRuleInfo
                {
                    Role = "student",
                    CanAddMembers = false,
                    CanManageMembers = false,
                    CanAssignLeadership = false,
                    CanEditProjectSettings = false,
                    CanDeleteTeam = false,
                    CanComment = true,
                    CanUploadFiles = true,
                    CanExportAgenda = false,
                    CanReviewDeliverables = false
                }
            };
        }

        public static List<TeamAccessRuleInfo> NormalizeAccessRules(IEnumerable<TeamAccessRuleInfo>? accessRules)
        {
            var defaults = CreateDefaultAccessRules()
                .ToDictionary(rule => rule.Role, StringComparer.OrdinalIgnoreCase);

            foreach (var storedRule in accessRules ?? Enumerable.Empty<TeamAccessRuleInfo>())
            {
                var normalizedRole = NormalizeRole(storedRule.Role);
                if (!defaults.TryGetValue(normalizedRole, out var baselineRule))
                {
                    continue;
                }

                defaults[normalizedRole] = new TeamAccessRuleInfo
                {
                    Role = normalizedRole,
                    CanAddMembers = baselineRule.CanAddMembers,
                    CanManageMembers = baselineRule.CanManageMembers,
                    CanAssignLeadership = baselineRule.CanAssignLeadership,
                    CanEditProjectSettings = baselineRule.CanEditProjectSettings,
                    CanDeleteTeam = baselineRule.CanDeleteTeam,
                    CanComment = storedRule.CanComment,
                    CanUploadFiles = storedRule.CanUploadFiles,
                    CanExportAgenda = storedRule.CanExportAgenda,
                    CanViewProfessorDashboard = baselineRule.CanViewProfessorDashboard || storedRule.CanViewProfessorDashboard,
                    CanReviewDeliverables = storedRule.CanReviewDeliverables
                };
            }

            return new[] { "student", "leader", "professor", "coordinator" }
                .Where(role => defaults.ContainsKey(role))
                .Select(role => defaults[role])
                .ToList();
        }

        public static TeamAccessRuleInfo ResolveAccessRule(TeamWorkspaceInfo? team, string? role)
        {
            var normalizedRole = NormalizeRole(role);
            var rules = NormalizeAccessRules(team?.AccessRules);
            return rules.FirstOrDefault(rule => string.Equals(NormalizeRole(rule.Role), normalizedRole, StringComparison.OrdinalIgnoreCase))
                ?? CreateDefaultAccessRules().First(rule => string.Equals(rule.Role, normalizedRole, StringComparison.OrdinalIgnoreCase));
        }

        public static bool CanManageMembers(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanManageMembers;
        }

        public static bool CanAddMembers(TeamWorkspaceInfo? team, string? role, string currentUserId)
        {
            if (team != null &&
                !string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(team.CreatedBy, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ResolveAccessRule(team, role).CanAddMembers;
        }

        public static bool CanAssignLeadership(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanAssignLeadership;
        }

        public static bool CanEditProjectSettings(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanEditProjectSettings;
        }

        public static bool CanDeleteTeam(TeamWorkspaceInfo? team, string? role, string currentUserId)
        {
            if (team == null)
            {
                return false;
            }

            if (string.Equals(team.CreatedBy, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ResolveAccessRule(team, role).CanDeleteTeam;
        }

        public static bool CanExportAgenda(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanExportAgenda;
        }

        public static bool CanComment(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanComment;
        }

        public static bool CanUploadFiles(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanUploadFiles;
        }

        public static bool CanReviewDeliverables(TeamWorkspaceInfo? team, string? role)
        {
            return ResolveAccessRule(team, role).CanReviewDeliverables;
        }

        public static string NormalizePermissionScope(string? permissionScope)
        {
            var normalized = permissionScope?.Trim().ToLowerInvariant() ?? string.Empty;
            return normalized switch
            {
                "course" => "course",
                "leadership" => "leadership",
                "private" => "private",
                _ => "team"
            };
        }

        public static bool CanViewAsset(TeamWorkspaceInfo? team, string? role, string? permissionScope, string currentUserId, string assetOwnerUserId)
        {
            if (team == null)
            {
                return false;
            }

            var normalizedRole = NormalizeRole(role);
            var normalizedScope = NormalizePermissionScope(permissionScope);
            if (!RestrictedAssetScopes.Contains(normalizedScope))
            {
                return true;
            }

            if (string.Equals(normalizedScope, "private", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(assetOwnerUserId) &&
                string.Equals(assetOwnerUserId, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return IsLeaderLike(normalizedRole)
                || IsProfessorLike(normalizedRole)
                || IsCoordinatorLike(normalizedRole);
        }

        public static bool CanUseProfessorDashboard(UserProfile? profile)
        {
            return profile != null && (IsProfessorLike(profile.Role) || IsCoordinatorLike(profile.Role));
        }

        public static bool CanClaimFocalProfessorRole(UserProfile? profile)
        {
            return profile != null && IsProfessorLike(profile.Role);
        }

        public static bool CanClaimFocalProfessorRole(string? role)
        {
            return IsProfessorLike(role);
        }
    }
}
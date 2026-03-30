using System;
using System.Collections.Generic;

namespace MeuApp
{
    /// <summary>
    /// Dados simulados para teste - remover quando Firebase permissions for resolvido
    /// </summary>
    public static class MockData
    {
        public static List<UserInfo> GetMockUsers()
        {
            return new List<UserInfo>
            {
                new UserInfo
                {
                    UserId = "user_pedro_lucas",
                    Name = "Pedro Lucas",
                    Email = "pedrolucasdesouzapessoa@gmail.com",
                    Registration = "2025.32.4218",
                    Course = "Engenharia de Software"
                },
                new UserInfo
                {
                    UserId = "user_maria_santos",
                    Name = "Maria Santos",
                    Email = "maria.santos@example.com",
                    Registration = "2025.32.4219",
                    Course = "Análise e Des. de Sistemas"
                },
                new UserInfo
                {
                    UserId = "user_joao_silva",
                    Name = "João Silva",
                    Email = "joao.silva@example.com",
                    Registration = "2025.32.4220",
                    Course = "Gestão de TI"
                },
                new UserInfo
                {
                    UserId = "user_ana_lima",
                    Name = "Ana Lima",
                    Email = "ana.lima@example.com",
                    Registration = "2025.32.4221",
                    Course = "Ciência da Computação"
                },
                new UserInfo
                {
                    UserId = "user_carlos_costa",
                    Name = "Carlos Costa",
                    Email = "carlos.costa@example.com",
                    Registration = "2025.32.4222",
                    Course = "Engenharia de Software"
                }
            };
        }

        public static List<UserInfo> SearchMockUsers(string query)
        {
            var allUsers = GetMockUsers();
            var results = new List<UserInfo>();

            string queryLower = query.ToLower();

            foreach (var user in allUsers)
            {
                if (user.Name.ToLower().Contains(queryLower) ||
                    user.Email.ToLower().Contains(queryLower) ||
                    user.Registration.Contains(queryLower))
                {
                    results.Add(user);
                }
            }

            return results;
        }
    }
}

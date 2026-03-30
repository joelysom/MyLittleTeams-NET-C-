using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuApp
{
    /// <summary>
    /// Classe para testar conexão com Firebase e diagnosticar problemas
    /// </summary>
    public class FirebaseConnectionTester
    {
        private const string FirebaseProjectId = "obsseractpi";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _idToken;

        public FirebaseConnectionTester(string idToken)
        {
            _idToken = idToken;
        }

        /// <summary>
        /// Teste completo de conexão
        /// </summary>
        public async Task<TestResult> RunFullTestAsync()
        {
            var result = new TestResult();

            Debug.WriteLine("\n========== TESTE FIREBASE INICIADO ==========\n");
            DebugHelper.WriteLine("\n========== TESTE FIREBASE INICIADO ==========\n");

            // Teste 1: Token válido?
            result.TokenValid = !string.IsNullOrEmpty(_idToken);
            Debug.WriteLine($"[TEST 1] Token válido: {result.TokenValid}");
            DebugHelper.WriteLine($"[TEST 1] Token válido: {result.TokenValid}");
            if (!result.TokenValid)
            {
                result.Errors.Add("❌ Token está vazio ou nulo!");
                return result;
            }

            Debug.WriteLine($"[TEST 1] Token length: {_idToken.Length} caracteres");
            DebugHelper.WriteLine($"[TEST 1] Token length: {_idToken.Length} caracteres");

            // Teste 2: Conectar ao Firestore
            result.FirestoreConnectionTest = await TestFirestoreConnectionAsync();
            Debug.WriteLine($"[TEST 2] Conexão Firestore: {(result.FirestoreConnectionTest.Success ? "✅ OK" : "❌ FALHOU")}");
            DebugHelper.WriteLine($"[TEST 2] Conexão Firestore: {(result.FirestoreConnectionTest.Success ? "✅ OK" : "❌ FALHOU")}");

            if (!result.FirestoreConnectionTest.Success)
            {
                result.Errors.Add($"❌ Conexão Firestore falhou: {result.FirestoreConnectionTest.ErrorMessage}");
                return result;
            }

            // Teste 3: Verificar se há documentos
            result.DocumentCountTest = await TestDocumentCountAsync();
            Debug.WriteLine($"[TEST 3] Documentos encontrados: {result.DocumentCountTest.DocumentCount}");
            DebugHelper.WriteLine($"[TEST 3] Documentos encontrados: {result.DocumentCountTest.DocumentCount}");

            if (result.DocumentCountTest.DocumentCount == 0)
            {
                result.Errors.Add("⚠️ Nenhum documento encontrado no Firestore (collection 'users' pode estar vazia)");
            }

            // Teste 4: Tentar extrair dados de um documento
            if (result.DocumentCountTest.DocumentCount > 0)
            {
                result.SampleDocumentTest = await TestSampleDocumentAsync();
                Debug.WriteLine($"[TEST 4] Extraction de documento: {(result.SampleDocumentTest.Success ? "✅ OK" : "❌ FALHOU")}");
                DebugHelper.WriteLine($"[TEST 4] Extraction de documento: {(result.SampleDocumentTest.Success ? "✅ OK" : "❌ FALHOU")}");

                if (result.SampleDocumentTest.Success)
                {
                    Debug.WriteLine($"[TEST 4] Sample data: {result.SampleDocumentTest.UserName} - {result.SampleDocumentTest.Registration}");
                    DebugHelper.WriteLine($"[TEST 4] Sample data: {result.SampleDocumentTest.UserName} - {result.SampleDocumentTest.Registration}");
                }
                else
                {
                    result.Errors.Add($"❌ Erro ao extrair documento: {result.SampleDocumentTest.ErrorMessage}");
                }
            }

            // Resultado final
            Debug.WriteLine($"\n========== TESTE FINALIZADO ==========");
            Debug.WriteLine($"Status: {(result.Errors.Count == 0 ? "✅ TUDO OK!" : "❌ PROBLEMAS DETECTADOS")}");
            Debug.WriteLine($"Total de erros: {result.Errors.Count}\n");

            DebugHelper.WriteLine($"\n========== TESTE FINALIZADO ==========");
            DebugHelper.WriteLine($"Status: {(result.Errors.Count == 0 ? "✅ TUDO OK!" : "❌ PROBLEMAS DETECTADOS")}");
            DebugHelper.WriteLine($"Total de erros: {result.Errors.Count}\n");

            return result;
        }

        /// <summary>
        /// Teste de conexão básica ao Firestore
        /// </summary>
        private async Task<ConnectionTestResult> TestFirestoreConnectionAsync()
        {
            var result = new ConnectionTestResult();

            try
            {
                var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users";
                
                Debug.WriteLine($"  Endpoint: {endpoint}");
                DebugHelper.WriteLine($"  Endpoint: {endpoint}");

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                
                Debug.WriteLine($"  Enviando requisição com Authorization header...");
                DebugHelper.WriteLine($"  Enviando requisição com Authorization header...");

                var response = await httpClient.SendAsync(request);

                Debug.WriteLine($"  Status HTTP: {response.StatusCode}");
                DebugHelper.WriteLine($"  Status HTTP: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    result.Success = true;
                    result.HttpStatusCode = (int)response.StatusCode;
                    
                    var content = await response.Content.ReadAsStringAsync();
                    result.ResponseSize = content.Length;
                    
                    Debug.WriteLine($"  Response size: {result.ResponseSize} bytes");
                    DebugHelper.WriteLine($"  Response size: {result.ResponseSize} bytes");
                }
                else
                {
                    result.Success = false;
                    result.HttpStatusCode = (int)response.StatusCode;
                    
                    var errorContent = await response.Content.ReadAsStringAsync();
                    result.ErrorMessage = $"HTTP {response.StatusCode}: {errorContent.Substring(0, Math.Min(200, errorContent.Length))}";
                    
                    Debug.WriteLine($"  Erro: {result.ErrorMessage}");
                    DebugHelper.WriteLine($"  Erro: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
                
                Debug.WriteLine($"  Exceção: {result.ErrorMessage}");
                DebugHelper.WriteLine($"  Exceção: {result.ErrorMessage}");
            }

            return result;
        }

        /// <summary>
        /// Testa contagem de documentos
        /// </summary>
        private async Task<DocumentCountResult> TestDocumentCountAsync()
        {
            var result = new DocumentCountResult();

            try
            {
                var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    result.ErrorMessage = $"Falha na requisição: {response.StatusCode}";
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(content))
                {
                    result.DocumentCount = 0;
                    result.ErrorMessage = "Resposta vazia";
                    return result;
                }

                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("documents", out var documents))
                    {
                        result.DocumentCount = 0;
                        foreach (var _ in documents.EnumerateArray())
                        {
                            result.DocumentCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Testa extração de um documento de exemplo
        /// </summary>
        private async Task<SampleDocumentResult> TestSampleDocumentAsync()
        {
            var result = new SampleDocumentResult();

            try
            {
                var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    result.ErrorMessage = $"Falha na requisição: {response.StatusCode}";
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();

                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("documents", out var documents))
                    {
                        var firstDoc = documents.EnumerateArray().GetEnumerator();
                        if (firstDoc.MoveNext())
                        {
                            var userDoc = firstDoc.Current;

                            // Extrair dados
                            if (userDoc.TryGetProperty("fields", out var fields))
                            {
                                // Nome
                                if (fields.TryGetProperty("name", out var nameField))
                                {
                                    if (nameField.TryGetProperty("stringValue", out var nameValue))
                                    {
                                        result.UserName = nameValue.GetString() ?? "";
                                    }
                                }

                                // Registration
                                if (fields.TryGetProperty("registration", out var regField))
                                {
                                    if (regField.TryGetProperty("stringValue", out var regValue))
                                    {
                                        result.Registration = regValue.GetString() ?? "";
                                    }
                                }

                                // Email
                                if (fields.TryGetProperty("email", out var emailField))
                                {
                                    if (emailField.TryGetProperty("stringValue", out var emailValue))
                                    {
                                        result.Email = emailValue.GetString() ?? "";
                                    }
                                }

                                result.Success = true;
                            }
                            else
                            {
                                result.ErrorMessage = "Campo 'fields' não encontrado no documento";
                            }
                        }
                    }
                    else
                    {
                        result.ErrorMessage = "Campo 'documents' não encontrado na resposta";
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }

    // Classe para resultados
    public class TestResult
    {
        public bool TokenValid { get; set; }
        public ConnectionTestResult FirestoreConnectionTest { get; set; } = new();
        public DocumentCountResult DocumentCountTest { get; set; } = new();
        public SampleDocumentResult SampleDocumentTest { get; set; } = new();
        public List<string> Errors { get; set; } = new();

        public string GetSummary()
        {
            var summary = $"🔍 TESTE FIREBASE SUMMARY:\n\n";
            summary += $"✅ Token válido: {TokenValid}\n";
            summary += $"✅ Conexão Firestore: {FirestoreConnectionTest.Success}\n";
            summary += $"✅ Documentos encontrados: {DocumentCountTest.DocumentCount}\n";

            if (Errors.Count > 0)
            {
                summary += $"\n❌ ERROS ENCONTRADOS ({Errors.Count}):\n";
                foreach (var error in Errors)
                {
                    summary += $"  {error}\n";
                }
            }
            else
            {
                summary += $"\n✅ TUDO OK! Sistema pronto para buscar.";
            }

            return summary;
        }
    }

    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public int HttpStatusCode { get; set; }
        public int ResponseSize { get; set; }
        public string ErrorMessage { get; set; } = "";
    }

    public class DocumentCountResult
    {
        public int DocumentCount { get; set; }
        public string ErrorMessage { get; set; } = "";
    }

    public class SampleDocumentResult
    {
        public bool Success { get; set; }
        public string UserName { get; set; } = "";
        public string Registration { get; set; } = "";
        public string Email { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}

# 🔥 Guia Completo de Debug - Persistência de Equipes

## O Que Foi Corrigido

### 1. **Estrutura de Salvamento Melhorada**
- Equipes agora são salvas na collection `teams` com ID único
- Referências de usuário-equipe são salvas em `userTeams` para carregamento rápido
- Cada membro da equipe recebe uma referência

### 2. **Estrutura de Dados no Firebase**

```
obsseractpi (Projeto Firebase)
└── collections
    ├── teams/
    │   └── {teamId}/
    │       ├── teamId
    │       ├── teamName
    │       ├── course
    │       ├── className
    │       ├── classId
    │       ├── members (array)
    │       ├── ucs (array)
    │       ├── createdAt
    │       ├── updatedAt
    │       ├── createdBy
    │       └── isActive
    │
    └── userTeams/
        └── {userId}_{teamId}/
            ├── userId
            ├── teamId
            ├── teamName
            └── addedAt
```

## Como Funciona Agora

### **Quando um Aluno Cria uma Equipe:**

1. Aluno preenche dados (nome, curso, turma, UCs, membros)
2. Clica "Criar equipe"
3. `CreateTeam_Click()` → `SaveTeamWorkspace()` → atualiza UI
4. `SaveTeamToFirestoreAsync()` → `TeamService.SaveTeamAsync()`
5. Equipe é salva em `teams/{teamId}`
6. Para cada membro: cria documento em `userTeams/{userId}_{teamId}`

### **Quando um Aluno Faz Login:**

1. App inicia → `MainWindow_Loaded()`
2. Inicializa `TeamService` com userId do aluno
3. Chama `LoadTeamsFromDatabaseAsync()`
4. `TeamService.LoadTeamsAsync()` executa:
   - Busca em `userTeams/` todos os documentos com userId
   - Para cada teamId encontrado, carrega dados de `teams/{teamId}`
   - Retorna lista de equipes

### **Resultado:**
✅ Aluno A cria equipe com Aluno B
✅ Aluno A vê a equipe (porque criou)
✅ Aluno B vê a mesma equipe (porque foi adicionado)

## Logs de Debug

Para acompanhar o process, abra o arquivo de debug:
- **Pressione `Ctrl+D` na aplicação**

### Logs de Salvamento
```
[TeamService.SaveTeam] ===== INICIANDO SALVAMENTO =====
[TeamService.SaveTeam] Equipe: 'Nome da Equipe'
[TeamService.SaveTeam] ClassId: 'turma-pi-001'
[TeamService.SaveTeam] Membros: 3
[TeamService.SaveTeam] UCs: 2
[TeamService.SaveTeam] TeamId gerado: 'turma-pi-001_nome_da_equipe'
[TeamService.SaveTeam] URL: https://firestore.googleapis.com/v1/projects/obsseractpi/databases/(default)/documents/teams/...
[TeamService.SaveTeam] Enviando requisição para Firebase...
[TeamService.SaveTeam] Status Code: 200
[TeamService.SaveTeam] ✅ Equipe 'Nome da Equipe' salva com sucesso!
[TeamService.SaveTeam] ===== SALVAMENTO CONCLUÍDO =====

[TeamService.SaveTeamReferences] Salvando referências para 3 membros...
[TeamService.SaveTeamReference] ✅ Ref para 'userId1'
[TeamService.SaveTeamReference] ✅ Ref para 'userId2'
[TeamService.SaveTeamReference] ✅ Ref para 'userId3'
[TeamService.SaveTeamReferences] ✅ Referências salvas com sucesso
```

### Logs de Carregamento
```
[TeamService.LoadTeams] ===== INICIANDO CARREGAMENTO =====
[TeamService.LoadTeams] Usuário: 'userId123'
[TeamService.GetUserTeamIds] Buscando equipes para 'userId123'...
[TeamService.GetUserTeamIds] TeamId: 'turma-pi-001_equipe1'
[TeamService.GetUserTeamIds] TeamId: 'turma-pi-001_equipe2'
[TeamService.GetUserTeamIds] Total: 2

[TeamService.LoadTeamById] Carregando 'turma-pi-001_equipe1'...
[TeamService.LoadTeams] ✅ Carregada: 'Equipe 1'

[TeamService.LoadTeamById] Carregando 'turma-pi-001_equipe2'...
[TeamService.LoadTeams] ✅ Carregada: 'Equipe 2'

[TeamService.LoadTeams] ===== CONCLUÍDO (2 equipes) =====
```

## Checklist de Testes

### Teste 1: Criar Equipe e Verificar Salvamento
- [ ] Faça login com **Aluno A**
- [ ] Vá em "Equipes" → "+ Criar equipe"
- [ ] Preencha:
  - Nome: "Projeto XYZ"
  - Curso: "Análise e Desenvolvimento de Sistemas"
  - Turma: "Turma A"
  - ID da Turma: "TURMA-PI-001"
  - UCs: "Projeto Integrador"
  - Membros: Selecione "Aluno B"
- [ ] Clique "Criar equipe"
- [ ] **Esperado**: Apareça mensagem de sucesso
- [ ] **Debug**: Pressione `Ctrl+D` e procure por "✅ salva com sucesso"

### Teste 2: Verificar se Aluno A Vê a Equipe Após Relogin
- [ ] Feche a aplicação
- [ ] Abra novamente com **Aluno A**
- [ ] Vá em "Equipes"
- [ ] **Esperado**: "Projeto XYZ" aparece na lista
- [ ] **Debug**: Procure por "✅ Carregada: 'Projeto XYZ'"

### Teste 3: Verificar se Aluno B Vê a Equipe
- [ ] Saia da conta de **Aluno A**
- [ ] Faça login com **Aluno B**
- [ ] Vá em "Equipes"
- [ ] **Esperado**: "Projeto XYZ" aparece na lista (porque foi adicionado)
- [ ] **Debug**: Localizar "Buscando equipes para xxB" e confirmar que carregou

## Possíveis Problemas e Soluções

### ❌ Equipe Desaparece ao Relogin
**Causa**: SaveTeamAsync não está completando
**Solução**: 
- Verificar logs de erro "❌ ERRO AO SALVAR"
- Verificar se o Firebase tem permissões de escrita
- Verificar se o token está válido

### ❌ Aluno B Não Vê a Equipe
**Causa**: Referência em `userTeams` não foi criada
**Solução**:
- Verificar logs "Salvando referências"
- Confirmar que o UserId de Aluno B foi adicionado corretamente
- Fazer novo login de Aluno B

### ❌ Erro de Permissão no Firebase
**Mensagem**: "HTTP 403: Forbidden"
**Solução**:
- Verificar regras de segurança do Firebase
- Certificar que o token tem escopo adequado
- Testar com `Ctrl+L` para validar conexão

## Estrutura de Código

### MainWindow.xaml.cs
- `MainWindow_Loaded()` - Inicializa TeamService
- `LoadTeamsFromDatabaseAsync()` - Carrega equipes do Firebase
- `SaveTeamWorkspace()` - Salva em memória + Firebase
- `SaveTeamToFirestoreAsync()` - Chamada assíncrona ao TeamService

### TeamService.cs
- `SaveTeamAsync()` - Salva equipe e referências
- `LoadTeamsAsync()` - Carrega todas as equipes do usuário
- `SaveTeamReferenceForMembersAsync()` - Cria referências para membros
- `GetUserTeamIdsAsync()` - Busca IDs de equipes do usuário
- `LoadTeamByIdAsync()` - Carrega dados específicos de uma equipe

## Próximos Passos Recomendados

1. **Testar com multiple navegadores** para validar que ambos os alunos veem
2. **Testar modificações** - editar equipe deve persistir
3. **Testar exclusão** - deletar equipe deve remover para todos
4. **Implementar sincronização em tempo real** com Firestore listeners

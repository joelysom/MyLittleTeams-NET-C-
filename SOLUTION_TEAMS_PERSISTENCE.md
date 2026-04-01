# 🔧 Solução: Persistência de Equipes no Firebase

## Problema Identificado
As equipes criadas pelos alunos não eram salvas no banco de dados. Quando a aplicação era fechada e reabierta, as equipes desapareciam completamente.

## Causa Raiz
O método `SaveTeamWorkspace()` estava apenas salvando as equipes em uma lista na memória (`_teamWorkspaces`). Quando a aplicação encerrava, essa memória era descartada e os dados eram perdidos.

## Solução Implementada

### 1. **Novo Arquivo: TeamService.cs**
   - Serviço responsável por comunicar com Firebase Firestore
   - Métodos principais:
     - `SaveTeamAsync()` - Salva/atualiza equipe no Firebase
     - `LoadTeamsAsync()` - Carrega todas as equipes do usuário
     - `DeleteTeamAsync()` - Deleta uma equipe

### 2. **Novo Arquivo: TeamModels.cs**
   - Classes públicas para estruturas de dados de equipes:
     - `TeamWorkspaceInfo` - Informações da equipe
     - `TeamTaskColumnInfo` - Colunas de tarefas
     - `TeamTaskCardInfo` - Cartões de tarefas
     - `TeamAssetInfo` - Arquivos compartilhados
     - `TeamNotificationInfo` - Notificações
     - `TeamCsdBoardInfo` - Quadro CSD

### 3. **Modificações em MainWindow.xaml.cs**
   - Adicionada variável: `private TeamService? _teamService`
   - Inicializa TeamService em `MainWindow_Loaded()`
   - Novo método: `LoadTeamsFromDatabaseAsync()` - Carrega equipes do Firebase na startup
   - `SaveTeamWorkspace()` agora também chama `SaveTeamToFirestoreAsync()`
   - Novo método: `SaveTeamToFirestoreAsync()` - Salva equipe no Firebase de forma assíncrona

### 4. **Modificações em UserSearchService.cs**
   - Adicionada propriedade `Role` à classe `UserInfo`

## Como Funciona Agora

### Criar Equipe
1. Usuário preenche dados da equipe (nome, curso, turma, etc)
2. Clica em "Criar equipe"
3. `CreateTeam_Click()` é acionado
4. Equipe é salva em memória via `SaveTeamWorkspace()`
5. `SaveTeamToFirestoreAsync()` é chamada automaticamente
6. Equipe é enviada para o Firebase Firestore via API HTTP

### Carregar Equipes
1. Aplicação inicia
2. `MainWindow_Loaded()` é acionado
3. `LoadTeamsFromDatabaseAsync()` carrega equipes do Firebase
4. Equipes são exibidas na interface

## Fluxo de Persistência

```
┌─────────────────┐
│ Criar/Editar    │
│   Equipe        │
└────────┬────────┘
         │
         ├─→ SaveTeamWorkspace() ──→ Memória (_teamWorkspaces)
         │                          ├─→ RenderTeamsList() [Atualiza UI]
         │
         └─→ SaveTeamToFirestoreAsync()
                  │
                  └─→ TeamService.SaveTeamAsync()
                       │
                       └─→ HTTP PATCH
                            │
                            └─→ Firebase Firestore
                                 │
                                 └─→ Persista permanentemente
```

## Testes Recomendados

1. **Criar Equipe**
   - Abra a aplicação
   - Vá para Equipes > Criar equipe
   - Preencha todos os dados
   - Clique em "Criar equipe"
   - Verifique se aparece na lista

2. **Persistência**
   - Fechaar a aplicação
   - Reabra a aplicação
   - **Esperado:** A equipe criada deve aparecer na lista

3. **Modificações**
   - Crie uma equipe
   - Adicione/remova membros
   - Feche e reabra a aplicação
   - **Esperado:** Todas as alterações devem ser preservadas

## Estrutura de Dados no Firebase

As equipes são armazenadas em:
```
projects/obsseractpi/databases/(default)/documents/teams/{teamId}
```

Com a estrutura:
```json
{
  "teamId": "...",
  "teamName": "...",
  "course": "...",
  "className": "...",
  "classId": "...",
  "members": [...],
  "ucs": [...],
  "createdAt": "...",
  "updatedAt": "...",
  "createdBy": "..."
}
```

## Notas Importantes

- ✅ Salvamento é automático e assíncrono
- ✅ Não bloqueia a interface durante o salvamento
- ✅ Cada alteração é persistida no Firebase
- ✅ Mensagens de debug registram todas as operações
- ⚠️ Requer token Firebase válido (`_idToken`)
- ⚠️ Requer conexão com internet

## Debug

Para verificar os logs de salvamento:
- Pressione `Ctrl+D` na aplicação para abrir o arquivo de debug

# 🔐 Regras de Segurança do Firebase Firestore

## Regras Necessárias para Persistência de Equipes

Para que o salvamento e carregamento de equipes funcione, é necessário configurar as regras de segurança do Firestore.

O modelo atual protege três áreas em conjunto:

- `teams/{teamId}`: documento principal da equipe com membros, metadados do workspace, colunas do board, assets e índices derivados por papel.
- `teams/{teamId}/taskCards/{cardId}`: cards do board em subcoleção com autorização fina para estrutura versus colaboração.
- `teams/{teamId}/milestones/{milestoneId}`: entregas/marcos em subcoleção com autorização fina para revisão versus colaboração.
- `userTeams/{userId}_{teamId}`: referência rápida para carregar as equipes visíveis ao usuário.
- `teamAssetFiles/{teamId}_{assetId}_v{n}`: conteúdo remoto versionado dos arquivos sincronizados pelo hub e pelos anexos de cards/milestones.

### **Configuração Recomendada para Desenvolvimento**

O arquivo fonte das regras agora está em `firestore.rules` na raiz do projeto. Publique exatamente esse conteúdo no Firestore para manter as coleções `teams`, `userTeams` e `teamAssetFiles` alinhadas com o código atual.

```javascript
// Use o conteúdo completo de firestore.rules.
// O trecho abaixo resume apenas os pontos que mudaram no modelo:
// - o documento da equipe agora salva memberRolesByUserId e listas como leaderIds/professorIds/coordinatorIds/studentIds;
// - updates estruturais exigem papel de liderança/docência/coordenacao;
// - updates de aluno no documento principal ficam limitados à superfície colaborativa remanescente (assets/notificações/chat);
// - cards do board e milestones agora ficam em subcoleções próprias, com rules separando colaboração de mudança estrutural;
// - userTeams aceita escrita de quem pode gerir membros na equipe;
// - o conteúdo remoto dos arquivos fica em teamAssetFiles e é protegido por papel + permissionScope.
```

### Ponto crítico desta implementação

O aplicativo cria ou atualiza a equipe em `teams/{teamId}` e, em seguida, grava documentos em `userTeams/{userId}_{teamId}` para todos os membros adicionados. Como professor, líder e coordenador agora também podem gerir membros, a regra de `userTeams` não pode mais depender apenas de `createdBy`; ela precisa aceitar quem tem papel gerencial no documento da equipe.

Outro ponto importante: o conteúdo binário do hub e dos anexos não fica mais inteiro dentro do documento `teams/{teamId}`. Cada versão é salva em `teamAssetFiles/...`, enquanto o documento da equipe guarda apenas os metadados, a versão atual e o histórico de versões. Isso reduz o risco de estourar o limite de tamanho do documento principal da equipe.

## Migração inicial após publicar as rules

Depois de publicar as rules novas, abra cada equipe com uma conta que tenha papel de `leader`, `professor` ou `coordinator` e provoque um salvamento da equipe. Esse primeiro save materializa as subcoleções `taskCards` e `milestones` a partir do estado atual do documento legado. Antes dessa migração inicial, contas de aluno não devem ser usadas como primeiro gravador dos work items refinados.

## Como Configurar no Firebase Console

1. Acesse [Firebase Console](https://console.firebase.google.com)
2. Selecione seu projeto **obsseractpi**
3. Vá em **Firestore Database**
4. Clique em **Rules** (abas no topo)
5. Limpe o conteúdo atual
6. Cole as regras acima
7. Clique **Publish**

## Verificação de Permissões

### ✅ Teste Rápido
Para testar se as permissões estão corretas:

1. Abra o **Firebase Console**
2. Vá em **Firestore** → **Rules** → **Test Rules**
3. Teste um read em `teams/{teamId}`:
   - Auth: `true` (usuário autenticado)
   - Request: `read`
   - Path: `teams/any_team_id`
   - **Resultado**: ✅ Permitido

4. Teste um write em `teams/{teamId}`:
   - Auth: `true`
   - Request: `create, update`
   - Path: `teams/any_team_id`
   - Data: `{ createdBy: "some_user_id" }`
   - **Resultado**: ✅ Permitido

5. Teste um update colaborativo em `teams/{teamId}/taskCards/{cardId}`:
   - Auth: `true`
   - Papel: `student`
   - Request: `update`
   - Altere apenas `comments`, `attachments`, `mentionedUserIds`, `updatedAt`, `updatedByUserId`
   - **Resultado**: ✅ Permitido

6. Teste um update estrutural em `teams/{teamId}/taskCards/{cardId}`:
   - Auth: `true`
   - Papel: `student`
   - Request: `update`
   - Altere `title`, `columnId`, `assignedUserIds` ou `priority`
   - **Resultado**: ❌ Negado

7. Teste um update de revisão em `teams/{teamId}/milestones/{milestoneId}`:
   - Auth: `true`
   - Papel: `professor`, `leader` ou `coordinator`
   - Request: `update`
   - Altere apenas `status`, `updatedAt`, `updatedByUserId`
   - **Resultado**: ✅ Permitido

## Tokens e Autenticação

O token que está sendo usado deve ter escopo para:
- ✅ Ler na collection `teams`
- ✅ Ler em `teams/{teamId}/taskCards` e `teams/{teamId}/milestones`
- ✅ Ler na collection `userTeams`
- ✅ Escrever na collection `teams`
- ✅ Escrever em `teams/{teamId}/taskCards` e `teams/{teamId}/milestones` quando o papel permitir
- ✅ Escrever na collection `userTeams`

### Verificar Token Válido

Pressione `Ctrl+D` na aplicação e procure por:
```
[TEST 1] Token válido: true
[TEST 1] Token length: XXX caracteres
```

## Problemas Comuns

### Erro: HTTP 403 (Forbidden)
```
HTTP 403: Error(13, PERMISSION_DENIED)
```
**Causa**: Regras de segurança não permitem a operação
**Solução**: 
1. Verificar as regras do Firestore
2. Confirmar que o usuário está autenticado
3. Testar com Firebase Console → Rules → Test Rules

### Erro: HTTP 404 (Not Found)
```
HTTP 404: Not Found
```
**Causa**: Documento não existe ou caminho incorreto
**Solução**:
1. Verificar o TeamId gerado (ver logs)
2. Confirmar que a coleção `teams` existe

### Erro: HTTP 401 (Unauthorized)
```
HTTP 401: Unauthorized
```
**Causa**: Token inválido ou expirado
**Solução**:
1. Relogar na aplicação
2. Testar botão "🧪" (Firebase test)
3. Verificar se o token tem permissões corretas

## Índices Recomendados

Para melhor performance, configure estes índices no Firestore:

### Índice 1: Buscar equipes por usuário
```
Collection: userTeams
Fields:
  - userId (Ascending)
  - addedAt (Descending)
```

### Índice 2: Buscar equipes ativas
```
Collection: teams
Fields:
  - isActive (Ascending)
  - createdAt (Descending)
```

## Monitoramento

Para acompanhar operações:

1. **Firebase Console** → **Firestore** → **Data**
   - Verify documentos em collections `teams`, `userTeams`, `teams/{teamId}/taskCards` e `teams/{teamId}/milestones`

2. **Firebase Console** → **Firestore** → **Rules**
   - Check recent denials for permission issues

3. **Logs da Aplicação** (Ctrl+D)
   - Acompanhado salvamentos e carregamentos
   - Verificar erros específicos

## Segurança em Produção

Para produção, considere regras mais restritivas:

```javascript
match /teams/{teamId} {
  // Apenas ver se é membro da equipe
  allow read: if request.auth.uid in resource.data.memberIds;
  
  // Apenas criador ou admin pode editar
  allow update: if request.auth.uid == resource.data.createdBy 
             || request.auth.uid in resource.data.adminIds;
}
```

## Suporte

Se encontrar problemas:
1. Verificar os **Logs** com `Ctrl+D`
2. Testar no **Firebase Console** → **Rules** → **Test Rules**
3. Verificar **Network** no DevTools (F12) para ver requisições HTTP
4. Validar token com botão "🧪" de teste Firebase

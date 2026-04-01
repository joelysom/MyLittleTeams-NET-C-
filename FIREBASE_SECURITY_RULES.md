# 🔐 Regras de Segurança do Firebase Firestore

## Regras Necessárias para Persistência de Equipes

Para que o salvamento e carregamento de equipes funcione, é necessário configurar as regras de segurança do Firestore.

### **Configuração Recomendada para Desenvolvimento**

O arquivo fonte das regras agora está em `firestore.rules` na raiz do projeto. Publique esse conteúdo no Firestore para manter as coleções `teams` e `userTeams` alinhadas com o código atual.

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    function signedIn() {
      return request.auth != null;
    }

    function teamVisible(data) {
      return signedIn() && (
        data.createdBy == request.auth.uid ||
        (data.memberIds is list && request.auth.uid in data.memberIds)
      );
    }

    function validTeamPayload(data) {
      return data.teamId is string
        && data.teamName is string
        && data.course is string
        && data.className is string
        && data.classId is string
        && data.createdBy is string
        && data.isActive is bool
        && data.createdAt is timestamp
        && data.updatedAt is timestamp
        && data.ucs is list
        && data.memberIds is list
        && request.auth.uid in data.memberIds
        && data.members is list;
    }

    match /teams/{teamId} {
      allow get: if teamVisible(resource.data);
      allow list: if signedIn();
      allow create: if signedIn()
        && validTeamPayload(request.resource.data)
        && request.resource.data.createdBy == request.auth.uid
        && request.resource.data.teamId == teamId;
      allow update: if teamVisible(resource.data)
        && validTeamPayload(request.resource.data)
        && request.resource.data.teamId == teamId
        && request.resource.data.createdBy == resource.data.createdBy;
      allow delete: if signedIn() && resource.data.createdBy == request.auth.uid;
    }

    match /userTeams/{documentId} {
      allow get: if signedIn() && resource.data.userId == request.auth.uid;
      allow list: if signedIn();
      allow create, update: if signedIn()
        && (
          request.resource.data.userId == request.auth.uid ||
          get(/databases/$(database)/documents/teams/$(request.resource.data.teamId)).data.createdBy == request.auth.uid
        )
        && request.resource.data.teamId is string
        && request.resource.data.teamName is string
        && request.resource.data.addedAt is timestamp;
      allow delete: if signedIn()
        && (
          resource.data.userId == request.auth.uid ||
          get(/databases/$(database)/documents/teams/$(resource.data.teamId)).data.createdBy == request.auth.uid
        );
    }
  }
}
```

### Ponto crítico desta implementação

O aplicativo cria a equipe em `teams/{teamId}` e, em seguida, grava documentos em `userTeams/{userId}_{teamId}` para todos os membros adicionados. Se a regra de `userTeams` permitir gravação apenas quando `request.auth.uid == userId`, o criador da equipe consegue salvar a equipe principal, mas falha ao criar as referências dos outros alunos. O resultado prático é exatamente o sintoma que você descreveu: a equipe parece criada localmente, mas não reaparece corretamente para todos ao relogar.

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

## Tokens e Autenticação

O token que está sendo usado deve ter escopo para:
- ✅ Ler na collection `teams`
- ✅ Ler na collection `userTeams`
- ✅ Escrever na collection `teams`
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
   - Verify documentos em collections `teams` e `userTeams`

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

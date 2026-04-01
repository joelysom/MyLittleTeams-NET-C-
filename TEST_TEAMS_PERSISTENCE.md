# ✅ Teste Passo-a-Passo: Persistência de Equipes

## Pré-requisitos
- ✅ Aplicação compilada e rodando
- ✅ 2 contas de usuário diferentes (Aluno A e Aluno B)
- ✅ Firebase conectado e autenticado
- ✅ Ctrl+D disponível para debug

---

## TESTE 1: Criar Equipe e Verificar Salvamento Imediato

### Passo 1.1: Login com Aluno A
```
1. Abra a aplicação
2. Faça login com conta de Aluno A
3. Navegue para "Equipes"
4. Clique em "+"
5. Selecione "Criar equipe"
```

### Passo 1.2: Preencher Dados
```
Nome da Equipe: "PI-2024-Test-01"
Curso: "Análise e Desenvolvimento de Sistemas"
Turma: "Turma A - Manhã"
ID da Turma: "TURMA-PI-001"
```

### Passo 1.3: Adicionar UCs
```
1. Selecione "Projeto Integrador"
2. Clique "Adicionar UC"
3. Resultado esperado: UC aparece como tag
```

### Passo 1.4: Adicionar Aluno B
```
1. No campo "Adicionar alunos", procure por "Aluno B"
2. Selecione na lista ou digite o nome
3. Clique "Adicionar aluno"
4. Resultado esperado: Nome aparece como tag
```

### Passo 1.5: Criar Equipe
```
1. Clique "Criar equipe"
2. Resultado esperado:
   - ✅ "Equipe criada e adicionada na lista"
   - Equipe aparece em "Equipes cadastradas"
```

### Passo 1.6: Verificar Debug
```
1. Pressione Ctrl+D para abrir arquivo de debug
2. Procure por:
   [TeamService.SaveTeam] ===== INICIANDO SALVAMENTO =====
   [TeamService.SaveTeam] Equipe: 'PI-2024-Test-01'
   [TeamService.SaveTeam] ✅ Equipe 'PI-2024-Test-01' salva com sucesso!
   [TeamService.SaveTeamReferences] ✅ Referências salvas com sucesso
3. Se ver esses logs, o Firebase recebeu os dados ✅
```

---

## TESTE 2: Fechar e Reabrir - Aluno A Vê a Equipe

### Passo 2.1: Fechar Aplicação
```
1. Saia da aplicação completamente
2. Aguarde 2 segundos
```

### Passo 2.2: Reabrir e Login
```
1. Inicie novamente a aplicação
2. Faça login Com Aluno A (mesma conta)
3. Navegue para "Equipes"
```

### Passo 2.3: Verificar Resultado
```
✅ ESPERADO: "PI-2024-Test-01" aparece na lista
❌ NÃO ESPERADO: 
   - Apenas "Sem equipes ativas"
   - Opção de "Criar equipe" vazia
```

### Passo 2.4: Verificar Debug
```
1. Pressione Ctrl+D
2. Procure por:
   [LoadTeamsFromDatabase] Carregando equipes do Firebase
   [TeamService.LoadTeams] Equipes encontradas: 1
   [TeamService.GetUserTeamIds] TeamId: 'turma-pi-001_pi-2024-test-01'
   [TeamService.LoadTeams] ✅ Carregada: 'PI-2024-Test-01'
3. Se ver esses logs, carregou corretamente ✅
```

---

## TESTE 3: Aluno B Vê a Mesma Equipe

### Passo 3.1: Sair e Login com Aluno B
```
1. Na aplicação, clique "Sair da conta" (Configurações)
2. Faça login com Aluno B
3. Navegue para "Equipes"
```

### Passo 3.2: Verificar Resultado
```
✅ ESPERADO: "PI-2024-Test-01" aparece na lista
   - MESMO SENDO criada por outro aluno!
   - Porque você (Aluno B) foi adicionado como membro
❌ NÃO ESPERADO:
   - Apenas "Sem equipes ativas"
```

### Passo 3.3: Verificar Debug
```
1. Pressione Ctrl+D
2. Procure por:
   [LoadTeamsFromDatabase] Usuário: '{userId_de_Aluno_B}'
   [GetUserTeamIds] Buscando equipes para '{userId_de_Aluno_B}'
   [GetUserTeamIds] TeamId: 'turma-pi-001_pi-2024-test-01'
   [LoadTeams] ✅ Carregada: 'PI-2024-Test-01'
3. Se os UserIds são diferentes e ainda carregou, funciona ✅
```

---

## TESTE 4: Verificar Dados no Firebase Console

### Passo 4.1: Acessar Firebase Console
```
1. Abra https://console.firebase.google.com
2. Selecione projeto "obsseractpi"
3. Vá em "Firestore Database"
```

### Passo 4.2: Verificar Collection "teams"
```
1. Expanda "teams"
2. Procure por documento com nome similar a:
   "turma-pi-001_pi-2024-test-01"
3. Verificar campos:
   - teamName: "PI-2024-Test-01" ✅
   - classId: "TURMA-PI-001" ✅
   - members: array com 2+ membros ✅
   - ucs: array com "Projeto Integrador" ✅
```

### Passo 4.3: Verificar Collection "userTeams"
```
1. Expanda "userTeams"
2. Procure por documentos com padrão:
   "{userId_de_Aluno_A}_turma-pi-001_pi-2024-test-01"
   "{userId_de_Aluno_B}_turma-pi-001_pi-2024-test-01"
3. Deve haver DOIS documentos (um para cada membro) ✅
4. Cada um tem:
   - userId: ✅
   - teamId: "turma-pi-001_pi-2024-test-01" ✅
   - teamName: "PI-2024-Test-01" ✅
```

---

## TESTE 5: Editar Equipe e Verificar Persistência

### Passo 5.1: Com Aluno A, Modificar Equipe
```
1. Faça login com Aluno A
2. Vá em "Equipes"
3. Clique na equipe "PI-2024-Test-01"
4. Tente adicionar:
   - Novo membro
   - Novo aluno
   - Novo UC
```

### Passo 5.2: Verificar Salvamento
```
1. Procure nos logs por:
   [SaveTeamToFirebase] Salvando equipe...
   [TeamService.SaveTeam] ===== INICIANDO SALVAMENTO =====
   [TeamService.SaveTeam] ✅ Equipe salva com sucesso!
```

### Passo 5.3: Relogin e Verificar
```
1. Feche a aplicação
2. Reabra e faça login com Aluno A
3. Vá em "Equipes"
4. Abra "PI-2024-Test-01"
5. ✅ ESPERADO: Novas alterações aparecem
```

---

## CHECKLIST DE SUCESSO

| Teste | Resultado | Status |
|-------|-----------|--------|
| Criar equipe | Mensagem sucesso | ☐ |
| Logs debug salvamento | Ver ✅ salva com sucesso | ☐ |
| Aluno A relogin | Equipe aparece | ☐ |
| Aluno B login | Equipe TAMBÉM aparece | ☐ |
| Firebase console > teams | Documento existe | ☐ |
| Firebase console > userTeams | 2+ documentos existem | ☐ |
| Editar equipe | Mudanças persistem | ☐ |

**Se todos os itens estão ☑️ : A persistência está funcionando! 🎉**

---

## Troubleshooting

### ❌ Equipe não aparece no relogin do Aluno A

**Debug:**
1. Pressione Ctrl+D
2. Procure por `[TeamService.LoadTeams]`
3. Se ver `ERRO: ...`, verá o problema

**Soluções:**
- [ ] Verificar se o SaveTeamAsync teve sucesso (ver "✅ salva")
- [ ] Verificar se há erro HTTP (403, 404, 401)
- [ ] Firebase Console: conferir se `teams/{teamId}` existe
- [ ] Firebase Console: conferir regras de segurança

### ❌ Aluno B não vê a equipe

**Debug:**
1. Com Aluno B, pressione Ctrl+D
2. Procure por `[GetUserTeamIds] Buscando equipes`
3. Se não encontrou nenhum teamId, o problema é a referência

**Soluções:**
- [ ] Verificar se `userTeams/{userId_de_B}_{teamId}` existe no Firebase
- [ ] Confirmar que UserId de Aluno B foi adicionado à equipe
- [ ] Firebase: verificar regras (userTeams pode estar bloqueado)

### ❌ Erro "HTTP 403: Forbidden"

**Causa:** Permissões do Firebase

**Soluções:**
1. Ir para Firebase Console → Firestore → Rules
2. Copiar as regras de [FIREBASE_SECURITY_RULES.md](FIREBASE_SECURITY_RULES.md)
3. Colar e Publish

### ❌ Erro "HTTP 404: Not Found"

**Causa:** Caminho incorreto no Firestore

**Debug:**
1. Ver o `[TeamService.SaveTeam] URL: ...` nos logs
2. Copiar a URL
3. Verificar se a collection `teams` existe no Firebase

---

## Logs Importantes para Verificar

### Salvamento Bem-Sucedido
```
✅ ===== INICIANDO SALVAMENTO =====
✅ TeamId gerado: 'turma-pi-001_pi-2024-test-01'
✅ Enviando requisição para Firebase...
✅ Status Code: 200
✅ Salvando referências para 2 membros...
✅ Referências salvas com sucesso
✅ ===== SALVAMENTO CONCLUÍDO =====
```

### Carregamento Bem-Sucedido
```
✅ ===== INICIANDO CARREGAMENTO =====
✅ Equipes encontradas: 1
✅ Buscando equipes para 'userId123'
✅ TeamId: 'turma-pi-001_pi-2024-test-01'
✅ Carregada: 'PI-2024-Test-01'
✅ ===== CONCLUÍDO (1 equipes) =====
```

---

## Próximos Passos Após Sucesso

1. **Teste com 3+ alunos** para validar múltiplas relações
2. **Teste exclusão** de equipe (verificar se desaparece para todos)
3. **Teste sincronização em tempo real** (abrir 2 navegadores lado-a-lado)
4. **Implementar notificações** email quando aluno é adicionado a equipe

---

**Data de Teste**: __________
**Responsável**: __________
**Resultado Final**: ☐ Sucesso | ☐ Parcial | ☐ Falha

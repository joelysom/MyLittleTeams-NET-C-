# 🔍 Guia de Diagnóstico - Problema de Busca

## O Que Foi Corrigido

Foi implementado um **sistema de logging detalhado** que permite diagnosticar exatamente por que os usuários não estão sendo encontrados.

## Como Usar o Modo Debug

### Mini-tela lateral em tempo real

Agora o projeto suporta uma **mini-tela lateral de debug** que abre ao lado da janela principal e mostra logs em tempo real de UI, Firebase, Firestore, sincronização, filas e erros.

### Ativar ao executar com `dotnet run`

1. Execute: `dotnet run -- --debug-console`
2. A aplicação abrirá normalmente.
3. A mini-tela de debug abrirá ao lado da janela ativa.

Alternativa por variável de ambiente:

1. Defina `MEUAPP_DEBUG_CONSOLE=1`
2. Execute `dotnet run`

### Desativar

- Para rodar sem a mini-tela: `dotnet run -- --no-debug-console`
- Durante a execução: `Ctrl+Shift+D` alterna a mini-tela ligada/desligada

### Ações rápidas

- `Ctrl+D`: abre o arquivo de log bruto
- Campo de filtro: localiza endpoint, método, status ou texto específico
- `Pausar`: congela a tela e acumula eventos em buffer
- `Auto-scroll`: acompanha a última linha automaticamente
- `Copiar`: copia o conteúdo visível da console

### Caminhos de log

- Log principal em tempo real: `logs\AppDebug.log` dentro da pasta de saída
- Log de erros de startup/UI: `MeuApp_Errors.log` dentro da pasta de saída

## O Que Procurar nos Logs

### ✅ Busca Funcionando
```
[14:30:15.234] === BUSCA INICIADA ===
[14:30:15.235] Query do usuário: 'joelyson'
[14:30:15.236] ID Token disponível: True
[14:30:15.237] Perfil do usuário: Seu Nome
[14:30:15.250] UserSearchService criado
[14:30:15.300] [SearchAllUsersAsync] Iniciando GET em: https://firestore.googleapis.com/...
[14:30:15.350] [SearchAllUsersAsync] Status HTTP: 200
[14:30:15.400] [SearchAllUsersAsync] Tamanho da resposta: 5234 caracteres
[14:30:15.450] [SearchAllUsersAsync] Encontrada propriedade 'documents'
[14:30:15.500] [SearchAllUsersAsync] Usuário #1: joelyson Alcantara da silva (2025.32.4217)
[14:30:15.510] [SearchAllUsersAsync] ✓ MATCH! Adicionando: joelyson Alcantara da silva
[14:30:15.520] Busca concluída. Resultados: 1
[14:30:15.530] === BUSCA FINALIZADA ===
```

### ❌ Problemas Possíveis

#### 1. **Token Vazio**
```
[14:30:15.235] ID Token disponível: False
[14:30:15.236] Busca cancelada: token vazio
```
**Solução**: Faça logout e login novamente.

#### 2. **Erro HTTP 401 (Não Autorizado)**
```
[14:30:15.350] [SearchAllUsersAsync] Status HTTP: 401
[14:30:15.355] [SearchAllUsersAsync] ERRO: Resposta sem sucesso (401)
```
**Solução**: O token pode estar expirado. Faça login novamente.

#### 3. **Erro HTTP 403 (Acesso Negado)**
```
[14:30:15.350] [SearchAllUsersAsync] Status HTTP: 403
```
**Solução**: Verifique as permissões de segurança do Firestore.

#### 4. **Nenhum Documento no Firestore**
```
[14:30:15.450] [SearchAllUsersAsync] Encontrada propriedade 'documents'
[14:30:15.500] [SearchAllUsersAsync] Total de documentos processados: 0
[14:30:15.510] Retornando 0 resultados
```
**Solução**: Os usuários não foram salvos no Firestore. Verifique se o cadastro foi bem-sucedido.

#### 5. **Nenhum Match com a Query**
```
[14:30:15.500] [SearchAllUsersAsync] Usuário #1: joelyson Alcantara da silva (2025.32.4217)
[14:30:15.510] [SearchAllUsersAsync] ✗ Sem match. Nome: 'joelyson alcantara da silva' | Email: '...' | Reg: '2025.32.4217' | Query: 'xyz'
[14:30:15.520] Retornando 0 resultados
```
**Solução**: A busca está funcionando, mas os dados não correspondem à query.

## Próximos Passos com o Log

1. **Copie o conteúdo do arquivo** de log
2. **Envie para diagnóstico** ou compartilhe com o suporte
3. O log mostrará:
   - Se o token está válido
   - Se o Firestore está respondendo
   - Quantos usuários estão salvos
   - Por que um usuário não está sendo encontrado

## Checklist de Verificação

Ao abrir o arquivo de log, procure por (em ordem):

- [ ] `ID Token disponível: True` - Token está OK?
- [ ] `Status HTTP: 200` - Firestore está respondendo?
- [ ] `Encontrada propriedade 'documents'` - Dados estão sendo retornados?
- [ ] `Total de documentos processados: X` - Quantos usuários existem (deve ser >= 2)?
- [ ] `Usuário #1:` - Os nomes estão aparecendo corretamente?
- [ ] `✓ MATCH!` - A busca encontrou uma correspondência?

## Informações dos Usuários de Teste

Conforme fornecido:

**Usuário 1:**
- Nome: joelyson Alcantara da silva
- Matrícula: 2025.32.4217
- Email: joelysomalcantaradasilva@gmail.com

**Usuário 2:**
- Nome: Pedro Lucas de Souza Pessoa
- Matrícula: 2025.32.4218
- Email: pedrolucasdesouzapessoa@gmail.com

Tente buscar por qualquer um desses dados.

## Dicas de Teste

```
Teste estas buscas (em qualquer ordem):

1. "joelyson" - buscar por nome parcial
2. "Alcantara" - buscar por sobrenome
3. "2025.32.4217" - buscar por matrícula exata
4. "joelysomalcantaradasilva@gmail.com" - buscar por email
5. "Pedro" - buscar por outro usuário
6. "2025.32" - buscar por matrícula parcial
```

Cada busca gerará um novo log que pode ser consultado.

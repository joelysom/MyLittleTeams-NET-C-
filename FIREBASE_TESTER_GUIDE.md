# 🧪 Testador de Conexão Firebase - Guia de Uso

## O Que É?

Um novo **botão de teste Firebase** foi adicionado à aplicação que diagnostica automaticamente:
- ✅ Se o token de autenticação está válido
- ✅ Se consegue conectar ao Firestore
- ✅ Quantos usuários estão salvos
- ✅ Se consegue extrair dados corretamente

## Como Usar

### Passo 1: Abrir a Aplicação
1. Compile e execute a aplicação normalmente
2. Faça login com suas credenciais

### Passo 2: Localizar o Botão
- Na **barra de ferramentas superior**
- Você verá um botão com **🧪** (Teste)
- Fica entre a barra de busca e as configurações

### Passo 3: Clicar para Testar
1. Clique no botão **🧪**
2. Aguarde a conclusão do teste (o botão mostrará **⏳** enquanto testa)
3. Uma janela com o resultado aparecerá

## Possíveis Resultados

### ✅ TUDO OK!
```
✅ Token válido: True
✅ Conexão Firestore: OK
✅ Documentos encontrados: 2

✅ TUDO OK! Sistema pronto para buscar.
```

**Ação**: A busca deve funcionar normalmente. Tente buscar "joelyson" ou qualquer matrícula.

---

### ❌ Token está vazio/nulo
```
✅ Token válido: False

❌ ERROS ENCONTRADOS:
  ❌ Token está vazio ou nulo!
```

**Causa**: Não fez login ou o token expirou  
**Solução**: Faça logout e login novamente

---

### ❌ Erro 401 (Não Autorizado)
```
✅ Token válido: True
✅ Conexão Firestore: FALHOU

❌ ERROS ENCONTRADOS:
  ❌ Conexão Firestore falhou: HTTP 401: {"error": ...}
```

**Causa**: Token expirado ou inválido  
**Solução**: Logout → Login novamente

---

### ❌ Erro 403 (Acesso Negado)
```
✅ Token válido: True
✅ Conexão Firestore: FALHOU

❌ ERROS ENCONTRADOS:
  ❌ Conexão Firestore falhou: HTTP 403: ...
```

**Causa**: Permissões insuficientes no Firestore  
**Solução**: Verifique as regras de segurança do Firestore

---

### ⚠️ Nenhum Documento Encontrado
```
✅ Token válido: True
✅ Conexão Firestore: OK
✅ Documentos encontrados: 0

❌ ERROS ENCONTRADOS:
  ⚠️ Nenhum documento encontrado no Firestore
```

**Causa**: Collection "users" está vazia ou não foi criada  
**Solução**: Cadastre novos usuários ou importe dados

---

### ❌ Erro ao Extrair Dados
```
✅ Token válido: True
✅ Conexão Firestore: OK
✅ Documentos encontrados: 2
✅ Extraction de documento: FALHOU

❌ ERROS ENCONTRADOS:
  ❌ Erro ao extrair documento: Campo 'fields' não encontrado...
```

**Causa**: Formato dos dados está diferente do esperado  
**Solução**: Compartilhe este resultado comigo para análise

---

## Combinação com Modo Debug

Para diagnóstico mais detalhado, combine o teste com o modo debug:

1. **Pressione Ctrl+D** para ativar logging
2. **Clique em 🧪** para testar Firebase
3. **Pressione Ctrl+D novamente** para abrir o log
4. Procure por `[TEST 1]`, `[TEST 2]`, `[TEST 3]`, `[TEST 4]`
5. Compartilhe o arquivo `/Desktop/AppDebug.log` se houver erro

## Fluxo de Diagnóstico Recomendado

```
1. Teste Firebase (🧪)
   ↓
   Sucesso? → Continue testando busca
   Erro? → Vá para passo 2
   
2. Ative Debug (Ctrl+D)
   
3. Teste Firebase novamente (🧪)
   
4. Abra Log (Ctrl+D)
   
5. Procure por [TEST X] para entender o erro
   
6. Execute busca e análise resultados
```

## Dicas

- 🔄 **Teste múltiplas vezes**: Se falhar uma vez, tente novamente
- 📱 **Verifique conexão de rede**: Se tiver erro de conexão
- ⏰ **Aguarde o resultado**: Não clique novamente enquanto testa
- 📋 **Compartilhe o resultado**: Se tiver erro, compartilhe a mensagem
- 🗂️ **Verifique o Firestore**: Confirme que os dados estão lá

## O Que Você Vai Aprender

Depois do teste, você saberá:
- ✅ Se o Firebase está acessível
- ✅ Se o token de autenticação é válido
- ✅ Quantos usuários existem
- ✅ Se o formato dos dados é correto
- ✅ Por que a busca pode não estar funcionando

## Próximos Passos Após Teste OK

Se o teste disser **✅ TUDO OK!**:

1. Tente fazer uma busca:
   - Digite "joelyson" na barra de busca
   - Clique em 🔍
   - Deveria ver o usuário na lista

2. Se ainda não encontrar:
   - Ative debug (Ctrl+D)
   - Faça busca
   - Abra log (Ctrl+D)
   - Compartilhe o resultado comigo

## Informações de Teste

```
Usuários cadastrados:
1. joelyson Alcantara da silva / 2025.32.4217
2. Pedro Lucas de Souza Pessoa / 2025.32.4218
```

Tente buscar por qualquer um desses dados.

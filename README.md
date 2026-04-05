# Obsseract

Aplicacao desktop WPF para acompanhamento de projetos integradores de alunos e professores orientadores, com autenticacao Firebase, busca global, conexoes academicas, chat, workspaces de equipe, agenda consolidada, dashboard docente, perfil profissional e central local de arquivos.

O projeto roda em Windows com .NET 8 e usa uma shell principal em `MainWindow` para concentrar os modulos operacionais do aluno. Hoje ele cobre tanto fluxo individual de apresentacao profissional quanto colaboracao academica entre equipes.

## Sumario

- [Visao Geral](#visao-geral)
- [Estado Atual do Projeto](#estado-atual-do-projeto)
- [Funcionalidades](#funcionalidades)
- [Arquitetura Tecnica](#arquitetura-tecnica)
- [Mapa de Arquivos e Modulos](#mapa-de-arquivos-e-modulos)
- [Persistencia e Integracoes](#persistencia-e-integracoes)
- [Dependencias](#dependencias)
- [Como Executar Localmente](#como-executar-localmente)
- [Fluxos Principais](#fluxos-principais)
- [Diagnostico e Observabilidade](#diagnostico-e-observabilidade)
- [Documentacao Complementar](#documentacao-complementar)
- [Limitacoes e Atencoes](#limitacoes-e-atencoes)
- [Roadmap Sugerido](#roadmap-sugerido)

## Visao Geral

O Obsseract foi desenhado para apoiar o ciclo de vida de projetos integradores dentro de contexto academico. A proposta atual do app e combinar, dentro de uma unica experiencia:

- autenticacao e bootstrap de perfil do aluno;
- busca de colegas e criacao de conexoes academicas;
- chat individual com historico persistido no Firestore;
- criacao e manutencao de equipes de projeto;
- board de tarefas em estilos Trello, Kanban e CSD;
- milestones, prazo principal e radar de entregas;
- agenda consolidada com leitura executiva do semestre;
- perfil profissional com avatar, bio, galeria e projetos em destaque;
- central local de arquivos com preview interno e assistente visual.

Em termos de dominio, o app esta mais proximo de um ambiente de colaboracao academica do que de um simples mensageiro. A experiencia atual mistura networking entre alunos, organizacao de times, visao de entregas e vitrine de perfil.

## Estado Atual do Projeto

- Plataforma: `net8.0-windows` com `UseWPF=true`.
- Interface principal: shell lateral em `MainWindow` com modulos de Chats, Conexoes, Equipes, Docencia, Calendario, Arquivos e Configuracoes.
- Persistencia remota: Firebase Authentication + Firestore via chamadas HTTP diretas.
- Persistencia local adicional: area de Arquivos em `%LocalAppData%\Obsseract\FilesHub\{userId}`.
- Configuracao remota: `AppConfig.cs` centraliza `FirebaseApiKey`, `FirebaseProjectId` e URLs do Firestore com suporte a `appsettings.local.json` e variaveis de ambiente.
- Status de compilacao desta base: `dotnet build MeuApp.csproj` concluido com `0 Warning(s)` e `0 Error(s)`.
- Status de testes automatizados: `dotnet test MeuApp.Tests/MeuApp.Tests.csproj` concluido com `6` testes aprovados.
- Regras de seguranca do Firestore: arquivo raiz `firestore.rules`.
- Logs de debug: `logs\AppDebug.log` sob a pasta de saida da aplicacao.
- Log de erros de startup/UI: `MeuApp_Errors.log` na pasta de saida.

## Funcionalidades

### 1. Autenticacao e onboarding

- Login e cadastro com Firebase Authentication.
- Cadastro com papel explicito de aluno ou professor orientador.
- Bootstrap de perfil local logo apos autenticacao, com hidratacao posterior via Firestore.
- Persistencia de papel, departamento academico, foco de orientacao e janela de atendimento no documento `users/{userId}`.
- Suporte a recuperacao de fluxo quando perfil remoto ainda nao foi totalmente carregado.
- Handlers globais de excecao na app para evitar falhas silenciosas de startup/UI.

### 2. Busca de usuarios e conexoes

- Busca global por nome, matricula, email, equipe, conversa e materiais do hub local.
- Cria solicitacoes de conexao entre alunos.
- Mantem estados de conexao como `pendingIncoming`, `pendingOutgoing` e `connected`.
- Mostra uma aba dedicada para solicitacoes recebidas, convites enviados, notificacoes e rede ativa.

### 3. Chat e conversas

- Conversas privadas persistidas em `conversations/{conversationId}`.
- Mensagens em `conversations/{conversationId}/messages/{messageId}`.
- Suporte a mensagem de texto, sticker, edicao e exclusao logica.
- Resumo de conversa atualizado com ultima mensagem, remetente e data.
- Existe uma janela dedicada `ChatWindow` para experiencia de chat isolada, com fallback simulado em cenarios sem historico remoto.

### 4. Equipes e workspace de projeto

- Criacao de equipes com curso, turma, UCs e membros.
- Criacao de equipes com semestre letivo, template academico e papel de cada participante.
- Persistencia em `teams` com referencias auxiliares em `userTeams` para carregamento por usuario.
- Workspace com indicadores de membros, tarefas, progresso, atrasos e entregas.
- Templates por curso/disciplina via `AcademicProjectTemplateCatalog` para preencher milestones, timeline e orientacao inicial.
- Camada de permissoes por papel via `TeamPermissionService` com diferenca entre aluno, lider e professor orientador.
- Views de board em tres estilos:
  - Trello
  - Kanban
  - CSD (Certainties, Assumptions, Doubts)
- Gestao de tarefas com prioridade, vencimento, responsaveis, horas estimadas, pontos de carga, papel recomendado e revisao docente.
- Gestao de milestones/entregas do projeto com mencoes, comentarios, anexos e sinalizacao de revisao do professor.
- Gestao de prazo principal e status do projeto.
- Timeline academica por semestre, leitura de carga por membro e brief automatico de apoio ao contexto academico.
- Notificacoes internas da equipe para movimentacoes relevantes.
- Upload de materiais da equipe, incluindo logo, imagens, documentos e planos, com escopo de permissao, versao e metadados de sincronizacao.

### 5. Dashboard docente

- Painel dedicado para professor orientador acompanhar varias equipes ao mesmo tempo.
- Agrupa equipes por curso e turma.
- Usa `AcademicRiskEngine` para medir risco, carga, atrasos, marcos pendentes e recomendacao automatica.
- Permite saltar direto para agenda filtrada ou abrir o workspace da equipe em foco.

### 6. Agenda integradora

- Painel consolidado no modulo `Calendario`.
- Junta prazo principal do projeto, milestones abertas e tarefas com vencimento.
- Junta tambem itens da timeline academica do semestre.
- Exibe foco imediato, janela dos proximos 7 dias, radar por equipe, atividade recente e leitura rapida do semestre.
- Filtros por equipe, tipo, status e janela de datas.
- Exportacao semanal em Excel (`.xlsx`) e PDF direto da agenda filtrada.
- Acoes rapidas da agenda permitem, sem sair do calendario:
  - criar nova entrega;
  - ajustar prazo principal do projeto;
  - criar nova tarefa do board.
- Cards da agenda podem abrir a equipe correspondente diretamente.

### 7. Perfil profissional do aluno

- Avatar por camadas com corpo, cabelo, acessorio e roupas.
- Nickname, titulo profissional, bio, habilidades e linguagens.
- Papel academico, departamento, foco academico e janela de atendimento.
- Links externos como portfolio e LinkedIn.
- Galeria profissional com dois modos:
  - imagens independentes;
  - galerias em bloco por evento/albuns.
- Titulo e descricao por imagem ou por album.
- Projetos em destaque carregados a partir das equipes visiveis ao usuario.
- Tela publica de perfil com viewer somente leitura.

### 8. Viewer de imagens e galeria

- Janela dedicada para visualizar imagem ou sequencia de slides.
- Modo proprietario com ajuste de enquadramento.
- Modo somente leitura para visualizacao publica.
- Painel lateral com contexto, descricao, zoom, contador e trilha de navegacao da galeria.
- Suporte a albuns/eventos dentro da galeria profissional.

### 9. Arquivos e CHOAS

- Hub local de arquivos com classificacao por contexto: projeto, sessao, trabalho e atividade.
- Persistencia local por usuario em `%LocalAppData%\Obsseract\FilesHub\{userId}`.
- Estrutura local do hub:
  - `state.json` para estado e metadados;
  - `archive\` para copias dos arquivos importados.
- Preview interno para:
  - imagens;
  - PDF via WebView2;
  - textos e codigo;
  - documentos Word, planilhas e apresentacoes via OpenXML.
- Mascote/assistente visual CHOAS dentro da experiencia de arquivos.

### 10. Debug, teste e apoio operacional

- Botao `🧪` no topo da app para diagnostico de conexao com Firebase.
- Atalho `Ctrl+D` para ativar logging e abrir o arquivo de log.
- Guias internos no repositorio para busca, equipes, Firebase e persistencia.
- Projeto `MeuApp.Tests` com cobertura automatizada da base academica nova: permissoes, risco, templates e helpers de configuracao.

## Arquitetura Tecnica

### Camadas principais

**Apresentacao (WPF / MahApps.Metro)**

- `LoginWindow` concentra autenticacao.
- `MainWindow` atua como shell e orquestrador de quase todo o produto.
- Janelas auxiliares cobrem chat dedicado, resultados de busca, perfil publico e viewer de galeria.

**Dominio / modelos**

- `UserProfile`, `UserInfo`, `Conversation`, `ChatMessage`, `TeamWorkspaceInfo` e modelos associados.
- O dominio de equipes cobre board, milestones, timeline do semestre, comentarios, anexos, materiais versionados, notificacoes e chat de projeto.

**Servicos de integracao**

- `ChatService`: mensagens e metadata de conversa.
- `ConnectionService`: solicitacoes e sincronizacao de conexoes.
- `TeamService`: persistencia de equipes e referencias por usuario.
- `UserSearchService`: leitura e filtro de usuarios no Firestore.
- `FirebaseConnectionTester`: diagnostico operacional.
- `AcademicRiskEngine`: leitura executiva de risco, atraso e carga por membro/equipe.
- `AcademicProjectTemplateCatalog`: modelos academicos por curso/disciplina.
- `TeamPermissionService`: normalizacao de papeis e permissoes do workspace.
- `AppConfig`: configuracao central do Firebase e construcao das URLs do Firestore.

**Persistencia**

- Firestore para usuarios, conversas, conexoes e equipes.
- sistema de arquivos local para o hub de Arquivos.

**Observabilidade**

- `DebugHelper` para trace em arquivo.
- `App.xaml.cs` para captura de excecoes de startup e UI.

### Padrões praticos adotados hoje

- Chamada HTTP direta ao Firestore, sem SDK oficial de cliente desktop.
- Shell unica de navegacao em `MainWindow`.
- Metodos de renderizacao dinamica em code-behind para construir trechos ricos de UI.
- Parte importante da logica de negocio tambem esta no code-behind, especialmente em `MainWindow.xaml.cs`.
- Uso misto de dados reais e fallbacks simulados em alguns modulos auxiliares, como `ChatWindow`.

## Mapa de Arquivos e Modulos

### Arquivos de aplicacao

| Arquivo | Responsabilidade |
|---|---|
| `App.xaml` | Recursos globais, fontes e estilos base da aplicacao. |
| `App.xaml.cs` | Startup, tratamento global de erros e log de falhas criticas. |
| `MeuApp.csproj` | Configuracao do projeto, dependencias NuGet, recursos e exclusoes de artefatos extras. |
| `AssemblyInfo.cs` | Metadados de assembly. |

### Janelas e telas

| Arquivo | Responsabilidade |
|---|---|
| `LoginWindow.xaml` / `LoginWindow.xaml.cs` | Login, cadastro, bootstrap de sessao e hidratacao inicial do perfil. |
| `MainWindow.xaml` / `MainWindow.xaml.cs` | Shell principal com chats, conexoes, equipes, calendario, arquivos e configuracoes. |
| `ChatWindow.xaml` / `ChatWindow.xaml.cs` | Janela dedicada de chat, com lista lateral e fallback simulado. |
| `SearchResultsWindow.xaml` / `SearchResultsWindow.xaml.cs` | Exibe resultados de busca, inicia conversa, cria conexao e abre perfil. |
| `UserProfileViewWindow.xaml` / `UserProfileViewWindow.xaml.cs` | Perfil publico/profissional do aluno com galeria e projetos destacados. |
| `GalleryImageViewerWindow.cs` | Viewer de imagens e slides para galeria profissional e projetos em destaque. |

### Dominio e modelos

| Arquivo | Responsabilidade |
|---|---|
| `ChatMessage.cs` | Modelo de mensagem com suporte a edicao, exclusao logica e stickers. |
| `Conversation.cs` | Modelo de conversa privada com preview e status de leitura. |
| `TeamModels.cs` | Modelos publicos de equipe, milestones, timeline, permissoes, comentarios, anexos, assets, board, notificacoes e chat do projeto. |

### Servicos

| Arquivo | Responsabilidade |
|---|---|
| `ChatService.cs` | Envio, leitura, edicao e exclusao logica de mensagens em Firestore. |
| `ConnectionService.cs` | Ciclo de vida das conexoes entre usuarios. |
| `TeamService.cs` | Criacao, atualizacao, carga e exclusao de equipes; sincroniza `teams` e `userTeams`. |
| `UserSearchService.cs` | Busca usuarios na colecao `users` e filtra localmente os matches. |
| `FirebaseConnectionTester.cs` | Testa token, conexao Firestore, contagem de documentos e amostra de dados. |
| `AcademicRiskEngine.cs` | Gera snapshots de risco, carga de membros e resumo para dashboard docente. |
| `AcademicProjectTemplateCatalog.cs` | Catalogo de templates de projeto por curso/disciplina. |
| `TeamPermissionService.cs` | Regras de papel e acesso dentro das equipes. |
| `AppConfig.cs` | Leitura de configuracao Firebase via arquivo local ou variavel de ambiente. |

### Utilitarios e suporte

| Arquivo | Responsabilidade |
|---|---|
| `DebugHelper.cs` | Logging em arquivo, rotacao simples por tamanho e abertura rapida do log. |
| `ErrorCapture.cs` | Utilitario comentado/arquivado para captura externa de erros. |
| `MockData.cs` | Dados auxiliares de apoio/teste na UI. |

### Configuracao e seguranca

| Arquivo | Responsabilidade |
|---|---|
| `firestore.rules` | Regras atuais de seguranca para `users`, `conversations`, `teams`, `connections`, `userTeams` e `userConnections`. |
| `appsettings.sample.json` | Exemplo de configuracao local do Firebase para desenvolvimento. |
| `FIREBASE_SECURITY_RULES.md` | Guia para publicar e validar as regras no Firestore. |

### Guias internos

| Arquivo | Assunto |
|---|---|
| `CHAT_DESIGN.md` | Contexto de design para a janela de chat dedicada. |
| `DEBUG_GUIDE.md` | Guia de debug da busca de usuarios. |
| `DEBUG_TEAMS_GUIDE.md` | Guia de debug da persistencia de equipes. |
| `FIREBASE_TESTER_GUIDE.md` | Uso do botao de teste Firebase. |
| `BUSCA_CORRIGIDA.md` | Historico e diagnostico da busca. |
| `SOLUTION_IMPLEMENTATION_SUMMARY.md` | Resumo executivo da persistencia de equipes. |
| `SOLUTION_TEAMS_PERSISTENCE.md` | Explicacao da estrategia de persistencia de equipes. |
| `TEST_TEAMS_PERSISTENCE.md` | Checklist de testes da persistencia de equipes. |

## Persistencia e Integracoes

### Firebase Authentication

- Login e cadastro de usuarios.
- Recuperacao de token e bootstrap de sessao.
- Token repassado para servicos HTTP que acessam o Firestore.

### Firestore

Colecoes e caminhos relevantes:

| Colecao | Uso principal |
|---|---|
| `users/{userId}` | Perfil profissional, papel academico, dados de orientacao, galeria e projetos destacados. |
| `conversations/{conversationId}` | Metadata da conversa privada. |
| `conversations/{conversationId}/messages/{messageId}` | Mensagens individuais. |
| `teams/{teamId}` | Workspace completo da equipe. |
| `userTeams/{userId}_{teamId}` | Referencia rapida de equipe por usuario. |
| `connections/{connectionId}` | Estado canonico da conexao entre dois usuarios. |
| `userConnections/{documentId}` | Caixa de entrada/saida operacional de conexoes por usuario. |

### Estrutura de equipe no Firestore

Uma equipe guarda, entre outros campos:

- identificacao da equipe (`teamId`, `teamName`, `course`, `className`, `classId`);
- metadados de controle (`createdBy`, `createdAt`, `updatedAt`, `isActive`);
- progresso (`projectProgress`, `projectDeadline`, `projectStatus`);
- membros e UCs;
- semestre, template e timestamp de sincronizacao;
- access rules por papel;
- timeline academica do semestre;
- milestones com comentarios, mencoes e anexos;
- assets com escopo, versao e historico;
- colunas e cards do board;
- notificacoes e chat do projeto;
- quadro CSD.

### Armazenamento local do hub de Arquivos

- Raiz: `%LocalAppData%\Obsseract\FilesHub\{userId}`
- Estado serializado: `state.json`
- Arquivos copiados/importados: `archive\`

Esse modulo e local ao dispositivo atual. Ele nao sincroniza automaticamente com Firebase nem com outros computadores.

## Dependencias

| Pacote | Papel no projeto |
|---|---|
| `MahApps.Metro` | Base visual das janelas Metro e componentes principais. |
| `MahApps.Metro.IconPacks` | Iconografia Material usada na shell e nos cards. |
| `FluentWPF` | Suporte adicional de estilo/efeitos na experiencia WPF. |
| `WpfAnimatedGif` | Animacao do mascote/ativos visuais como CHOAS. |
| `Microsoft.Web.WebView2` | Preview interno de PDF no hub de Arquivos. |
| `DocumentFormat.OpenXml` | Leitura interna de documentos Word, planilhas e apresentacoes. |

## Como Executar Localmente

### Requisitos

- Windows 10 ou 11.
- .NET 8 SDK instalado.
- Runtime do WebView2 instalado para preview interno de PDF.
- Acesso ao projeto Firebase configurado via arquivo local ou variavel de ambiente.
- Firestore ativo e com regras publicadas a partir de `firestore.rules`.

### Passos recomendados

1. Clone o repositorio.
2. Entre na pasta do projeto.
3. Publique as regras do arquivo `firestore.rules` no projeto Firebase correto.
4. Crie `appsettings.local.json` a partir de `appsettings.sample.json` ou configure:
  - `OBSSERACT_FIREBASE_API_KEY`
  - `OBSSERACT_FIREBASE_PROJECT_ID`
5. Restaure os pacotes:

```powershell
dotnet restore
```

6. Compile:

```powershell
dotnet build "MeuApp.csproj"
```

7. Execute:

```powershell
dotnet run
```

8. Execute os testes automatizados quando alterar a fundacao academica:

```powershell
dotnet test "MeuApp.Tests/MeuApp.Tests.csproj"
```

### Observacoes importantes de configuracao

- O projeto define `RestorePackagesPath` para `.nuget\packages` dentro do proprio repositorio.
- Imagens em `img\Archives\**\*` sao copiadas para a saida como `Content`.
- Pastas de artefato alternativas como `obj_codex`, `obj_verify`, `bin_verify` e projetos auxiliares como `MeuApp.Tests` sao excluidos explicitamente do projeto WPF principal para evitar duplicacao de compilacao.

## Fluxos Principais

### 1. Login e inicializacao

1. Usuario entra com email e senha.
2. `LoginWindow` autentica no Firebase.
3. `MainWindow` abre com perfil bootstrap.
4. O app hidrata o perfil remoto e inicializa servicos de equipes e conexoes.

### 2. Buscar colegas e criar conexoes

1. Usuario digita nome, email ou matricula.
2. `UserSearchService` consulta `users`.
3. `SearchResultsWindow` apresenta resultados.
4. O usuario pode iniciar conversa, abrir perfil ou enviar solicitacao de conexao.

### 3. Trabalhar no chat

1. Conversa e selecionada na shell principal ou em janela dedicada.
2. `ChatService` carrega historico do Firestore.
3. Mensagens podem ser enviadas, editadas ou apagadas logicamente.
4. A metadata da conversa e sincronizada com ultimo resumo.

### 4. Criar e operar uma equipe

1. Usuario cria a equipe com turma, curso, UCs e membros.
2. `TeamService` salva em `teams` e em `userTeams` para todos os membros.
3. O workspace passa a concentrar board, milestones, prazo, materiais e notificacoes.
4. O calendario consolida automaticamente os prazos encontrados.

### 5. Organizar entregas pelo calendario

1. O modulo `Calendario` agrega dados de equipes carregadas.
2. Cards de agenda mostram urgencias, timeline e radar de risco.
3. O usuario pode abrir equipe, criar entrega, ajustar prazo principal ou criar tarefa sem trocar de modulo.

### 6. Evoluir o perfil profissional

1. O aluno ajusta avatar, bio, linguagens, links e habilidades.
2. Pode publicar galeria profissional com fotos soltas ou albuns/eventos.
3. Seleciona projetos em destaque a partir das equipes disponiveis.
4. O perfil publico mostra galeria, projetos e links em modo somente leitura.

### 7. Gerenciar materiais no hub de Arquivos

1. Usuario adiciona um ou varios arquivos.
2. O app arquiva uma copia local no hub do usuario.
3. O usuario classifica o item por contexto.
4. O preview interno tenta renderizar imagem, PDF, texto ou documentos Office.

## Diagnostico e Observabilidade

### Logging

- `Ctrl+D` ativa o modo de diagnostico ou abre o log existente.
- Caminho real do log de debug no codigo atual:

```text
<pasta de saida da aplicacao>\logs\AppDebug.log
```

- Caminho real do log de erros gerais:

```text
<pasta de saida da aplicacao>\MeuApp_Errors.log
```

### Testador de Firebase

- Ha um botao `🧪` na barra superior da aplicacao.
- Ele valida:
  - token;
  - conexao com Firestore;
  - contagem de documentos;
  - leitura de amostra de documento.

### Observacao importante sobre documentos antigos

Alguns guias historicos da raiz ainda citam `Desktop\AppDebug.log` como destino do log. O codigo atual em `DebugHelper.cs` grava em `logs\AppDebug.log` dentro da pasta de saida da aplicacao. Se houver divergencia entre um guia antigo e o comportamento atual, confie primeiro no codigo.

## Documentacao Complementar

- [CHAT_DESIGN.md](CHAT_DESIGN.md)
- [DEBUG_GUIDE.md](DEBUG_GUIDE.md)
- [DEBUG_TEAMS_GUIDE.md](DEBUG_TEAMS_GUIDE.md)
- [FIREBASE_SECURITY_RULES.md](FIREBASE_SECURITY_RULES.md)
- [FIREBASE_TESTER_GUIDE.md](FIREBASE_TESTER_GUIDE.md)
- [BUSCA_CORRIGIDA.md](BUSCA_CORRIGIDA.md)
- [SOLUTION_IMPLEMENTATION_SUMMARY.md](SOLUTION_IMPLEMENTATION_SUMMARY.md)
- [SOLUTION_TEAMS_PERSISTENCE.md](SOLUTION_TEAMS_PERSISTENCE.md)
- [TEST_TEAMS_PERSISTENCE.md](TEST_TEAMS_PERSISTENCE.md)

Esses arquivos registram historico de implementacao, testes e debug. O README serve como visao consolidada; os guias acima aprofundam partes especificas.

## Limitacoes e Atencoes

- O projeto e Windows-only por depender de WPF.
- O acesso a Firebase esta configurado por constantes no codigo. Para uso real em equipe ou publicacao, vale externalizar configuracao e segredos operacionais.
- O modulo de Arquivos e local ao dispositivo; ele nao substitui um armazenamento em nuvem sincronizado.
- O preview de PDF depende do WebView2 estar instalado e funcional no Windows.
- Parte importante da aplicacao ainda esta concentrada em code-behind extenso, especialmente em `MainWindow.xaml.cs`.
- Alguns documentos da raiz sao historicos e podem descrever etapas de debug ja superadas ou parcialmente divergentes do estado atual.
- `ChatWindow` possui fallback simulado quando nao ha historico remoto disponivel; a shell principal e a fonte mais rica da experiencia atual.
- O Firestore precisa estar alinhado com as regras de `firestore.rules`; divergencias nas regras podem quebrar silenciosamente fluxo de equipe, conexoes ou mensagens.

## Roadmap Sugerido

- Externalizar configuracao de Firebase e ambiente.
- Separar `MainWindow.xaml.cs` em modulos menores ou ViewModels por dominio.
- Adicionar sincronizacao em tempo real com listeners ou polling estruturado.
- Criar testes automatizados para servicos e mapeamentos Firestore.
- Evoluir o calendario para filtros por equipe, semana, risco e tipo de entrega.
- Sincronizar o hub de Arquivos com nuvem e permissao por equipe.
- Adicionar dashboards para professor/orientador acompanhar varias equipes.
- Criar exportacao de relatorios e snapshots de board/agenda.
- Fortalecer offline-first para leitura de dados essenciais.
- Padronizar e revisar toda a documentacao historica da raiz para remover divergencias antigas.

---

## Resumo rapido

Se voce precisa entender o projeto em pouco tempo, este e o mapa minimo:

1. `LoginWindow` autentica e abre a shell.
2. `MainWindow` concentra o produto inteiro.
3. `TeamService`, `ChatService`, `ConnectionService` e `UserSearchService` fazem a ponte com Firestore.
4. `TeamModels` descreve o dominio de projeto integrador.
5. `firestore.rules` e obrigatorio para alinhar seguranca com o codigo.
6. `Ctrl+D` e o botao `🧪` sao os atalhos principais para diagnostico.

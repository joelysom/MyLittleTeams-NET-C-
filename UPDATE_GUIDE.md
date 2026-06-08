# Atualizacoes automaticas

O Choas agora verifica atualizacoes no GitHub Releases durante a inicializacao. Quando encontra uma release com versao maior que a versao instalada, ele mostra um aviso, baixa o pacote e inicia a atualizacao.

## Como publicar uma nova versao

1. Atualize a versao em `MeuApp.csproj`, por exemplo de `1.0.0` para `1.0.1`.
2. Envie as alteracoes para o GitHub.
3. Crie e envie uma tag com a mesma versao:

```powershell
git tag v1.0.1
git push origin v1.0.1
```

O workflow `.github/workflows/release.yml` cria uma GitHub Release e anexa um pacote `Choas-v1.0.1-win-x64.zip`. Esse `.zip` e o formato recomendado para atualizacao, porque contem todos os arquivos publicados do app.

## Pacotes aceitos

O updater procura assets anexados na release e aceita:

- `.msi`
- `.exe` com nome contendo `setup`, `installer` ou `install`
- `.zip` com a pasta publicada do aplicativo
- `.exe` portatil de arquivo unico

Se houver mais de um asset, instaladores tem prioridade, depois `.zip`, depois `.exe` portatil.

## Configuracao

Por padrao o app consulta:

```json
{
  "Updater": {
    "Enabled": true,
    "GitHubOwner": "joelysom",
    "GitHubRepository": "MyLittleTeams-NET-C-",
    "IncludePrereleases": false
  }
}
```

Voce pode sobrescrever isso em `appsettings.local.json` ou pelas variaveis:

- `CHOAS_UPDATER_ENABLED`
- `CHOAS_GITHUB_OWNER`
- `CHOAS_GITHUB_REPOSITORY`
- `CHOAS_UPDATER_INCLUDE_PRERELEASES`

## Observacao importante

Um commit novo no GitHub nao gera atualizacao sozinho. O app so notifica quando existe uma GitHub Release com tag maior que a versao instalada, por exemplo `v1.0.1`, e com um pacote anexado.

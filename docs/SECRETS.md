# Configuração de segredos e variáveis de ambiente

Para não expor senhas e chaves no repositório, a aplicação utiliza **variáveis de ambiente** e, quando aplicável, **Kubernetes Secrets**. Nenhuma senha ou chave secreta é commitada no Git.

**Como rodar tudo agora (local):** O arquivo **`.env.example`** já contém as **mesmas senhas de desenvolvimento** que o projeto usava antes (adminpassword, guest, etc.). Basta copiar para `.env` e rodar — não precisa inventar ou alterar nada. O arquivo `.env` não é commitado (está no `.gitignore`).

## Desenvolvimento local e Docker Compose

1. **Copie o arquivo de exemplo para `.env`** na raiz do projeto (só uma vez):
   ```bash
   cp .env.example .env
   ```

2. **Opcional:** edite o `.env` se quiser mudar alguma senha. Para rodar local com os valores que já existiam, não é necessário alterar nada.

3. O `.gitignore` já contém `.env` e `.env.*`, então o arquivo `.env` não será enviado ao GitHub.

4. **Docker Compose**: ao rodar `docker-compose up`, o Compose carrega automaticamente o `.env` da raiz. Assim as senhas vêm do seu `.env` local, não do repositório.

## Rodando os serviços .NET localmente (sem Docker)

Defina as variáveis de ambiente antes de rodar os projetos, ou use **User Secrets** (recomendado no Visual Studio):

```bash
# Exemplo no PowerShell (Windows)
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=agro_identity_db;Username=admin;Password=SUA_SENHA"
$env:JwtSettings__SecretKey = "SUA_CHAVE_JWT"
```

Ou crie um arquivo `.env` na raiz e use uma extensão/ferramenta que carrega `.env` antes de rodar (por exemplo, `dotenv` em scripts).

## Kubernetes

No Kubernetes as senhas não ficam nos YAMLs do repositório. Use um **Secret** criado a partir do seu ambiente:

1. **Criar o Secret a partir do `.env`** (rode na máquina onde você tem o `.env` configurado):
   ```bash
   kubectl create secret generic agro-secrets --from-env-file=.env -n default
   ```

2. Os manifests em `k8s/apps/` e `k8s/infra/` estão configurados para usar o secret `agro-secrets` quando existir. Os arquivos `*-secret.yaml` são **templates**: não contêm valores reais; o secret é criado com `kubectl ... --from-env-file=.env` ou preenchido manualmente.

3. **Postgres e Grafana** no K8s também podem receber variáveis do mesmo secret ou de um secret específico; veja os comentários nos YAMLs em `k8s/infra/`.

# Grafana – datasource PostgreSQL

O datasource do Grafana (provisioning) não contém senha no repositório (`password: ""` no YAML). Configure a senha **uma vez** pela UI do Grafana:

1. Acesse o Grafana (ex.: http://localhost:3000).
2. Login com o usuário/senha definidos em `GF_SECURITY_ADMIN_*` no seu `.env`.
3. Vá em **Configuration** → **Data sources** → **PostgreSQL (Telemetria/Alertas)** → **Save & test**.
4. Informe a senha do Postgres (a mesma do seu `.env`, ex.: `POSTGRES_PASSWORD`) e salve.

Assim o repositório continua sem senhas e o datasource funciona após essa configuração manual.

## Resumo

| Onde              | O que fazer |
|-------------------|-------------|
| Repositório (Git) | Apenas `.env.example` (sem valores reais). Nunca commitar `.env`. |
| Máquina local     | Copiar `.env.example` → `.env` e preencher. |
| Docker Compose    | Usar `.env` na raiz; o Compose lê as variáveis automaticamente. |
| Kubernetes        | Criar Secret com `kubectl create secret generic agro-secrets --from-env-file=.env`. |
| appsettings.json  | Não contém senhas; valores vêm de variáveis de ambiente. |

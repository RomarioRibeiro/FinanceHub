# Diretrizes do Repositorio

## Estrutura e Arquitetura

`FinanceHub.sln` e o ponto de entrada da solucao. A aplicacao MVC ASP.NET Core (.NET 8) esta em `FinanceHub/`.

- `Controllers/`: recebem requisicoes MVC e devem permanecer finos, delegando regras aos servicos.
- `Models/`: entidades de dominio, enums, excecoes e view models; o dashboard usa `HomeDashboardViewModel`.
- `Services/`: regras de negocio, escopo do usuario atual, autenticacao, calculos e processamento de recorrencias.
- `Repositories/` e `Data/`: EF Core, repositorio generico, unit of work, `FinanceHubContext` e seed de desenvolvimento.
- `ModelBinders/`: contem o binder decimal para valores monetarios em pt-BR.
- `Views/`: Razor views por controller; layouts em `Views/Shared/`.
- `wwwroot/`: CSS, JavaScript e dependencias estaticas.
- `Migrations/`: historico de schema do EF Core.
- `Tests/`: reservado para um futuro projeto de testes; nao ha projeto de testes conectado a solucao.

Os modulos atuais sao usuarios e perfis, categorias, contas/saldos, transacoes, lancamentos recorrentes, metas, relatorios e dashboard.

## Regras de Dominio

- Dados financeiros pertencem ao usuario autenticado. Preserve o filtro do `UsuarioAtualService` em leituras e alteracoes.
- Categorias definem receita ou despesa. Nao aceite o tipo financeiro enviado pela tela como fonte de verdade.
- Inclusao, edicao e exclusao de transacoes devem manter `Conta.SaldoAtual` consistente e usar `IUnitOfWork` para atualizar saldo e lancamento atomicamente.
- Uma transacao so pode usar conta ativa do usuario atual. Recorrencias devem validar conta, categoria e estado ativo antes de gerar transacoes.
- Valores monetarios devem ser `decimal` e usar `DecimalModelBinder`; falhas de regra usam `RegraNegocioException`.

## Desenvolvimento e Validacao

- `dotnet build .\FinanceHub.sln`: compila a solucao.
- `dotnet run --project .\FinanceHub\FinanceHub.csproj`: executa a aplicacao.
- `dotnet watch run --project .\FinanceHub\FinanceHub.csproj`: executa com hot reload.
- `dotnet ef database update --project .\FinanceHub\FinanceHub.csproj --startup-project .\FinanceHub\FinanceHub.csproj`: aplica migrations.
- `dotnet user-secrets set "ConnectionStrings:FinanceHubContext" "<connection-string>" --project .\FinanceHub\FinanceHub.csproj`: configura o banco local sem versionar credenciais.
- `dotnet publish .\FinanceHub\FinanceHub.csproj -c Release -o .\.codex-build\publish`: gera a publicacao para IIS.

O banco e SQL Server. Em ambientes fora de desenvolvimento, forneca a connection string por `ConnectionStrings__FinanceHubContext`. Para IIS, instale o ASP.NET Core Hosting Bundle do .NET 8, use um Application Pool com `No Managed Code`, configure essa variavel no servidor e reinicie o pool. HTTPS e certificado pertencem ao binding do IIS. Em desenvolvimento, o seed aplica migrations e cria dados de exemplo; nao trate esses dados como credenciais de producao.

Nao ha testes automatizados. Execute o build, valide manualmente o fluxo MVC afetado e valide migrations quando houver alteracao de schema. Ao adicionar testes, crie um projeto em `Tests/` e nomeie arquivos pelo alvo, por exemplo `TransacaoServiceTests.cs`.

## Estilo e Seguranca

Use quatro espacos, nullable habilitado, `PascalCase` para tipos/membros/pastas de views e `camelCase` para locais e parametros. Sufixe servicos, repositorios e controllers com `Service`, `Repository` e `Controller`.

A aplicacao usa cookie, politica global de usuario autenticado e cultura `pt-BR`. Preserve antiforgery nas alteracoes e mantenha apenas Login e AcessoNegado anonimos. Os CRUDs de Usuarios, Perfis e Categorias sao exclusivos do papel `Administrador`; o menu pode ocultar links, mas a autorizacao do controller e obrigatoria. O POST de login deve manter o rate limit de cinco tentativas por IP a cada 15 minutos.

Nunca versione segredos. Use User Secrets localmente e variaveis de ambiente na hospedagem.

## Commits e Pull Requests

Use commits curtos e imperativos em portugues, uma mudanca logica por commit, por exemplo `Ajusta recorrencias, saldos e transacoes`.

Em pull requests, informe mudanca funcional, telas ou modulos afetados, migrations necessarias, capturas visuais e validacoes manuais executadas.

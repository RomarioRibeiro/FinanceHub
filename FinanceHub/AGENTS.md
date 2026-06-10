# FinanceHub

## Sobre o Projeto

Sistema de controle financeiro pessoal desenvolvido em C# ASP.NET MVC.

## Tecnologias

- C#
- ASP.NET MVC 5
- Entity Framework
- SQL Server
- Bootstrap
- Razor

## Arquitetura

O projeto utiliza:

- Controllers
- Services
- Repositories
- Entities
- ViewModels

## Regras

- Sempre utilizar Repository Pattern.
- Não acessar Entity Framework diretamente nos Controllers.
- Toda regra de negócio deve ficar nas Entities ou Services.
- Seguir princípios SOLID.
- Explicar alterações antes de modificar código.

## Quando analisar uma entidade

Explique:

- Responsabilidade
- Relacionamentos
- Métodos
- Regras de negócio


## Modo de Explicação

Sempre que eu pedir uma alteração:

1. Explique o problema.
2. Explique a solução.
3. Mostre o código.
4. Explique linha por linha.
5. Informe quais conceitos de C# ou arquitetura estão sendo utilizados.

## Objetivo

Ajudar o desenvolvedor a aprender arquitetura, boas práticas e entender as regras de negócio antes de implementar alterações.



Quero que você crie o restante das entidades do domínio financeiro seguindo a arquitetura atual do projeto.

Antes de criar os arquivos, me apresente a proposta contendo:
1. Lista das entidades que serão criadas
2. Responsabilidade de cada entidade
3. Relacionamentos entre elas
4. Propriedades de cada entidade
5. Métodos de regra de negócio
6. Impacto no banco de dados
7. Onde cada arquivo será criado

Considere inicialmente estas entidades:

- Categoria
- Conta
- Receita
- Despesa
- LancamentoFinanceiro
- TipoLancamento
- FormaPagamento
- Usuario
- MetaFinanceira
- Orçamento
- Transferencia

Regras importantes:

- Não altere código ainda.
- Primeiro explique a modelagem.
- Depois aguarde minha aprovação.
- Use C# ASP.NET MVC com Entity Framework.
- Siga Repository Pattern e Service Layer.
- Não coloque regra de negócio no Controller.
- Crie entidades com validações básicas.
- Utilize DataAnnotations quando necessário.
- Crie relacionamentos corretamente.
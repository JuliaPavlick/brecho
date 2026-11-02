# brecho
Este projeto consiste no desenvolvimento de um sistema web de brechó online, criado para organizar e facilitar a venda de roupas usadas em uma plataforma digital. O sistema permite que diferentes tipos de usuários utilizem o site de acordo com suas funções dentro da plataforma.  Cada pessoa possui um único cadastro no sistema e pode assumir diferentes papéis, como cliente, vendedora externa ou funcionária. Cada um desses papéis possui permissões e funcionalidades específicas.  Os clientes podem navegar pelos produtos disponíveis, visualizar informações das roupas, favoritar itens e acompanhar produtos de interesse. As vendedoras externas podem cadastrar roupas para venda e acompanhar o status de seus produtos dentro da plataforma. Já as funcionárias possuem acesso a funcionalidades administrativas, como gerenciamento de produtos e controle geral do sistema.  O sistema também possui um módulo de gerenciamento de produtos, onde cada item pode conter informações como nome, descrição, preço, status e imagens associadas. Além disso, existe um sistema de favoritos que permite aos usuários salvar produtos para visualizar posteriormente.

## Tecnologias

- **Back-end:** C# com ASP.NET Core MVC (.NET 10)
- **Banco de dados:** PostgreSQL em produção (Render) e SQLite no desenvolvimento local, via Entity Framework Core
- **Front-end:** views Razor (`.cshtml`) com o HTML, CSS e JavaScript originais
- **Hospedagem:** Render, com Docker

## Estrutura

```
GarimpoChic/
├── Controllers/   ContaController (login, cadastro), ClienteController, FuncionariaController
├── Models/        Usuario, Produto, Pedido, Favorito, Evento, Repasse
├── Data/          BrechoContext (EF Core), conexão com o banco e dados iniciais
├── Util/          formatação (moeda, datas, CPF), login, relatórios do painel
├── Views/         páginas Razor
└── wwwroot/       CSS, imagens e JavaScript
Dockerfile         imagem usada pela Render
render.yaml        Blueprint da Render (cria o banco e o site)
```

## Rodar no computador

Precisa do [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project GarimpoChic
```

Abra http://localhost:5213. Sem a variável `DATABASE_URL`, o site cria sozinho um banco SQLite (`GarimpoChic/garimpo.db`) com dados de demonstração. As contas de demonstração usam a senha definida em `SenhaDemo` no arquivo `GarimpoChic/appsettings.Development.json`. A Julia (CPF 529.982.247-25) é cliente e funcionária.

Para começar do zero, apague o `garimpo.db`.

## Publicar na Render

1. No painel da Render: **New → Blueprint** e escolha este repositório. O `render.yaml` cria o Postgres `garimpo-db` e o site `garimpo-chic`.
2. A Render pede três valores para a primeira funcionária:
   - `ADMIN_CPF`: CPF que vai entrar como funcionária
   - `ADMIN_SENHA`: senha dessa conta
   - `ADMIN_NOME`: nome que aparece no painel
3. Depois do deploy, entre com esse CPF e essa senha.

Opcional: para ter os dados de demonstração também na Render, adicione as variáveis `DadosDemo=true` e `SenhaDemo=<uma senha>`. Eles só são criados quando o banco ainda não tem produtos.

> O Postgres gratuito da Render expira 30 dias depois de criado. Depois disso é preciso migrar para um plano pago ou recriar o banco.

## Rotas

| Página | Endereço |
|---|---|
| Login | `/` |
| Cadastro | `/cadastro` |
| Recuperar senha | `/recuperar-senha` |
| Seleção de perfil | `/perfis` |
| Redefinir senha (link do e-mail) | `/redefinir-senha` |
| Cliente | `/inicio`, `/explorar`, `/favoritos`, `/sacola`, `/compras`, `/perfil` |
| Vender para a loja | `/vender` |
| Funcionária | `/painel`, `/painel/produtos`, `/painel/pedidos`, `/painel/calendario`, `/painel/financeiro`, `/painel/clientes` |

## E-mail (recuperação de senha)

O link "Esqueci minha senha" manda um e-mail com um link que vale por 1 hora e só pode ser usado uma vez. Para o envio funcionar, configure na Render (aba **Environment** do serviço `garimpo-chic`):

| Variável | Exemplo |
|---|---|
| `SMTP_HOST` | `smtp.gmail.com` |
| `SMTP_PORTA` | `587` |
| `SMTP_USUARIO` | o e-mail que envia |
| `SMTP_SENHA` | senha de app do provedor (no Gmail: "Senhas de app") |
| `SMTP_REMETENTE` | opcional; padrão é o `SMTP_USUARIO` |

Sem `SMTP_HOST`, o e-mail não é enviado: o conteúdo (com o link) aparece no log do servidor. Isso é útil para testar no computador.

## Banco de dados e migrations

Em produção (Postgres), o site aplica as migrations sozinho ao iniciar. Para mudar o modelo:

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMudanca --project GarimpoChic -o Data/Migrations
```

As migrations são geradas para o Postgres. No desenvolvimento (SQLite), o banco é criado direto pelo modelo: depois de mudar o modelo, apague o `GarimpoChic/garimpo.db`.

## Ainda não implementado

- **Pagamento online:** finalizar a compra cria o pedido e marca as peças como vendidas, mas o pagamento é combinado direto com a loja.
- **Repasse automático às vendedoras:** os repasses aparecem no financeiro, mas ainda não são gerados quando uma peça de vendedora externa é vendida.

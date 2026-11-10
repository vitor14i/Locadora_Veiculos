# FrotaGO

Sistema de locação de veículos desenvolvido em C# com WPF, Entity Framework Core e SQL Server, organizado em camadas MVC + Repository.

O FrotaGO permite que clientes reservem carros e motos e acompanhem suas reservas, enquanto atendentes e gerentes iniciam locações, registram pagamentos e devoluções e administram a frota da locadora.

## Perfis de usuário

| Perfil | Principais ações |
|---|---|
| **Cliente** | Criar conta, buscar veículos disponíveis, reservar e acompanhar reservas |
| **Atendente** | Criar reservas na loja, iniciar locações, registrar pagamentos e devoluções e cadastrar clientes |
| **Gerente** | Tudo o que o atendente faz, além de gerenciar veículos, manutenções, atendentes e relatórios |

## Tecnologias

- C# e .NET 8
- WPF
- Entity Framework Core 8, com abordagem Code First
- SQL Server LocalDB

## Como rodar

### Pré-requisitos
- Visual Studio 2022 com as cargas de trabalho **Desenvolvimento para desktop com .NET** e **Armazenamento e processamento de dados**.
- SQL Server LocalDB, instalado junto com o Visual Studio.

### Passos
1. Clone o repositório pelo Visual Studio, em **Git > Clonar Repositório**.
2. Abra o arquivo `FrotaGO.sln`.
3. Rode o projeto com **F5**.

Ao abrir, o sistema cria o banco `FrotaGO` no LocalDB, aplica as migrations e cadastra os dados iniciais automaticamente. Não é necessário rodar `Add-Migration` nem `Update-Database`.

Se o LocalDB não estiver disponível, uma mensagem mostra o erro e o aplicativo é encerrado.

### Logins de teste

Todos usam a senha `Frotago@123`.

| Perfil | E-mail |
|---|---|
| Gerente | gerente@frotago.com |
| Atendente | ana@frotago.com |
| Atendente | bruno@frotago.com |
| Cliente pessoa física | maria@email.com |
| Cliente pessoa física | joao@email.com |
| Cliente pessoa jurídica | contato@capixaba.com |

## Estrutura do repositório

```text
FrotaGO/
├── FrotaGO.sln
├── README.md
├── docs/                   Diagramas, telas e documentação do trabalho
└── src/
    └── FrotaGO/
        ├── Controllers/    Recebem as ações das telas e acionam os repositórios
        ├── Data/           DbContext e dados iniciais
        ├── Helpers/        Utilitários, como hash de senha e padronização de documentos
        ├── Migrations/     Histórico do banco gerado com Code First
        ├── Models/         Classes do domínio e enums
        ├── Repositories/   Acesso ao banco de dados
        └── Views/          Janelas e telas WPF
```

## Arquitetura

O sistema segue o fluxo:

```text
View → Controller → Repository → DbContext → Banco de dados
```

- **Model:** classes do domínio, com atributos, relacionamentos e regras próprias.
- **View:** telas WPF, sem acesso direto ao banco.
- **Controller:** recebe as ações do usuário e coordena as operações.
- **Repository:** concentra as consultas e gravações no banco.

A herança é mapeada com uma tabela por hierarquia, o padrão do Entity Framework. Por isso, as tabelas `Usuarios`, `Veiculos` e `Pagamentos` guardam todas as suas subclasses, identificadas pela coluna `Discriminator`.

## Regras de negócio implementadas

- **Senha:** mínimo de 8 caracteres, salva sempre como hash.
- **Tarifa:** `Moto` e `Carro` com câmbio manual usam a diária × dias. `Carro` com câmbio automático tem acréscimo de 10% na diária.
- **CPF e CNPJ:** salvos sem máscara, somente com números. Use `DocumentoHelper.SomenteNumeros()` ao cadastrar e buscar.
- **Contratos:** os veículos entram pelo método `Contrato.AdicionarItem(veiculo, dias)`, que registra diária, dias e quilometragem inicial. `Contrato.Veiculos` é usado apenas para consulta.

## Onde foi aplicado SOLID

- **S (responsabilidade única):** `PasswordHasher` cuida só do hash de senha, `DocumentoHelper` só padroniza documentos, `DbSeeder` só cadastra os dados iniciais e `FrotaGODbContext` só mapeia o banco.
- **O (aberto/fechado):** `Veiculo.Calcular()` é abstrato, e `Carro` e `Moto` têm regras próprias. Um novo tipo de veículo pode ser criado sem alterar as classes existentes. O mesmo vale para `Pagamento.Validar()`, em `Cartao` e `Pix`.
- **L (substituição de Liskov):** `Carro` e `Moto` substituem `Veiculo`, `PessoaFisica` e `PessoaJuridica` substituem `Cliente`, e `Cartao` e `Pix` substituem `Pagamento`.
- **I (segregação de interfaces):** `ITarifa` tem um único método.
- **D (inversão de dependência):** `Contrato.AdicionarItem()` depende da abstração `Veiculo`, e não de `Carro` ou `Moto`. Os controllers vão depender das interfaces dos repositórios.

## Onde foi aplicado Clean Code

- Nomes descritivos e iguais aos do diagrama de classes.
- Métodos pequenos, cada um com uma única responsabilidade.
- Sem números mágicos: o acréscimo do câmbio automático fica na constante `Carro.AcrescimoCambioAutomatico`.
- Precisão dos campos `decimal` configurada em um só lugar, em `ConfigureConventions`.
- Inicialização do banco isolada em `App.TentarInicializarBanco()`.
- Um tipo por arquivo e namespaces organizados por camada.

## Fluxo de trabalho

- A branch `main` guarda apenas código que compila e funciona.
- Cada issue é desenvolvida em sua própria branch, como `feature/login`, e entra na `main` por pull request revisado por outro integrante.
- Quem alterar um model gera uma nova migration na mesma branch, com um nome descritivo, como `Add-Migration AdicionaCampoX`.
- Nunca altere o banco manualmente. Toda mudança deve vir de uma migration.

## Links

- Planejamento (Linear): https://linear.app/atomicrat/team/POO/overview
- Diagramas: [`docs/diagramas`](docs/diagramas)
- Vídeo de apresentação: [link]

## Equipe

- Flavio Storary
- Gabriel Oliveira Brito
- Ivan Cardozo Scolforo Moreira
- Vitor Claudino Claudiano

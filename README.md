# FrotaGO

WPF (.NET 8) + EF Core 8 + SQL Server LocalDB (Code First).

## Como rodar
1. Abra `FrotaGO.sln` (na raiz) no Visual Studio 2022 (workloads ".NET desktop" e "Armazenamento e processamento de dados").
2. Package Manager Console (projeto padrão = FrotaGO):
   ```
   Add-Migration CriacaoInicial
   Update-Database
   ```
3. Confira na migration gerada que a tabela `ItensContrato` usa `Id` como chave primária
   (procure por `table.PrimaryKey("PK_ItensContrato", x => x.Id)`).
4. Execute (F5). Ao abrir, o `DbSeeder` popula o banco se estiver vazio. Se o LocalDB não estiver disponível,
   uma `MessageBox` mostra o erro e o aplicativo é encerrado.

> Já tinha rodado uma versão anterior? O seeder só popula banco vazio. Para receber a nova senha e os
> CPF/CNPJ sem máscara, rode `Drop-Database` e depois `Update-Database`.

Logins de teste (senha `Frotago@123`): gerente@frotago.com, ana@frotago.com, bruno@frotago.com, maria@email.com, joao@email.com, contato@capixaba.com.

## Camadas (MVC)
- Models: `Models/` e `Models/Enums/`
- Data: `Data/` (DbContext + Seeder)
- Helpers: `Helpers/` (`PasswordHasher`, `DocumentoHelper`)
- Repositories / Controllers / Views: pastas prontas para as próximas issues

## Regras de negócio implementadas
- **Senha:** mínimo de 8 caracteres (a senha dos dados iniciais é `Frotago@123`).
- **Cálculo de tarifa (`Veiculo.Calcular`):**
  - `Moto`: diária × dias.
  - `Carro` com câmbio **Manual**: diária × dias.
  - `Carro` com câmbio **Automático**: acréscimo de **10%** na diária (diária arredondada a 2 casas) × dias.
- **CPF/CNPJ:** sempre salvos sem máscara, somente dígitos (11 e 14 caracteres). Para buscar, normalize o
  texto digitado com `DocumentoHelper.SomenteNumeros(...)`.
- **Contratos:** os veículos entram sempre por `Contrato.Itens` (use `Contrato.AdicionarItem(veiculo, dias)`),
  pois o `ItemContrato` guarda diária, dias e km inicial. `Contrato.Veiculos` é apenas para consulta.

## Onde foi aplicado SOLID
- S: `Helpers/PasswordHasher.cs` só cuida de hash; `Helpers/DocumentoHelper.cs` só padroniza documentos; `DbSeeder` só popula; `FrotaGODbContext` só mapeia; `App.TentarInicializarBanco` só inicializa o banco.
- O: `Veiculo.Calcular` abstrato: `Carro` (acréscimo de 10% no câmbio automático) e `Moto` (diária simples) têm regras próprias, e um novo tipo de veículo (ex.: caminhonete) pode ser criado sem alterar `Veiculo`, `Carro`, `Moto` nem `Contrato.AdicionarItem`. Também em `Pagamento.Validar` (`Cartao`/`Pix`).
- L: `Carro`/`Moto` substituem `Veiculo`; `PessoaFisica`/`PessoaJuridica` substituem `Cliente`; `Cartao`/`Pix` substituem `Pagamento`.
- I: `ITarifa` tem um único método.
- D: será aplicado nas próximas issues (Controllers dependendo de interfaces de Repositories). Hoje `Contrato.AdicionarItem` já depende da abstração `Veiculo` (`Calcular`), e não de `Carro`/`Moto`.

## Onde foi aplicado Clean Code
- Nomes descritivos, iguais ao diagrama; métodos pequenos com uma responsabilidade (`Models/*.cs`).
- Sem números mágicos: o acréscimo do câmbio automático é a constante `Carro.AcrescimoCambioAutomatico`; a senha padrão é `DbSeeder.SenhaPadrao`.
- Precisão decimal centralizada em `ConfigureConventions` (sem repetição).
- Inicialização do banco isolada em `App.TentarInicializarBanco` (try/catch com `MessageBox`), sem misturar tratamento de erro com a lógica de `OnStartup`.
- Seed e hash de senha em classes dedicadas; um tipo por arquivo; namespaces por camada.

## Pendência do trabalho
A funcionalidade com SQL puro (`FromSqlRaw`/`ExecuteSqlRaw`) ficará para uma issue de repositório/relatório.

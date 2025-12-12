# KeduPayments

Aplicação ASP.NET Core Web API para gerenciar planos de pagamento de responsáveis financeiros no contexto educacional.

## Requisitos atendidos

- Cadastro de responsável financeiro (`POST /responsaveis`).
- Cadastro de centro de custo customizável via API (`POST /centros-de-custo`, `GET /centros-de-custo`).
- Criação de plano de pagamento com cobranças e cálculo automático do valor total (`POST /planos-de-pagamento`).
- Consulta do plano e do total (`GET /planos-de-pagamento/{id}`, `GET /planos-de-pagamento/{id}/total`).
- Listagem de planos de um responsável (`GET /responsaveis/{id}/planos-de-pagamento`).
- Listagem de cobranças de um responsável, incluindo campos e vencimento derivado (`GET /responsaveis/{id}/cobrancas`).
- Quantidade de cobranças de um responsável (`GET /responsaveis/{id}/cobrancas/quantidade`).
- Registro de pagamento de uma cobrança com atualização de status (`POST /cobrancas/{id}/pagamentos`).

Obs.: Nesta versão de demonstração o armazenamento é InMemory. Integração com PostgreSQL via EF Core/Npgsql pode ser habilitada facilmente em um próximo passo.

## Como executar

```sh
dotnet build KeduPayments/KeduPayments.csproj
dotnet run --project KeduPayments
```

Servidor inicia em `http://localhost:5172/`.

## Exemplos (PowerShell)

```powershell
# 1) Criar responsável
$resp = Invoke-RestMethod -Method Post -Uri http://localhost:5172/responsaveis -ContentType 'application/json' -Body '{"nome":"Joao"}'

# 2) Criar centro de custo
$centro = Invoke-RestMethod -Method Post -Uri http://localhost:5172/centros-de-custo -ContentType 'application/json' -Body '{"nome":"MATRICULA"}'

# 3) Criar plano de pagamento
$payload = '{"responsavelId":'+$resp.id+',"centroDeCusto":'+$centro.id+',"cobrancas":[{"valor":500.00,"dataVencimento":"2025-03-10","metodoPagamento":"BOLETO"},{"valor":500.00,"dataVencimento":"2025-04-10","metodoPagamento":"PIX"}]}'
$plano = Invoke-RestMethod -Method Post -Uri http://localhost:5172/planos-de-pagamento -ContentType 'application/json' -Body $payload

# 4) Consultar total do plano
Invoke-RestMethod -Method Get -Uri http://localhost:5172/planos-de-pagamento/$($plano.id)/total

# 5) Listar cobranças do responsável
Invoke-RestMethod -Method Get -Uri http://localhost:5172/responsaveis/$($resp.id)/cobrancas | ConvertTo-Json -Depth 5

# 6) Registrar pagamento da cobrança 1
Invoke-RestMethod -Method Post -Uri http://localhost:5172/cobrancas/1/pagamentos -ContentType 'application/json' -Body '{"valor":500.00,"dataPagamento":"2025-12-12T10:00:00Z"}'
```

## Esquema dos dados

- Responsável financeiro: id, nome
- Centro de custo: id, nome
- Plano de pagamento: id, `responsavelId`, `centroDeCustoId`, `cobrancas[]`, `valorTotal`
- Cobrança: id, `planoDePagamentoId`, valor, `dataVencimento`, `metodoPagamento` (BOLETO|PIX), `codigoPagamento`, `status` (EMITIDA|PAGA|CANCELADA), `vencida` (derivado)
- Pagamento: id, `cobrancaId`, valor, `dataPagamento`

## Próximos passos

- Adicionar EF Core com provider PostgreSQL (Npgsql).
- Migrations e `DbContext` para persistência.
- Opcional: expor GraphQL (HotChocolate).


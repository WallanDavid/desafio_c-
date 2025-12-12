using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

static async Task EnsureOk(HttpResponseMessage r)
{
    if (!r.IsSuccessStatusCode)
    {
        var body = await r.Content.ReadAsStringAsync();
        throw new Exception($"HTTP {(int)r.StatusCode}: {body}");
    }
}

static async Task<JsonDocument> ReadJson(HttpResponseMessage r)
{
    var s = await r.Content.ReadAsStreamAsync();
    return await JsonDocument.ParseAsync(s);
}

var baseUrl = "http://localhost:5172";
var http = new HttpClient();

for (int i = 0; i < 20; i++)
{
    try
    {
        var ping = await http.GetAsync(baseUrl + "/centros-de-custo");
        if (ping.IsSuccessStatusCode) break;
    }
    catch { }
    await Task.Delay(250);
}

var respCreate = await http.PostAsync(baseUrl + "/responsaveis", new StringContent("{\"nome\":\"Teste\"}", Encoding.UTF8, "application/json"));
await EnsureOk(respCreate);
var respJson = await ReadJson(respCreate);
var responsavelId = respJson.RootElement.GetProperty("id").GetInt32();

var centroCreate = await http.PostAsync(baseUrl + "/centros-de-custo", new StringContent("{\"nome\":\"MATRICULA\"}", Encoding.UTF8, "application/json"));
await EnsureOk(centroCreate);
var centroJson = await ReadJson(centroCreate);
var centroId = centroJson.RootElement.GetProperty("id").GetInt32();

var payload = $"{{\"responsavelId\":{responsavelId},\"centroDeCusto\":{centroId},\"cobrancas\":[{{\"valor\":500.00,\"dataVencimento\":\"2025-03-10\",\"metodoPagamento\":\"BOLETO\"}},{{\"valor\":500.00,\"dataVencimento\":\"2025-04-10\",\"metodoPagamento\":\"PIX\"}}]}}";
var planoCreate = await http.PostAsync(baseUrl + "/planos-de-pagamento", new StringContent(payload, Encoding.UTF8, "application/json"));
await EnsureOk(planoCreate);
var planoJson = await ReadJson(planoCreate);
var planoId = planoJson.RootElement.GetProperty("id").GetInt32();
var valorTotal = planoJson.RootElement.GetProperty("valorTotal").GetDecimal();
if (valorTotal != 1000m) throw new Exception("ValorTotal incorreto");

var cobrancasArr = planoJson.RootElement.GetProperty("cobrancas");
if (cobrancasArr.GetArrayLength() != 2) throw new Exception("Quantidade de cobranças incorreta");
var cobranca1Id = cobrancasArr[0].GetProperty("id").GetInt32();
var cobranca2Id = cobrancasArr[1].GetProperty("id").GetInt32();

var totalGet = await http.GetAsync(baseUrl + $"/planos-de-pagamento/{planoId}/total");
await EnsureOk(totalGet);
var totalDoc = await ReadJson(totalGet);
if (totalDoc.RootElement.GetDecimal() != 1000m) throw new Exception("Total do plano incorreto");

var cobrancasList = await http.GetAsync(baseUrl + $"/responsaveis/{responsavelId}/cobrancas");
await EnsureOk(cobrancasList);
var cobrancasDoc = await ReadJson(cobrancasList);
if (cobrancasDoc.RootElement.GetArrayLength() != 2) throw new Exception("Lista de cobranças incorreta");

var pagar1 = await http.PostAsync(baseUrl + $"/cobrancas/{cobranca1Id}/pagamentos", new StringContent("{\"valor\":500.00,\"dataPagamento\":\"2025-12-12T10:00:00Z\"}", Encoding.UTF8, "application/json"));
await EnsureOk(pagar1);

var cobrancasList2 = await http.GetAsync(baseUrl + $"/responsaveis/{responsavelId}/cobrancas");
await EnsureOk(cobrancasList2);
var cobrancasDoc2 = await ReadJson(cobrancasList2);
foreach (var item in cobrancasDoc2.RootElement.EnumerateArray())
{
    var id = item.GetProperty("id").GetInt32();
    var status = item.GetProperty("status").GetString();
    if (id == cobranca1Id && status != "PAGA") throw new Exception("Cobrança 1 não está PAGA");
    if (id == cobranca2Id && status != "EMITIDA") throw new Exception("Cobrança 2 deveria continuar EMITIDA");
}

Console.WriteLine("Teste end-to-end OK");

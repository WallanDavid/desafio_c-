using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

public class ApiTests
{
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

    [Fact]
    public async Task FluxoPrincipal()
    {
        var baseUrl = "http://localhost:5172";
        var http = new HttpClient();

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
        Assert.Equal(1000m, valorTotal);

        var cobrancasArr = planoJson.RootElement.GetProperty("cobrancas");
        Assert.Equal(2, cobrancasArr.GetArrayLength());
        var cobranca1Id = cobrancasArr[0].GetProperty("id").GetInt32();

        var pagar1 = await http.PostAsync(baseUrl + $"/cobrancas/{cobranca1Id}/pagamentos", new StringContent("{\"valor\":500.00,\"dataPagamento\":\"2025-12-12T10:00:00Z\"}", Encoding.UTF8, "application/json"));
        await EnsureOk(pagar1);
    }
}

using Domain;
using DTOs;
using Repositories;
using Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRouting();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<Repositories.InMemoryStore>();
builder.Services.AddSingleton<Repositories.IResponsavelRepository, Repositories.InMemoryResponsavelRepository>();
builder.Services.AddSingleton<Repositories.ICentroDeCustoRepository, Repositories.InMemoryCentroDeCustoRepository>();
builder.Services.AddSingleton<Repositories.IPlanoRepository, Repositories.InMemoryPlanoRepository>();
builder.Services.AddSingleton<Repositories.ICobrancaRepository, Repositories.InMemoryCobrancaRepository>();
builder.Services.AddSingleton<Services.IPagamentoService, Services.PagamentoService>();
builder.Services.AddSingleton<Services.IPaymentCodeGenerator, Services.PaymentCodeGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

 

app.MapPost("/responsaveis", (CriarResponsavelDto dto, IResponsavelRepository repo) =>
{
    var r = repo.Add(dto.Nome);
    return Results.Created($"/responsaveis/{r.Id}", r);
});

app.MapPost("/centros-de-custo", (CriarCentroDeCustoDto dto, ICentroDeCustoRepository repo) =>
{
    var c = repo.Add(dto.Nome);
    return Results.Created($"/centros-de-custo/{c.Id}", c);
});

app.MapGet("/centros-de-custo", (ICentroDeCustoRepository repo) => Results.Ok(repo.GetAll()));

app.MapPost("/planos-de-pagamento", (CriarPlanoDto dto, IResponsavelRepository respRepo, ICentroDeCustoRepository centroRepo, IPlanoRepository planoRepo, IPaymentCodeGenerator codeGen) =>
{
    var resp = respRepo.Get(dto.ResponsavelId) ?? throw new InvalidOperationException("Responsável não encontrado");
    var centro = centroRepo.Get(dto.CentroDeCusto) ?? throw new InvalidOperationException("Centro de custo não encontrado");

    var plano = new PlanoDePagamento
    {
        ResponsavelId = resp.Id,
        CentroDeCustoId = centro.Id,
        Cobrancas = dto.Cobrancas.Select(item => new Cobranca
        {
            Valor = item.Valor,
            DataVencimento = item.DataVencimento,
            MetodoPagamento = Enum.Parse<MetodoPagamento>(item.MetodoPagamento, true),
            Status = StatusCobranca.EMITIDA,
            CodigoPagamento = codeGen.Generate(Enum.Parse<MetodoPagamento>(item.MetodoPagamento, true))
        }).ToList()
    };

    planoRepo.Add(plano);
    return Results.Created($"/planos-de-pagamento/{plano.Id}", new
    {
        plano.Id,
        plano.ResponsavelId,
        plano.CentroDeCustoId,
        ValorTotal = plano.ValorTotal,
        Cobrancas = plano.Cobrancas.Select(c => new
        {
            c.Id,
            c.Valor,
            c.DataVencimento,
            c.MetodoPagamento,
            c.CodigoPagamento,
            c.Status,
            Vencida = c.EstaVencida(DateOnly.FromDateTime(DateTime.UtcNow))
        })
    });
});

app.MapGet("/planos-de-pagamento/{id:int}", (int id, IPlanoRepository repo, ICentroDeCustoRepository centroRepo) =>
{
    var plano = repo.Get(id);
    if (plano is null) return Results.NotFound();
    var centro = centroRepo.Get(plano.CentroDeCustoId);
    return Results.Ok(new
    {
        plano.Id,
        plano.ResponsavelId,
        CentroDeCusto = centro,
        ValorTotal = plano.ValorTotal,
        Cobrancas = plano.Cobrancas.Select(c => new
        {
            c.Id,
            c.Valor,
            c.DataVencimento,
            c.MetodoPagamento,
            c.CodigoPagamento,
            c.Status,
            Vencida = c.EstaVencida(DateOnly.FromDateTime(DateTime.UtcNow))
        })
    });
});

app.MapGet("/planos-de-pagamento/{id:int}/total", (int id, IPlanoRepository repo) =>
{
    var plano = repo.Get(id);
    return plano is null ? Results.NotFound() : Results.Ok(plano.ValorTotal);
});

app.MapGet("/responsaveis/{id:int}/planos-de-pagamento", (int id, IResponsavelRepository repo) =>
{
    var planos = repo.GetPlanosByResponsavel(id).Select(p => new { p.Id, p.CentroDeCustoId, ValorTotal = p.ValorTotal });
    return Results.Ok(planos);
});

app.MapGet("/responsaveis/{id:int}/cobrancas", (int id, IResponsavelRepository repo) =>
{
    var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
    var cobrancas = repo.GetCobrancasByResponsavel(id).Select(c => new
    {
        c.Id,
        c.PlanoDePagamentoId,
        c.Valor,
        c.DataVencimento,
        c.MetodoPagamento,
        c.CodigoPagamento,
        c.Status,
        Vencida = c.EstaVencida(hoje)
    });
    return Results.Ok(cobrancas);
});

app.MapGet("/responsaveis/{id:int}/cobrancas/quantidade", (int id, IResponsavelRepository repo) =>
{
    var count = repo.GetCobrancasByResponsavel(id).Count();
    return Results.Ok(count);
});

app.MapPost("/cobrancas/{id:int}/pagamentos", (int id, RegistrarPagamentoDto dto, IPagamentoService svc) =>
{
    try
    {
        var pg = svc.RegistrarPagamento(id, dto.Valor, dto.DataPagamento);
        return Results.Created($"/pagamentos/{pg.Id}", pg);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

app.Run();

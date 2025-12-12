namespace DTOs
{
    public record CriarResponsavelDto(string Nome);
    public record CriarCentroDeCustoDto(string Nome);

    public record CriarPlanoDto(int ResponsavelId, int CentroDeCusto, List<CriarCobrancaItemDto> Cobrancas);
    public record CriarCobrancaItemDto(decimal Valor, DateOnly DataVencimento, string MetodoPagamento);

    public record RegistrarPagamentoDto(decimal Valor, DateTime DataPagamento);
}

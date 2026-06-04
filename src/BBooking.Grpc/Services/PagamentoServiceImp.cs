using Grpc.Core;
using System;
using System.Threading.Tasks;
namespace BBooking.Grpc.Services;

public class PagamentoServiceImp : PagamentoService.PagamentoServiceBase
{
    public override Task<PagamentoResponse> ElaboraPagamento(PagamentoRequest request, ServerCallContext context)
    {
        if (request.Importo > 0)
        {
            return Task.FromResult(new PagamentoResponse
            {
                Successo = true,
                TransazioneId = Guid.NewGuid().ToString(),
                Messaggio = "Pagamento autorizzato"
            });
        }

        return Task.FromResult(new PagamentoResponse
        {
            Successo = false,
            TransazioneId = string.Empty,
            Messaggio = "Importo non valido"
        });
    }
}
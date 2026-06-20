using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Services
{
    public class RecorrenciaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecorrenciaBackgroundService> _logger;

        public RecorrenciaBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<RecorrenciaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessarRecorrenciasAsync(stoppingToken);

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcessarRecorrenciasAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<RecorrenciaProcessorService>();
                var totalGerado = await processor.GerarPendentesAsync(cancellationToken);

                if (totalGerado > 0)
                {
                    _logger.LogInformation(
                        "Processamento automatico de recorrencias gerou {TotalGerado} transacoes.",
                        totalGerado);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Falha de banco ao processar recorrencias automaticamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada ao processar recorrencias automaticamente.");
            }
        }
    }
}

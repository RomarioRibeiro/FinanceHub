using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Services
{
    public class MetaLembreteBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MetaLembreteBackgroundService> _logger;

        public MetaLembreteBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MetaLembreteBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessarAsync(stoppingToken);

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

        private async Task ProcessarAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider
                    .GetRequiredService<MetaLembreteProcessorService>();
                var total = await processor.ProcessarPendentesAsync(cancellationToken);

                if (total > 0)
                {
                    _logger.LogInformation(
                        "Processamento de metas entregou {Total} lembrete(s).",
                        total);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Falha de banco ao processar lembretes de metas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada ao processar lembretes de metas.");
            }
        }
    }
}

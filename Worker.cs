using TokenBucketWorkerService.Services;
namespace TokenBucketWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TokenBucket _bucket;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://jsonplaceholder.typicode.com/posts";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _bucket = new TokenBucket(1, 10); // Taxa: 2 tokens/s, Capacidade: 5 tokens
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker iniciado em {time}", DateTimeOffset.Now);

            int requestCount = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Tokens no momento: {num_tokens} - Executado em: {Data}", _bucket.VerifyToken(), DateTime.Now);
                if (_bucket.Consume(4))
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(ApiUrl, stoppingToken);
                        if (response.IsSuccessStatusCode)
                        {
                            var data = await response.Content.ReadAsStringAsync(stoppingToken);
                            _logger.LogInformation("Requisição {RequestCount}: Sucesso! - Executado em: {Data}", ++requestCount, DateTime.Now);
                        }
                        else
                        {
                            _logger.LogWarning("Requisição {RequestCount}: Falhou com Status Code {StatusCode} - Executado em: {Data}", ++requestCount, response.StatusCode, DateTime.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar a requisição {RequestCount}- Executado em: {Data}", ++requestCount, DateTime.Now);
                    }
                }
                else
                {
                    _logger.LogWarning("Requisição {RequestCount}: Bloqueada (Sem tokens disponíveis) - Executado em: {Data}", ++requestCount, DateTime.Now);
                }

                // Aguarda 500ms antes de tentar novamente
                await Task.Delay(500, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _httpClient?.Dispose();
            base.Dispose();
        }
    }
}

using System.Net.Http.Json;
using ConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace ConsoleApp.Workers
{
    public class CircuitBreakerWorker : BackgroundService
    {
        private readonly ILogger<CircuitBreakerWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public CircuitBreakerWorker(ILogger<CircuitBreakerWorker> logger,
            IConfiguration configuration,
            AsyncCircuitBreakerPolicy circuitBreaker)
        {
            _logger = logger;
            _configuration = configuration;
            _circuitBreaker = circuitBreaker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = new HttpClient();
            var urlApiContagem = "http://localhost:5296/contador";

            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    var resultado = await _circuitBreaker.ExecuteAsync<ResultadoContador>(() =>
                    {
                        return httpClient.GetFromJsonAsync<ResultadoContador>(urlApiContagem);
                    });

                    _logger.LogInformation($"* {DateTime.Now:HH:mm:ss} * " +
                        $"Circuito = {_circuitBreaker.CircuitState} | " +
                        $"Contador = {resultado.ValorAtual} | " +
                        $"Mensagem = {resultado.MensagemVariavel}");
                }
                catch ( Exception ex )
                {
                    _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                        $"Circuito = {_circuitBreaker.CircuitState} | " +
                        $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");
                }

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}

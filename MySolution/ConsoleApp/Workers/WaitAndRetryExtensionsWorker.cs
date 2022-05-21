using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using ConsoleApp.Models;

namespace ConsoleApp.Workers
{
    public class WaitAndRetryExtensionsWorker : BackgroundService
    {
        private readonly ILogger<WaitAndRetryExtensionsWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncPolicy _resiliencePolicy;

        public WaitAndRetryExtensionsWorker(ILogger<WaitAndRetryExtensionsWorker> logger,
            IConfiguration configuration,
            AsyncPolicy resiliencePolicy)
        {
            _logger = logger;
            _configuration = configuration;
            _resiliencePolicy = resiliencePolicy;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = new HttpClient();
            //var urlApiContagem = _configuration ["UrlApiContagem"];
            var urlApiContagem = "http://localhost:5296/contador";

            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    var resultado = await _resiliencePolicy.ExecuteAsync<ResultadoContador>(() =>
                    {
                        return httpClient.GetFromJsonAsync<ResultadoContador>(urlApiContagem);
                    });

                    _logger.LogInformation($"* {DateTime.Now:HH:mm:ss} * " +
                        $"Contador = {resultado.ValorAtual} | " +
                        $"Mensagem = {resultado.MensagemVariavel}");
                }
                catch ( Exception ex )
                {
                    _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                        $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

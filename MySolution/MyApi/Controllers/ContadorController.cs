using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApi.Models;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContadorController : ControllerBase
    {
        private static readonly Contador _CONTADOR = new Contador();
        private readonly ILogger<ContadorController> _logger;
        private readonly IConfiguration _configuration;

        public ContadorController(ILogger<ContadorController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public ResultadoContador Get()
        {
            int valorAtualContador;
            lock ( _CONTADOR )
            {
                _CONTADOR.Incrementar();
                valorAtualContador = _CONTADOR.ValorAtual;
            }

            // Wait and Retry
            if ( valorAtualContador % 4 == 0 )
            {
                _logger.LogError("Simulando falha...");
                throw new Exception("Simulação de falha!");
            }


            /// CIRCUIT BREAKER
            //if ( valorAtualContador > 5 && valorAtualContador <= 12 )
            //{
            //    _logger.LogError("Simulando falha...");
            //    throw new Exception("Simulação de falha!");
            //}

            _logger.LogInformation($"Contador - Valor atual: {valorAtualContador}");

            return new()
            {
                ValorAtual = valorAtualContador,
                Local = _CONTADOR.Local,
                Kernel = _CONTADOR.Kernel,
                TargetFramework = _CONTADOR.TargetFramework,
                MensagemFixa = "Teste",
                MensagemVariavel = _configuration ["MensagemVariavel"]
            };
        }
    }
}
using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using System.Data.SQLite;
using Dapper.Contrib.Extensions;
using Groffe.AspNetCore.Indisponibilidade.Testes.Models;

namespace Groffe.AspNetCore.Indisponibilidade.Testes
{
    public class TestesChecagemIndisponibilidade
    {
        [Fact]
        public void TestarDisponibilidadeCenario01()
        {
            SimularMiddleware(null).Should()
                .Be(200, "* foi simulado um periodo de disponibilidade *");
        }

        [Fact]
        public void TestarDisponibilidadeCenario02()
        {
            var dataHoraAtual = DateTime.Now;
            SimularMiddleware(new PeriodoIndisponibilidade[]
            {
                new ()
                {
                    InicioIndisponibilidade = dataHoraAtual.AddMinutes(-5),
                    TerminoIndisponibilidade = dataHoraAtual.AddMinutes(-4),
                    Mensagem = "Método TestarDisponibilidadeCenario02"
                },
                new ()
                {
                    InicioIndisponibilidade = dataHoraAtual.AddMinutes(4),
                    TerminoIndisponibilidade = dataHoraAtual.AddMinutes(5),
                    Mensagem = "Método TestarDisponibilidadeCenario02"
                }
            }).Should().Be(200, "* foi simulado um periodo de disponibilidade *");
        }

        [Fact]
        public void TestarIndisponibilidade()
        {
            var dataHoraAtual = DateTime.Now;
            SimularMiddleware(new PeriodoIndisponibilidade[]
            {
                new ()
                {
                    InicioIndisponibilidade = dataHoraAtual.AddMinutes(-1),
                    TerminoIndisponibilidade = dataHoraAtual.AddMinutes(1),
                    Mensagem = "Método TestarIndisponibilidade"
                }
            }).Should().Be(403, "* foi simulada uma indisponibilidade *");
        }

        private int SimularMiddleware(
            PeriodoIndisponibilidade[] indisponibilidades)
        {
            var connectionString =
                $"Data Source=./indisponibilidade-{DateTime.Now:yyyyMMdd-HHmmss}.db";
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
            services.ConfigureChecagemIndisponibilidade(
                DBChecagemIndisponibilidade.SQLite, connectionString);

            if (indisponibilidades is not null && indisponibilidades.Length > 0)
            {
                var conexao = new SQLiteConnection(connectionString);
                conexao.Insert(indisponibilidades);
            }

            var mockRequestDelegate = new Mock<RequestDelegate>(MockBehavior.Loose);

            var serviceProvider = services.BuildServiceProvider();
            var responseMiddleware = new DefaultHttpContext().Response;

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.Setup(httpContext => httpContext.RequestServices)
                .Returns(() => serviceProvider);
            mockHttpContext.Setup(httpContext => httpContext.Response)
                .Returns(() => responseMiddleware);

            var checagemIndisponibilidade =
                new ChecagemIndisponibilidade(mockRequestDelegate.Object);
            checagemIndisponibilidade.Invoke(mockHttpContext.Object).Wait();

            return responseMiddleware.StatusCode;
        }
    }
}
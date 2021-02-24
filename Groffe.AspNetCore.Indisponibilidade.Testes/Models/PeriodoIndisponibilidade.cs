using System;
using Dapper.Contrib.Extensions;

namespace Groffe.AspNetCore.Indisponibilidade.Testes.Models
{
    [Table("Indisponibilidade")]
    public class PeriodoIndisponibilidade
    {
        public DateTime InicioIndisponibilidade { get; set; }
        public DateTime TerminoIndisponibilidade { get; set; }
        public string Mensagem { get; set; }
    }
}
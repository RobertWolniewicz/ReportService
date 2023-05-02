using ReportService.Core.Domains;
using System;
using System.Collections.Generic;

namespace ReportService.Core.Repositories
{
    public class ErrorRepository
    {
        public List<Error> GetLastErrors(int intervalInMinutes)
        {
            //pobieranie z bazy danych
            return new List<Error>
            {
                new Error {Message = "Bład testowy 1", Date = DateTime.Now},
                new Error {Message = "Bład testowy 2", Date = DateTime.Now}
            };
        }
    }
}

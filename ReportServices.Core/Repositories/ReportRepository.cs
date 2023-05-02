using ReportService.Core.Domains;
using System;
using System.Collections.Generic;

namespace ReportService.Core.Repositories
{
    public class ReportRepository
    {
        public Report GetLastNotSentReport()
        {
            //pobieranie z bazy danych ostatniego raportu

            return new Report
            {
                Id = 1,
                Title = "R/1/2023",
                Date = new DateTime(2023, 1, 1, 12, 0, 0),
                Positions = new List<ReportPosition>
                {
                    new ReportPosition
                    {
                        Id = 1,
                        ReportId = 1,
                        Title = "Position 1",
                        Description = "Descriptio 1",
                        Value = 43.01M
                    },
                     new ReportPosition
                    {
                        Id = 2,
                        ReportId = 1,
                        Title = "Position 2",
                        Description = "Descriptio 2",
                        Value = 4311M
                    },
                      new ReportPosition
                    {
                        Id =31,
                        ReportId = 1,
                        Title = "Position 3",
                        Description = "Descriptio 3",
                        Value = 1.99M
                    }
                }
            };
        }

        public void ReportSent(Report report)
        {
            report.IsSend = true;
            //zapis do bazy danych
        }

    }
}

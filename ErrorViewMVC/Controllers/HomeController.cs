using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using ErrorViewMVC.Models;
using System.Data.SqlClient;

namespace ErrorViewMVC.Controllers
{
    public class HomeController : Controller
    {
        private List<ServerConnection> JournalServers;
        private List<Error> Errors = new List<Error>();

        public HomeController()
        {
            this.JournalServers = this.LoadServers();
        }

        public IActionResult Index()
        {
            var model = this.GetAvailableServers();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [NonAction]
        private List<string> GetAvailableServers()
        {
            return JournalServers.Select(server => server.ServerName).ToList();
        }

        [NonAction]
        private List<ServerConnection> LoadServers()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<ServerConnection>));
            return ser.Deserialize(System.IO.File.OpenRead("wwwroot\\JournalServers.xml")) as List<ServerConnection>;
        }

        [HttpGet]
        public IActionResult GetErrors(string server, string loginHint, int itemsCount)
        {
            this.Errors.Clear();

            ServerConnection selectedServer = this.JournalServers.Find(s => s.ServerName == server);

            string queryString = $"SELECT Top {itemsCount} * FROM Journal WHERE Severite = 'Error' AND LoginAD like '%{loginHint}%' ORDER BY IdJournal DESC";
            string connectionString = $"Server={selectedServer.ServerName};Database={selectedServer.Database};User Id={selectedServer.Login};Password={selectedServer.Password};";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        this.Errors.Add(new Error
                        {
                            Machine = reader["NomMachine"].ToString(),
                            Application = reader["NomApplication"].ToString(),
                            LoginAd = reader["LoginAd"].ToString(),
                            Severite = reader["Severite"].ToString(),
                            Priorite = (int)reader["Priorite"],
                            Id = (int)reader["IdJournal"],
                            DateLog = (DateTime)reader["DateEvenement"],
                            Message = reader["MessageFormate"].ToString()
                        });
                    }
                }
                catch (Exception exp)
                {
                    // this.AppExecutionMsg = $"Un probléme avec le chargement des erreurs de la BD: {exp.Message}";
                    return View("Error", exp);
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }

            return View("ErrorList", this.Errors);
        }
    }
}

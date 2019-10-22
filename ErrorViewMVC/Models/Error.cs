using System;

namespace ErrorViewMVC.Models
{
    public class Error
    {
        public string Message { get; set; }

        public string LoginAd { get; set; }

        public string Severite { get; set; }

        public int Id { get; set; }

        public int Priorite { get; set; }

        public string Application { get; set; }

        public string Machine { get; set; }

        public DateTime DateLog { get; set; }
    }
}

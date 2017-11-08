﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealSimpleNet.Helpers;

namespace RealSimpleNetSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            TestBillingApi();
        }

        static void testDownload()
        {
            //Http http = new Http();
            //HttpResponse response = 
            //http.Get(
            //"http://dev.eboletos.com.mx/admin/downloadbill/pdf/20171108114751STY090223LX3XEXX010101000"
            //);
            //System.IO.File.WriteAllText("20171108114751STY090223LX3XEXX010101000.pdf", response.Data);

            Http http = new Http();
            http.Download(
                "http://dev.eboletos.com.mx/admin/downloadbill/pdf/20171108114751STY090223LX3XEXX010101000", 
                "20171108114751STY090223LX3XEXX010101000.pdf"
            );
        }

        private static void Test()
        {
            Http http = new Http();
            http.AddHeader("eso", "es");
            http.AddParameter("foo", "bar");
            HttpResponse response
                = http.Get("http://prosyss.com");
            Console.Write(response.Data);
            Console.Read();
        }

        class Pedido2
        {
            public Pedido2()
            {

            }

            public Pedido2(int pedido_id, int cliente_id,
                DateTime fecha_pedido, DateTime fecha_entrega, int estatus)
            {
                this.pedido_id = pedido_id;
                this.cliente_id = cliente_id;
                this.fecha_pedido = fecha_pedido;
                this.fecha_entrega = fecha_entrega;
                this.estatus = estatus;
            }
            public int pedido_id;
            public int cliente_id;
            public DateTime fecha_pedido;
            public DateTime fecha_entrega;
            public int estatus;
        }

        class Pedido
        {
            public Pedido()
            {

            }

            public Pedido(int pedido_id, int miembro_id, int cliente_id,
                DateTime fecha_pedido, DateTime fecha_entrega, int estatus)
            {
                this.pedido_id = pedido_id;
                this.miembro_id = miembro_id;
                this.cliente_id = cliente_id;
                this.fecha_pedido = fecha_pedido;
                this.fecha_entrega = fecha_entrega;
                this.estatus = estatus;
            }
            public int pedido_id;
            public int miembro_id;
            public int cliente_id;
            public DateTime fecha_pedido;
            public DateTime fecha_entrega;
            public int estatus;
        }

        static void Rest()
        {
            GetPedidos();
        }

        class TicketInfo
        {
            public string ticketId;
            public string customerTaxId;
            public string customerEmail;
            public string pdfUrl;
            public string xmlUrl;
        }

        /// <summary>
        /// Do a Rest Post
        /// </summary>
        static void TestBillingApi()
        {
            string endPoint = "http://localhost/eboletos/billingapi/bill";
            endPoint = "http://dev.eboletos.com.mx/billingapi/bill";
            string token = "8495cac4fa9156d509ec300c63b763966792f004";
            string key = "9e92f522f46124d19e36e3ad049cf78022faaca5";

            Http http = new Http();
            HttpResponse response;

            http.AddParameter("ticketId", "123456789020009");
            http.AddParameter("customerTaxId", "XEXX010101000");
            http.AddParameter("customerEmail", "lespino@prosyss.com");
            http.AddParameter("airportId", "AICM");
            http.AddParameter("companyId", "44");
            http.AddParameter("companyBusinessId", "YELLOWCAB");
            http.AddParameter("zoneId", "1");
            http.AddParameter("serviceTypeId", "1");
            http.AddParameter("payFormId", "04");
            http.AddParameter("fare", "5.00");
            http.AddParameter("taxRate", "0.000000");

            http.AddHeader("token", token);
            http.AddHeader("key", key);
            
            response = http.Post(
                endPoint
            );

            Dictionary<string,object> info = response.Deserialize<Dictionary<string,object>>();

            if (info.ContainsKey("error"))
            {
                Console.WriteLine(String.Format("{0}", info["description"]));
                Console.Read();
                return;
            }

            Console.WriteLine("TicketInfo: " + info["pdfUrl"]);

            string pdfUrl = info["pdfUrl"].ToString();            
            Console.WriteLine();
            string pdfFileName = info["billId"].ToString() + ".pdf";
            Console.WriteLine("pdf: " + pdfFileName);
            http.Download(pdfUrl, pdfFileName);
            //System.Diagnostics.Process.Start(pdfFileName);
            RealSimpleNet.Helpers.PrintHelper.PrintFile(pdfFileName);
            Console.Read();
               
        } // end Post

        static void Post()
        {
            RestClient rest = new RestClient("http://localhost/rspos/api/v1/");
            string location = rest.Post("pedidos", new Pedido(10, 10, 10, DateTime.Now, DateTime.Now.AddDays(1), 10));            
            Console.Write("location: ");
            Console.Write(location);
            Console.Read();
        }

        static void GetPedidos()
        {
            RestClient rest = new RestClient("http://localhost/rspos/api/v1/");
            List<Pedido2> pedidos = rest.Get<List<Pedido2>>("pedidos");
            foreach( Pedido2 pedido in pedidos)
            {
                Console.Write(pedido.pedido_id);
                Console.Write("\n");
            }            
            Console.Read();
        }

        static void Get()
        {
            RestClient rest = new RestClient("http://localhost/rspos/api/v1/");
            Console.Write(rest.Get("pedidos"));
            Console.Read();
        }

        static void Rest1()
        {
            //  endpoint
            string endpoint = "http://localhost/rspos/api/v1";

            // Post
            Http http = new Http();
            HttpResponse response;
            response = http.Post(
                endpoint + "/pedidos",
                new Dictionary<string,object>()
                {
                    { "pedido_id", 1 },
                    { "miembro_id", 1 },
                    { "cliente_id", 1 },
                    { "fecha_pedido", DateTime.Now.ToString("yyyy-MM-dd") },
                    { "fecha_entrega", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") },
                    { "estatus", 1 }
                }
            );

            // Post            
            response = http.Post(
                endpoint + "/pedidos",
                new Pedido(
                    7,7,7,DateTime.Now,DateTime.Now.AddDays(1),7
                ),
                Http.ContentTypes.Json
            );

            //  Print data
            Console.WriteLine(response.Data);

            //  Get
            response = http.Get(endpoint + "/pedidos");
            //  Print data
            Console.WriteLine(response.Data);

            //  Put
            response = http.Put(
                endpoint + "/pedidos/1",
                new Dictionary<string, object>()
                {
                    { "miembro_id", 2 },
                    { "cliente_id", 2 },
                    { "fecha_pedido", DateTime.Now.ToString("yyyy-MM-dd") },
                    { "fecha_entrega", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") },
                    { "estatus", 2 }
                }
            );
            //  Print data
            Console.WriteLine(response.Data);

            //  Get
            response = http.Get(endpoint + "/pedidos/1");
            //  Print data
            Console.WriteLine(response.Data);
        }
    }
}

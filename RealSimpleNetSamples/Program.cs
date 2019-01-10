using System;
using System.Collections.Generic;
using RealSimpleNet.Helpers;

namespace RealSimpleNetSamples
{    
    class Program
    {
        static void Main(string[] args)
        {
            //RestEntityTest();
            Console.WriteLine(Crypt.Checksum("somefile.txt"));

            Console.WriteLine(Crypt.Checksum("somefile.txt"));

            Console.WriteLine(Crypt.Checksum("somefile.txt"));

            Console.Read();

        }

        static void RestEntityTest()
        {
            TicketInfo t = new TicketInfo();
            t.customerEmail = "somemail";
            t.customerTaxId = "sometaxid";
            t.pdfUrl = "someurl";
            t.ticketId = "1230404";
            t.xmlUrl = "someurl";
            t.SetEndpoint("http://localhost/rspos/v1/api/");
            t.OnRestfullSuccess += T_OnSuccess;
            t.OnRestfullError += T_OnError;
            t.RestfulPost();
        }

        private static void T_OnError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        private static void T_OnSuccess(string response)
        {
            Console.WriteLine(response);
        }

        static void onDownload(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Completed");            
        }

        static void onProgress(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
        }

        static void testDownload()
        {
            //Http http = new Http();
            //HttpResponse response = 
            //http.Get(
            //"https://i1.wp.com/desarrollo.espino.info/files/2017/12/28preguntasdesarrolladorNet.png"
            //);
            //System.IO.File.WriteAllText("28preguntasdesarrolladorNet.png", response.Data);

            Http http = new Http();
            http.DownloadAsync(
                "https://i1.wp.com/desarrollo.espino.info/files/2017/12/28preguntasdesarrolladorNet.png",
                "28preguntasdesarrolladorNet.png",
                onDownload,
                onProgress
            );

            Console.Read();
        }

        private static void Test()
        {
            Http http = new Http();
            http.AddHeader("eso", "es");
            http.AddParameter("foo", "bar");
            HttpResponse response
                = http.Get("https://desarrollo.espino.info");
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

        class TicketInfo : RealSimpleNet.Libraries.RestEntity
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
            string endPoint = "http://localhost/rsbill/billingapi/bill";
            endPoint = "http://dev.espino.info/rsbill/billingapi/bill";
            string token = "8495cac4fa9156d509ec300c63b763966792f004";
            string key = "9e92f522f46124d19e36e3ad049cf78022faaca5";

            Http http = new Http();
            HttpResponse response;

            http.AddParameter("ticketId", "123456789020014");
            http.AddParameter("customerTaxId", "XEXX010101000");
            http.AddParameter("customerEmail", "fakemail@espinoserver.com");
            http.AddParameter("serviceTypeId", "1");
            http.AddParameter("payFormId", "04");
            http.AddParameter("fare", "5.00");
            http.AddParameter("taxRate", "0.000000");
            http.AddParameter("dateTime", "2017-11-10 12:59:30");
            http.AddParameter("payFormName", "VISA");

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
            PrintHelper.RawPrintFile(pdfFileName);
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

using System;
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
            Rest();
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

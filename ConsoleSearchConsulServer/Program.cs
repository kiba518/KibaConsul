using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSearchConsulServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var consulClient = new ConsulClient(x => x.Address = new Uri($"http://192.168.1.178:8500"));//请求注册的 Consul 地址
            var ret = consulClient.Agent.Services();
             
            var allServer = ret.GetAwaiter().GetResult();
            var allServerDic= allServer.Response;//这个是个dictionary 他的key是string类型 就是8500/ui上services的instance的id 就是服务实例id,如7b04cd06-1bfb-4a3d-8e3b-c0e689237d74
             
            var test1 = allServerDic.First();
            string name = test1.Value.Service;//服务名,就是注册的那个test1
            string serverAddress = test1.Value.Address; 
            int serverPort = test1.Value.Port;

            //我们可以在客户端启动的时候，调用一下consul来查找服务
            //比如，我们可以在服务集合里查找 服务名叫test1的服务 然后在调用它
            //这样，当服务器改变了test1的ip和端口，我们依然可以在集群里找他test1新的ip和端口了
            Console.ReadKey();
        }

        public async Task Do(ConsulClient consulClient,string serviceName)
        {
            
            var services = await consulClient.Catalog.Service(serviceName);
            if (services.Response.Length > 0)
            {
                var service = services.Response[0];
                string serverAddress = service.ServiceAddress;
                int serverPort = service.ServicePort;
            } 
            Console.ReadKey();
        }
    }
}

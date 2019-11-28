using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Consul;
using System.Text;

namespace WebCoreConsulServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            RegisterConsul();//ע�᱾����consul��Ⱥ
            //Console.WriteLine(HelloConsul().GetAwaiter().GetResult());//Key-Value��Put��Get
        }

        public static void RegisterConsul()
        {
            var consulClient = new ConsulClient(p => { p.Address = new Uri($"http://127.0.0.1:8500"); });//����ע��� Consul ��ַ
            //��������ip ���Ǳ�����ip������˿�8500 �����Ĭ��ע�����˿� 
            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//����������ú�ע��
                Interval = TimeSpan.FromSeconds(10),//����̶���ʱ�����һ�Σ�https://localhost:44308/api/Health
                HTTP = $"https://localhost:44308/api/Health",//��������ַ 44308��visualstudio�����Ķ˿�
                Timeout = TimeSpan.FromSeconds(5)
            };
             
            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck }, 
                ID = Guid.NewGuid().ToString(),
                Name = "test1",
                Address = "https://localhost/",
                Port = 44308,
                
            };

            consulClient.Agent.ServiceRegister(registration).Wait();//ע����� 

            //consulClient.Agent.ServiceDeregister(registration.ID).Wait();//registration.ID��guid
            //������ֹͣʱ��Ҫȡ������ע�ᣬ��Ȼ���´���������ʱ������ע��һ������
            //���ǣ�����÷����ڲ���������consul���Զ�ɾ��������񣬴�Լ2��3���Ӿͻ�ɾ�� 

        }
        /// <summary>
        /// Key-Value��Put��Get
        /// </summary>
        /// <returns></returns>
        public static async Task<string> HelloConsul()
        {
            using (var client = new ConsulClient())
            {
                var putPair = new KVPair("hello")
                {
                    Value = Encoding.UTF8.GetBytes("Hello Consul")
                };

                var putAttempt = await client.KV.Put(putPair);

                if (putAttempt.Response)
                {
                    var getPair = await client.KV.Get("hello");
                    return Encoding.UTF8.GetString(getPair.Response.Value, 0,
                        getPair.Response.Value.Length);
                }
                return "";
            }
        }
    }
}

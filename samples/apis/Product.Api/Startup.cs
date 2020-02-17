using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Product.Api.Infrastructure;

namespace Product.Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<ProductDbContext>(options =>
                options.UseNpgsql(Configuration["postgres:write"]));
            services.AddControllers();
            //services.AddGrpc(option =>
            //{
            //    if (Environment.IsDevelopment())
            //    {
            //        option.EnableDetailedErrors = true;
            //    }
            //});

            //services.AddGrpcValidation();
            //services.AddGrpcRequestValidator(this.GetType().Assembly);

            //services.AddPole(option =>
            //{
            //    option.AddManageredAssemblies(this.GetType().Assembly);
            //    option.AutoInjectionDependency();
            //    option.AutoInjectionCommandHandlersAndDomainEventHandlers();
            //    option.AddPoleEntityFrameworkCoreDomain();

            //    option.AddPoleReliableMessage(messageOption =>
            //    {
            //        messageOption.AddMasstransitRabbitmq(rabbitoption =>
            //        {
            //            rabbitoption.RabbitMqHostAddress = Configuration["RabbitmqConfig:HostAddress"];
            //            rabbitoption.RabbitMqHostUserName = Configuration["RabbitmqConfig:HostUserName"];
            //            rabbitoption.RabbitMqHostPassword = Configuration["RabbitmqConfig:HostPassword"];
            //            rabbitoption.QueueNamePrefix = Configuration["ServiceName"];
            //            rabbitoption.EventHandlerNameSuffix = "IntegrationEventHandler";
            //            rabbitoption.RetryConfigure =
            //                r =>
            //                {
            //                    r.Intervals(TimeSpan.FromSeconds(0.1)
            //                           , TimeSpan.FromSeconds(1)
            //                           , TimeSpan.FromSeconds(4)
            //                           , TimeSpan.FromSeconds(16)
            //                           , TimeSpan.FromSeconds(64)
            //                           );
            //                    r.Ignore<DbUpdateException>(exception =>
            //                    {
            //                        var sqlException = exception.InnerException as PostgresException;
            //                        return sqlException != null && sqlException.SqlState == "23505";
            //                    });
            //                };
            //        });
            //        messageOption.AddMongodb(mongodbOption =>
            //        {
            //            mongodbOption.ServiceCollectionName = Configuration["ServiceName"];
            //            mongodbOption.Servers = Configuration.GetSection("MongoConfig:Servers").Get<MongoHost[]>();
            //        });
            //        messageOption.AddEventAssemblies(typeof(Startup).Assembly)
            //              .AddEventHandlerAssemblies(typeof(Startup).Assembly);
            //        messageOption.NetworkInterfaceGatewayAddress = Configuration["ReliableMessageOption:NetworkInterfaceGatewayAddress"];
            //    });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UsePoleReliableMessage();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                //endpoints.MapGrpcService<ProductTypeService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}

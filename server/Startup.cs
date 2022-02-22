using app.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using app.Services.User;
using app.Services.JWT;
using Microsoft.AspNetCore.Identity;
using System;
//using Microsoft.AspNet.OData.Extensions;
////using Microsoft.OData.Edm;
//using Microsoft.AspNet.OData.Builder;
//using Microsoft.AspNet.OData.Batch;

namespace app
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));


            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            var jwtTokenConfig = Configuration.GetSection("jwtTokenConfig").Get<JwtTokenConfig>();
            services.AddSingleton(jwtTokenConfig);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            services.AddSingleton<IJwtAuthManager, JwtAuthManager>();
            services.AddHostedService<JwtClearExpToken>();
            
            services.AddScoped<IUser, User>();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll",
            //        builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            //});

            services.AddCors();

            services.AddControllers().AddNewtonsoftJson(x => 
            {
                x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //x.UseMemberCasing();
            });
            
            //services.AddOData();
        }

        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env,
            AppDbContext appDbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            appInitContext.initContextDB(appDbContext, userManager, roleManager);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseODataBatching();
            app.UseHttpsRedirection();
            app.UseRouting();
            
            app.UseCors(builder => {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                //var defaultBatchHandler = new DefaultODataBatchHandler();
                //defaultBatchHandler.MessageQuotas.MaxNestingDepth = 2;
                //defaultBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 10;
                //defaultBatchHandler.MessageQuotas.MaxReceivedMessageSize = 100;

                endpoints.MapControllers();
                //endpoints.EnableDependencyInjection();
                //endpoints.Select().Filter().Expand().OrderBy();
                //endpoints.MapODataRoute(
                //    routeName: "api", 
                //    routePrefix: "api", 
                //    model: GetEdmModel(),
                //    batchHandler: new DefaultODataBatchHandler()
                //    );

            });
        }

        //private static IEdmModel GetEdmModel()
        //{
        //    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        //    //builder.EntitySet<Event>("Event");
        //    return builder.GetEdmModel();
        //}
    }
}

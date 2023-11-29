//The Startup class is responsible for configuring the application's services and defining how the HTTP request pipeline should be set up.


using FundooApplication.Controllers;
using FundooManager.IManager;
using FundooManager.Manager;
using FundooRepository.Context;
using FundooRepository.IRepository;
using FundooRepository.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FundooManager.Manager;
using FundooRepo.Repository;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using FundooModel.User;
using Microsoft.VisualBasic;
using static System.Collections.Specialized.BitVector32;
using Microsoft.Data.SqlClient.Server;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.DataProtection;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Buffers.Text;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.Net;
using FundooModel.Notes;
//These are using directives, which allow the code to reference various namespaces and classes necessary for building an ASP.NET Core application

namespace FundooApplication
{
    public class Startup
    {
        //This defines the Startup class and its constructor.The Startup class is where you configure the application's services and request processing pipeline. It takes an IConfiguration object as a parameter, which provides access to configuration settings.
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        //services required by the application are configured
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //This line configures a database context using Entity Framework Core.It specifies the database connection string using the UserDbConnection from the configuration.
            services.AddDbContextPool<UserDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("UserDbConnection")));
            //user
            //Here, services for user management are registered using dependency injection. For example, IUserManager and IUserRepository are interfaces for managing users and interacting with the user data repository.The UserManager and UserRepository classes provide the implementation of these interfaces.
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IUserRepository, UserRepository>();
            //Notes
            services.AddScoped<INotesManger, FundooManager.Manager.NotesManager>();
            services.AddScoped<INotesRepository, NotesRepository>();
            //collabrtor
            services.AddScoped<ICollaboratorManager, CollaboratorManager>();
            services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
            //labels
            services.AddScoped<ILabelsManager, LabelsManager>();
            services.AddScoped<ILabelsRepository, LabelsRepository>();

            //this section registers services related to labels and their interactions with the data repository
            services.AddSwaggerGen(c =>
            {
                //This configures Swagger, a tool for documenting APIs. It defines the API's version and provides metadata like the title and description.
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fundoo App", Version = "v1", Description = "Fundoo Applcation" });

                //his section specifies how to use JWT(JSON Web Token) for authentication in Swagger.It describes the token format and how it should be included in the request header for authentication.
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                //This part defines a security requirement for Swagger.It specifies that requests to the API should be secured with the "Bearer" token defined earlier.
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                         {
                            Reference = new OpenApiReference

                             {

                                 Type=ReferenceType.SecurityScheme,

                                 Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
            //This code configures JWT-based authentication.It sets the default authentication and challenge schemes to use JWT.It also defines the parameters for token validation, including the key for signing and validating JWT tokens.
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {

                options.TokenValidationParameters = new TokenValidationParameters

                {

                    ValidateIssuer = false,

                    ValidateAudience = false,

                    ValidateLifetime = false,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Key"])) //Configuration["JwtToken:SecretKey"]

                };


            });
            //This configures a distributed cache using Redis, a popular in-memory data store. It specifies the Redis server's location and the instance name.
            //redish
            services.AddDistributedRedisCache(Options =>
            {
                Options.Configuration = "localhost:6379";
                Options.InstanceName = "Fundoocache";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //In the Configure method, the application's request processing pipeline is configured. If the environment is in development mode, it enables the developer exception page to display detailed error information.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //This line redirects HTTP requests to HTTPS, ensuring secure communication.
            app.UseHttpsRedirection();

            //cors platform between swagger and ui
            app.UseCors(x => x
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

            //These lines configure routing, authentication, and authorization for the application. They define the order in which these middleware components are applied to incoming requests.
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            //This section defines how endpoints are mapped to controllers.It specifies that controllers should handle incoming requests.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //These lines enable and configure Swagger UI, which provides a user-friendly interface for exploring and testing the API.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fundoo App");
            });
        }
    }
}




//In summary, the Startup class in this code is responsible for configuring the services, authentication, and request processing pipeline for an ASP.NET Core application.It also sets up Swagger for API documentation and Redis for distributed caching. The code follows best practices for separating concerns and using dependency injection for managing services and repositories related to users, notes, collaborators, and labels.

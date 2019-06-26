using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace WebApi
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
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                options.ReturnHttpNotAcceptable = true;
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IUserRepository, MongoUserRepository>();
            services.AddSingleton<IGameRepository, MongoGameRepository>();
            services.AddSingleton<ITurnsRepository, MongoTurnsRepository>();
            services.AddSingleton(p =>
            {
                var mongoConnectionString = Environment.GetEnvironmentVariable("PROJECT5100_MONGO_CONNECTION_STRING")
                                           ?? "mongodb://localhost:27017";
                var mongoClient = new MongoClient(mongoConnectionString);
                return mongoClient.GetDatabase("web-game-tests");
            });

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Player, PlayerDto>();
                cfg.CreateMap<UserEntity, UserDto>()
                .ForMember(destinationMember => destinationMember.FullName, 
                    opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"));
                cfg.CreateMap<CreatedUserDto, UserEntity>();
                cfg.CreateMap<UpdatedUserDto, UserEntity>();
                cfg.CreateMap<GameEntity, GameDto>().ForMember(d => d.Players, opt => opt.MapFrom(s => s.Players));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

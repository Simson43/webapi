using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApi.Samples
{
    public static class SwaggerExtensions
    {
        public static void AddSwaggerGeneration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // Создаем документ с описанием API
                c.SwaggerDoc("web-game", new OpenApiInfo
                {
                    Title = "Web Game API",
                    Version = "0.1",
                });

                c.DescribeAllEnumsAsStrings();

                // Конфигурируем Swashbuckle, чтобы использовались Xml Documentation Comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Конфигурируем Swashbuckle, чтобы работали атрибуты
                c.EnableAnnotations();
            });
        }

        public static void UseSwaggerWithUI(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/web-game/swagger.json", "Web Game API");
                c.RoutePrefix = string.Empty;
            });
        }

        public static string GetSwaggerDocument(this IWebHost host, string documentName)
        {
            var sw = (ISwaggerProvider)host.Services.GetService(typeof(ISwaggerProvider));
            var doc = sw.GetSwagger(documentName);

            return JsonConvert.SerializeObject(
                doc,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
        }
    }
}
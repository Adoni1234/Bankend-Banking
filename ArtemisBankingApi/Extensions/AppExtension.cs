namespace ArtemisBankingApi.Extensions
{
    public static class AppExtension
    {
        // Hace que la APP use swagger
        public static void UseSwaggerExtension(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder) {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                var versionDescriptions = routeBuilder.DescribeApiVersions(); // Obtiene las diferentes versiones
                if (versionDescriptions != null && versionDescriptions.Any()) {
                    
                    foreach (var apiVersion in versionDescriptions) { // itera por cada version y genera una URL
                        var url = $"/swagger/{apiVersion.GroupName}/swagger.json";
                        var name = $"ArtemisBanking API - {apiVersion.GroupName.ToUpperInvariant()}";
                        opt.SwaggerEndpoint(url,name);
                    }                
                }
            });
        }
    }
}

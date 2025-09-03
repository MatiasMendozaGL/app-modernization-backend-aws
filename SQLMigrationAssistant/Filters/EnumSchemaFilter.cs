using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SQLMigrationAssistant.API.Filters
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                schema.Type = "string";
                schema.Format = null;

                var enumNames = Enum.GetNames(context.Type);
                foreach (var enumName in enumNames)
                {
                    schema.Enum.Add(new OpenApiString(enumName));
                }
            }
        }
    }
}

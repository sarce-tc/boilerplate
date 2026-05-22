using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Microservice.Application.Common.Validation
{
    public static class UpdateFieldValidator
    {
        private static readonly HashSet<string> ForbiddenNames = new()
        {
            "Id", "CreatedAt", "UpdatedAt"
        };

        public static void Validate<T>(Expression<Func<T, object>>[] properties)
        {
            var type = typeof(T);
            var forbidden = GetForbiddenProperties(type);

            foreach (var prop in properties)
            {
                var memberBody = prop.Body switch
                {
                    UnaryExpression unary => unary.Operand as MemberExpression,
                    MemberExpression member => member,
                    _ => throw new InvalidOperationException("Expresión inválida")
                };

                if (memberBody == null)
                    throw new InvalidOperationException("No se pudo resolver la propiedad");

                var name = memberBody.Member.Name;

                if (forbidden.Contains(name))
                    throw new InvalidOperationException($"No se permite modificar el campo '{name}'");
            }
        }

        private static HashSet<string> GetForbiddenProperties(Type type)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    ForbiddenNames.Contains(p.Name) ||
                    p.GetCustomAttribute<KeyAttribute>() is not null ||
                    p.GetCustomAttribute<DatabaseGeneratedAttribute>() is not null)
                .Select(p => p.Name)
                .ToHashSet();
        }
    }
}

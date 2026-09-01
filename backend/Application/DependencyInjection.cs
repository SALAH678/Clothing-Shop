using Application.Common.Behaviours;
using FluentValidation;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddMediatR(cnf =>
        {
            cnf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cnf.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cnf.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cnf.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            cnf.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

        return services;
    }
}

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Data;
using ExpensesBook.Domain.Calculators;
using ExpensesBook.Domain.Repositories;
using ExpensesBook.Domain.Services;
using ExpensesBook.LocalStorageRepositories;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace ExpensesBook;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddMudServices();

        #region register services

        AddRepositories(builder.Services);

        AddAppServices(builder.Services);

        builder.Services.AddTransient<IJsonData, JsonData>();

        AddCalculators(builder.Services);

        #endregion

        await builder.Build().RunAsync();
    }

    private static void AddCalculators(IServiceCollection services)
    {
        services.AddTransient<PeriodExpenseCalculator>();
        services.AddTransient<CashBalanceCalculator>();
        services.AddTransient<LimitsCalculator>();
        services.AddTransient<YearExpensesCalculator>();
    }

    private static void AddAppServices(IServiceCollection services)
    {
        services.AddTransient<ICategoriesService, CategoriesService>();
        services.AddTransient<IGroupsService, GroupsService>();
        services.AddTransient<IExpensesService, ExpensesService>();
        services.AddTransient<IIncomesService, IncomesService>();
        services.AddTransient<ILimitsService, LimitsService>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        services.AddScoped<IGroupsRepository, GroupsRepository>();
        services.AddScoped<IGroupDefaultCategoryRepository, GroupDefaultCategoryRepository>();
        services.AddScoped<IExpensesRepository, ExpensesRepository>();
        services.AddScoped<IIncomesRepository, IncomesRepository>();
        services.AddScoped<ILimitsRepository, LimitsRepository>();
    }
}

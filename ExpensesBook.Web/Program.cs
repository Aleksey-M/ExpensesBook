using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.App;
using ExpensesBook.Data;
using ExpensesBook.Domain.Calculators;
using ExpensesBook.Domain.Repositories;
using ExpensesBook.Domain.Services;
//using ExpensesBook.LocalStorageRepositories;
using ExpensesBook.IndexedDbRepositories;
using IdbLib;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace ExpensesBook.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<MainApp>("#app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddScoped<IndexedDbManager>();
        builder.Services.AddMudServices();

        #region register services

        AddRepositories(builder.Services);

        AddAppServices(builder.Services);

        builder.Services.AddScoped<IGlobalDataManager, GlobalDataManager>();

        AddCalculators(builder.Services);

        #endregion

        await builder.Build().RunAsync();
    }

    private static void AddCalculators(IServiceCollection services)
    {
        services.AddScoped<PeriodExpenseCalculator>();
        services.AddScoped<CashBalanceCalculator>();
        services.AddScoped<LimitsCalculator>();
        services.AddScoped<YearExpensesCalculator>();
    }

    private static void AddAppServices(IServiceCollection services)
    {
        services.AddScoped<ICategoriesService, CategoriesService>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<IExpensesService, ExpensesService>();
        services.AddScoped<IIncomesService, IncomesService>();
        services.AddScoped<ILimitsService, LimitsService>();
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

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Data;
using ExpensesBook.Domain.Calculators;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using ExpensesBook.Domain.Services;
using ExpensesBook.LocalStorageRepositories;
using ExpensesBook.LocalStorageRepositories2;
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

        AddEntities(builder.Services);

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

    private static void AddEntities(IServiceCollection services)
    {
        services.AddTransient<ICategoriesService, CategoriesService>();
        services.AddTransient<IGroupsService, GroupsService>();
        services.AddTransient<IExpensesService, ExpensesService>();
        services.AddTransient<IIncomesService, IncomesService>();
        services.AddTransient<ILimitsService, LimitsService>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        //services.AddTransient<ICategoriesRepository, CategoriesRepository>();
        //services.AddTransient<ICategoriesListRepository, CategoriesRepository>();
        //services.AddTransient<ILocalStorageGenericRepository<Category>, CategoriesRepository>();

        services.AddScoped<ICategoriesRepository, LocalStorageRepositories2.CategoriesRepository>();
        services.AddScoped<ICategoriesListRepository, LocalStorageRepositories2.CategoriesListRepository>();

        services.AddTransient<IGroupsRepository, GroupsRepository>();
        services.AddTransient<IGroupsListRepository, GroupsRepository>();
        services.AddTransient<ILocalStorageGenericRepository<Group>, GroupsRepository>();
        services.AddTransient<IGroupDefaultCategoryRepository, GroupDefaultCategoryRepository>();
        services.AddTransient<ILocalStorageGenericRepository<GroupDefaultCategory>, GroupDefaultCategoryRepository>();

        services.AddTransient<IExpensesRepository, ExpensesRepository>();
        services.AddTransient<ILocalStorageGenericRepository<Expense>, ExpensesRepository>();

        services.AddTransient<IIncomesRepository, IncomesRepository>();
        services.AddTransient<ILocalStorageGenericRepository<Income>, IncomesRepository>();

        services.AddTransient<ILimitsRepository, LimitsRepository>();
        services.AddTransient<ILocalStorageGenericRepository<Limit>, LimitsRepository>();
    }
}

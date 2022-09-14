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

        builder.Services.AddTransient<ICategoriesRepository, CategoriesRepository>();
        builder.Services.AddTransient<ILocalStorageGenericRepository<Category>, CategoriesRepository>();
        builder.Services.AddTransient<ICategoriesService, CategoriesService>();

        builder.Services.AddTransient<IGroupsRepository, GroupsRepository>();
        builder.Services.AddTransient<ILocalStorageGenericRepository<Group>, GroupsRepository>();
        builder.Services.AddTransient<IGroupDefaultCategoryRepository, GroupDefaultCategoryRepository>();
        builder.Services.AddTransient<ILocalStorageGenericRepository<GroupDefaultCategory>, GroupDefaultCategoryRepository>();
        builder.Services.AddTransient<IGroupsService, GroupsService>();

        builder.Services.AddTransient<IExpensesRepository, ExpensesRepository>();
        builder.Services.AddTransient<ILocalStorageGenericRepository<Expense>, ExpensesRepository>();
        builder.Services.AddTransient<IExpensesService, ExpensesService>();

        builder.Services.AddTransient<IIncomesRepository, IncomesRepository>();
        builder.Services.AddTransient<ILocalStorageGenericRepository<Income>, IncomesRepository>();
        builder.Services.AddTransient<IIncomesService, IncomesService>();

        builder.Services.AddTransient<ILimitsRepository, LimitsRepository>();
        builder.Services.AddTransient<ILocalStorageGenericRepository<Limit>, LimitsRepository>();
        builder.Services.AddTransient<ILimitsService, LimitsService>();

        builder.Services.AddTransient<IJsonData, JsonData>();

        builder.Services.AddTransient<PeriodExpenseCalculator>();
        builder.Services.AddTransient<CashBalanceCalculator>();
        builder.Services.AddTransient<LimitsCalculator>();
        builder.Services.AddTransient<YearExpensesCalculator>();

        #endregion

        await builder.Build().RunAsync();
    }
}

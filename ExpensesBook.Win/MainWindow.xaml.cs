using System.Windows;
using Blazored.LocalStorage;
using ExpensesBook.Data;
using ExpensesBook.Domain.Calculators;
using ExpensesBook.Domain.Repositories;
using ExpensesBook.Domain.Services;
using ExpensesBook.LocalStorageRepositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace ExpensesBook.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBlazorWebViewDeveloperTools();

            serviceCollection.AddBlazoredLocalStorage();
            serviceCollection.AddMudServices();

            AddRepositories(serviceCollection);

            AddAppServices(serviceCollection);

            serviceCollection.AddScoped<IGlobalDataManager, GlobalDataManager>();

            AddCalculators(serviceCollection);

            Resources.Add("services", serviceCollection.BuildServiceProvider());
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
}

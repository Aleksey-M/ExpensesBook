using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public sealed class IncomesRepository : BaseIndexedDbRepository<Income>, IIncomesRepository
{
    protected override string CollectionName => "incomes";

    public IncomesRepository(IndexedDbManager manager) : base(manager)
    {
    }

    public async Task AddIncome(Income income) => await AddEntity(income);

    public async Task DeleteIncome(Guid incomeId) => await DeleteEntity(incomeId);

    public async Task<List<Income>> GetIncomes(CancellationToken token) => await GetCollection() ?? new();

    public async Task UpdateIncome(Income income) => await UpdateEntity(income);

    public async Task AddIncomes(IEnumerable<Income> incomes) => await SetCollection(incomes.ToList());

    public async Task Clear() => await Clear<List<Income>>();
}

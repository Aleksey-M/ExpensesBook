﻿using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

public sealed class IncomesRepository : BaseLocalStorageRepository<Income>, IIncomesRepository
{
    protected override string CollectionName => "incomes";

    public IncomesRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task AddIncome(Income income) => await AddEntity(income);

    public async Task DeleteIncome(Guid incomeId) => await DeleteEntity(incomeId);

    public async Task<List<Income>> GetIncomes(CancellationToken token) => await GetCollection(token) ?? new();

    public async Task UpdateIncome(Income income) => await UpdateEntity(income);

    public async Task AddIncomes(IEnumerable<Income> incomes) => await SetCollection(incomes.ToList());

    public async Task Clear() => await Clear<List<Income>>();
}

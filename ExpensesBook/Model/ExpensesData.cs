using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExpensesBook.Model
{
    internal class ExpensesData
    {
        private readonly List<ExpensesCategory> _categories = new List<ExpensesCategory>();
        private readonly List<ExpensesGroup> _groups = new List<ExpensesGroup>();
        private readonly List<ExpenseItem> _expenses = new List<ExpenseItem>();
        private readonly List<Limit> _limits = new List<Limit>();
        private readonly List<SavingsItem> _savings = new List<SavingsItem>();

        public ReadOnlyCollection<ExpensesCategory> Categories { get; private set; }
        public ReadOnlyCollection<ExpensesGroup> Groups { get; private set; }
        public ReadOnlyCollection<ExpenseItem> Expenses { get; private set; }
        public ReadOnlyCollection<Limit> Limits { get; private set; }
        public ReadOnlyCollection<SavingsItem> Savings { get; private set; }
        public bool IsDataUpdated {get; set;}

        public ExpensesData() 
        {
            Categories = new ReadOnlyCollection<ExpensesCategory>(_categories);
            Groups = new ReadOnlyCollection<ExpensesGroup>(_groups);
            Expenses = new ReadOnlyCollection<ExpenseItem>(_expenses);
            Limits = new ReadOnlyCollection<Limit>(_limits);
            Savings = new ReadOnlyCollection<SavingsItem>(_savings);
        }
        
        private class ExpensesDataSerializable
        {
            public List<ExpensesCategory> Categories { get; set; }
            public List<ExpensesGroup> Groups { get; set; }
            public List<ExpenseItem> Expenses { get; set; }
            public List<Limit> Limits { get; set; }
            public List<SavingsItem> Savings { get; set; }
        }

        public string SerializeToJson()
        {
            var serializableData = new ExpensesDataSerializable
            {
                Categories = this.Categories.ToList(),
                Groups = Groups.ToList(),
                Expenses = Expenses.ToList(),
                Limits = Limits.ToList(),
                Savings = this.Savings.ToList()
            };
            return System.Text.Json.JsonSerializer.Serialize(serializableData);
        }

        public void DeserializeAndUpdateFromJson(string json)
        {
            var deserializedData = System.Text.Json.JsonSerializer.Deserialize<ExpensesDataSerializable>(json);
            
            _categories.Clear();
            _categories.AddRange(deserializedData?.Categories ?? Enumerable.Empty<ExpensesCategory>());

            _groups.Clear();
            _groups.AddRange(deserializedData?.Groups ?? Enumerable.Empty<ExpensesGroup>());

            _expenses.Clear();
            _expenses.AddRange(deserializedData?.Expenses ?? Enumerable.Empty<ExpenseItem>());

            _limits.Clear();
            _limits.AddRange(deserializedData?.Limits ?? Enumerable.Empty<Limit>());

            _savings.Clear();
            _savings.AddRange(deserializedData?.Savings ?? Enumerable.Empty<SavingsItem>());

            IsDataUpdated = false;
        }

        public bool AddCategory(string categoryName)
        {
            if (Categories.Any(c => c.Name.Equals(categoryName, System.StringComparison.OrdinalIgnoreCase))) return false;
            
                var newCategory = new ExpensesCategory
                {
                    Id = Guid.NewGuid(),
                    Sort = Categories.Select(c => c.Sort).DefaultIfEmpty().Max() + 1,
                    Name = categoryName
                };
                _categories.Add(newCategory);

            IsDataUpdated = true;
            return true;
        }

        public bool UpdateCategoryName(Guid id, string categoryNewName)
        {
            if (Categories.Any(c => c.Name.Equals(categoryNewName, StringComparison.OrdinalIgnoreCase))) return false;

            var cat = Categories.SingleOrDefault(c => c.Id == id);
            if (cat == null) return false;

            cat.Name = categoryNewName;
            IsDataUpdated = true;
            return true;
        }

        public bool DeleteCategory(Guid id)
        {
            var cat = Categories.SingleOrDefault(c => c.Id == id);
            if (cat == null) return false;

            if (Expenses.Any(e => e.CategoryId == id)) return false;

            _categories.Remove(cat);
            IsDataUpdated = true;
            return true;
        }

        public bool AddGroup(string groupName)
        {
            if (Groups.Any(c => c.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))) return false;

            var newGroup = new ExpensesGroup
            {
                Id = Guid.NewGuid(),
                Sort = Groups.Select(c => c.Sort).DefaultIfEmpty().Max() + 1,
                Name = groupName
            };
            _groups.Add(newGroup);
            IsDataUpdated = true;
            return true;
        }

        public bool UpdateGroupName(Guid id, string groupNewName)
        {
            if (Groups.Any(c => c.Name.Equals(groupNewName, StringComparison.OrdinalIgnoreCase))) return false;

            var group = Groups.SingleOrDefault(c => c.Id == id);
            if (group == null) return false;

            group.Name = groupNewName;
            IsDataUpdated = true;
            return true;
        }

        public bool DeleteGroup(Guid id)
        {
            var group = Groups.SingleOrDefault(c => c.Id == id);
            if (group == null) return false;

            foreach(var exp in _expenses)
            {
                if(exp.GroupId == id)
                {
                    exp.GroupId = null;
                }
            }

            _groups.Remove(group);
            IsDataUpdated = true;
            return true;
        }

        public bool AddExpenseItem(ExpenseItemDto expenseDto)
        {
            var expense = new ExpenseItem
            {
                Id = Guid.NewGuid(),
                Description = expenseDto.Description,
                Date = expenseDto.Date,
                Amounth = expenseDto.Amounth,
                CategoryId = Guid.Parse(expenseDto.CategoryId),
                GroupId = expenseDto.GroupId == null ? (Guid?)null : Guid.Parse(expenseDto.GroupId)
            };

            _expenses.Add(expense);
            IsDataUpdated = true;
            return true;
        }

        public bool UpdateExpenseItem(ExpenseItemDto expenseDto)
        {
            var exp = Expenses.SingleOrDefault(e => e.Id.ToString() == expenseDto.Id);
            if (exp == null) return false;

            exp.Description = expenseDto.Description;
            exp.Date = expenseDto.Date;
            exp.Amounth = expenseDto.Amounth;
            exp.CategoryId = string.IsNullOrEmpty(expenseDto.CategoryId) ? exp.CategoryId : Guid.Parse(expenseDto.CategoryId);
            exp.GroupId = string.IsNullOrEmpty(expenseDto.GroupId) ? (Guid?)null : Guid.Parse(expenseDto.GroupId);

            IsDataUpdated = true;
            return true;
        }

        public bool DeleteExpenseItem(string guidId)
        {
            var exp = Expenses.SingleOrDefault(e => e.Id.ToString() == guidId);
            if (exp == null) return false;

            _expenses.Remove(exp);

            IsDataUpdated = true;
            return true;
        }

        public bool AddLimit(LimitDto limitDto)
        {
            if (limitDto.EndExcluded < limitDto.StartIncluded) return false;

            var newLimit = new Limit
            {
                Id = Guid.NewGuid(),
                Description = limitDto.Description,
                LimitAmounth = limitDto.LimitAmounth,
                StartIncluded = limitDto.StartIncluded,
                EndExcluded = limitDto.EndExcluded
            };

            _limits.Add(newLimit);

            IsDataUpdated = true;
            return true;
        }

        public bool UpdateLimit(LimitDto limitDto)
        {
            var limit = Limits.SingleOrDefault(l => l.Id.ToString() == limitDto.Id);
            if (limit == null) return false;

            if (limitDto.EndExcluded < limitDto.StartIncluded) return false;

            limit.Description = limitDto.Description;
            limit.LimitAmounth = limitDto.LimitAmounth;
            limit.StartIncluded = limitDto.StartIncluded;
            limit.EndExcluded = limitDto.EndExcluded;

            IsDataUpdated = true;
            return true;
        }

        public bool DeleteLimit(string guidId)
        {
            var limit = Limits.SingleOrDefault(l => l.Id.ToString() == guidId);
            if (limit == null) return false;

            _limits.Remove(limit);
            IsDataUpdated = true;
            return true;
        }

        public bool AddSaving(SavingsDto newSaving)
        {
            if (Savings.Any(s => s.Year == newSaving.Year && s.Month == newSaving.Month)) return false;
            if (newSaving.Income < 0) return false;
            if (newSaving.Month > 12 || newSaving.Month < 1) return false;

            var saving = new SavingsItem
            {
                Id = Guid.NewGuid(),
                Description = newSaving.Description,
                Year = newSaving.Year,
                Month = newSaving.Month,
                Income = newSaving.Income
            };

            _savings.Add(saving);

            IsDataUpdated = true;
            return true;
        }

        public bool UpdateSaving(SavingsDto updatedSaving)
        {            
            if (updatedSaving.Income <= 0) return false;
            if (updatedSaving.Month > 12 || updatedSaving.Month < 1) return false;

            var sav = Savings.SingleOrDefault(s => s.Year == updatedSaving.Year && s.Month == updatedSaving.Month);
            if (sav != null && sav.Id.ToString() != updatedSaving.Id) return false;

            sav = Savings.SingleOrDefault(s => s.Id.ToString() == updatedSaving.Id);
            if (sav == null) return false;

            sav.Description = updatedSaving.Description;
            sav.Year = updatedSaving.Year;
            sav.Month = updatedSaving.Month;
            sav.Income = updatedSaving.Income;
            
            IsDataUpdated = true;
            return true;
        }

        public bool DeleteSaving(string savingId)
        {
            var saving = Savings.SingleOrDefault(s => s.Id.ToString() == savingId);
            if (saving == null) return false;

            _savings.Remove(saving);
            IsDataUpdated = true;
            return true;
        }
    }    
}

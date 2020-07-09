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

        public ReadOnlyCollection<ExpensesCategory> Categories { get; private set; }
        public ReadOnlyCollection<ExpensesGroup> Groups { get; private set; }
        public ReadOnlyCollection<ExpenseItem> Expenses { get; private set; }
        public ReadOnlyCollection<Limit> Limits { get; private set; }

        public ExpensesData() 
        {
            Categories = new ReadOnlyCollection<ExpensesCategory>(_categories);
            Groups = new ReadOnlyCollection<ExpensesGroup>(_groups);
            Expenses = new ReadOnlyCollection<ExpenseItem>(_expenses);
            Limits = new ReadOnlyCollection<Limit>(_limits);
        }
        
        private class ExpensesDataSerializable
        {
            public List<ExpensesCategory> Categories { get; set; }
            public List<ExpensesGroup> Groups { get; set; }
            public List<ExpenseItem> Expenses { get; set; }
            public List<Limit> Limits { get; set; }
        }

        public string SerializeToJson()
        {
            var serializableData = new ExpensesDataSerializable
            {
                Categories = this.Categories.ToList(),
                Groups = Groups.ToList(),
                Expenses = Expenses.ToList(),
                Limits = Limits.ToList()
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
            return true;
        }

        public bool UpdateCategoryName(Guid id, string categoryNewName)
        {
            if (Categories.Any(c => c.Name.Equals(categoryNewName, StringComparison.OrdinalIgnoreCase))) return false;

            var cat = Categories.SingleOrDefault(c => c.Id == id);
            if (cat == null) return false;

            cat.Name = categoryNewName;
            return true;
        }

        public bool DeleteCategory(Guid id)
        {
            var cat = Categories.SingleOrDefault(c => c.Id == id);
            if (cat == null) return false;

            if (Expenses.Any(e => e.CategoryId == id)) return false;

            _categories.Remove(cat);
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
            return true;
        }

        public bool UpdateGroupName(Guid id, string groupNewName)
        {
            if (Groups.Any(c => c.Name.Equals(groupNewName, StringComparison.OrdinalIgnoreCase))) return false;

            var group = Groups.SingleOrDefault(c => c.Id == id);
            if (group == null) return false;

            group.Name = groupNewName;
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
            return true;
        }

    }    
}

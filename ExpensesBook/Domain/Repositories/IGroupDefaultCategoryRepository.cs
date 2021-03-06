﻿using ExpensesBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpensesBook.Domain.Repositories
{
    internal interface IGroupDefaultCategoryRepository
    {
        ValueTask AddGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories);
        ValueTask DeleteGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories);
        ValueTask<List<GroupDefaultCategory>> GetGroupDefaultCategories(Guid? categoryId, Guid? groupId);
    }
}

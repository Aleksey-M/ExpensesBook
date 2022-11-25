using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public static class CollectionsNames
{
    public const string Categories = "categories";

    public const string Expenses = "expenses";

    public const string GroupDefaultCategories = "groupdefaultcategories";

    public const string Groups = "groups";

    public const string Incomes = "incomes";

    public const string Limits = "limits";

    public static IEnumerable<ObjectStoreInfo> ObjectStores
    {
        get
        {
            yield return new ObjectStoreInfo { Name = Categories, KeyPath = "id" };
            yield return new ObjectStoreInfo { Name = Expenses, KeyPath = "id" };
            yield return new ObjectStoreInfo { Name = Groups, KeyPath = "id" };
            yield return new ObjectStoreInfo { Name = GroupDefaultCategories, KeyPath = "id" };
            yield return new ObjectStoreInfo { Name = Incomes, KeyPath = "id" };
            yield return new ObjectStoreInfo { Name = Limits, KeyPath = "id" };
        }
    }
}

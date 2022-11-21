namespace ExpensesBook.Pages;

public enum PageState { Unknown, List, Edit, Add }

public static class PageStateCommonActions
{
    internal static string SaveActionName(this PageState state) => state switch
    {
        PageState.Edit => "Сохранить",
        PageState.Add => "Добавить",
        _ => ""
    };
}

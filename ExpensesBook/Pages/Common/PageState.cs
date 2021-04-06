namespace ExpensesBook.Pages
{
    internal enum PageState { Unknown, List, Edit, Add }

    internal static class PageStateCommonActions
    {
        internal static string SaveActionName(this PageState state) => state switch
        {
            PageState.Edit => "Сохранить",
            PageState.Add => "Добавить",
            _ => ""
        };
    }
}

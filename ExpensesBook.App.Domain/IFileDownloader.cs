namespace ExpensesBook.Data;

public interface IFileDownloader
{
    Task SaveFile(string fileName, string base64String);

    Task SaveJsonFile(string fileName, string jsonString);
}

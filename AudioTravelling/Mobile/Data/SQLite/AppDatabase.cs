using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _connection;

    public AppDatabase(string dbPath)
    {
        _connection = new SQLiteAsyncConnection(dbPath);
    }

    public SQLiteAsyncConnection Connection => _connection;
}
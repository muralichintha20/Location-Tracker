using LocationHeatMap.Models;
using Microsoft.Maui.Storage;
using SQLite;

namespace LocationHeatMap;

public class LocationRepository
{
    private readonly Lazy<SQLiteAsyncConnection> _lazyConn;

    public LocationRepository()
    {
        _lazyConn = new Lazy<SQLiteAsyncConnection>(() =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
            return new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        });
    }

    private SQLiteAsyncConnection Conn => _lazyConn.Value;

    public async Task InitAsync()
    {
        await Conn.CreateTableAsync<LocationPoint>();
    }

    public async Task InsertAsync(LocationPoint point)
    {
        await Conn.InsertAsync(point);
    }

    public async Task<List<LocationPoint>> GetAllAsync()
    {
        return await Conn.Table<LocationPoint>()
            .OrderBy(p => p.TimestampUtc)
            .ToListAsync();
    }

    public async Task ClearAsync()
    {
        await Conn.DeleteAllAsync<LocationPoint>();
    }
}
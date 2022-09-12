using Domain;
using FluentResults;
using Npgsql;

namespace Database;

public struct HistoryDatabaseRow
{
    public Int32 Id;
    public String Query;
    public Boolean Valid;
    public Double Result;
}

public class DatabaseHistoryRepository
{
    private DatabaseConnectionProvider _connectionProvider;
    
    private String _historyTableName = "Calculations";
    private String _historyIdColumn = "id";
    private String _historyQueryColumn = "query";
    private String _historyValidColumn = "valid";
    private String _historyResultColumn = "result";

    private String GetSelectHistoryString(Int32 id)
    {
        return String.Format("SELECT {1}, {2}, {3}, {4} FROM {0} WHERE {1} = {5}",
            _historyTableName, _historyIdColumn, _historyQueryColumn, _historyValidColumn, _historyResultColumn,
            id);
    }

    private String GetInsertHistoryString(Int32 id, String query, Boolean valid, Double result)
    {
        return String.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES ({5}, '{6}', {7}, {8})",
            _historyTableName, _historyIdColumn, _historyQueryColumn, _historyValidColumn, _historyResultColumn,
            id, query, valid, result);
    }
    
    public DatabaseHistoryRepository(String databaseUser, String password, String databaseName)
    {
        _connectionProvider = new DatabaseConnectionProvider(databaseUser, password, databaseName);
    }

    public List<HistoryDatabaseRow> Get(Int32 key)
    {
        String getHistoryString = GetSelectHistoryString(key);

        NpgsqlCommand selectIdCommand = new NpgsqlCommand(getHistoryString, _connectionProvider.GetConnection());
        NpgsqlDataReader reader = selectIdCommand.ExecuteReader();
        List<HistoryDatabaseRow> history = new List<HistoryDatabaseRow>();
        
        while (reader.HasRows)
        {
            int idColumn = 0, queryColumn = 1, validColumn = 2, resultColumn = 3;
            while (reader.Read())
            {
                HistoryDatabaseRow row = new HistoryDatabaseRow();
                row.Id = reader.GetInt32(idColumn);
                row.Query = reader.GetString(queryColumn);
                row.Valid = reader.GetBoolean(validColumn);
                row.Result = reader.GetDouble(resultColumn);
                history.Add(row);
            }

            reader.NextResult();
        }

        return history;
    }

    public void Update(HistoryDatabaseRow value)
    {
        String insertHistoryString = GetInsertHistoryString(value.Id, value.Query, value.Valid, value.Result);
        
        NpgsqlCommand insertHistoryCommand = new NpgsqlCommand(insertHistoryString, _connectionProvider.GetConnection());
        int ret = insertHistoryCommand.ExecuteNonQuery();

        if (ret == -1) throw new NpgsqlException("ExecuteNonQuery() inside insert failed");
    }
}
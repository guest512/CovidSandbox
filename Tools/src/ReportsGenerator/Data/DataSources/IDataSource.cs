namespace ReportsGenerator.Data.DataSources
{
    public interface IDataSource
    {
        IDataSourceReader GetReader();
    }
}
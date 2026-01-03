using System.Threading.Tasks;

public interface IAccountBasedDataGatewayService
{
    void RegisterDataModel(IWritableModel model);
    void UnRegisterDataModel(IWritableModel model);
    void ClearModels();

    Task<bool> WriteData(string accountId, string dataKey, bool clearAll);
    Task<bool> ReadData(string accountId, string dataKey);
    string GetData(string model_id);
}

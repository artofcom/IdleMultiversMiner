using System.Threading.Tasks;

public interface IDataGatewayService 
{
    string AccountId { get; set; }
    bool IsDirty { get; set; }

    void RegisterDataModel(IWritableModel model);
    void UnRegisterDataModel(IWritableModel model);
    void ClearModels();

    Task<bool> WriteData(string dataKey, bool clearAll);
    Task<bool> ReadData(string dataKey);
    string GetData(string model_id);
}

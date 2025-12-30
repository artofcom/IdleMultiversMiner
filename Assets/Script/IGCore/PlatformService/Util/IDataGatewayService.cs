using System.Threading.Tasks;

public interface IDataGatewayService 
{
    void RegisterDataModel(IWritableModel model);
    void UnRegisterDataModel(IWritableModel model);
    void ClearModels();

    Task WriteData(string filePath, bool clearAll);
    Task ReadData(string filePath);
    string GetData(string model_id);
}

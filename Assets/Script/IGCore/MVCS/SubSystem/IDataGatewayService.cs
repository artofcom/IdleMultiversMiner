using UnityEngine;

public interface IDataGatewayService 
{
    void RegisterDataModel(IWritableModel model);
    void UnRegisterDataModel(IWritableModel model);
    void ClearModels();

    void WriteData(string filePath, bool clearAll);
    void ReadData(string filePath);
    string GetData(string model_id);
}

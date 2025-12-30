using System.Collections.Generic;
using System;

public interface IWritableModel
{
    // Get current status of the Model
    // multi setter -- Key, data.
    List<Tuple<string, string>> GetSaveDataWithKeys();
}
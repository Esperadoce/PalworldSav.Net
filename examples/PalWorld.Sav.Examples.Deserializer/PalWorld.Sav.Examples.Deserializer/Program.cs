

using System;
using PalWorld.Sav.Serializer;
using System.Collections.Generic;

var result = await SavFileHandler.AnalyseSavAsync("LevelMeta.sav");

Console.WriteLine($"Sav File Data : {result}");

var dictionary = new Dictionary<string, string>
{
    {".worldSaveData.CharacterSaveParameterMap.Key", "Struct"},
    {".worldSaveData.FoliageGridSaveDataMap.Key", "Struct"},
    {".worldSaveData.FoliageGridSaveDataMap.ModelMap.InstanceDataMap.Key", "Struct"},
    {".worldSaveData.MapObjectSpawnerInStageSaveData.Key", "Struct"},
    {".worldSaveData.ItemContainerSaveData.Key", "Struct"},
    {".worldSaveData.CharacterContainerSaveData.Key", "Struct"}
};

var jsonData = await UeSave.DeserializeAsync(result.DecompressedData.ToArray(), dictionary);

Console.WriteLine("JsonData :");
Console.WriteLine(jsonData);

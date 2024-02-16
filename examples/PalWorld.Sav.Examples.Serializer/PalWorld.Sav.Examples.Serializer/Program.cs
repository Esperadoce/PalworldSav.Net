using System;
using System.IO;
using PalWorld.Sav.Serializer;


string jsonFilePath = "example.json";
string content = File.ReadAllText(jsonFilePath);
var data = await UeSave.SerializeAsync(content);

string filePath = $"example.{Guid.NewGuid()}.sav";
await SavFileHandler.WriteSavAsync(new SaveFileFormatWriter(filePath, data), true);


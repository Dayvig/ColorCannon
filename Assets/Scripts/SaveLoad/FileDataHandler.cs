using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        dataToLoad = sr.ReadToEnd();
                    }
                }
                loadedData = JsonConvert.DeserializeObject<GameData>(dataToLoad);
                if (loadedData.chunks != null)
                {
                    for (int i = 0; i < loadedData.chunks.Count; i++)
                    {
                        Debug.Log(loadedData.chunks.Count);
                        loadedData.chunks[i] = WaveSpawningSystem.instance.returnSpecialChunkFromGeneric(loadedData.chunks[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred while trying to load data :" + fullPath + "\n" + e);
            }

        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        Debug.Log("Saving Game " + data.currentLevel);
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string DataToStore = JsonConvert.SerializeObject(data);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(DataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occurred while trying to save data to file :" + fullPath + "\n" + e);
        }
    }
}

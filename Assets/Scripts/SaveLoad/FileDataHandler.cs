using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private string settingDirPath = "";
    private string settingsFileName = "";
    private string unlockDirPath = "";
    private string unlockFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName, string settingDirPath, string settingsFileName, string unlockDirPath, string unlockFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.settingDirPath = settingDirPath;
        this.settingsFileName = settingsFileName;
        this.unlockDirPath = unlockDirPath;
        this.unlockFileName = unlockFileName;
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
                        loadedData.chunks[i] = WaveSpawningSystem.instance.returnSpecialChunkFromGeneric(loadedData.chunks[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred while trying to load data :" + fullPath + "\n" + e);
            }
            loadedData.undiscoveredEasyMechanics = loadedData.undiscoveredEasyMechanics.Distinct().ToList();
            loadedData.undiscoveredMedMechanics = loadedData.undiscoveredMedMechanics.Distinct().ToList();
            loadedData.currentMechanics = loadedData.currentMechanics.Distinct().ToList();

        }

        return loadedData;
    }

    public SettingsData LoadSettings()
    {
        string fullPath = Path.Combine(settingDirPath, settingsFileName);
        SettingsData loadedData = null;
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
                loadedData = JsonConvert.DeserializeObject<SettingsData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred while trying to load data :" + fullPath + "\n" + e);
            }

        }

        return loadedData;

    }

    public UnlockData LoadUnlocks()
    {
        string fullPath = Path.Combine(unlockDirPath, unlockFileName);
        UnlockData loadedData = null;
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
                loadedData = JsonConvert.DeserializeObject<UnlockData>(dataToLoad);
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
        if (!SaveLoadManager.instance.isAndroidBuild)
        {
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
        else
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                string DataToStore = JsonConvert.SerializeObject(data);
                using (StreamWriter writer = new StreamWriter(fullPath))
                {
                    writer.Write(DataToStore);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred while trying to save data to file :" + fullPath + "\n" + e);
            }

        }
    }

    public void SaveSettings(SettingsData data)
    {
        string fullPath = Path.Combine(settingDirPath, settingsFileName);
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

    public void SaveUnlocks(UnlockData data)
    {
        string fullPath = Path.Combine(unlockDirPath, unlockFileName);
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

//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    //private variables
    private string dataDirPath = "";
    private string dataFileName = "";

    //constructor
    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    //loading files
    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);  //using Path.Combine to allow different path separators on different systems
        GameData loadedData = null;
        if(File.Exists(fullPath))
        {
            try
            {
                //load the serialized data from the Json 
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //deserializing the data back to C# from the Json
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e) //error catching/handling + logging to help us know the problem
            {
                Debug.LogError("Error occurred when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    //saving files
    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);  //using Path.Combine to allow different path separators on different systems
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));  //creating the directory if doesn't already exist
            
            string dataToStore = JsonUtility.ToJson(data, true); //serializing our current C# data into Json

            //write the serialized Json data to the file
            using(FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        
        }
        catch (Exception e) //error catching/handling + logging to help us know the problem
        {
            Debug.LogError("Error occurred when trying to save data to file: " +  fullPath + "\n" + e);
        }
    }
}

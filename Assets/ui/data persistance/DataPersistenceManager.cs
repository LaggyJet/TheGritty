//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string FileName;
    [SerializeField] private bool useEncryption;

    //private variables
    public static GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    public FileDataHandler dataHandler;
    public static DataPersistenceManager Instance { get; private set; }

    private void Awake() //makes sure there's only one instance (singleton)
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        Instance = this;
        dataPersistenceObjects = new List<IDataPersistence>();
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, FileName, useEncryption); //Application.persistentDataPath gives the operating system standard directory for persisting data in a unity project
    }

     private void Start() //method for starting the game
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    //game management methods
    public void NewGame()
    {
        gameData = new GameData();
        Debug.Log("Creating a new game file");
        dataHandler.Save(gameData);
    }
    public void LoadGame() //load method
    {
        //load any saved data from file using the data handler
        gameData = dataHandler.Load();
        //load saved data using data handler + and send data to needed scripts
        //if no data to load, create new game
        if (gameData == null)
        {
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
        Debug.Log("Resuming game file");
    }
    public void SaveGame() //save method
    {
        //pass the data to scripts so they can update and save using data handler
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }
        dataHandler.Save(gameData);
        Debug.Log("Saving game file");
    }

    //private void OnApplicationQuit() //saves the game when the application is stopped
    //{
    //    SaveGame();
    //}

    private List<IDataPersistence> FindAllDataPersistenceObjects() //list holding the objects that are being saved and loaded
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}

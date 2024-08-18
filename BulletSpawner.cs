using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using MiniJSON;

// Data class representing music information
[System.Serializable]
public class MusicData
{
    // Name of the music track
    public string name;
    // Path of the music JSON file
    public string path;
    // Times when bullets will be spawned
    public List<float> spawnTimes = new List<float>();
}

public class BulletSpawner : MonoBehaviour
{
    #region Variables
    //----------------------Public Variables----------------------
    [Header("Bullets")]
    /*
        The prefab for the object that will be fired in sync with the music.
        Required components in this prefab:
        |--> BulletController.cs (Script controlling the bullets)
        |--> Any Collider (Set up collision as needed.)
        |--> Rigidbody 2D
        |--> BulletController.cs
        |--> (OPTIONAL) AccompanyRhythmController.cs (Synchronizes rhythm with bullet objects.)
    */
    public List<GameObject> bulletPrefab;

    // Restricts the firing angle of the object being fired from the spawn point.
    public float minAngle = 160f;
    public float maxAngle = 210f;

    [Header("Notes Data")]
    // Music data
    public List<MusicData> musicDataList;
    // Path of the JSON file to be read from the Editor
    public string jsonFilePath;

    //----------------------Private Variables----------------------
    // Current bullet index
    private int bulletIndex = 0;
    // Represents the elapsed time.
    private float elapsedTime = 0f;
    // Checks if the bullet attack has started.
    private bool startBulletAttack;
    // This script retrieves the index of the enemy's current attack.
    // For example, it represents the index of each lick attack in a music track.
    private AttackModule attackModule;
    //
    private Transform bulletParent;
    #endregion

    private void Awake()
    {
        attackModule = GameObject.FindWithTag("Enemy").GetComponent<AttackModule>();
        bulletParent = GameObject.FindWithTag("Bullets").transform;
    }

    /*
        Customize the system in the Update function according to YOUR NEEDS. The scenario here is as follows:

        Elements in the bulletPrefab list, except the last one, are note objects that can be continuously fired.
        The last element is the object that will be fired only once at the end of the attack and has a different function from the other notes.
        
        The term ALL BULLETS refers to the elements in the list where JSON data is read and written.
    */
    void Update()
    {
        // If the attack has not started, the update function does not execute.
        if (!startBulletAttack) return;

        // Updates the time
        elapsedTime += Time.deltaTime;

        // Checks the spawn times of bullets and creates the bullet if the condition is met
        if (bulletIndex < musicDataList[attackModule.enemyAttackIndex - 1].spawnTimes.Count && elapsedTime >= musicDataList[attackModule.enemyAttackIndex - 1].spawnTimes[bulletIndex])
        {
            SpawnBullet();
            bulletIndex++;
        }

        // If all bullets are fired and the attack is still ongoing, create the last bullet
        if (bulletIndex == musicDataList[attackModule.enemyAttackIndex - 1].spawnTimes.Count && attackModule.attackAction)
        {
            SpawnBullet(true);
        }

        // If all bullets are fired, stop the bullet spawning
        if (bulletIndex >= musicDataList[attackModule.enemyAttackIndex - 1].spawnTimes.Count)
        {
            StopBulletSpawning();
        }
    }

    // The lastBullet parameter in this function MAY NOT BE USED based on YOUR NEEDS. The usage scenario is mentioned in the comment above the Update function.
    void SpawnBullet(bool lastBullet = false)
    {
        GameObject bullet;

        // Randomly rotates the bullet. This is done to make it appear natural. No need to make the values changeable.
        float zRot = UnityEngine.Random.Range(-15, 15);

        if (!lastBullet)
        {
            /*  Selects and creates a random bullet prefab. Does not include the last element. This may vary in your scenario. To modify:
              
                |---> int random = UnityEngine.Random.Range(0, bulletPrefab.Count - 1); <---|
                
                This means including all elements in the list.
            */
            int random = UnityEngine.Random.Range(0, bulletPrefab.Count - 2);
            bullet = Instantiate(bulletPrefab[random], transform.position, Quaternion.Euler(0, 0, zRot), bulletParent);

            // Sets angle ranges for the BulletController component. This script only fires based on angle, so this system may not need to be shown.
            // This process varies based on the scenario.
            bullet.GetComponent<BulletController>().minAngle = minAngle;
            bullet.GetComponent<BulletController>().maxAngle = maxAngle;
        }
        else
        {
            // Creates the last bullet. If you don't need this in your scenario, don't use the else block.
            Instantiate(bulletPrefab[^1], transform.position, Quaternion.Euler(0, 0, zRot), bulletParent);
        }

    }

    // Function to start the bullet spawning, callable from anywhere.
    public void StartBulletSpawning()
    {
        startBulletAttack = true;
    }

    /*  Function to stop the bullet spawning, callable from anywhere.
        
        The destroyBullets parameter in the function is related to the last bullet. If not needed in your scenario, you can remove it.
        The function will then become:

        public void StopBulletSpawning()
        {
            bulletIndex = 0;
            elapsedTime = 0f;
            startBulletAttack = false;
        }
    */
    public void StopBulletSpawning(bool destroyBullets = false)
    {
        bulletIndex = 0;
        elapsedTime = 0f;
        startBulletAttack = false;

        if (!destroyBullets) return;
        foreach (Transform bullets in bulletParent)
        {
            Destroy(bullets.gameObject);
        }
    }

    //--------The following code is not needed to run at runtime. We will control it through the Editor.--------

    // Function to read the JSON file. This function should be called via a UNITY_EDITOR button.
    public void ReadJSON()
    {
        string jsonString;
        try
        {
            // Reads the JSON file
            jsonString = File.ReadAllText(jsonFilePath);
            Debug.Log("Json Reading.");
        }
        // Logs debug messages based on the situation.
        catch (FileNotFoundException)
        {
            Debug.LogError("JSON file not found! :(");
            return;
        }
        catch (IOException e)
        {
            Debug.LogError("JSON error message ----> " + e.Message);
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError("An unexpected error occurred ----> " + e.Message);
            return;
        }

        // Retrieves the name of the JSON file. Strips the extension from the name and uses it to define the note's name.
        int startIndex = jsonFilePath.LastIndexOf('/') + 1;
        int endIndex = jsonFilePath.LastIndexOf(".json");
        int length = endIndex - startIndex;

        string fileName = jsonFilePath.Substring(startIndex, length);

        // Creates music data and adds it to the list.
        MusicData musicDataListElement = new MusicData
        {
            name = fileName,
            path = jsonFilePath
        };

        musicDataList.Add(musicDataListElement);

        // Parses JSON data.
        Dictionary<string, object> jsonData = Json.Deserialize(jsonString) as Dictionary<string, object>;
        List<object> tracks = jsonData["tracks"] as List<object>;

        // For each track, retrieves notes and their times and adds them to the list.
        foreach (object trackObj in tracks)
        {
            Dictionary<string, object> track = trackObj as Dictionary<string, object>;
            List<object> notes = track["notes"] as List<object>;

            foreach (object noteObj in notes)
            {
                Dictionary<string, object> note = noteObj as Dictionary<string, object>;
                float time = Convert.ToSingle(note["time"]);
                musicDataListElement.spawnTimes.Add(time);
            }
        }
    }
}

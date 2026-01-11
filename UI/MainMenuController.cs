   using UnityEngine;
   using UnityEngine.SceneManagement;

   public class MainMenuController : MonoBehaviour
   {
       void Update()
       {
           if (Input.GetKeyDown(KeyCode.Space))
           {
               // Will be replaced with proper character creation
               Debug.Log("Starting game...");
           }
       }
   }
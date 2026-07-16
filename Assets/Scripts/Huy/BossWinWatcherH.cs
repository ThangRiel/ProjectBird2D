// using System.Collections;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class BossWinWatcherH : MonoBehaviour
// {
//     [SerializeField] private string[] bossTags = { "Boss", "Boss2" };
//     [SerializeField] private float checkInterval = 0.25f;
//     [SerializeField] private float winDelayAfterBossGone = 0.2f;
//     [SerializeField] private KeyCode debugKillBossKey = KeyCode.K;

//     private bool hasSeenBoss;
//     private bool winTriggered;

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//     private static void CreateWatcher()
//     {
//         SceneManager.sceneLoaded += HandleSceneLoaded;
//         EnsureWatcherExists();
//     }

//     private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         EnsureWatcherExists();
//     }

//     private static void EnsureWatcherExists()
//     {
//         if (FindAnyObjectByType<BossWinWatcherH>(FindObjectsInactive.Include) != null)
//             return;

//         GameObject watcher = new GameObject("BossWinWatcherH");
//         watcher.AddComponent<BossWinWatcherH>();
//         DontDestroyOnLoad(watcher);
//     }

//     private void OnEnable()
//     {
//         SceneManager.sceneLoaded += ResetForScene;
//         ResetState();
//         StartCoroutine(WatchBossRoutine());
//     }

//     private void OnDisable()
//     {
//         SceneManager.sceneLoaded -= ResetForScene;
//         StopAllCoroutines();
//     }

//     private void Update()
//     {
// #if UNITY_EDITOR || DEVELOPMENT_BUILD
//         if (Input.GetKeyDown(debugKillBossKey))
//             DebugKillBoss();
// #endif
//     }

//     private void ResetForScene(Scene scene, LoadSceneMode mode)
//     {
//         ResetState();
//     }

//     private void ResetState()
//     {
//         hasSeenBoss = false;
//         winTriggered = false;
//     }

//     private IEnumerator WatchBossRoutine()
//     {
//         WaitForSecondsRealtime wait = new WaitForSecondsRealtime(checkInterval);

//         while (true)
//         {
//             bool bossExists = BossExists();

//             if (bossExists)
//             {
//                 hasSeenBoss = true;
//             }
//             else if (hasSeenBoss && !winTriggered)
//             {
//                 winTriggered = true;
//                 yield return new WaitForSecondsRealtime(winDelayAfterBossGone);
//                 ShowWin();
//             }

//             yield return wait;
//         }
//     }

//     private bool BossExists()
//     {
//         return FindBossObject() != null;
//     }

//     private GameObject FindBossObject()
//     {
//         foreach (string bossTag in bossTags)
//         {
//             if (string.IsNullOrWhiteSpace(bossTag))
//                 continue;

//             try
//             {
//                 GameObject boss = GameObject.FindGameObjectWithTag(bossTag);
//                 if (boss != null)
//                     return GetBossRoot(boss);
//             }
//             catch (UnityException)
//             {
//                 // Tag may not exist in every teammate scene.
//             }
//         }

//         return FindBossByComponentName();
//     }

//     private GameObject FindBossByComponentName()
//     {
//         MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
//             FindObjectsInactive.Exclude,
//             FindObjectsSortMode.None);

//         foreach (MonoBehaviour behaviour in behaviours)
//         {
//             if (behaviour == null)
//                 continue;

//             string typeName = behaviour.GetType().Name;
//             if (typeName == "BossAI" || typeName == "BossAI2")
//                 return GetBossRoot(behaviour.gameObject);
//         }

//         return null;
//     }

//     private GameObject GetBossRoot(GameObject bossPart)
//     {
//         if (bossPart == null)
//             return null;

//         Transform current = bossPart.transform;
//         Transform bestBossTransform = current;

//         while (current.parent != null)
//         {
//             current = current.parent;

//             MonoBehaviour[] parentBehaviours = current.GetComponents<MonoBehaviour>();

//             foreach (MonoBehaviour behaviour in parentBehaviours)
//             {
//                 if (behaviour == null)
//                     continue;

//                 string typeName = behaviour.GetType().Name;
//                 if (typeName == "BossAI" || typeName == "BossAI2")
//                     bestBossTransform = current;
//             }

//             if (HasBossTag(current.gameObject))
//                 bestBossTransform = current;
//         }

//         return bestBossTransform.gameObject;
//     }

//     private bool HasBossTag(GameObject target)
//     {
//         foreach (string bossTag in bossTags)
//         {
//             if (string.IsNullOrWhiteSpace(bossTag))
//                 continue;

//             try
//             {
//                 if (target.CompareTag(bossTag))
//                     return true;
//             }
//             catch (UnityException)
//             {
//                 // Tag may not exist in every teammate scene.
//             }
//         }

//         return false;
//     }

//     private void DebugKillBoss()
//     {
//         GameObject boss = FindBossObject();
//         if (boss == null)
//         {
//             Debug.Log("[BossWinWatcherH] No boss found for debug kill.");
//             return;
//         }

//         hasSeenBoss = true;
//         Debug.Log($"[BossWinWatcherH] Debug killed boss '{boss.name}'.");
//         Destroy(boss);
//     }

//     private void ShowWin()
//     {
//         GameManager manager = FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
//         if (manager != null)
//         {
//             // manager.WinGame();
//             return;
//         }

//         GameUIH gameUI = FindAnyObjectByType<GameUIH>(FindObjectsInactive.Include);
//         if (gameUI != null)
//         {
//             gameUI.ShowWin();
//             Time.timeScale = 0f;
//             return;
//         }

//         Debug.LogWarning("Boss was defeated, but no GameManager or GameUIH was found to show the winning panel.");
//     }
// }
// ! không hiểu cái này dùng làm gì hết
// using UnityEngine;
// using UnityEngine.UI;

// public class MinitSettings : MonoBehaviour
// {
//     public Slider volumeSlider;
//     public Toggle fullscreenToggle;

//     private void Start()
//     {
//         // Load lại cấu hình cũ khi mở menu
//         LoadSettings();

//         // Lắng nghe sự kiện đổi giá trị trên UI
//         volumeSlider.onValueChanged.AddListener(SetVolume);
//         fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
//     }

//     public void SetVolume(float volume)
//     {
//         AudioListener.volume = volume;
//         PlayerPrefs.SetFloat("MasterVolume", volume);
//     }

//     public void SetFullscreen(bool isFullscreen)
//     {
//         Screen.fullScreen = isFullscreen;
//         PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
//     }

//     public void SaveAndClose()
//     {
//         PlayerPrefs.Save(); // Lưu cứng vào máy
//         gameObject.SetActive(false); // Ẩn menu đi
//     }

//     private void LoadSettings()
//     {
//         // Load âm lượng (mặc định là 100%)
//         float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
//         volumeSlider.value = savedVolume;
//         AudioListener.volume = savedVolume;

//         // Load chế độ màn hình (mặc định theo hệ thống)
//         int savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
//         fullscreenToggle.isOn = (savedFullscreen == 1);
//         Screen.fullScreen = (savedFullscreen == 1);
//     }
// }
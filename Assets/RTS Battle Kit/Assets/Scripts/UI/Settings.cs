using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
	
	public bool multiplayer;
	
	//Variables visible in the inspector
	[Header("Reset settings")]
	public bool deletePlayerPrefs;
	
	[Header("Dropdown")]
	public GameObject qualityDropdown;
	public GameObject antiAliasingDropdown;
	public GameObject VsyncDropdown;
	public GameObject textureQualityDropdown;
	public GameObject blendWeightsDropdown;
	public GameObject anisotropicFilteringDropdown;
	public GameObject shadowCascadesDropdown;
	public GameObject shadowProjectionDropdown;
	
	[Header("Sliders")]
	public Slider brightnessSlider;
	public Slider volumeSlider;
	public Slider shadowDistanceSlider;
	public Slider camMoveSpeedSlider;
	public Slider camZoomSpeedSlider;
	public Slider camMouseSensitivitySlider;
	public Slider camClampAngleSlider;
	
	//Variables not visible in the inspector
	public static GameObject settingsMenu;
	private GameObject generalPanel;
	private GameObject musicPanel;
	private GameObject lightingPanel;
	private GameObject cameraPanel;
	
	void Start(){
		//If you want to reset settings, delete all playerprefs and set them to default settings
		if(deletePlayerPrefs || PlayerPrefs.GetFloat("camMoveSpeed") == 0){
			DeletePlayerPrefs();
		}
		
		CamController.movespeed = PlayerPrefs.GetFloat("camMoveSpeed");
		camMoveSpeedSlider.value = PlayerPrefs.GetFloat("camMoveSpeed");
		
		CamController.zoomSpeed = PlayerPrefs.GetFloat("camZoomSpeed");
		camZoomSpeedSlider.value = PlayerPrefs.GetFloat("camZoomSpeed");
		
		CamController.mouseSensitivity = PlayerPrefs.GetFloat("camMouseSensitivity");
		camMouseSensitivitySlider.value = PlayerPrefs.GetFloat("camMouseSensitivity");
		
		CamController.clampAngle = PlayerPrefs.GetFloat("camClampAngle");
		camClampAngleSlider.value = PlayerPrefs.GetFloat("camClampAngle");
		
		//Set quality level and show it in the dropdown list
		QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityLevel"));
		qualityDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("QualityLevel");
		
		//Set anisotropic filtering and show it in the dropdown list
		if(PlayerPrefs.GetInt("anisotropicFiltering") == 0){
		QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
		}
		if(PlayerPrefs.GetInt("anisotropicFiltering") == 1){
		QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
		}
		if(PlayerPrefs.GetInt("anisotropicFiltering") == 2){
		QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
		}
		anisotropicFilteringDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("anisotropicFiltering");
		
		//Set blendweights and show it in the dropdown list
		if(PlayerPrefs.GetInt("blendWeights") == 0){
		QualitySettings.blendWeights = BlendWeights.OneBone;
		}
		if(PlayerPrefs.GetInt("blendWeights") == 1){
		QualitySettings.blendWeights = BlendWeights.TwoBones;
		}
		if(PlayerPrefs.GetInt("blendWeights") == 2){
		QualitySettings.blendWeights = BlendWeights.FourBones;
		}
		blendWeightsDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("blendWeights");
		
		//Set anti anti aliasing and show it in the dropdown list
		QualitySettings.antiAliasing = PlayerPrefs.GetInt("AntiAliasing");
		if(PlayerPrefs.GetInt("AntiAliasing") == 0){
		antiAliasingDropdown.GetComponent<Dropdown>().value = 0;
		}
		if(PlayerPrefs.GetInt("AntiAliasing") == 2){
		antiAliasingDropdown.GetComponent<Dropdown>().value = 1;
		}
		if(PlayerPrefs.GetInt("AntiAliasing") == 4){
		antiAliasingDropdown.GetComponent<Dropdown>().value = 2;
		}
		if(PlayerPrefs.GetInt("AntiAliasing") == 8){
		antiAliasingDropdown.GetComponent<Dropdown>().value = 3;
		}
		
		//Set Vsync and show it in the dropdown list
		QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
		VsyncDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("Vsync");
		
		//Set texture quality and show it in the dropdown list
		QualitySettings.masterTextureLimit = PlayerPrefs.GetInt("textureQuality");
		textureQualityDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("textureQuality");
		
		//Set ambient light to a new color with playerprefs and set the slider to the right value
		RenderSettings.ambientLight = new Color(PlayerPrefs.GetFloat("ambientLight"), PlayerPrefs.GetFloat("ambientLight"), PlayerPrefs.GetFloat("ambientLight"), 1);
		brightnessSlider.value = PlayerPrefs.GetFloat("ambientLight");
		
		//Set audio volume and set the slider to the right value
		AudioListener.volume = PlayerPrefs.GetFloat("volume");
		volumeSlider.value = PlayerPrefs.GetFloat("volume");
		
		//Set shadow distance and set slider to it's value
		QualitySettings.shadowDistance = PlayerPrefs.GetInt("shadowDistance");
		shadowDistanceSlider.value = PlayerPrefs.GetInt("shadowDistance"); 
		
		//Set shadow cascades and show it in the dropdown list
		QualitySettings.shadowCascades = PlayerPrefs.GetInt("shadowCascades") * 2;
		shadowCascadesDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("shadowCascades");
		
		//Set shadow projection option and show it in dropdown list
		if(PlayerPrefs.GetInt("shadowProjection") == 0){
		QualitySettings.shadowProjection = ShadowProjection.CloseFit;
		}
		if(PlayerPrefs.GetInt("shadowProjection") == 1){
		QualitySettings.shadowProjection = ShadowProjection.StableFit;
		}
		shadowProjectionDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("shadowProjection");
		
		//Find general panel in the scene using name
		generalPanel = GameObject.Find("General panel");
		
		//Find music/sound panel in the scene using name and make sure it is not active
		musicPanel = GameObject.Find("Music panel");
		musicPanel.SetActive(false);
		
		//Find lighting panel and make sure it is not active
		lightingPanel = GameObject.Find("Lighting panel");
		lightingPanel.SetActive(false);
		
		cameraPanel = GameObject.Find("Camera panel");
		cameraPanel.SetActive(false);
		
		//Find the whole settings menu to switch if on/off using button in the top left corner
		settingsMenu = GameObject.Find("Settings menu content");
		settingsMenu.SetActive(false);
	}
	
	//This happens when you click the quality level dropdown list
	//It saves the clicked quality level and uses it, than it applies all settings again using playerprefs to make sure other settings won't be changed
	public void SetQuality(int qualityLevel){
    PlayerPrefs.SetInt("QualityLevel", qualityLevel);
	QualitySettings.SetQualityLevel(qualityLevel);
	
	if(PlayerPrefs.GetInt("blendWeights") == 0){
	QualitySettings.blendWeights = BlendWeights.OneBone;
	}
	if(PlayerPrefs.GetInt("blendWeights") == 1){
	QualitySettings.blendWeights = BlendWeights.TwoBones;
	}
	if(PlayerPrefs.GetInt("blendWeights") == 2){
	QualitySettings.blendWeights = BlendWeights.FourBones;
	}
	blendWeightsDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("blendWeights");
		
	QualitySettings.antiAliasing = PlayerPrefs.GetInt("AntiAliasing");
	if(PlayerPrefs.GetInt("AntiAliasing") == 0){
	antiAliasingDropdown.GetComponent<Dropdown>().value = 0;
	}
	if(PlayerPrefs.GetInt("AntiAliasing") == 2){
	antiAliasingDropdown.GetComponent<Dropdown>().value = 1;
	}
	if(PlayerPrefs.GetInt("AntiAliasing") == 4){
	antiAliasingDropdown.GetComponent<Dropdown>().value = 2;
	}
	if(PlayerPrefs.GetInt("AntiAliasing") == 8){
	antiAliasingDropdown.GetComponent<Dropdown>().value = 3;
	}
		
	QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
	VsyncDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("Vsync");
		
	QualitySettings.masterTextureLimit = PlayerPrefs.GetInt("textureQuality");
	textureQualityDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("textureQuality");
	
	if(PlayerPrefs.GetInt("anisotropicFiltering") == 0){
	QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
	}
	if(PlayerPrefs.GetInt("anisotropicFiltering") == 1){
	QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
	}
	if(PlayerPrefs.GetInt("anisotropicFiltering") == 2){
	QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
	}
	anisotropicFilteringDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("anisotropicFiltering");
	
	QualitySettings.shadowDistance = PlayerPrefs.GetInt("shadowDistance");
	shadowDistanceSlider.value = PlayerPrefs.GetInt("shadowDistance");
	
	QualitySettings.shadowCascades = PlayerPrefs.GetInt("shadowCascades") * 2;
	shadowCascadesDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("shadowCascades");
	
	if(PlayerPrefs.GetInt("shadowProjection") == 0){
	QualitySettings.shadowProjection = ShadowProjection.CloseFit;
	}
	if(PlayerPrefs.GetInt("shadowProjection") == 1){
	QualitySettings.shadowProjection = ShadowProjection.StableFit;
	}
	shadowProjectionDropdown.GetComponent<Dropdown>().value = PlayerPrefs.GetInt("shadowProjection");
	}
	
	//This happens when you click the AntiAliasing dropdown list (saves clicked option and uses it)
	public void AntiAliasing(int antiAliasingOptions){
	if(antiAliasingOptions == 0){
    PlayerPrefs.SetInt("AntiAliasing", 0);
	}
	if(antiAliasingOptions == 1){
    PlayerPrefs.SetInt("AntiAliasing", 2);
	}
	if(antiAliasingOptions == 2){
    PlayerPrefs.SetInt("AntiAliasing", 4);
	}
	if(antiAliasingOptions == 3){
    PlayerPrefs.SetInt("AntiAliasing", 8);
	}
	QualitySettings.antiAliasing = PlayerPrefs.GetInt("AntiAliasing");
	}
	
	//This happens when you click the Vsync dropdown list (again saves clicked option and uses it)
	public void VSync(int vsync){
    PlayerPrefs.SetInt("Vsync", vsync);
	QualitySettings.vSyncCount = vsync;
	}
	
	//This happens when you click the Texture Quality dropdown list (again saves clicked option and uses it)
	public void TextureQuality(int textureQuality){
    PlayerPrefs.SetInt("textureQuality", textureQuality);
	QualitySettings.masterTextureLimit = textureQuality;
	}
	
	//This happens when you click the BlendWeights dropdown list (again saves clicked option and uses it)
	public void SetBlendWeights(int weights){
    PlayerPrefs.SetInt("blendWeights", weights);
	if(PlayerPrefs.GetInt("blendWeights") == 0){
	QualitySettings.blendWeights = BlendWeights.OneBone;
	}
	if(PlayerPrefs.GetInt("blendWeights") == 1){
	QualitySettings.blendWeights = BlendWeights.TwoBones;
	}
	if(PlayerPrefs.GetInt("blendWeights") == 2){
	QualitySettings.blendWeights = BlendWeights.FourBones;
	}
	}
	
	//This happens when you click the anisotropic Filtering dropdown list (again saves clicked option and uses it)
	public void SetAnisotropicFiltering(int Filtering){
    PlayerPrefs.SetInt("anisotropicFiltering", Filtering);
	if(PlayerPrefs.GetInt("anisotropicFiltering") == 0){
	QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
	}
	if(PlayerPrefs.GetInt("anisotropicFiltering") == 1){
	QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
	}
	if(PlayerPrefs.GetInt("anisotropicFiltering") == 2){
	QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
	}
	}
	
	//This happens when you move the slider handle, it sets the ambient light color to the slider value and saves the value
	public void SetAmbientLight(){
	RenderSettings.ambientLight = new Color(brightnessSlider.value, brightnessSlider.value, brightnessSlider.value, 1);
	PlayerPrefs.SetFloat("ambientLight", brightnessSlider.value);
	}
	
	//This happens when you move the slider handle, it sets the game volume to slider value and saves the volume
	public void SetVolume(){
	AudioListener.volume = volumeSlider.value;
	PlayerPrefs.SetFloat("volume", volumeSlider.value);
	}
	//If you click the mute button, it sets volume to 0, saves volume setting and moves the slider handle by changing slider value
	public void Mute(){
	AudioListener.volume = 0f;
	PlayerPrefs.SetFloat("volume", 0f);	
	volumeSlider.value = 0f;
	}
	
	//This happens when you move the slider handle, it sets shadow distance and saves it
	public void SetShadowDistance(){
	QualitySettings.shadowDistance = shadowDistanceSlider.value;
	PlayerPrefs.SetInt("shadowDistance", (int)shadowDistanceSlider.value); 
	}
	
	public void SetCamMoveSpeed(){
	CamController.movespeed = camMoveSpeedSlider.value;
	PlayerPrefs.SetFloat("camMoveSpeed", camMoveSpeedSlider.value); 
	}
	
	public void SetCamZoomSpeed(){
	CamController.zoomSpeed = camZoomSpeedSlider.value;
	PlayerPrefs.SetFloat("camZoomSpeed", camZoomSpeedSlider.value); 
	}
	
	public void SetCamMouseSensitivity(){
	CamController.mouseSensitivity = camMouseSensitivitySlider.value;
	PlayerPrefs.SetFloat("camMouseSensitivity", camMouseSensitivitySlider.value); 
	}
	
	public void SetCamClampAngle(){
	CamController.clampAngle = camClampAngleSlider.value;
	PlayerPrefs.SetFloat("camClampAngle", camClampAngleSlider.value); 
	}
	
	//This happens when you click the Shadow Cascades dropdown (saves clicked option and uses it, the * 2 is because shadowcascades uses 0, 2, 4 and playerprefs uses the values 0, 1, 2)
	public void SetShadowCascades(int cascades){
	PlayerPrefs.SetInt("shadowCascades", cascades);
	QualitySettings.shadowCascades = cascades * 2;
	}
	
	//This happens when you click the Shadow Projection dropdown list (again saves clicked option and uses it)
	public void SetShadowProjection(int projection){
    PlayerPrefs.SetInt("shadowProjection", projection);
	if(PlayerPrefs.GetInt("shadowProjection") == 0){
	QualitySettings.shadowProjection = ShadowProjection.CloseFit;
	}
	if(PlayerPrefs.GetInt("shadowProjection") == 1){
	QualitySettings.shadowProjection = ShadowProjection.StableFit;
	}
	}
	
	//Set general panel active when you click the gear icon
	public void GeneralPanelActive(){
		musicPanel.SetActive(false);
		lightingPanel.SetActive(false);
		cameraPanel.SetActive(false);
		generalPanel.SetActive(true);
	}
	
	//Set music panel active when you click the music icon
	public void MusicPanelActive(){
		generalPanel.SetActive(false);
		lightingPanel.SetActive(false);
		cameraPanel.SetActive(false);
		musicPanel.SetActive(true);
	}
	
	//Set lighting panel active when you click the light icon
	public void LightningPanelActive(){
		musicPanel.SetActive(false);
		generalPanel.SetActive(false);
		cameraPanel.SetActive(false);
		lightingPanel.SetActive(true);
	}
	
	public void CameraPanelActive(){
		musicPanel.SetActive(false);
		generalPanel.SetActive(false);
		lightingPanel.SetActive(false);
		cameraPanel.SetActive(true);
	}
	
	//Open whole settings menu when gear icon in top left corner is clicked
	public void openSettingsMenu(){
		if(!multiplayer){
		Time.timeScale = 0;
		}
		settingsMenu.SetActive(true);
	}
	//Close whole settings menu when arrow icon in top left corner is clicked
	public void closeSettingsMenu(){
		if(!multiplayer){
		Time.timeScale = 1;
		}
		settingsMenu.SetActive(false);
	}
	
	void DeletePlayerPrefs(){
		
		PlayerPrefs.DeleteAll();
			
			PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
			PlayerPrefs.SetInt("AntiAliasing", QualitySettings.antiAliasing);
			PlayerPrefs.SetInt("Vsync", QualitySettings.vSyncCount);
			PlayerPrefs.SetInt("textureQuality", QualitySettings.masterTextureLimit);
			PlayerPrefs.SetFloat("ambientLight", brightnessSlider.value);
			PlayerPrefs.SetFloat("volume", volumeSlider.value);
			PlayerPrefs.SetInt("shadowDistance", (int)shadowDistanceSlider.value);
			PlayerPrefs.SetInt("shadowCascades", QualitySettings.shadowCascades/2);
			PlayerPrefs.SetFloat("camMoveSpeed", camMoveSpeedSlider.value);
			PlayerPrefs.SetFloat("camZoomSpeed", camZoomSpeedSlider.value);
			PlayerPrefs.SetFloat("camMouseSensitivity", camMouseSensitivitySlider.value);
			PlayerPrefs.SetFloat("camClampAngle", camClampAngleSlider.value);
			
			if(QualitySettings.blendWeights == BlendWeights.OneBone){
			PlayerPrefs.SetInt("blendWeights", 0);
			}
			if(QualitySettings.blendWeights == BlendWeights.TwoBones){
			PlayerPrefs.SetInt("blendWeights", 1);
			}
			if(QualitySettings.blendWeights == BlendWeights.FourBones){
			PlayerPrefs.SetInt("blendWeights", 2);
			}
			
			if(QualitySettings.anisotropicFiltering == AnisotropicFiltering.Disable){
			PlayerPrefs.SetInt("anisotropicFiltering", 0);
			}
			if(QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable){
			PlayerPrefs.SetInt("anisotropicFiltering", 1);
			}
			if(QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable){
			PlayerPrefs.SetInt("anisotropicFiltering", 2);
			}
			
			if(QualitySettings.shadowProjection == ShadowProjection.CloseFit){
			PlayerPrefs.SetInt("shadowProjection", 0);
			}
			if(QualitySettings.shadowProjection == ShadowProjection.StableFit){
			PlayerPrefs.SetInt("shadowProjection", 1);
			}
	}
}

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static GameManager;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class UIManager : MonoBehaviour, IDataPersistence
{

    public static UIManager instance { get; set; }

    public List<Sprite> EnemySprites = new List<Sprite>();
    public List<WaveMod> WaveMods = new List<WaveMod>();
    public GameObject previousUpgrades;
    public GameObject PostWaveUIPanel;
    public GameObject SettingsPanel;
    public Slider masterVSlider;
    public Slider musicVSlider;
    public Slider sfxVSlider;
    public Slider splatterSliderSlider;
    public Toggle cycleToggle;
    public GameObject WaveUIPanel;
    public GameObject WaveModPanel;
    public GameObject currentUpgradePanel;
    public Transform PreviewPanelRow1;
    public Transform PreviewPanelRow2;
    //public Transform PreviewPanelRow3;
    public List<Transform> currentUpgradeRows = new List<Transform>();
    public Transform WaveModPanelRow;
    public GameObject ChunkPreview;
    public GameObject WaveModPreview;
    public GameObject UpgradePreview;
    public GameObject PreviewImage;
    public GameObject UpgradePanel;
    public Transform UpgradePreviewPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    public GameObject RefreshUpgradesButton;
    public TextMeshProUGUI WaveText;
    public GameObject MainMenuPanel;
    public GameObject NotebookPanel;
    
    public GameModel modelGame;
    public GameManager gameManager;

    public GameObject toolTip;
    public TextMeshProUGUI toolTipText;
    public RectTransform toolTipRect;
    public RectTransform toolTipTextRect;
    public bool toolTipActive = false;
    public bool refreshActive = true;

    //Note: put in game model
    //private float xBounds = 3;
    //private float yBounds = 5;
    public float toolTipTimer;
    private float toolTipDissappearInterval = 0.05f;

    public int[] baseToolTipOffset = { 0, 0 };
    public int[] baseToolTipSize = { 350, 150 };
    public int[] extendedToolTipSize = { 350, 230 };

    public static string[] WaveUIText = new string[] { "Wave: ", " / 10" };

    public float postWaveUITimer = 0.0f;
    public float postWaveUISwingInterval1;
    public float postWaveUISwingInterval2;
    public float winLoseUISwingInterval1;
    public float winLoseUISwingInterval2;

    private RectTransform PostUIRect;
    private RectTransform SettingsRect;
    private RectTransform WinRect;
    private RectTransform LoseRect;
    public RectTransform currentAnimationTarget;

    public Vector3 swingStart;
    public Vector3 swingMid;
    public Vector3 swingEnd;
    public Vector3 swingRotStart;
    public Vector3 swingRotMid;
    public Vector3 swingRotEnd;

    public Vector3 winLoseStartPos;
    public Vector3 winLosMidPos;
    public Vector3 winLoseEndPos;
    public Vector3 winLoseStartRot;
    public Vector3 winLoseMidRot;
    public Vector3 winLoseEndRot;

    public Vector3 settingsStartPos;
    public Vector3 settingsMidPos;
    public Vector3 settingsEndPos;
    public Vector3 settingsStartRot;
    public Vector3 settingsMidRot;
    public Vector3 settingsEndRot;

    public Vector3 mainMenuTitlePos;
    public Vector3 mainMenuTitleAway;
    public Vector3 mainMenuButtonsPos;
    public Vector3 mainMenuButtonsAway;
    public bool mainMenuAnimations = false;
    public float mainMenuAnimationInterval;
    public RectTransform mainMenuTitleRect;
    public RectTransform mainMenuButtonsRect;

    private bool swingIn = true;
    private Vector3 start;
    private Vector3 end;
    private Vector3 rotStart;
    private Vector3 rotEnd;
    private float interval1;
    private float interval2;

    public Image[] UISHIELDS = new Image[3];
    Player player;

    public PlayGameButton playButton;
    public GameObject newGameButton;
    public TitleTextScript titleTextScript;
    public GameObject mainMenuButton;
    public PauseButton pause;
    public MuteButton mute;

    public PromodeScript promode;
    public TextMeshProUGUI ProModeDisplay;

    public enableTutorial tutorialToggle;

    //public Sprite Arena;
    public SpriteRenderer arenaBackground;
    public TextMeshProUGUI arenaText;
    public UnlockButton unlockButton;
    public NotebookArrowScript incArrow;
    public MenuRainbowInkDisplay notebookInkDisplay;


    public class WaveMod
    {
        public WaveModifier modifier;
        public bool isNew;

        [JsonConstructor]
        public WaveMod(WaveModifier modifier, bool isNew)
        {
            this.modifier = modifier;
            this.isNew = isNew;
        }

        public WaveMod(WaveModifier modifier)
        {
            this.modifier = modifier;
            this.isNew = false;
        }
    }

    public enum WaveModifier
    {
        NUMEROUS,
        FASTER,
        BIGGER_WAVE,
        DIFFICULT,
        MONOCHROME
    }

    public void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Multiple UIManagers Detected.");
        }
        instance = this;
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void initSettings()
    {
        SettingsPanel.SetActive(true);
        masterVSlider.value = SoundManager.instance.masterVolume;
        musicVSlider.value = SoundManager.instance.musicVolume;
        sfxVSlider.value = SoundManager.instance.sfxVolume;
        splatterSliderSlider.value = GameManager.instance.splatterVal;
        cycleToggle.isOn = GameManager.instance.doubleTapCycle;
    }

    public void initialize()
    {
        toolTipRect = toolTip.GetComponent<RectTransform>();
        toolTipTextRect = toolTipText.gameObject.GetComponent<RectTransform>();
        deactivateWinLoseUI();
        PostUIRect = PostWaveUIPanel.GetComponent<RectTransform>();
        WinRect = WinPanel.GetComponent<RectTransform>();
        LoseRect = LosePanel.GetComponent<RectTransform>();
        SettingsRect = SettingsPanel.GetComponent<RectTransform>();
        sfxVSlider.onValueChanged.AddListener(delegate {
            ToggleValueChanged(sfxVSlider);
        });
        playButton.initialize();

        activateMainMenuUI();
        tutorialToggle.init();
        setArenaImage();
    }

    public void SettingsUpdate()
    {
        SoundManager.instance.masterVolume = masterVSlider.value;
        SoundManager.instance.musicVolume = musicVSlider.value;
        SoundManager.instance.CalculateMusicVolume();
        SoundManager.instance.sfxVolume = sfxVSlider.value;
        GameManager.instance.splatterVal = splatterSliderSlider.value;
        GameManager.instance.doubleTapCycle = cycleToggle.isOn;
        
    }

    void ToggleValueChanged(Slider change)
    {
        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[0]);
    }

    public void activatePostWaveAnimations(bool swingingIn)
    {
        currentAnimationTarget = PostUIRect;
        swingIn = swingingIn;
        start = swingIn ? swingStart : swingEnd;
        end = swingIn ? swingEnd : swingStart;
        rotStart = swingIn ? swingRotStart : swingRotEnd;
        rotEnd = swingIn ? swingRotEnd : swingRotStart;
        interval1 = swingIn ? postWaveUISwingInterval1 : postWaveUISwingInterval2;
        interval2 = swingIn ? postWaveUISwingInterval2 : postWaveUISwingInterval1;
        postWaveUITimer = 0.0f;
        currentAnimationTarget.anchoredPosition = start;

        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[2], -0.5f, 0.5f);

    }

    public void activateSettingsAnimations(bool swingingIn)
    {
        currentAnimationTarget = SettingsRect;
        swingIn = swingingIn;
        start = swingIn ? settingsStartPos : settingsEndPos;
        end = swingIn ? settingsEndPos : settingsStartPos;
        rotStart = swingIn ? settingsStartRot : settingsEndRot;
        rotEnd = swingIn ? settingsEndRot : settingsStartRot;
        interval1 = swingIn ? postWaveUISwingInterval1 : postWaveUISwingInterval2;
        interval2 = swingIn ? postWaveUISwingInterval2 : postWaveUISwingInterval1;
        postWaveUITimer = 0.0f;
        currentAnimationTarget.anchoredPosition = start;

        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[2], -0.5f, 0.5f);

    }

    public void activateMainMenuAnimations(bool reverse)
    {
        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[2], -0.5f, 0.5f);
        postWaveUITimer = 0.0f;
        mainMenuAnimations = true;
        swingIn = reverse;
    }

    public void activateWinLoseAnimations(bool swingingIn, bool win)
    {
        currentAnimationTarget = win ? WinRect : LoseRect;
        swingIn = swingingIn;
        start = swingIn ? winLoseStartPos : winLoseEndPos;
        end = swingIn ? winLoseEndPos : winLoseStartPos;
        rotStart = swingIn ? winLoseStartRot : winLoseEndRot;
        rotEnd = swingIn ? winLoseEndRot : winLoseStartRot;
        interval1 = swingIn ? winLoseUISwingInterval1 : winLoseUISwingInterval2;
        interval2 = swingIn ? winLoseUISwingInterval2 : winLoseUISwingInterval1;
        postWaveUITimer = 0.0f;
        currentAnimationTarget.anchoredPosition = start;

        if (win)
        {
            GameManager.instance.gameAudio.pitch = 1;
            SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[3]);
        }
        else
        {
           SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[4], -0.5f, 0.5f);
        }
    }

    public void PostWaveUIAndAnimationUpdate()
    {
        if (mainMenuAnimations)
        {
            MainMenuAnimationUpdate(swingIn);
            return;
        }
        if (postWaveUITimer == -1f)
        {
            if (toolTipTimer < toolTipDissappearInterval)
            {
                toolTipTimer += Time.deltaTime;
                if (toolTipTimer >= toolTipDissappearInterval)
                {
                    SetToolTip(false);
                }
                else
                {
                    SetToolTip(true);
                }
            }
        }
        else
        {
            if (currentAnimationTarget == PostUIRect)
            {
                animationUpdate(start, swingMid, end, rotStart, swingRotMid, rotEnd, swingIn ? GameState.POSTWAVE : GameState.WAVE);
            }
            else if (currentAnimationTarget == WinRect || currentAnimationTarget == LoseRect)
            {
                animationUpdate(winLoseStartPos, winLosMidPos, winLoseEndPos, winLoseStartRot, winLoseMidRot, winLoseEndRot, currentAnimationTarget == WinRect ? GameState.WIN : GameState.LOSE);
            }
            else
            {
                animationUpdate(start, settingsMidPos, end, rotStart, settingsMidRot, rotEnd, swingIn ? GameState.SETTINGS : GameManager.instance.returnState);
            }
        }
    }

    void animationUpdate(Vector3 startPos, Vector3 midPos, Vector3 endPos, Vector3 startRot, Vector3 midRot, Vector3 endRot, GameManager.GameState endState)
    {
        postWaveUITimer += Time.deltaTime;

        if (postWaveUITimer < interval1)
        {
            currentAnimationTarget.anchoredPosition = Vector3.Lerp(startPos, midPos, postWaveUITimer / interval1);
            currentAnimationTarget.transform.rotation = Quaternion.Euler(Vector3.Lerp(startRot, midRot, postWaveUITimer / interval1));
        }
        else if (postWaveUITimer < interval2 + interval1)
        {
            currentAnimationTarget.anchoredPosition = Vector3.Lerp(midPos, endPos, (postWaveUITimer - interval1) / interval2);
            currentAnimationTarget.transform.rotation = Quaternion.Euler(Vector3.Lerp(midRot, endRot, (postWaveUITimer - interval1) / interval2));
        }
        else
        {
            postWaveUITimer = -1f;
            GameManager.instance.SetState(endState);
        }
    }

    void MainMenuAnimationUpdate(bool reverse)
    {
        if (!reverse)
        {
            postWaveUITimer += Time.deltaTime;
            mainMenuTitleRect.anchoredPosition = Vector3.Lerp(mainMenuTitlePos, mainMenuTitleAway, postWaveUITimer / mainMenuAnimationInterval);
            mainMenuButtonsRect.anchoredPosition = Vector3.Lerp(mainMenuButtonsPos, mainMenuButtonsAway, postWaveUITimer / mainMenuAnimationInterval);

            if (postWaveUITimer >= mainMenuAnimationInterval)
            {
                mainMenuAnimations = false;
                PostProcessingManager.instance.SetBlur(false);
                GameManager.instance.SetState(GameState.NOTEBOOK);
                UIManager.instance.activateNotebookUI(true);
                unlockButton.setupUnlockButton();
            }
        }
        else
        {
            postWaveUITimer += Time.deltaTime;
            mainMenuTitleRect.anchoredPosition = Vector3.Lerp(mainMenuTitleAway, mainMenuTitlePos, postWaveUITimer / mainMenuAnimationInterval);
            mainMenuButtonsRect.anchoredPosition = Vector3.Lerp(mainMenuButtonsAway, mainMenuButtonsPos, postWaveUITimer / mainMenuAnimationInterval);

            if (postWaveUITimer >= mainMenuAnimationInterval)
            {
                mainMenuAnimations = false;
                PostProcessingManager.instance.SetBlur(true);
                UIManager.instance.titleTextScript.Reset();
                GameManager.instance.SetState(GameState.MAINMENU);
                UIManager.instance.activateNotebookUI(false);
                SaveLoadManager.instance.SaveUnlocks();
            }

        }
    }

    public void ResetMainMenuPositions()
    {
        mainMenuTitleRect.anchoredPosition = mainMenuTitlePos;
        mainMenuButtonsRect.anchoredPosition = mainMenuButtonsPos;
    }

    public void activateNotebookUI(bool visible)
    {
        NotebookPanel.SetActive(visible);
        unlockButton.setupUnlockButton();
        incArrow.setupArrows();
    }

    public void renderToolTip(String inputText)
    {
        if (inputText.ToCharArray().Length < modelGame.CHARACTERMAX)
        {
            toolTipRect.sizeDelta = new Vector2(baseToolTipSize[0], baseToolTipSize[1]);
            toolTipTextRect.sizeDelta = new Vector2(baseToolTipSize[0]-20, baseToolTipSize[1]-20);
        }
        else
        {
            toolTipRect.sizeDelta = new Vector2(extendedToolTipSize[0], extendedToolTipSize[1]);
            toolTipTextRect.sizeDelta = new Vector2(extendedToolTipSize[0] - 20, extendedToolTipSize[1] - 20);
        }
        Vector3 mousePos = Input.mousePosition;
        
        Vector3 toolTipPos = new Vector3(mousePos.x + (mousePos.x >= 150 ? -baseToolTipOffset[0] : baseToolTipOffset[0]), mousePos.y - baseToolTipOffset[1], 0);
        toolTip.transform.position = toolTipPos;
        toolTipText.text = inputText;
        toolTipTimer = 0.0f;
    }

    public void SetToolTip(bool isActive)
    {
        toolTip.SetActive(isActive);
    }

    public void AddWaveMod(WaveModifier waveMod)
    {
        foreach (WaveMod mod in WaveMods){
            mod.isNew = false;
        }
        WaveMods.Add(new WaveMod(waveMod, true));
    }

    public void activateWinScreen()
    {
        MainMenuPanel.SetActive(false);
        WinPanel.SetActive(true);
        WaveUIPanel.SetActive(false);
        ProModeDisplay.text = "Pro Mode Level " + GameManager.instance.maxProModeLevelUnlocked + " Unlocked!";
    }

    public void activateMainMenuUI()
    {
        deactivatePostWaveUI();
        HideWaveMods();
        deactivateWinLoseUI();
        WaveUIPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
        newGameButton.SetActive(true);
        playButton.initialize();
        player.SelectorRing.StartAnimation(true);
        promode.initialize();
    }

    public void deactivateMainMenuUI()
    {
        MainMenuPanel.SetActive(false);
    }

    public void activateLoseScreen()
    {
        MainMenuPanel.SetActive(false);
        LosePanel.SetActive(true);
        WaveUIPanel.SetActive(false);
    }

    public void deactivateWinLoseUI()
    {
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
    }

    public void activatePostWaveUI()
    {
        MainMenuPanel.SetActive(false);
        PostWaveUIPanel.SetActive(true);
        WaveUIPanel.SetActive(false);
        UpgradePanel.SetActive(true);
        WaveModPreview.SetActive(true);
        currentUpgradePanel.SetActive(true);
        SetCurrentPlayerUpgradePreviews(true);
        SetWaveModPreviews(true);
        SetUpgradesVisible(true);
        //RefreshUpgradesButton.SetActive(refreshActive);
        WaveText.text = WaveUIText[0] + WaveSpawningSystem.instance.Level + WaveUIText[1];
        refreshActive = true;
    }

    public void deactivatePostWaveUI()
    {
        WaveUIPanel.SetActive(true);
        mainMenuButton.SetActive(false);
        pause.ren.sprite = GameModel.instance.UpgradeImages[12];

        PostWaveUIPanel.SetActive(false);
        UpgradePanel.SetActive(false);
        WaveModPreview.SetActive(false);
        currentUpgradePanel.SetActive(false);
        //RefreshUpgradesButton.SetActive(false);
    }

    public void deactivateWaveUI()
    {
        WaveUIPanel.SetActive(false);
    }

    public void SetCurrentPlayerUpgradePreviews(bool isActive)
    {
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                currentUpgradeRows[i].GetChild(k).gameObject.SetActive(isActive);
            }
        }

        for (int i = UISHIELDS.Length - 1; i >= 0; i--)
        {
            UISHIELDS[i].enabled = (player.lives >= i + 1);
        }

    }

    public void SetWaveModPreviews(bool isActive)
    {
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            WaveModPanelRow.GetChild(i).gameObject.SetActive(isActive);
        }

    }

    public void SetupChunkPreview(WaveSpawningSystem.Chunk c, int row)
    {
        GameObject newChunkPreview;
        if (row == 1)
        {
            newChunkPreview = Instantiate(ChunkPreview, PreviewPanelRow1);
        }
        else
        {
            newChunkPreview = Instantiate(ChunkPreview, PreviewPanelRow2);
        }
        for (int k = 0; k < c.colors.Length; k++)
        {
            GameObject newChunkPreviewImage = Instantiate(PreviewImage, newChunkPreview.transform);
            Image enemyImage = newChunkPreviewImage.GetComponent<Image>();
            enemyImage.sprite = UIManager.instance.EnemySprites[c.imageID];
            Color newColor = modelGame.ColorToColor(c.colors[k]);
            if (!c.isDarkened)
            {
                enemyImage.color = newColor;
            }
            else
            {
                float div = modelGame.darkenedColorDivisor;
                enemyImage.color = new Color(newColor.r / div, newColor.g / div, newColor.b / div, newColor.a);
            }
        }
        genericPreviewScript thisPreview = newChunkPreview.GetComponent<genericPreviewScript>();
        thisPreview.modText = modelGame.GetChunkTextFromType(c);
        thisPreview.setupCollider();
    }

    public void SetupWaveModUI()
    {
        HidePlayerModsOnly();
        ConstructAllWaveModPreviews();
        foreach (Upgrade u in player.upgrades)
        {
            if (u.color.Equals(GameModel.GameColor.NONE) && currentUpgradeRows[3].childCount >= 2)
            {
                AddNewPlayerUpgradeToPreview(u, 4);
                updateUpgradeChevrons(u, 4);
            }
            else
            {
                AddNewPlayerUpgradeToPreview(u, GameModel.instance.GetPlayerUpgradePreviewColorRowFromColor(u.color));
                updateUpgradeChevrons(u, GameModel.instance.GetPlayerUpgradePreviewColorRowFromColor(u.color));
            }
        }
        SetCurrentPlayerUpgradePreviews(true);
    }

    public void ClearSpecialUpgrades()
    {
        List<Transform> toMove = new List<Transform>();
        for (int i = 0; i < currentUpgradeRows[3].childCount; i++)
        {
            toMove.Add(currentUpgradeRows[3].GetChild(i));
        }
        for (int i = 0; i < currentUpgradeRows[4].childCount; i++)
        {
            toMove.Add(currentUpgradeRows[4].GetChild(i));
        }
        foreach (Transform t in toMove)
        {
            t.SetParent(previousUpgrades.transform);
            t.gameObject.SetActive(false);
        }
    }

    void ConstructAllWaveModPreviews()
    {
        HideWaveModsOnly();
        ConstructWaveModifierPreview(WaveModifier.NUMEROUS);
        ConstructWaveModifierPreview(WaveModifier.FASTER);
        ConstructWaveModifierPreview(WaveModifier.DIFFICULT);
        ConstructWaveModifierPreview(WaveModifier.MONOCHROME);
    }

    void ConstructWaveModifierPreview(WaveModifier mod)
    {
        int count = 0;
        bool showGlow = false;
        foreach (WaveMod nextMod in WaveMods)
        {
            if (nextMod.modifier.Equals(mod))
            {
                count++;
                showGlow = nextMod.isNew;
            }
        }
        if (count > 0)
        {
            GameObject newWaveModPreview = Instantiate(WaveModPreview, WaveModPanelRow.transform);
            Image modImage = newWaveModPreview.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            Image chevronImage = newWaveModPreview.transform.GetChild(1).GetComponent<Image>();
            chevronImage.sprite = count < 7 ? modelGame.UpgradeImages[count + 5] : modelGame.UpgradeImages[11];
            Transform glowObject = newWaveModPreview.transform.GetChild(3);
            glowObject.gameObject.SetActive(showGlow);
            if (count >= 7)
            {
                newWaveModPreview.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + count;
            }
            genericPreviewScript previewScript = newWaveModPreview.GetComponent<genericPreviewScript>();
            previewScript.SetText(modelGame.GetWaveModTextFromType(mod, count));
            previewScript.setupCollider();
            modImage.sprite = modelGame.WaveModImageFromType(mod);
        }
    }

    public void SetupNextUpgradePreview(GameManager.Upgrade u)
    {
        GameObject newUpgradePreview = Instantiate(UpgradePreview, UpgradePreviewPanel);
        UpgradeButton button = newUpgradePreview.GetComponent<UpgradeButton>();
        button.upp = u;
        Image upgradeImage = newUpgradePreview.transform.GetChild(0).GetChild(1).GetComponent<Image>();
        Image chevronImage = newUpgradePreview.transform.GetChild(1).GetComponent<Image>();
        chevronImage.sprite = u.factor < 7 ? modelGame.UpgradeImages[u.factor+5] : modelGame.UpgradeImages[11];
        upgradeImage.sprite = modelGame.UpgradeImageFromType(u.type);
        upgradeImage.color = modelGame.ColorToColor(u.color);
        button.initialize(u);
        String toolText = modelGame.GetUpgradeTextFromType(u);
        if (u.type.Equals(UpgradeType.PULSE))
        {
            foreach (Upgrade u2 in player.upgrades)
            {
                if (u2.type.Equals(UpgradeType.PULSE))
                {
                    toolText = GameModel.instance.UpgradeTexts[18];
                }
            }
        }
        newUpgradePreview.GetComponent<genericPreviewScript>().modText = toolText;
        newUpgradePreview.SetActive(true);
    }

    public void AddNewPlayerUpgradeToPreview(GameManager.Upgrade u, int colorRow)
    {
        bool toCreateNew = true;
        foreach (Transform upgradeToCheck in currentUpgradeRows[colorRow])
        {
            if (upgradeToCheck.name.Contains(u.type.ToString()))
            {
                toCreateNew = false;
                updateUpgradeChevrons(u, colorRow);
            }
        }
        if (toCreateNew)
        {
            createNewPlayerUpgradePreview(u, colorRow);
        }
    }

    void createNewPlayerUpgradePreview (GameManager.Upgrade u, int rowToCreateIn)
    {
        GameObject newUpgradePreview = Instantiate(WaveModPreview, currentUpgradeRows[rowToCreateIn]);
        newUpgradePreview.name = u.type.ToString();
        Image upImage = newUpgradePreview.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        upImage.sprite = modelGame.UpgradeImageFromType(u.type);
        upImage.color = modelGame.ColorToColor(u.color);
        newUpgradePreview.GetComponent<genericPreviewScript>().modText = modelGame.GetUpgradeTextFromType(u);
        updateUpgradeChevrons(u, GameModel.instance.GetPlayerUpgradePreviewColorRowFromColor(u.color));
    }

    public void updateUpgradeChevrons(Upgrade u, int rowToCheck)
    {
            foreach (Transform upgradeToCheck in currentUpgradeRows[rowToCheck])
            {
                if (upgradeToCheck.name.Contains(u.type.ToString()))
                {
                    Image chevronImage = upgradeToCheck.transform.GetChild(1).GetComponent<Image>();
                    chevronImage.sprite = u.factor < 7 ? modelGame.UpgradeImages[u.factor + 5] : modelGame.UpgradeImages[11];
                    if (u.factor >= 7)
                    {
                        upgradeToCheck.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + u.factor;
                    }
                }
            }
    }




    void SetColor(Image i, Color c)
    {
        i.color = c;
    }

    public void WipePreviewImages()
    {
        HideWave();
        WipeUpgrades();
    }

    public void WipeUpgrades()
    {
        List<Transform> toMove = new List<Transform>();
        for (int i = 0; i < UpgradePreviewPanel.transform.childCount; i++)
        {
            toMove.Add(UpgradePreviewPanel.GetChild(i));
        }
        foreach (Transform t in toMove)
        {
            t.SetParent(previousUpgrades.transform);
            t.gameObject.SetActive(false);
        }
    }

    public void SetUpgradesVisible(bool visible)
    {
        UpgradePanel.gameObject.SetActive(visible);
        foreach (Transform g in UpgradePreviewPanel.GetComponentsInChildren<Transform>())
        {
            g.gameObject.SetActive(visible);
        }
    }

    public void HideWave()
    {
        for (int i = 0; i < PreviewPanelRow1.childCount; i++)
        {
            PreviewPanelRow1.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < PreviewPanelRow2.childCount; i++)
        {
            PreviewPanelRow2.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                currentUpgradeRows[i].GetChild(k).gameObject.SetActive(false);
            }
        }
    }

    public void HideWaveMods()
    {
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            Destroy(WaveModPanelRow.GetChild(i).gameObject);
        }
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                Destroy(currentUpgradeRows[i].GetChild(k).gameObject);
            }
        }
    }

    public void HidePlayerModsOnly()
    {
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                currentUpgradeRows[i].GetChild(k).gameObject.SetActive(false);
            }
        }
    }

    public void HideWaveModsOnly()
    {
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            Destroy(WaveModPanelRow.GetChild(i).gameObject);
        }
    }

    public void ClearWaveMods()
    {
        WaveMods.Clear();
    }

    public void DestroyAll()
    {
        for (int i = 0; i < PreviewPanelRow1.childCount; i++)
        {
            Destroy(PreviewPanelRow1.GetChild(i).gameObject);
        }
        for (int i = 0; i < PreviewPanelRow2.childCount; i++)
        {
            Destroy(PreviewPanelRow2.GetChild(i).gameObject);
        }
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            Destroy(WaveModPanelRow.GetChild(i).gameObject);
        }
        for (int i = 0; i < UpgradePreviewPanel.childCount; i++)
        {
            Destroy(UpgradePreviewPanel.GetChild(i).gameObject);
        }
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                Destroy(currentUpgradeRows[i].GetChild(k).gameObject);
            }
        }
    }

    public void setArenaImage()
    {
        arenaBackground.sprite = GameModel.instance.arenaImages[GameManager.instance.arena];
        arenaText.text = GameModel.instance.arenaNames[GameManager.instance.arena];
    }

    public void LoadData(GameData data)
    {
        WaveMods = data.waveUpgrades;
        refreshActive = data.refreshActive;
        //RefreshUpgradesButton.gameObject.SetActive(refreshActive);
    }

    public void SaveData(ref GameData data)
    {
        data.waveUpgrades = WaveMods;
        data.refreshActive = refreshActive;
    }
}

// Decompiled with JetBrains decompiler
// Type: SQEX.Ebony.Jenova.Data.Configuration.Configuration
// Assembly: JenovaData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A121E2BE-2AE2-4B35-86C9-2B1E4661CBF5
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\FFXVModOrganizer\LuminousStudio\luminous\sdk\tools\Backend\AssetConverterFramework\BuildCoordinator\bin\JenovaData.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Configuration;

[XmlRoot("Configuration")]
[Serializable]
public class Configuration
{
    private static XmlSerializer serializer;
    private int allNodeFilterAutoCompleteFilterMode = 1;
    private int allNodeSearchType;
    private int anchorSnapMode = 1;
    private bool bIsLoadLastPackageWhenStartup;
    private bool bMergePropertyAtMultiSelect;
    private bool bShowTipsView = true;
    private bool bUseAutoCompletionFixidCodeSnippet = true;
    private int clickCurveMode = 4;
    private string codeEditorFontFamily = "Consolas";
    private double codeEditorFontSize = 14.0;
    private bool codeEditorShowEndOfLine;
    private bool codeEditorShowSpaces;
    private bool codeEditorShowTabs;
    private bool codeEditorTabMode;

    private string codeEditorThemePath = GetCodeEditorThemeDirectory() + "Dark.xaml";
    private int currentFindLayoutNo = 1;
    private int currentLayoutNo = 1;
    private int execProcessBuildMode;
    private int execProcessMode;
    private int extrapolationMode = 1;
    private List<string> favoriteNodes = new();
    private int favoritePackageAutoCompleteFilterMode = 5;
    private List<string> favoritePackages = new();
    private string filePath;
    private int findAutoCompleteFilterMode = 5;
    private double findResultTypeWidth = 100.0;
    private int findSearchType;
    private bool findViewAutoCollapseWhenSelectionChanged = true;
    private bool findViewIncludeTextScript;
    private int findViewOpenSequenceFormat;
    private int fixIdLookupSource = 2;
    private int fixIdPopulatingMode;
    private int fixIdViewAutoCompleteFilterMode = 1;
    private bool isAutoUploadCrashDump = true;
    private bool isDisplayTimeLineSectionBoxUI = true;
    private bool isDisplayTimeLineSectionUI;
    private bool isEnableKeyOvertake = true;
    private bool isEnableSectionIsSnapToMajorTicks = true;
    private bool isEnableSectionIsSnapToTrackItem = true;
    private bool isEnableTimeBarIsSnapToFrame = true;
    private bool isEnableTimeBarIsSnapToMajorTicks = true;
    private bool isEnableTimeBarIsSnapToTrackItem = true;
    private bool isEnableTrackItemIsSnapOnMove = true;
    private bool isEnableTrackItemIsSnapOnResize = true;
    private bool isEnableTrackItemIsSnapToCurrentTimeBar = true;
    private bool isEnableTrackItemIsSnapToFrame = true;
    private bool isEnableTrackItemIsSnapToMajorTicks = true;
    private bool isEnableTrackItemIsSnapToSection = true;
    private bool isEnableTrackItemIsSnapToTrackItem = true;
    private bool isEnableTrackItemIsSnapToWorkArea = true;
    private bool isEnableWorkAreaIsSnapToMajorTicks = true;
    private bool isEnableWorkAreaIsSnapToTrackItem = true;
    private bool isLayoutGameWindow;
    private bool isLevelCurveLoadLayoutWhenStartup;
    private bool isLevelCurveSaveLayoutWhenExit;
    private bool isLoadFindLayoutWhenStartup = true;
    private bool isLoadLayoutWhenStartup;
    private bool isMaximizedGameWindow;
    private bool isModifyTrayNodeIndexMode_;
    private bool isRippleEdit = true;
    private bool isSaveFindLayoutWhenExit = true;
    private bool isSaveLayoutWhenExit;
    private bool isTimeLineLoadLayoutWhenStartup;
    private bool isTimeLineSaveLayoutWhenExit;
    private bool isUseBetaTest;
    private bool isUseEbonyEditorP4ChangeList = true;
    private bool isUseEntityIndexSeparator;
    private bool isUseLazyLoadingTest = true;
    private bool isUseLmColorPicker = true;
    private bool isUseP4 = true;
    private bool isUseUE4LikeSequenceEditor = true;
    private bool isUseUnlimitedPropertyValue;
    private int labeledVariableAutoCompleteFilterMode = 5;
    private List<string> lastFindStringList = new();
    private string lastInputFavoritePackageString;
    private string lastInputFixIDLabel;
    private string lastNodeListFilterCategory;
    private string lastNodeListFilterClassName;
    private string lastNodeListFilterNodeName;
    private int lastNodeListFilterType;
    private string lastOpenFile_;
    private string lastSelectedFixIdPrefix;
    private string lastSelectedFixIdTable;
    private string lastSelectedProjectFolder;
    private int lastSelectedTabIndex;
    private List<string> latestOpenFiles = new();
    private List<string> latestUseNodes = new();
    private int levelCurveCurrentLayoutNo = 1;
    private bool m_anchorSnapsToFrame = true;
    private bool m_anchorSnapsToIntegerYValue;
    private int m_curveAutoFitMode = 2;
    private int m_curveFitMode;
    private int m_curveSelectionMode = 1;
    private int m_defaultAnchorType = 2;
    private int nodeDebuggerCommunicationInterval = 5;
    private bool nodeDebuggerEnabled;
    private List<PackageSizeInfo> packageSizeInfoList_ = new();
    private double propertyNameColumnWidth = 150.0;
    private bool quertySaveBeforeRun = true;
    private int sectionUIActionMode;
    private bool showVerboseTips;
    private double snapTolerance = 12.0;
    private int timeLineCurrentLayoutNo = 1;
    private int timeLineDebuggerCommunicationInterval = 5;
    private bool timeLineDebuggerEnabled;
    private int timeLineFovUseValueTypeFov = 1;
    private int timeLineRotationUseValueTypeRoll = 1;
    private int timeLineRotationUseValueTypeTilt = 2;
    private int timeLineRotationUseValueTypeYaw = 1;
    private bool timeLineStartBlendIsUpdateEveryFrame = true;
    private int timeLineTrackDisplayForm = 1;
    private double timeLineTrackHolderWidth = 270.0;
    private bool useWpfSoftwareRendering;
    private int variableDebuggerCommunicationInterval = 15;
    private bool variableDebuggerEnabled;
    private List<string> vlinkList_ = new();

    public string LastOpenFile
    {
        get => lastOpenFile_;
        set => lastOpenFile_ = value;
    }

    public List<string> LatestOpenFiles
    {
        get => latestOpenFiles;
        set => latestOpenFiles = value;
    }

    public bool IsMaxmizedGameWindow
    {
        get => isMaximizedGameWindow;
        set => isMaximizedGameWindow = value;
    }

    [DefaultValue(false)]
    public bool IsLoadLastPackageWhenStartup
    {
        get => bIsLoadLastPackageWhenStartup;
        set => bIsLoadLastPackageWhenStartup = value;
    }

    public bool IsShowTipsView
    {
        get => bShowTipsView;
        set => bShowTipsView = value;
    }

    public bool ShowVerboseTips
    {
        get => showVerboseTips;
        set => showVerboseTips = value;
    }

    public string LastSelectedProjectFolder
    {
        get => lastSelectedProjectFolder;
        set => lastSelectedProjectFolder = value;
    }

    [DefaultValue(true)]
    public bool UseAutoCompletionFixidCodeSnippet
    {
        get => bUseAutoCompletionFixidCodeSnippet;
        set => bUseAutoCompletionFixidCodeSnippet = value;
    }

    public string CodeEditorFontFamily
    {
        get => codeEditorFontFamily;
        set => codeEditorFontFamily = value;
    }

    public double CodeEditorFontSize
    {
        get => codeEditorFontSize;
        set => codeEditorFontSize = value;
    }

    public bool CodeEditorShowTabs
    {
        get => codeEditorShowTabs;
        set => codeEditorShowTabs = value;
    }

    public string CodeEditorThemePath
    {
        get => codeEditorThemePath;
        set => codeEditorThemePath = value;
    }

    public bool CodeEditorShowSpaces
    {
        get => codeEditorShowSpaces;
        set => codeEditorShowSpaces = value;
    }

    public bool CodeEditorShowEndOfLine
    {
        get => codeEditorShowEndOfLine;
        set => codeEditorShowEndOfLine = value;
    }

    public bool CodeEditorTabMode
    {
        get => codeEditorTabMode;
        set => codeEditorTabMode = value;
    }

    public bool LaunchVR { get; set; }

    public bool LaunchViewer { get; set; }

    public bool LaunchTitle { get; set; }

    public List<string> LastFindStringList
    {
        get => lastFindStringList;
        set => lastFindStringList = value;
    }

    public int FindViewSearchType
    {
        get => findSearchType;
        set => findSearchType = value;
    }

    public int FindWindowAutoCompleteFilterMode
    {
        get => findAutoCompleteFilterMode;
        set => findAutoCompleteFilterMode = value;
    }

    public int FindViewOpenSequenceFormat
    {
        get => findViewOpenSequenceFormat;
        set => findViewOpenSequenceFormat = value;
    }

    public bool FindViewAutoCollapseWhenSelectionChanged
    {
        get => findViewAutoCollapseWhenSelectionChanged;
        set => findViewAutoCollapseWhenSelectionChanged = value;
    }

    public bool FindViewIncludeTextScript
    {
        get => findViewIncludeTextScript;
        set => findViewIncludeTextScript = value;
    }

    [DefaultValue(100.0)]
    public double FindResultTypeWidth
    {
        get => findResultTypeWidth;
        set
        {
            if (findResultTypeWidth == value)
            {
                return;
            }

            findResultTypeWidth = value;
        }
    }

    [DefaultValue(1)]
    public int CurrentFindLayoutNo
    {
        get => currentFindLayoutNo;
        set => currentFindLayoutNo = value;
    }

    [DefaultValue(true)]
    public bool IsLoadFindLayoutWhenStartup
    {
        get => isLoadFindLayoutWhenStartup;
        set => isLoadFindLayoutWhenStartup = value;
    }

    [DefaultValue(true)]
    public bool IsSaveFindLayoutWhenExit
    {
        get => isSaveFindLayoutWhenExit;
        set => isSaveFindLayoutWhenExit = value;
    }

    [XmlIgnore]
    public string FindLayoutFilePath
    {
        get
        {
            string path2;
            switch (CurrentFindLayoutNo)
            {
                case 2:
                    path2 = "FindLayout2.xml";
                    break;
                case 3:
                    path2 = "FindLayout3.xml";
                    break;
                case 4:
                    path2 = "FindLayout4.xml";
                    break;
                default:
                    path2 = "FindLayout.xml";
                    break;
            }

            return Path.Combine(Path.GetDirectoryName(filePath), path2);
        }
    }

    [DefaultValue(false)]
    public bool NodeDebuggerEnabled
    {
        get => nodeDebuggerEnabled;
        set
        {
            if (nodeDebuggerEnabled == value)
            {
                return;
            }

            nodeDebuggerEnabled = value;
        }
    }

    [DefaultValue(5)]
    public int NodeDebuggerCommunicationInterval
    {
        get => nodeDebuggerCommunicationInterval;
        set
        {
            if (nodeDebuggerCommunicationInterval == value)
            {
                return;
            }

            nodeDebuggerCommunicationInterval = value;
        }
    }

    [DefaultValue(false)]
    public bool TimeLineDebuggerEnabled
    {
        get => timeLineDebuggerEnabled;
        set
        {
            if (timeLineDebuggerEnabled == value)
            {
                return;
            }

            timeLineDebuggerEnabled = value;
        }
    }

    [DefaultValue(5)]
    public int TimeLineDebuggerCommunicationInterval
    {
        get => timeLineDebuggerCommunicationInterval;
        set
        {
            if (timeLineDebuggerCommunicationInterval == value)
            {
                return;
            }

            timeLineDebuggerCommunicationInterval = value;
        }
    }

    [DefaultValue(false)]
    public bool VariableDebuggerEnabled
    {
        get => variableDebuggerEnabled;
        set
        {
            if (variableDebuggerEnabled == value)
            {
                return;
            }

            variableDebuggerEnabled = value;
        }
    }

    [DefaultValue(15)]
    public int VariableDebuggerCommunicationInterval
    {
        get => variableDebuggerCommunicationInterval;
        set
        {
            if (variableDebuggerCommunicationInterval == value)
            {
                return;
            }

            variableDebuggerCommunicationInterval = value;
        }
    }

    public string LastInputFixIDLabel
    {
        get => lastInputFixIDLabel;
        set => lastInputFixIDLabel = value;
    }

    public string LastSelectedFixIdTable
    {
        get => lastSelectedFixIdTable;
        set => lastSelectedFixIdTable = value;
    }

    public string LastSelectedFixIdPrefix
    {
        get => lastSelectedFixIdPrefix;
        set => lastSelectedFixIdPrefix = value;
    }

    public int FixIdViewAutoCompleteFilterMode
    {
        get => fixIdViewAutoCompleteFilterMode;
        set => fixIdViewAutoCompleteFilterMode = value;
    }

    public int LastFixIdLookupSource
    {
        get => fixIdLookupSource;
        set => fixIdLookupSource = value;
    }

    public int FixIdPopulatingMode
    {
        get => fixIdPopulatingMode;
        set => fixIdPopulatingMode = value;
    }

    public int CurrentLayoutNo
    {
        get => currentLayoutNo;
        set => currentLayoutNo = value;
    }

    public bool IsLoadLayoutWhenStartup
    {
        get => isLoadLayoutWhenStartup;
        set => isLoadLayoutWhenStartup = value;
    }

    public bool IsSaveLayoutWhenExit
    {
        get => isSaveLayoutWhenExit;
        set => isSaveLayoutWhenExit = value;
    }

    public bool IsLayoutGameWindow
    {
        get => isLayoutGameWindow;
        set => isLayoutGameWindow = value;
    }

    public bool IsUseMultiSequenceEditor => true;

    public string CultureName
    {
        get => "en-GB";
        set
        {
            var x = value;
        }
    }

    public bool JapaneseLanguage => CultureName == "ja-JP";

    [DefaultValue(false)]
    public bool MergePropertyAtMultiSelect
    {
        get => bMergePropertyAtMultiSelect;
        set => bMergePropertyAtMultiSelect = value;
    }

    [DefaultValue(false)]
    public bool IsModifyTrayNodeIndexMode
    {
        get => isModifyTrayNodeIndexMode_;
        set => isModifyTrayNodeIndexMode_ = value;
    }

    [DefaultValue(true)]
    public bool IsUseLmColorPicker
    {
        get => isUseLmColorPicker;
        set => isUseLmColorPicker = value;
    }

    [DefaultValue(false)]
    public bool IsUseEntityIndexSeparator
    {
        get => isUseEntityIndexSeparator;
        set => isUseEntityIndexSeparator = value;
    }

    [DefaultValue(true)]
    public bool IsUseEbonyEditorP4ChangeList
    {
        get => isUseEbonyEditorP4ChangeList;
        set => isUseEbonyEditorP4ChangeList = value;
    }

    [DefaultValue(true)]
    public bool IsUseLazyLoadingTest
    {
        get => isUseLazyLoadingTest;
        set => isUseLazyLoadingTest = value;
    }

    [DefaultValue(true)]
    public bool IsUseP4
    {
        get => isUseP4;
        set => isUseP4 = value;
    }

    [DefaultValue(false)]
    public bool IsUseBetaTest
    {
        get => isUseBetaTest;
        set => isUseBetaTest = value;
    }

    [DefaultValue(true)]
    public bool IsUseUE4LikeSequenceEditor
    {
        get => isUseUE4LikeSequenceEditor;
        set => isUseUE4LikeSequenceEditor = value;
    }

    [DefaultValue(false)]
    public bool IsUseUnlimitedPropertyValue
    {
        get => isUseUnlimitedPropertyValue;
        set => isUseUnlimitedPropertyValue = value;
    }

    [DefaultValue(true)]
    public bool IsAutoUploadCrashDump
    {
        get => isAutoUploadCrashDump;
        set => isAutoUploadCrashDump = value;
    }

    [DefaultValue(false)]
    public bool UseWPFSoftwareRendering
    {
        get => useWpfSoftwareRendering;
        set => useWpfSoftwareRendering = value;
    }

    public int LastNodeListFilterType
    {
        get => lastNodeListFilterType;
        set => lastNodeListFilterType = value;
    }

    public string LastNodeListFilterCategory
    {
        get => lastNodeListFilterCategory;
        set => lastNodeListFilterCategory = value;
    }

    public string LastNodeListFilterNodeName
    {
        get => lastNodeListFilterNodeName;
        set => lastNodeListFilterNodeName = value;
    }

    public string LastNodeListFilterClassName
    {
        get => lastNodeListFilterClassName;
        set => lastNodeListFilterClassName = value;
    }

    public int AllNodeFilterAutoCompleteFilterMode
    {
        get => allNodeFilterAutoCompleteFilterMode;
        set => allNodeFilterAutoCompleteFilterMode = value;
    }

    public int AllNodeSearchType
    {
        get => allNodeSearchType;
        set => allNodeSearchType = value;
    }

    public List<string> LatestUseNodes
    {
        get => latestUseNodes;
        set => latestUseNodes = value;
    }

    public List<string> FavoriteNodes
    {
        get => favoriteNodes;
        set => favoriteNodes = value;
    }

    public List<string> FavoritePackages
    {
        get => favoritePackages;
        set => favoritePackages = value;
    }

    public string LastInputFavoritePackageString
    {
        get => lastInputFavoritePackageString;
        set => lastInputFavoritePackageString = value;
    }

    public int FavoritePackageAutoCompleteFilterMode
    {
        get => favoritePackageAutoCompleteFilterMode;
        set => favoritePackageAutoCompleteFilterMode = value;
    }

    public int LabeledVariableAutoCompleteFilterMode
    {
        get => labeledVariableAutoCompleteFilterMode;
        set => labeledVariableAutoCompleteFilterMode = value;
    }

    public int LastSelectedTabIndex
    {
        get => lastSelectedTabIndex;
        set => lastSelectedTabIndex = value;
    }

    [DefaultValue("")] public string GameFlagFilterLabel { get; set; }

    [DefaultValue("")] public string GameFlagFilterValue { get; set; }

    [DefaultValue("")] public string LabeledVariableFilterLabel { get; set; }

    [DefaultValue(0)] public int LabeledVariableFilterVariableType { get; set; }

    [DefaultValue(0)] public int LabeledVariableFilterScope { get; set; }

    [DefaultValue("")] public string LabeledVariableFilterValue { get; set; }

    [DefaultValue(0)] public int GameFlagDefaultDropCreateSequence { get; set; }

    [DefaultValue(0)] public int LabeledVariableDefaultDropCreateSequence { get; set; }

    [DefaultValue("")] public string RemoteEventFilterLabel { get; set; }

    [DefaultValue(0)] public int RemoteEventDefaultDropCreateSequence { get; set; }

    [DefaultValue(0)]
    public int ExecProcessMode
    {
        get => execProcessMode;
        set => execProcessMode = value;
    }

    [DefaultValue(0)]
    public int ExecProcessBuildMode
    {
        get => execProcessBuildMode;
        set => execProcessBuildMode = value;
    }

    [DefaultValue(true)]
    public bool QuertySaveBeforeRun
    {
        get => quertySaveBeforeRun;
        set => quertySaveBeforeRun = value;
    }

    [DefaultValue(150.0)]
    public double PropertyNameColumnWidth
    {
        get => propertyNameColumnWidth;
        set
        {
            if (propertyNameColumnWidth == value)
            {
                return;
            }

            propertyNameColumnWidth = value;
        }
    }

    public int LevelCurveCurrentLayoutNo
    {
        get => levelCurveCurrentLayoutNo;
        set => levelCurveCurrentLayoutNo = value;
    }

    public bool IsLevelCurveLoadLayoutWhenStartup
    {
        get => isLevelCurveLoadLayoutWhenStartup;
        set => isLevelCurveLoadLayoutWhenStartup = value;
    }

    public bool IsLevelCurveSaveLayoutWhenExit
    {
        get => isLevelCurveSaveLayoutWhenExit;
        set => isLevelCurveSaveLayoutWhenExit = value;
    }

    [XmlIgnore]
    public string LevelCurveLayoutFilePath
    {
        get
        {
            string path2;
            switch (LevelCurveCurrentLayoutNo)
            {
                case 2:
                    path2 = "LevelCurveLayout2.xml";
                    break;
                case 3:
                    path2 = "LevelCurveLayout3.xml";
                    break;
                case 4:
                    path2 = "LevelCurveLayout4.xml";
                    break;
                default:
                    path2 = "LevelCurveLayout.xml";
                    break;
            }

            return Path.Combine(Path.GetDirectoryName(filePath), path2);
        }
    }

    [DefaultValue(270.0)]
    public double TimeLineTrackHolderWidth
    {
        get => timeLineTrackHolderWidth;
        set => timeLineTrackHolderWidth = value;
    }

    [DefaultValue(1)]
    public int TimeLineTrackDisplayForm
    {
        get => timeLineTrackDisplayForm;
        set => timeLineTrackDisplayForm = value;
    }

    [DefaultValue(false)]
    public bool IsDisplayTimeLineSectionUI
    {
        get => isDisplayTimeLineSectionUI;
        set => isDisplayTimeLineSectionUI = value;
    }

    [DefaultValue(false)]
    public bool IsDisplayTimeLineSectionBoxUI
    {
        get => isDisplayTimeLineSectionBoxUI;
        set => isDisplayTimeLineSectionBoxUI = value;
    }

    [DefaultValue(true)]
    public bool IsEnableKeyOvertake
    {
        get => isEnableKeyOvertake;
        set => isEnableKeyOvertake = value;
    }

    [DefaultValue(false)] public bool TimeLineDebugLoop { get; set; }

    public int TimeLineCurrentLayoutNo
    {
        get => timeLineCurrentLayoutNo;
        set => timeLineCurrentLayoutNo = value;
    }

    public bool IsTimeLineLoadLayoutWhenStartup
    {
        get => isTimeLineLoadLayoutWhenStartup;
        set => isTimeLineLoadLayoutWhenStartup = value;
    }

    public bool IsTimeLineSaveLayoutWhenExit
    {
        get => isTimeLineSaveLayoutWhenExit;
        set => isTimeLineSaveLayoutWhenExit = value;
    }

    [XmlIgnore]
    public string TimeLineLayoutFilePath
    {
        get
        {
            string path2;
            switch (TimeLineCurrentLayoutNo)
            {
                case 2:
                    path2 = "TimeLineLayout2.xml";
                    break;
                case 3:
                    path2 = "TimeLineLayout3.xml";
                    break;
                case 4:
                    path2 = "TimeLineLayout4.xml";
                    break;
                default:
                    path2 = "TimeLineLayout.xml";
                    break;
            }

            return Path.Combine(Path.GetDirectoryName(filePath), path2);
        }
    }

    [DefaultValue(true)]
    public bool IsEnableTrackItemIsSnapToFrame
    {
        get => isEnableTrackItemIsSnapToFrame;
        set => isEnableTrackItemIsSnapToFrame = value;
    }

    [DefaultValue(true)]
    public bool IsEnableTrackItemIsSnapToTrackItem
    {
        get => isEnableTrackItemIsSnapToTrackItem;
        set => isEnableTrackItemIsSnapToTrackItem = value;
    }

    public bool IsEnableTrackItemIsSnapOnMove
    {
        get => isEnableTrackItemIsSnapOnMove;
        set => isEnableTrackItemIsSnapOnMove = value;
    }

    public bool IsEnableTrackItemIsSnapOnResize
    {
        get => isEnableTrackItemIsSnapOnResize;
        set => isEnableTrackItemIsSnapOnResize = value;
    }

    public bool IsEnableTrackItemIsSnapToWorkArea
    {
        get => isEnableTrackItemIsSnapToWorkArea;
        set => isEnableTrackItemIsSnapToWorkArea = value;
    }

    public bool IsEnableTrackItemIsSnapToSection
    {
        get => isEnableTrackItemIsSnapToSection;
        set => isEnableTrackItemIsSnapToSection = value;
    }

    public bool IsEnableTrackItemIsSnapToCurrentTimeBar
    {
        get => isEnableTrackItemIsSnapToCurrentTimeBar;
        set => isEnableTrackItemIsSnapToCurrentTimeBar = value;
    }

    public bool IsEnableTrackItemIsSnapToMajorTicks
    {
        get => isEnableTrackItemIsSnapToMajorTicks;
        set => isEnableTrackItemIsSnapToMajorTicks = value;
    }

    public bool IsEnableTimeBarIsSnapToFrame
    {
        get => isEnableTimeBarIsSnapToFrame;
        set => isEnableTimeBarIsSnapToFrame = value;
    }

    public bool IsEnableTimeBarIsSnapToTrackItem
    {
        get => isEnableTimeBarIsSnapToTrackItem;
        set => isEnableTimeBarIsSnapToTrackItem = value;
    }

    public bool IsEnableTimeBarIsSnapToMajorTicks
    {
        get => isEnableTimeBarIsSnapToMajorTicks;
        set => isEnableTimeBarIsSnapToMajorTicks = value;
    }

    public bool IsEnableWorkAreaIsSnapToTrackItem
    {
        get => isEnableWorkAreaIsSnapToTrackItem;
        set => isEnableWorkAreaIsSnapToTrackItem = value;
    }

    public bool IsEnableWorkAreaIsSnapToMajorTicks
    {
        get => isEnableWorkAreaIsSnapToMajorTicks;
        set => isEnableWorkAreaIsSnapToMajorTicks = value;
    }

    public bool IsEnableSectionIsSnapToTrackItem
    {
        get => isEnableSectionIsSnapToTrackItem;
        set => isEnableSectionIsSnapToTrackItem = value;
    }

    public bool IsEnableSectionIsSnapToMajorTicks
    {
        get => isEnableSectionIsSnapToMajorTicks;
        set => isEnableSectionIsSnapToMajorTicks = value;
    }

    public double SnapTolerance
    {
        get => snapTolerance;
        set => snapTolerance = value;
    }

    public int ExtrapolationMode
    {
        get => extrapolationMode;
        set => extrapolationMode = value;
    }

    public int AnchorSnapMode
    {
        get => anchorSnapMode;
        set => anchorSnapMode = value;
    }

    public int ClickCurveMode
    {
        get => clickCurveMode;
        set => clickCurveMode = value;
    }

    public int SectionUIActionMode
    {
        get => sectionUIActionMode;
        set => sectionUIActionMode = value;
    }

    [DefaultValue(true)]
    public bool IsRippleEdit
    {
        get => isRippleEdit;
        set => isRippleEdit = value;
    }

    [DefaultValue(true)]
    public bool AnchorSnapsToFrame
    {
        get => m_anchorSnapsToFrame;
        set => m_anchorSnapsToFrame = value;
    }

    [DefaultValue(false)]
    public bool AnchorSnapsToIntegerYValue
    {
        get => m_anchorSnapsToIntegerYValue;
        set => m_anchorSnapsToIntegerYValue = value;
    }

    public int CurveAutoFitMode
    {
        get => m_curveAutoFitMode;
        set => m_curveAutoFitMode = value;
    }

    public int DefaultAnchorType
    {
        get => m_defaultAnchorType;
        set => m_defaultAnchorType = value;
    }

    public int CurveSelectionMode
    {
        get => m_curveSelectionMode;
        set => m_curveSelectionMode = value;
    }

    public int CurveFitMode
    {
        get => m_curveFitMode;
        set => m_curveFitMode = value;
    }

    public int TimeLineRotationUseValueTypeYaw
    {
        get => timeLineRotationUseValueTypeYaw;
        set => timeLineRotationUseValueTypeYaw = value;
    }

    public int TimeLineRotationUseValueTypeTilt
    {
        get => timeLineRotationUseValueTypeTilt;
        set => timeLineRotationUseValueTypeTilt = value;
    }

    public int TimeLineRotationUseValueTypeRoll
    {
        get => timeLineRotationUseValueTypeRoll;
        set => timeLineRotationUseValueTypeRoll = value;
    }

    public int TimeLineFovUseValueTypeFov
    {
        get => timeLineFovUseValueTypeFov;
        set => timeLineFovUseValueTypeFov = value;
    }

    public bool TimeLineStartBlendIsUpdateEveryFrame
    {
        get => timeLineStartBlendIsUpdateEveryFrame;
        set => timeLineStartBlendIsUpdateEveryFrame = value;
    }

    public List<string> vlinkList
    {
        get => vlinkList_;
        set => vlinkList_ = value;
    }

    public List<PackageSizeInfo> PackageSizeInfoList
    {
        get => packageSizeInfoList_;
        set => packageSizeInfoList_ = value;
    }

    [DefaultValue(false)] public bool SequenceEditorGridSnap { get; set; }

    [DefaultValue(true)] public bool SequenceEditorLiveEdit { get; set; } = true;

    [XmlIgnore]
    public string DockingLayoutFilePath
    {
        get
        {
            string path2;
            switch (CurrentLayoutNo)
            {
                case 2:
                    path2 = "Layout2.xml";
                    break;
                case 3:
                    path2 = "Layout3.xml";
                    break;
                case 4:
                    path2 = "Layout4.xml";
                    break;
                default:
                    path2 = "Layout.xml";
                    break;
            }

            return Path.Combine(Path.GetDirectoryName(filePath), path2);
        }
    }

    public void AddLatestOpenFile(string path)
    {
        path = Project.GetDataRelativePath(path);
        RemoveLatestOpenFile(path);
        latestOpenFiles.Insert(0, path);
        while (latestOpenFiles.Count > 20)
        {
            latestOpenFiles.RemoveAt(20);
        }
    }

    public void RemoveLatestOpenFile(string path)
    {
        if (!latestOpenFiles.Contains(path))
        {
            return;
        }

        latestOpenFiles.Remove(path);
    }

    public static string GetCodeEditorThemeDirectory()
    {
        return "";
        //Luminous.EnvironmentSettings.EnvironmentSettings.GetSDKPath() + "\\tools\\Jenova\\Themes\\CodeEditorThemes\\";
    }

    public void AddLatestFindStringList(string findStr)
    {
        RemoveLatestFindStringList(findStr);
        lastFindStringList.Insert(0, findStr);
        while (lastFindStringList.Count > 10)
        {
            lastFindStringList.RemoveAt(10);
        }
    }

    public void RemoveLatestFindStringList(string findStr)
    {
        if (!lastFindStringList.Contains(findStr))
        {
            return;
        }

        lastFindStringList.Remove(findStr);
    }

    public void AddLatestUseNode(string nodeName)
    {
        RemoveLatestUseNode(nodeName);
        latestUseNodes.Insert(0, nodeName);
        while (latestUseNodes.Count > 20)
        {
            latestUseNodes.RemoveAt(20);
        }

        DocumentInterface.DocumentContainer.LatestUseNodeUpdate();
    }

    public void RemoveLatestUseNode(string nodeName)
    {
        if (!latestUseNodes.Contains(nodeName))
        {
            return;
        }

        latestUseNodes.Remove(nodeName);
    }

    public void ClearLatestUseNode()
    {
        latestUseNodes.Clear();
        DocumentInterface.DocumentContainer.LatestUseNodeUpdate();
    }

    public void AddFavoriteNode(string nodeName)
    {
        RemoveFavoriteNode(nodeName);
        favoriteNodes.Insert(0, nodeName);
        DocumentInterface.DocumentContainer.FavoriteNodeUpdate();
    }

    public void RemoveFavoriteNode(string nodeName)
    {
        if (!favoriteNodes.Contains(nodeName))
        {
            return;
        }

        favoriteNodes.Remove(nodeName);
    }

    public void InsertFavoriteNode(int index, string nodeName)
    {
        if (index == -1)
        {
            favoriteNodes.Add(nodeName);
        }
        else
        {
            favoriteNodes.Insert(index, nodeName);
        }
    }

    public void UpAndDownFavoriteNode(bool isUp, string nodeName)
    {
        var index = favoriteNodes.FindIndex(s => s == nodeName);
        if (isUp)
        {
            if (index <= 0)
            {
                return;
            }

            RemoveFavoriteNode(nodeName);
            InsertFavoriteNode(index - 1, nodeName);
        }
        else
        {
            if (index < 0 || index >= favoriteNodes.Count() - 1)
            {
                return;
            }

            RemoveFavoriteNode(nodeName);
            InsertFavoriteNode(index + 1, nodeName);
        }
    }

    public void AddFavoritePackage(string packageName)
    {
        RemoveFavoritePackage(packageName);
        favoritePackages.Insert(0, packageName);
        DocumentInterface.DocumentContainer.FavoritePackageUpdate();
    }

    public void RemoveFavoritePackage(string packageName)
    {
        if (!favoritePackages.Contains(packageName))
        {
            return;
        }

        favoritePackages.Remove(packageName);
    }

    public void InsertFavoritePackage(int index, string packageName)
    {
        if (index == -1)
        {
            favoritePackages.Add(packageName);
        }
        else
        {
            favoritePackages.Insert(index, packageName);
        }
    }

    public void UpAndDownFavoritePackage(bool isUp, string packageName)
    {
        var index = favoritePackages.FindIndex(s => s == packageName);
        if (isUp)
        {
            if (index <= 0)
            {
                return;
            }

            RemoveFavoritePackage(packageName);
            InsertFavoritePackage(index - 1, packageName);
        }
        else
        {
            if (index < 0 || index >= favoritePackages.Count() - 1)
            {
                return;
            }

            RemoveFavoritePackage(packageName);
            InsertFavoritePackage(index + 1, packageName);
        }
    }

    public void AddVLink(string path)
    {
        RemoveVLink(path);
        vlinkList_.Add(path);
    }

    public void RemoveVLink(string path)
    {
        vlinkList_.Remove(path);
    }

    public int GetPackageSize(string path)
    {
        if (PackageSizeInfoList == null)
        {
            return 0;
        }

        foreach (var packageSizeInfo in PackageSizeInfoList)
        {
            if (packageSizeInfo.Path == path)
            {
                return packageSizeInfo.Size;
            }
        }

        return 0;
    }

    public void SetPackageSize(string i_path, int count)
    {
        if (PackageSizeInfoList == null)
        {
            return;
        }

        if (PackageSizeInfoList.Count > 8)
        {
            PackageSizeInfoList.RemoveAt(0);
        }

        var str = i_path.Replace('\\', '/');
        foreach (var packageSizeInfo in PackageSizeInfoList)
        {
            if (packageSizeInfo.Path == str)
            {
                packageSizeInfo.Size = count;
                return;
            }
        }

        PackageSizeInfoList.Add(new PackageSizeInfo
        {
            Path = str,
            Size = count
        });
    }

    public bool Save()
    {
        try
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (serializer == null)
            {
                serializer = new XmlSerializer(typeof(Configuration));
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fileStream, this);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Jenova: error: configuration save {0} {1}", filePath, ex.Message);
        }

        return false;
    }

    public static Configuration OpenOrNew(
        string path,
        JenovaApplicationConfiguration applicationConfig = null)
    {
        if (applicationConfig == null)
        {
            applicationConfig = new JenovaApplicationConfiguration();
        }

        if (!File.Exists(path))
        {
            var configuration = new Configuration();
            configuration.filePath = path;
            if (!applicationConfig.UsePerforce)
            {
                configuration.isUseP4 = false;
            }

            configuration.CultureName = applicationConfig.DefaultCultureName;
            return configuration;
        }

        if (serializer == null)
        {
            // try
            // {
                serializer = new XmlSerializer(typeof(Configuration));
            // }
            // catch (Exception ex)
            // {
            //     Console.Error.WriteLine(ex.Message);
            // }
        }

        if (serializer != null)
        {
            for (var index = 0; index < 10; ++index)
            {
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        if (serializer.Deserialize(fileStream) is Configuration configuration6)
                        {
                            configuration6.filePath = path;
                            if (applicationConfig != null && !applicationConfig.UsePerforce)
                            {
                                configuration6.isUseP4 = false;
                            }

                            // configuration6.CodeEditorTheme = new ThemeInfo();
                            // configuration6.CodeEditorTheme.Load(configuration6.CodeEditorThemePath);
                            return configuration6;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Jenova: configuration open {0} {1}, retry {2}", path, ex.Message,
                        index);
                }

                Thread.Sleep(2000);
            }
        }

        var configuration7 = new Configuration();
        configuration7.filePath = path;
        //configuration7.CodeEditorTheme = new ThemeInfo();
        //configuration7.CodeEditorTheme.Load(configuration7.CodeEditorThemePath);
        if (!applicationConfig.UsePerforce)
        {
            configuration7.isUseP4 = false;
        }

        configuration7.CultureName = applicationConfig.DefaultCultureName;
        Console.Error.WriteLine("Jenova: configuration open failed {0}", path);
        //var num = (int)MessageBox.Show("Jenova: configuration open failed - " + path);
        return configuration7;
    }

    public class PackageSizeInfo
    {
        public string Path { get; set; }

        public int Size { get; set; }
    }
}
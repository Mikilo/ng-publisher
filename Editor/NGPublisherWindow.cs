using NGToolsStandalone_For_NGPublisher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEditor;

namespace NGPublisher
{
	using NGToolsStandalone_For_NGPublisherEditor;
	using UnityEngine;

	[PrewarmEditorWindow]
	public class NGPublisherWindow : NGEditorWindow, IHasCustomMenu
	{
		private class VetPopup : PopupWindowContent
		{
			private AutoEditValue<VersionDetailed.Package.Version.UnityPackage.Vetting>	vetting;
			private string			target;
			private Vets.Option[]	options;

			public	VetPopup(AutoEditValue<VersionDetailed.Package.Version.UnityPackage.Vetting> vetting, string target, Vets.Option[] options)
			{
				this.vetting = vetting;
				this.target = target;
				this.options = options;
			}

			public override Vector2	GetWindowSize()
			{
				return new Vector2(100F, Constants.SingleLineHeight * this.options.Length + 2F);
			}

			public override void	OnGUI(Rect r)
			{
				r.height = Constants.SingleLineHeight;

				string	value;

				if (this.target == nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.platforms))
					value = this.vetting.input.platforms;
				else if (this.target == nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.unity_versions))
					value = this.vetting.input.unity_versions;
				else if (this.target == nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.srp_type))
					value = this.vetting.input.srp_type;
				else
					throw new Exception("Vet \"" + this.target + "\" is not implemented.");

				r.xMin += 2F;

				using (new EditorGUI.DisabledScope(this.vetting.input.allow_edit == false))
				{
					for (int i = 0, max = this.options.Length; i < max; ++i)
					{
						Vets.Option	option = this.options[i];

						EditorGUI.BeginChangeCheck();
						bool	toggled = GUI.Toggle(r, value.Contains(option.code), option.label);
						if (EditorGUI.EndChangeCheck() == true)
						{
							List<string>	list = new List<string>();

							if (value.Length > 0)
								list.AddRange(value.Split(','));

							if (toggled == true)
								list.Add(option.code);
							else
								list.Remove(option.code);

							this.vetting.output = new VersionDetailed.Package.Version.UnityPackage.Vetting()
							{
								allow_edit = this.vetting.input.allow_edit,
								dependencies = this.vetting.input.dependencies,
								//genesis_vetting_id = this.vetting.input.genesis_vetting_id,
								id = this.vetting.input.id,
								platforms = this.vetting.input.platforms,
								srp_type = this.vetting.input.srp_type,
								status = this.vetting.input.status,
								unity_versions = this.vetting.input.unity_versions,
							};

							if (this.target == nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.platforms))
								this.vetting.output.platforms = string.Join(",", list);
							else if (this.target == nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.unity_versions))
								this.vetting.output.unity_versions = string.Join(",", list);
							else if (this.target == nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.srp_type))
								this.vetting.output.srp_type = string.Join(",", list);

							if (this.vetting.ManualUpdate() == false)
								this.vetting.input = this.vetting.output;

							Utility.RepaintEditorWindow(typeof(NGPublisherWindow));
						}

						r.y += r.height;
					}
				}
			}
		}

		private class DependenciesPopup : PopupWindowContent
		{
			private AutoEditValue<VersionDetailed.Package.Version.UnityPackage.Vetting>	vetting;

			public	DependenciesPopup(AutoEditValue<VersionDetailed.Package.Version.UnityPackage.Vetting> vetting)
			{
				this.vetting = vetting;
			}

			public override Vector2	GetWindowSize()
			{
				return new Vector2(500F, Constants.SingleLineHeight * (this.vetting.input.dependencies.Length + 1));
			}

			public override void	OnGUI(Rect rect)
			{
				Rect	r = rect;
				r.height = Constants.SingleLineHeight;

				for (int i = 0, max = this.vetting.input.dependencies.Length; i < max; ++i)
				{
					string	dependency = this.vetting.input.dependencies[i];

					if (this.vetting.input.allow_edit == true)
						r.width -= 20F;

					EditorGUI.BeginChangeCheck();
					dependency = EditorGUI.TextField(r, dependency);
					if (dependency == string.Empty)
					{
						GUI.enabled = false;
						GUI.Label(r, "Copy/paste full Unity Asset Store V2 asset URL here");
						GUI.enabled = true;
					}
					if (EditorGUI.EndChangeCheck() == true && this.vetting.input.allow_edit == true)
					{
						this.vetting.output = new VersionDetailed.Package.Version.UnityPackage.Vetting()
						{
							allow_edit = this.vetting.input.allow_edit,
							dependencies = this.vetting.input.dependencies.Clone() as string[],
							//genesis_vetting_id = this.vetting.input.genesis_vetting_id,
							id = this.vetting.input.id,
							platforms = this.vetting.input.platforms,
							srp_type = this.vetting.input.srp_type,
							status = this.vetting.input.status,
							unity_versions = this.vetting.input.unity_versions,
						};
						this.vetting.output.dependencies[i] = dependency;

						if (this.vetting.ManualUpdate() == false)
							this.vetting.input = this.vetting.output;

						Utility.RepaintEditorWindow(typeof(NGPublisherWindow));
					}

					if (this.vetting.input.allow_edit == true)
					{
						r.x += r.width;

						r.width = 20F;
						if (GUI.Button(r, "X") == true)
						{
							List<string>	list = new List<string>(this.vetting.input.dependencies);

							list.RemoveAt(i);

							this.vetting.output = new VersionDetailed.Package.Version.UnityPackage.Vetting()
							{
								allow_edit = this.vetting.input.allow_edit,
								dependencies = list.ToArray(),
								//genesis_vetting_id = this.vetting.input.genesis_vetting_id,
								id = this.vetting.input.id,
								platforms = this.vetting.input.platforms,
								srp_type = this.vetting.input.srp_type,
								status = this.vetting.input.status,
								unity_versions = this.vetting.input.unity_versions,
							};

							if (this.vetting.ManualUpdate() == false)
								this.vetting.input = this.vetting.output;

							Utility.RepaintEditorWindow(typeof(NGPublisherWindow));
							return;
						}

						r.width = rect.width;
						r.x = rect.x;
					}

					r.y += r.height;
				}

				if (this.vetting.input.allow_edit == true && GUI.Button(r, "+") == true)
				{
					this.vetting.output = new VersionDetailed.Package.Version.UnityPackage.Vetting()
					{
						allow_edit = this.vetting.input.allow_edit,
						dependencies = this.vetting.input.dependencies,
						//genesis_vetting_id = this.vetting.input.genesis_vetting_id,
						id = this.vetting.input.id,
						platforms = this.vetting.input.platforms,
						srp_type = this.vetting.input.srp_type,
						status = this.vetting.input.status,
						unity_versions = this.vetting.input.unity_versions,
					};

					Array.Resize(ref this.vetting.output.dependencies, this.vetting.output.dependencies.Length + 1);
					this.vetting.output.dependencies[this.vetting.output.dependencies.Length - 1] = string.Empty;

					if (this.vetting.ManualUpdate() == false)
						this.vetting.input = this.vetting.output;

					Utility.RepaintEditorWindow(typeof(NGPublisherWindow));
					return;
				}
			}
		}

		private class AutoEditValue<T> : IDisposable
		{
			public const float	RestoreButtonWidth = 20F;

			private static GUIContent	resetContent = new GUIContent("✘", "Reset value");

			public int	Id { get { return this.id; } }
			public Rect	Rect { get { return this.Modified == true ? new Rect(layoutRect.x, layoutRect.y, layoutRect.width - 20F, layoutRect.height) : this.layoutRect; } }
			public bool	Modified { get; private set; }

			/// <summary>Use this property with its counter part EndWithOutput. Combo Begin/End replaces the usage of a using().</summary>
			public T	BeginWithInput
			{
				get
				{
					this.Start();
					return this.input;
				}
			}
			/// <summary>Use this property with its counter part BeginWithInput. Combo Begin/End replaces the usage of a using().</summary>
			public T	EndWithOutput
			{
				set
				{
					this.output = value;
					this.Dispose();
				}
			}

			public bool	isLayout;
			public Rect	layoutRect;
			public T	input;
			public T	output;

			public bool	hasCustomResetRect;
			public Rect	resetRect;

			private Dictionary<object, object>	modifiedValues;
			private int							id;
			private T							initial;

			public	AutoEditValue(Dictionary<object, object> modifiedValues, int id, T value)
			{
				this.modifiedValues = modifiedValues;
				this.id = id;
				this.initial = value;
				this.input = value;
			}

			public void	Dispose()
			{
				if (this.Modified == true)
				{
					Rect	r;

					if (this.hasCustomResetRect == true)
						r = this.resetRect;
					else
					{
						if (this.isLayout == true)
							r = GUILayoutUtility.GetRect(AutoEditValue<T>.RestoreButtonWidth, Constants.SingleLineHeight, EditorStyles.textField, GUILayoutOptionPool.Width(AutoEditValue<T>.RestoreButtonWidth));
						else
							r = new Rect(this.layoutRect.xMax - AutoEditValue<T>.RestoreButtonWidth, this.layoutRect.y, AutoEditValue<T>.RestoreButtonWidth, this.layoutRect.height);

						r.y -= 1F;
					}

					if (GUI.Button(r, AutoEditValue<T>.resetContent, GeneralStyles.ToolbarAltButton) == true)
					{
						this.output = this.initial;
						EditorGUIUtility.editingTextField = false;
					}
				}

				if (EditorGUI.EndChangeCheck() == true)
					this.ManualUpdate();

				if (this.isLayout == true)
					EditorGUILayout.EndHorizontal();
			}

			/// <summary>Removes itself from the list of modified values and returns the current input.</summary>
			/// <returns></returns>
			public T	Apply()
			{
				this.modifiedValues.Remove(id);
				return this.input;
			}

			public AutoEditValue<T>	Start()
			{
				if (this.isLayout == true)
					EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();

				return this;
			}

			public bool	ManualUpdate()
			{
				if (this.output != null && object.Equals(this.output, this.initial) == false)
				{
					this.Modified = true;
					this.input = this.output;

					if (this.modifiedValues.ContainsKey(this.id) == false)
						this.modifiedValues.Add(this.id, this);

					return true;
				}
				else
				{
					this.modifiedValues.Remove(this.id);
					return false;
				}
			}
		}

		public enum MainTab
		{
			Packages,
			Sales,
			Submit,
			Publish
		}

		public enum LanguageTab
		{
			KeyImages,
			Metadata,
			AudioVideo,
			Screenshots
		}

		public const string	Title = "NG Publisher";
		public const string	LastDatabasePathKeyPref = NGPublisherWindow.Title + "_lastDatabasePath";
		public const string	PackagesScrollbarOffsetKeyPref = NGPublisherWindow.Title + "_packagesScrollbarOffset";
		public const string	Path = "Assets/PublisherDatabase.asset";
		public const string	UnsupportedImageDescription = "This image is not supported. Use the version below.";
		public const int	IconImageWidthRequired = 160;
		public const int	IconImageHeightRequired = 160;
		public const string	IconImageDescription = "This image is visible while users are browsing the Asset Store. May also be used for promotional purposes.";
		public const int	CardImageWidthRequired = 420;
		public const int	CardImageHeightRequired = 280;
		public const string	CardImageDescription = "This is the main thumbnail visible while browsing the Asset Store. May also be used for promotional purposes.";
		public const int	CoverImageWidthRequired = 1950;
		public const int	CoverImageHeightRequired = 1300;
		public const string	CoverImageDescription = "The Cover Image is used on the main product page for your asset. May also be used for promotional purposes.";
		public const int	SocialMediaImageWidthRequired = 1200;
		public const int	SocialMediaImageHeightRequired = 630;
		public const string	SocialMediaImageDescription = "This image is used by our community team on social media posts, and may also be used on the Asset Store home page and other promotions.\nPlease do not add text or logo overlays onto this image.";
		public const float	DescriptionMaxWidth = 820F;
		public const float	ThumbnailHeight = 100F;
		public const int	MaxStringLength = 16382;
		public const float	PreviewMargin = 10F;
		public const float	DescriptionPreviewMinWidth = 600F;

		public static Color	HighlightColor = Color.green;

		public static readonly CategoryTips[]	Tips = new CategoryTips[]
		{
			new CategoryTips
			{
				tips = new Tip[]
				{
					new Tip("Ctrl-F to open up the packages list.", () => { XGUIHighlightManager.Highlight(NGPublisherWindow.Title + ".Package"); }),
					new Tip("Hold Alt to display more informations."),
					new Tip("Right-click on a locator to open the options.", () => { XGUIHighlightManager.Highlight(NGPublisherWindow.Title + ".Package", NGPublisherWindow.Title + ".Version", NGPublisherWindow.Title + ".Language"); }),
					new Tip("Left-click on a locator to switch back to it.", () => { XGUIHighlightManager.Highlight(NGPublisherWindow.Title + ".Package", NGPublisherWindow.Title + ".Version"); }),
				}
			}
		};

		private static readonly char[]	Extensions = new char[] { 'a', 'b', 'f', 'r', 'p', 'x' };

		private static GUIContent	applyContent = new GUIContent("✔", "Apply changes");

		public string	username = string.Empty;
		[NonSerialized]
		public string	password = string.Empty;
		[NonSerialized]
		public string	TFACode = string.Empty;
		public string	kharma_session = string.Empty;
		public string	kharma_token = string.Empty;
		public Session	lastSession;

		public int			selectedPackage;
		public int			selectedVersion;
		public string		selectedLanguage;
		public MainTab		mainTab;
		public LanguageTab	languageTab;

		[NonSerialized]
		private PublisherDatabase	database;
		private AssetStoreAPI		storeAPI;
		private PublisherAPI		publisherAPI;
		private Vector2				versionScrollPosition;
		private Vector2				versionLanguageScrollPosition;
		private string				databasePath = string.Empty;

		private Statuses			displayStatus = Statuses.All;
		private VerticalScrollbar	packagesScrollbar;

		private Package					targetHighlightPackage;
		private BgColorContentAnimator	targetHighlightPackageAnim;

		private Vector2	submitScrollPosition;
		private Vector2	publishScrollPosition;

		private Dictionary<object, object>	modifiedValues = new Dictionary<object, object>();

		[NonSerialized]
		private Texture2D	linkIcon;
		[NonSerialized]
		public GUIStyle		breadcrumbLeft;
		[NonSerialized]
		public GUIStyle		breadcrumbMid;

		private bool		displayPublishNotes;

		private Vector2				salesScrollPosition;
		private bool				editExport = false;
		[NonSerialized]
		private string				exportLabel;
		[NonSerialized]
		private string[]			assetsAsLabel;
		private IPackageExporter[]	exporters;
		[NonSerialized]
		private string[]			exportersAsLabel;
		private int					selectedAssetForExport = -1;
		private int					selectedExporter;
		private int					selectedPeriodForExport = -1;

		private int				selectedPeriod;
		private bool			displaySales = true;
		private bool			displayDownloads = true;
		private QueueRoutine	exportRoutine;

		[NonSerialized]
		private float	restoreOffset;

		private ErrorPopup		errorPopup = new ErrorPopup(NGPublisherWindow.Title, "An error occurred. Try to reopen " + NGPublisherWindow.Title + ".");
		private MessageCenter	messageCenter = new MessageCenter();

		[MenuItem("Window/" + NGPublisherWindow.Title, priority = Constants.MenuItemPriority - 100)]
		private static void		Open()
		{
			Utility.OpenWindow<NGPublisherWindow>(NGPublisherWindow.Title);
		}

		protected virtual void	OnEnable()
		{
			Utility.LoadEditorPref(this, NGEditorPrefs.GetPerProjectPrefix());

			Utility.RegisterWindow(this);
			Utility.RestoreIcon(this);

			this.storeAPI = new AssetStoreAPI();
			this.publisherAPI = new PublisherAPI(this.lastSession);
			this.exporters = Utility.CreateAllInstancesOf<IPackageExporter>();
			this.exportersAsLabel = new string[this.exporters.Length];
			for (int i = 0, max = this.exportersAsLabel.Length; i < max; ++i)
				this.exportersAsLabel[i] = this.exporters[i].Name;

			this.selectedExporter = Mathf.Clamp(this.selectedExporter, 0, this.exporters.Length - 1);

			string	path = NGEditorPrefs.GetString(NGPublisherWindow.LastDatabasePathKeyPref, NGPublisherWindow.Path, true);

			if (string.IsNullOrEmpty(path) == true)
				path = NGPublisherWindow.Path;

			this.database = AssetDatabase.LoadAssetAtPath<PublisherDatabase>(path);
			if (this.database == null)
			{
				try
				{
					this.database = ScriptableObject.CreateInstance<PublisherDatabase>();
					AssetDatabase.CreateAsset(this.database, path);
					Debug.Log("Publisher database generated at \"" + path + "\".");
				}
				catch (UnityException ex)
				{
					Debug.LogException(ex);

					if (this.database != null)
						Object.DestroyImmediate(this.database);
					this.database = null;
				}
			}

			this.wantsMouseMove = true;
		}

		protected virtual void	OnDisable()
		{
			Utility.UnregisterWindow(this);

			if (this.packagesScrollbar != null)
				NGEditorPrefs.SetFloat(NGPublisherWindow.PackagesScrollbarOffsetKeyPref, this.packagesScrollbar.Offset, true);

			if (this.database != null)
				NGEditorPrefs.SetString(NGPublisherWindow.LastDatabasePathKeyPref, AssetDatabase.GetAssetPath(this.database), true);

			if (this.publisherAPI.IsConnected == true)
				this.lastSession = this.publisherAPI.Session;
			else
				this.lastSession = null;

			Utility.SaveEditorPref(this, NGEditorPrefs.GetPerProjectPrefix());
		}

		protected override void	OnSubGUI()
		{
			if (this.linkIcon == null)
				this.linkIcon = EditorGUIUtility.FindTexture("SceneLoadOut");

			this.errorPopup.OnGUILayout();

			try
			{
				if (this.database == null)
				{
					EditorGUILayout.HelpBox("Your publisher data must be saved in an asset inside the project.\nChoose the path below:", MessageType.Info);

					EditorGUI.BeginChangeCheck();
					this.databasePath = NGEditorGUILayout.SaveFileField("Database Path", this.databasePath, "database", "asset");
					if (EditorGUI.EndChangeCheck() == true)
					{
						if (this.databasePath.StartsWith(Application.dataPath) == true)
							this.databasePath = this.databasePath.Substring(Application.dataPath.Length + 1 - "Assets/".Length);
					}

					bool	hasInvalidPath = this.databasePath.StartsWith("Assets/") == false;
					if (hasInvalidPath == true)
						EditorGUILayout.HelpBox("The path must begin with \"Assets/\"", MessageType.Warning);

					using (new EditorGUI.DisabledScope(hasInvalidPath))
					{
						if (GUILayout.Button("Generate database") == true)
						{
							this.database = ScriptableObject.CreateInstance<PublisherDatabase>();
							AssetDatabase.CreateAsset(this.database, this.databasePath);
						}
					}
					return;
				}

				if (this.publisherAPI.IsConnected == false)
				{
					this.OnGUIAuthentication();
					return;
				}

				bool	isGettingPackages = this.publisherAPI.IsGettingAllPackages();

				if (this.database.HasPackages == false && isGettingPackages == false)
				{
					isGettingPackages = true;
					this.database.RequestAllPackages(this.publisherAPI, this.HandleErrorOnly);
				}

				if (this.database.HasLanguages == false && this.publisherAPI.IsGettingLanguages() == false)
					this.database.RequestLanguages(this.publisherAPI, this.HandleErrorOnly);

				if (this.database.HasVets == false && this.publisherAPI.IsGettingVettingConfig() == false)
					this.database.RequestVettingConfig(this.publisherAPI, this.HandleErrorOnly);

				if (this.database.HasVoucherPackages == false && this.publisherAPI.IsGettingVoucherPackages(this.publisherAPI.Session.publisher) == false)
					this.database.RequestVoucherPackages(this.publisherAPI, this.HandleErrorOnly);

				if (this.database.HasVouchers == false && this.publisherAPI.IsGettingVouchers(this.publisherAPI.Session.publisher) == false)
					this.database.RequestVouchers(this.publisherAPI, this.HandleErrorOnly);

				if (this.database.HasPeriods == false && this.publisherAPI.IsGettingPeriods(this.publisherAPI.Session.publisher) == false)
					this.database.RequestPeriods(this.publisherAPI, this.HandleErrorOnly);

				using (new EditorGUILayout.HorizontalScope())
				{
					Utility.content.text = this.publisherAPI.Session.name + " (" + this.publisherAPI.Session.publisher + ")";
					Utility.content.image = TextureCache.Get(this.publisherAPI.Session.keyimage.icon);
					GUILayout.Label(Utility.content, GeneralStyles.Title1, GUILayoutOptionPool.Height(32F));

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Packages") == true)
					{
						this.mainTab = MainTab.Packages;
						return;
					}

					if (GUILayout.Button("Sales") == true)
					{
						this.mainTab = MainTab.Sales;
						return;
					}

					using (new EditorGUI.DisabledScope(isGettingPackages))
					{
						Utility.content.text = "Refresh packages";

						if (isGettingPackages == true)
							Utility.content.image = GeneralStyles.StatusWheel.image;
						else
							Utility.content.image = UtilityResources.Refresh;

						if (GUILayout.Button(Utility.content, GUILayoutOptionPool.Height(18F)) == true)
						{
							this.database.RequestAllPackages(this.publisherAPI, this.HandleErrorOnly);
							return;
						}

						Utility.content.image = null;
					}

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Disconnect") == true)
					{
						this.publisherAPI.Disconnect();
						return;
					}
				}

				if (isGettingPackages == true)
				{
					Utility.content.text = "Loading packages...";
					Utility.content.image = GeneralStyles.StatusWheel.image;
					GUILayout.Label(Utility.content, GeneralStyles.BigCenterText);
					Utility.content.image = null;
					this.Repaint();
					return;
				}

				if (this.mainTab == MainTab.Packages)
					this.OnGUIMainPackages();
				else if (this.mainTab == MainTab.Sales)
					this.OnGUIMainSales();
				else if (this.mainTab == MainTab.Submit)
					this.OnGUISubmit();
				else if (this.mainTab == MainTab.Publish)
					this.OnGUIPublish();
			}
			catch (ExitGUIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				this.errorPopup.error = ex;
			}
		}

		private void	OnGUIPublish()
		{
			EditorGUILayout.HelpBox("Publishing a version is not instant and might take a moment before being effective.", MessageType.Info);

			bool	hasPendingReviews = false;
			bool	isGettingVersion = false;
			Rect	canPublishAll = GUILayoutUtility.GetRect(0F, 24F);
			canPublishAll.xMin = canPublishAll.xMax - 150F;
			canPublishAll.x -= 10F;
			canPublishAll.height = 20F;

			using (var scroll = new EditorGUILayout.ScrollViewScope(this.publishScrollPosition))
			{
				this.publishScrollPosition = scroll.scrollPosition;

				for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
				{
					Package	package = this.database.Packages[i];

					for (int j = 0, max2 = package.versions.Length; j < max2; ++j)
					{
						Version	version = package.versions[j];

						if (version.detailed == null)
						{
							isGettingVersion = isGettingVersion == true || this.publisherAPI.IsGettingPackageVersion(version.id);

							if (isGettingVersion == false)
							{
								this.database.RequestGetPackageVersion(this.publisherAPI, version, this.HandleErrorOnly);
								isGettingVersion = true;
							}
						}
						else if (version.detailed.allow_publish == true)
						{
							hasPendingReviews = true;

							Rect	r = GUILayoutUtility.GetRect(0F, 24F + 4F, GUILayoutOptionPool.ExpandWidthTrue);

							r.x += 4F;
							r.width -= 8F;
							r.height -= 4F;
							this.Box(r, GUIContent.none);

							r.x += 4F;
							r.width -= 8F;
							float	w = r.width;

							Utility.content.text = version.name;
							r.y += 4F;
							r.height = 16F;
							r.width = GUI.skin.button.CalcSize(Utility.content).x;
							if (GUI.Button(r, Utility.content) == true)
							{
								this.mainTab = MainTab.Packages;
								this.selectedPackage = i;
								this.selectedVersion = j;
								this.selectedLanguage = null;
							}
							r.width = w;

							r.x = r.xMax - 80F - 10F - 80F - 10F - 100F - 4F;
							r.width = 80F;

							var	version_name = this.Edit(r, version.id + "version_name".GetHashCode(), version.version_name);
							r.x += r.width + 10F;
							var	price = this.Edit(r, version.id + "price".GetHashCode(), version.price);

							using (LabelWidthRestorer.Get(13F))
							{
								Utility.content.text = "V";
								Utility.content.tooltip = "Version name";
								this.Box(version_name.Rect, GUIContent.none);
								EditorGUI.TextField(version_name.Rect, Utility.content, version.version_name, EditorStyles.label);

								Utility.content.text = "$";
								Utility.content.tooltip = "Price";
								this.Box(price.Rect, GUIContent.none);
								EditorGUI.FloatField(price.Rect, Utility.content, version.price, EditorStyles.label);
							}

							bool	isPublishing = this.publisherAPI.IsPublishing(version.id) == true;

							using (new EditorGUI.DisabledScope(isPublishing))
							{
								r.x += r.width + 10F;
								r.width = 100F;
								if (GUI.Button(r, "Publish") == true)
								{
									version.RequestPublish(this.publisherAPI, string.Empty, response =>
									{
										if (this.CheckRequestResponse(response) == true)
										{
											Version	v = response.context as Version;
											Package	p = v.package;

											for (int k = 0, max3 = p.versions.Length; k < max3; ++k)
											{
												if (p.versions[k] == v)
												{
													List<Version>	list = new List<Version>(p.versions);
													list.Remove(v);
													p.versions = list.ToArray();
													break;
												}
											}
										}

										this.Repaint();
									});

									return;
								}

								if (isPublishing == true)
								{
									GUI.Label(r, GeneralStyles.StatusWheel);
									this.Repaint();
								}
							}
						}
					}
				}

				Utility.content.tooltip = null;

				if (hasPendingReviews == false)
					EditorGUILayout.HelpBox("No pending review available for submission.", MessageType.Info);

				if (isGettingVersion == true)
				{
					Utility.content.text = "Loading pending reviews...";
					Utility.content.image = GeneralStyles.StatusWheel.image;
					GUILayout.Label(Utility.content);
					Utility.content.image = null;

					this.Repaint();
				}
			}

			using (new EditorGUI.DisabledScope(hasPendingReviews == false || isGettingVersion == true))
			{
				if (GUI.Button(canPublishAll, "Publish All") == true && EditorUtility.DisplayDialog(NGPublisherWindow.Title, "Confirm publishing all?", "Yes", "No") == true)
				{
					for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
					{
						Package	package = this.database.Packages[i];

						for (int j = 0, max2 = package.versions.Length; j < max2; ++j)
						{
							Version	version = package.versions[j];

							if (version.detailed != null && version.detailed.allow_publish == true)
							{
								version.RequestPublish(this.publisherAPI, string.Empty, response =>
								{
									if (this.CheckRequestResponse(response) == true)
									{
										Version	v = response.context as Version;
										Package	p = v.package;

										for (int k = 0, max3 = p.versions.Length; k < max3; ++k)
										{
											if (p.versions[k] == v)
											{
												List<Version>	list = new List<Version>(p.versions);
												list.Remove(v);
												p.versions = list.ToArray();
												break;
											}
										}
									}

									this.Repaint();
								});

								break;
							}
						}
					}
				}
			}
		}

		private void	OnGUISubmit()
		{
			using (var scroll = new EditorGUILayout.ScrollViewScope(this.submitScrollPosition))
			{
				this.submitScrollPosition = scroll.scrollPosition;

				bool	hasDraft = false;
				bool	isGettingVersion = false;

				for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
				{
					Package	package = this.database.Packages[i];

					for (int j = 0, max2 = package.versions.Length; j < max2; ++j)
					{
						Version	version = package.versions[j];

						if (version.detailed == null)
						{
							isGettingVersion = isGettingVersion == true || this.publisherAPI.IsGettingPackageVersion(version.id);

							if (isGettingVersion == false)
							{
								this.database.RequestGetPackageVersion(this.publisherAPI, version, this.HandleErrorOnly);
								isGettingVersion = true;
							}
						}
						else if (version.detailed.allow_submit == true)
						{
							hasDraft = true;

							Rect	r = GUILayoutUtility.GetRect(0F, 24F + 4F, GUILayoutOptionPool.ExpandWidthTrue);

							r.x += 4F;
							r.width -= 8F;
							r.height -= 4F;
							this.Box(r, GUIContent.none);

							r.x += 4F;
							r.width -= 8F;
							float	w = r.width;

							Utility.content.text = version.name;
							r.y += 4F;
							r.height = 16F;
							r.width = GUI.skin.button.CalcSize(Utility.content).x;
							if (GUI.Button(r, Utility.content) == true)
							{
								this.mainTab = MainTab.Packages;
								this.selectedPackage = i;
								this.selectedVersion = j;
								this.selectedLanguage = null;
							}
							r.width = w;

							r.x = r.xMax - 80F - 10F - 80F - 10F - 100F - 4F;
							r.width = 80F;

							Rect	rApply = r;
							rApply.y -= 1F;
							rApply.x -= 50F + 4F;
							rApply.width = 50F;

							var	version_name = this.Edit(r, version.id + "version_name".GetHashCode(), version.version_name);
							r.x += r.width + 10F;
							var	price = this.Edit(r, version.id + "price".GetHashCode(), version.price);

							using (LabelWidthRestorer.Get(13F))
							{
								Utility.content.text = "V";
								Utility.content.tooltip = "Version name";
								{
									this.Box(version_name.Rect, GUIContent.none);
									version_name.EndWithOutput = EditorGUI.TextField(version_name.Rect, Utility.content, version_name.BeginWithInput);
								}

								Utility.content.text = "$";
								Utility.content.tooltip = "Price";
								this.Box(price.Rect, GUIContent.none);
								price.EndWithOutput = EditorGUI.FloatField(price.Rect, Utility.content, price.BeginWithInput);
							}

							bool	isSubmitting = this.publisherAPI.IsCanSubmit(version.id) == true || this.publisherAPI.IsSubmitting(version.id) == true;

							using (new EditorGUI.DisabledScope(isSubmitting))
							{
								r.x += r.width + 10F;
								r.width = 100F;
								if (GUI.Button(r, "Submit") == true && (this.modifiedValues.Count == 0 || EditorUtility.DisplayDialog(NGPublisherWindow.Title, "You modified values and did not validate them.\n\nAre you sure you would like to submit?", "Submit", "Cancel") == true))
								{
									version.RequestCanSubmit(this.publisherAPI, response =>
									{
										if (this.CheckRequestResponse(response) == true)
										{
											SubmissionWindow.Start(version.name, (autoPublish, comments) =>
											{
												version.RequestSubmit(this.publisherAPI, autoPublish, comments, submissionResponse =>
												{
													if (this.CheckRequestResponse(submissionResponse) == true)
													{
														Version	v = submissionResponse.context as Version;
														Package	p = v.package;

														for (int k = 0, max3 = p.versions.Length; k < max3; ++k)
														{
															if (p.versions[k] == v)
															{
																List<Version>	list = new List<Version>(p.versions);
																list.Remove(v);
																p.versions = list.ToArray();
																break;
															}
														}
													}
												});
											});
										}

										this.Repaint();
									});

									return;
								}

								if (isSubmitting == true)
								{
									GUI.Label(r, GeneralStyles.StatusWheel);
									this.Repaint();
								}
							}

							if (version_name.Modified == true ||
								price.Modified == true)
							{
								if (GUI.Button(rApply, NGPublisherWindow.applyContent, GeneralStyles.ToolbarValidButton) == true)
								{
									version.RequestSetPackage(this.publisherAPI,
															  version_name.input,
															  version.publishnotes,
															  version.category_id,
															  price.input,
															  response =>
									{
										if (this.CheckRequestResponse(response) == true)
										{
											EditorGUIUtility.editingTextField = false;
											version.version_name = version_name.Apply();
											version.price = price.Apply();
										}

										this.Repaint();
									});
								}
							}
						}
					}
				}

				Utility.content.tooltip = null;

				if (hasDraft == false)
					EditorGUILayout.HelpBox("No draft available for submission.", MessageType.Info);

				if (isGettingVersion == true)
				{
					Utility.content.text = "Loading drafts...";
					Utility.content.image = GeneralStyles.StatusWheel.image;
					GUILayout.Label(Utility.content);
					Utility.content.image = null;

					this.Repaint();
				}
			}
		}

		private void	OnGUIMainSales()
		{
			string	today = DateTime.Now.ToString("yyyy-MM-dd");
			bool	isFetchingPeriods = this.publisherAPI.IsGettingPeriods(this.publisherAPI.Session.publisher);

			if (isFetchingPeriods == true)
			{
				Utility.content.text = "Loading periods...";
				Utility.content.image = GeneralStyles.StatusWheel.image;
				GUILayout.Label(Utility.content, GeneralStyles.BigCenterText);
				Utility.content.image = null;
				this.Repaint();
				return;
			}

			if (this.database.HasPeriods == true)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (this.assetsAsLabel == null)
					{
						this.assetsAsLabel = new string[this.database.Packages.Length + 1];
						this.assetsAsLabel[0] = "All";

						for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
							this.assetsAsLabel[i + 1] = this.database.Packages[i].name;
					}

					this.selectedAssetForExport = Mathf.Clamp(this.selectedAssetForExport, 0, this.assetsAsLabel.Length - 1);
					this.selectedPeriodForExport = Mathf.Clamp(this.selectedPeriodForExport, 0, this.database.Periods.Length - 1);

					if (this.editExport == true)
					{
						GUILayout.Label("Export");
						this.selectedAssetForExport = NGEditorGUILayout.Popup(this.selectedAssetForExport, this.assetsAsLabel, GUILayoutOptionPool.Width(100F));
						GUILayout.Label("to");
						this.selectedExporter = NGEditorGUILayout.Popup(this.selectedExporter, this.exportersAsLabel, GUILayoutOptionPool.Width(50F));
						GUILayout.Label("since");
						this.selectedPeriodForExport = NGEditorGUILayout.Popup(null, this.selectedPeriodForExport, this.database.Periods, p => p.name);

						if (GUILayout.Button("Save") == true)
						{
							this.editExport = !this.editExport;
							this.exportLabel = null;
						}
					}
					else
					{
						if (this.exportLabel == null)
							this.exportLabel = "Export " + this.assetsAsLabel[this.selectedAssetForExport] + " to " + this.exportersAsLabel[this.selectedExporter] + " since " + this.database.Periods[this.selectedPeriodForExport].name;

						using (BgColorContentRestorer.Get(GeneralStyles.HighlightActionButton))
						{
							if (GUILayout.Button(this.exportLabel, "ButtonLeft") == true)
							{
								this.exportRoutine = QueueRoutine.Create();
								this.exportRoutine.HandleError(qr =>
								{
									this.exportRoutine = null;
									Utility.AsyncProgressBarClear();
									Debug.LogError(qr.CurrentCommand.Error);
								});
								this.exportRoutine.AddRoutineCommand(this.ExportMetrics);
								this.exportRoutine.Start();
							}
						}

						if (GUILayout.Button("☰", "ButtonRight") == true)
							this.editExport = !this.editExport;
					}

					if (this.exportRoutine != null)
					{
						if (GUILayout.Button("Cancel Export") == true)
						{
							Utility.AsyncProgressBarClear();
							this.exportRoutine.Stop();
							this.exportRoutine = null;
						}
					}

					GUILayout.FlexibleSpace();

					if (this.database.Periods[0].value == DateTime.Now.Year * 100 + DateTime.Now.Month + 1 && this.publisherAPI.IsGettingPeriods(this.publisherAPI.Session.publisher) == false)
						this.database.RequestPeriods(this.publisherAPI, this.HandleErrorOnly);

					Utility.content.text = "Refresh";
					Utility.content.image = UtilityResources.Refresh;
					if (GUILayout.Button(Utility.content, GUILayoutOptionPool.Height(Constants.SingleLineHeight)) == true)
					{
						Period	period2 = this.database.Periods[this.selectedPeriod];

						this.database.RequestSaleCounts(this.publisherAPI, period2, this.HandleErrorOnly);
						this.database.RequestFreeDownloads(this.publisherAPI, period2, this.HandleErrorOnly);
					}
					Utility.content.image = null;

					Rect	r = GUILayoutUtility.GetRect(80F, Constants.SingleLineHeight);
					r.y += 3F;
					this.selectedPeriod = NGEditorGUILayout.Popup(r, null, this.selectedPeriod, this.database.Periods, p => p.name);
				}

				Event	currentEvent = Event.current;

				if (currentEvent.type == EventType.KeyDown && currentEvent.control == true)
				{
					if (currentEvent.keyCode == KeyCode.RightArrow)
					{
						if (this.selectedPeriod - 1 >= 0)
						{
							--this.selectedPeriod;
							GUI.FocusControl(null);
							Event.current.Use();
						}
					}
					else if (currentEvent.keyCode == KeyCode.LeftArrow)
					{
						if (this.selectedPeriod + 1 < this.database.Periods.Length)
						{
							++this.selectedPeriod;
							GUI.FocusControl(null);
							Event.current.Use();
						}
					}
				}

				Period	period = this.database.Periods[this.selectedPeriod];
				bool	isFetchingSaleCounts = this.publisherAPI.IsGettingSaleCounts(this.publisherAPI.Session.publisher, period.value);

				if (period.HasSales == false && isFetchingSaleCounts == false)
				{
					isFetchingSaleCounts = true;
					this.database.RequestSaleCounts(this.publisherAPI, period, this.HandleErrorOnly);
				}

				bool	isFetchingFreeDownloads = this.publisherAPI.IsGettingFreeDownloads(this.publisherAPI.Session.publisher, period.value);

				if (period.HasDownloads == false && isFetchingFreeDownloads == false)
				{
					isFetchingFreeDownloads = true;
					this.database.RequestFreeDownloads(this.publisherAPI, period, this.HandleErrorOnly);
				}

				EditorGUILayout.Space();

				using (var scroll = new EditorGUILayout.ScrollViewScope(this.salesScrollPosition))
				{
					this.salesScrollPosition = scroll.scrollPosition;

					this.displaySales = EditorGUILayout.Foldout(this.displaySales, "Sales", true);
					if (this.displaySales == true)
					{
						if (isFetchingSaleCounts == true)
						{
							Utility.content.text = "Fetching sales...";
							Utility.content.image = GeneralStyles.StatusWheel.image;
							GUILayout.Label(Utility.content, GeneralStyles.BigCenterText);
							Utility.content.image = null;
							this.Repaint();
						}

						if (period.HasSales == true)
						{
							float	maxPackageWidth = 0F;

							if (period.HasSales == true)
							{
								for (int i = 0, max = period.Sales.Length; i < max; ++i)
								{
									Sale	sale = period.Sales[i];

									Utility.content.text = sale.asset;
									float	width = GeneralStyles.LeftButton.CalcSize(Utility.content).x;

									if (maxPackageWidth < width)
										maxPackageWidth = width;
								}
							}

							using (new EditorGUILayout.HorizontalScope())
							{
								EditorGUILayout.LabelField("Package", EditorStyles.boldLabel, GUILayoutOptionPool.MaxWidth(maxPackageWidth));
								EditorGUILayout.LabelField("Price", EditorStyles.boldLabel, GUILayoutOptionPool.Width(60F));
								EditorGUILayout.LabelField("Qty", EditorStyles.boldLabel, GUILayoutOptionPool.Width(50F));
								EditorGUILayout.LabelField("Refunds", EditorStyles.boldLabel, GUILayoutOptionPool.Width(60F));
								EditorGUILayout.LabelField("Chargebacks", EditorStyles.boldLabel, GUILayoutOptionPool.Width(90F));
								EditorGUILayout.LabelField("Gross", EditorStyles.boldLabel, GUILayoutOptionPool.Width(80F));
								EditorGUILayout.LabelField("First", EditorStyles.boldLabel, GUILayoutOptionPool.Width(75F));
								EditorGUILayout.LabelField("Last", EditorStyles.boldLabel, GUILayoutOptionPool.Width(75F));
							}

							for (int i = 0, max = period.Sales.Length; i < max; ++i)
							{
								Sale	sale = period.Sales[i];

								using (new EditorGUILayout.HorizontalScope())
								{
									if (GUILayout.Button(sale.asset, GeneralStyles.LeftButton, GUILayoutOptionPool.MaxWidth(maxPackageWidth)) == true)
										Application.OpenURL(sale.short_url);

									EditorGUILayout.TextField(sale.price, GUILayoutOptionPool.Width(60F));
									EditorGUILayout.IntField(sale.quantity, GUILayoutOptionPool.Width(50F));
									EditorGUILayout.IntField(sale.refunds, GUILayoutOptionPool.Width(60F));
									EditorGUILayout.IntField(sale.chargebacks, GUILayoutOptionPool.Width(90F));
									EditorGUILayout.TextField(sale.gross, GUILayoutOptionPool.Width(80F));

									using (BgColorContentRestorer.Get(today == sale.first, Color.green))
									{
										EditorGUILayout.TextField(sale.first, GUILayoutOptionPool.Width(75F));
									}

									using (BgColorContentRestorer.Get(today == sale.last, Color.green))
									{
										EditorGUILayout.TextField(sale.last, GUILayoutOptionPool.Width(75F));
									}
								}
							}
						}
					}

					this.displayDownloads = EditorGUILayout.Foldout(this.displayDownloads, "Downloads", true);
					if (this.displayDownloads == true)
					{
						if (isFetchingFreeDownloads == true)
						{
							Utility.content.text = "Fetching downloads...";
							Utility.content.image = GeneralStyles.StatusWheel.image;
							GUILayout.Label(Utility.content, GeneralStyles.BigCenterText);
							Utility.content.image = null;
							this.Repaint();
						}

						if (period.HasDownloads == true)
						{
							float	maxPackageWidth = 0F;

							if (period.HasSales == true)
							{
								for (int i = 0, max = period.Downloads.Length; i < max; ++i)
								{
									Download	download = period.Downloads[i];

									Utility.content.text = download.asset;
									float	width = GeneralStyles.LeftButton.CalcSize(Utility.content).x;

									if (maxPackageWidth < width)
										maxPackageWidth = width;
								}
							}

							using (new EditorGUILayout.HorizontalScope())
							{
								EditorGUILayout.LabelField("Package", EditorStyles.boldLabel, GUILayoutOptionPool.MaxWidth(maxPackageWidth));
								EditorGUILayout.LabelField("Qty", EditorStyles.boldLabel, GUILayoutOptionPool.Width(50F));
								EditorGUILayout.LabelField("First", EditorStyles.boldLabel, GUILayoutOptionPool.Width(75F));
								EditorGUILayout.LabelField("Last", EditorStyles.boldLabel, GUILayoutOptionPool.Width(75F));
							}

							for (int i = 0, max = period.Downloads.Length; i < max; ++i)
							{
								Download	download = period.Downloads[i];

								using (new EditorGUILayout.HorizontalScope())
								{
									if (GUILayout.Button(download.asset, GeneralStyles.LeftButton, GUILayoutOptionPool.MaxWidth(maxPackageWidth)) == true)
										Application.OpenURL(download.short_url);

									EditorGUILayout.IntField(download.quantity, GUILayoutOptionPool.Width(50F));

									using (BgColorContentRestorer.Get(today == download.first, Color.green))
									{
										EditorGUILayout.TextField(download.first, GUILayoutOptionPool.Width(75F));
									}

									using (BgColorContentRestorer.Get(today == download.last, Color.green))
									{
										EditorGUILayout.TextField(download.last, GUILayoutOptionPool.Width(75F));
									}
								}
							}
						}
					}
				}
			}
		}

		private void	OnGUIMainPackages()
		{
			this.selectedPackage = Mathf.Clamp(this.selectedPackage, 0, this.database.Packages.Length - 1);

			if (this.selectedPackage < 0 || this.selectedPackage >= this.database.Packages.Length)
				return;

			if (this.breadcrumbLeft == null)
			{
				this.breadcrumbLeft = new GUIStyle(UnityStyles.Breadcrumbleft);
				this.breadcrumbLeft.padding = new RectOffset(10, 10, 0, 0);
				this.breadcrumbLeft.stretchHeight = true;
				this.breadcrumbLeft.alignment = TextAnchor.MiddleCenter;

				this.breadcrumbMid = new GUIStyle(UnityStyles.Breadcrumbmid);
				this.breadcrumbMid.padding = new RectOffset(10, 10, 0, 0);
				this.breadcrumbMid.stretchHeight = true;
				this.breadcrumbMid.alignment = TextAnchor.MiddleCenter;

				this.packagesScrollbar = new VerticalScrollbar(0F, 0F, 0F);
				this.restoreOffset = NGEditorPrefs.GetFloat(NGPublisherWindow.PackagesScrollbarOffsetKeyPref, -1, true);

				//if (restoreOffset > 0F)
				//{
				//	Event	e = EditorGUIUtility.CommandEvent("RestoreOffset");
				//	e.button = (int)restoreOffset;
				//	this.SendEvent(new Event(e));
				//}
			}

			Package										package = this.database.Packages[this.selectedPackage];
			Version										version = null;
			VersionDetailed.Package.Version.Language	language = null;

			if (this.selectedVersion != -1)
			{
				this.selectedVersion = Mathf.Clamp(this.selectedVersion, 0, package.versions.Length - 1);

				if (this.selectedVersion >= 0 && this.selectedVersion < package.versions.Length)
				{
					version = package.versions[this.selectedVersion];

					if (this.selectedLanguage != null)
					{
						if (version.detailed != null)
							language = version.detailed.package.version[this.selectedLanguage];
					}
				}
			}

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				string	packageLabel = package.name;
				Utility.content.text = packageLabel;

				Rect	r = GUILayoutUtility.GetRect(Utility.content, this.breadcrumbLeft);

				if (Event.current.type == EventType.ValidateCommand)
				{
					if (Event.current.commandName == "Find")
						Event.current.Use();
				}
				else if (GUI.Button(r, Utility.content, this.breadcrumbLeft) == true ||
						 (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "Find"))
				{
					if (this.selectedVersion != -1 && Event.current.button == 0 && Event.current.type != EventType.ExecuteCommand)
					{
						this.selectedVersion = -1;
						this.selectedLanguage = null;
					}
					else
					{
						List<string>	labels = new List<string>(this.database.Packages.Length);

						for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
							labels.Add(this.database.Packages[i].name);

						if (Event.current.type == EventType.ExecuteCommand)
						{
							r.position += Utility.GetParentScreenPosition(this).position + this.position.position;
							r.y += 24F;
						}
						else
							r.position = GUIUtility.GUIToScreenPoint(r.position);

						r.x += this.breadcrumbLeft.padding.left;
						r.y += r.height;

						StringSelector	selector = StringSelector.Create(r, false, labels.ToArray(), (s) =>
						{
							this.selectedPackage = labels.IndexOf(s);

							if (version != null)
							{
								Package	newPackage = this.database.Packages[this.selectedPackage];

								for (int i = 0, max = newPackage.versions.Length; i < max; ++i)
								{
									Version	element = newPackage.versions[i];

									if (version.status == element.status)
									{
										this.selectedVersion = i;
										break;
									}
								}
							}
							else
							{
								float	offset = 0F;

								for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
								{
									float	height = this.GetPackageHeight(this.database.Packages[i]);

									if (i == this.selectedPackage)
									{
										this.targetHighlightPackage = this.database.Packages[i];
										this.targetHighlightPackageAnim = new BgColorContentAnimator(this.Repaint, 0F, 1F);
										this.targetHighlightPackageAnim.af.speed = .5F;

										if (offset < this.packagesScrollbar.Offset)
											this.packagesScrollbar.Offset = offset;
										if (offset + height > this.packagesScrollbar.Offset + this.packagesScrollbar.MaxHeight)
											this.packagesScrollbar.Offset = offset + height - this.packagesScrollbar.MaxHeight;
										break;
									}

									offset += height;
								}
							}

							GUI.FocusControl(null);
							this.Repaint();
						}, true);

						selector.CloseOnSelection = true;
						selector.Window.Selected = this.selectedPackage;
						selector.Window.Scrollbar.RealHeight = (selector as ISearchSelector).GetTotalHeight();
						selector.Window.JumpToSelected();

						GUIUtility.ExitGUI();
					}
				}
				XGUIHighlightManager.DrawHighlight(NGPublisherWindow.Title + ".Package", this, r, XGUIHighlights.Glow | XGUIHighlights.Rectangle);

				if (version != null)
				{
					Utility.content.text = Utility.NicifyVariableName(version.status);
					r = GUILayoutUtility.GetRect(Utility.content, this.breadcrumbMid);

					using (ColorStyleRestorer.Get(this.breadcrumbMid, Status.GetColor(version.status)))
					{
						if (GUI.Button(r, Utility.content, this.breadcrumbMid) == true)
						{
							if (this.selectedLanguage != null && Event.current.button == 0)
								this.selectedLanguage = null;
							else
							{
								List<string>	labels = new List<string>(package.versions.Length);

								for (int i = 0, max = package.versions.Length; i < max; ++i)
									labels.Add(Utility.NicifyVariableName(package.versions[i].status));

								r.position = GUIUtility.GUIToScreenPoint(r.position);
								r.x += this.breadcrumbMid.padding.left;
								r.y += r.height;

								StringSelector	selector = StringSelector.Create(r, false, labels.ToArray(), (s) =>
								{
									this.selectedVersion = labels.IndexOf(s);
									EditorGUIUtility.editingTextField = false;
									GUI.FocusControl(null);
									this.Repaint();
								}, true);

								selector.CloseOnSelection = true;
								selector.Window.Selected = this.selectedVersion;
								selector.Window.Scrollbar.RealHeight = (selector as ISearchSelector).GetTotalHeight();
								selector.Window.JumpToSelected();

								GUIUtility.ExitGUI();
							}
						}
					}
					XGUIHighlightManager.DrawHighlight(NGPublisherWindow.Title + ".Version", this, r, XGUIHighlights.Glow | XGUIHighlights.Rectangle);

					if (string.IsNullOrEmpty(this.selectedLanguage) == false)
					{
						Utility.content.text = this.selectedLanguage;
						r = GUILayoutUtility.GetRect(Utility.content, this.breadcrumbMid);

						if (GUI.Button(r, Utility.content, this.breadcrumbMid) == true)
						{
							int	n = this.database.versions.FindIndex(v => v.package.version.id == version.id);

							if (n != -1)
							{
								VersionDetailed					versionDetail = this.database.versions[n];
								VersionDetailed.Package.Version	VDPVersion = versionDetail.package.version;
								List<string>					labels = new List<string>();

								foreach (VersionDetailed.Package.Version.Language lang in VDPVersion.EachLanguage)
									labels.Add(lang.languageCode);

								r.position = GUIUtility.GUIToScreenPoint(r.position);
								r.x += this.breadcrumbMid.padding.left;
								r.y += r.height;

								StringSelector	selector = StringSelector.Create(r, false, labels.ToArray(), (s) =>
								{
									this.selectedLanguage = s;
									GUI.FocusControl(null);
									this.Repaint();
								}, true);

								selector.CloseOnSelection = true;
								selector.Window.Selected = labels.IndexOf(this.selectedLanguage);
								selector.Window.Scrollbar.RealHeight = (selector as ISearchSelector).GetTotalHeight();
								selector.Window.JumpToSelected();

								GUIUtility.ExitGUI();
							}
						}
						XGUIHighlightManager.DrawHighlight(NGPublisherWindow.Title + ".Language", this, r, XGUIHighlights.Glow | XGUIHighlights.Rectangle);
					}
				}

				GUILayout.FlexibleSpace();

				if (this.database.HasPackages == true)
				{
					bool	canSubmit = false;

					for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
					{
						package = this.database.Packages[i];

						for (int j = 0, max2 = package.versions.Length; j < max2; ++j)
						{
							if (package.versions[j].status == Status.Draft)
							{
								canSubmit = true;
								break;
							}
						}
					}

					if (canSubmit == true && GUILayout.Button("Multi Submit", EditorStyles.toolbarButton) == true)
						this.mainTab = MainTab.Submit;

					bool	canPublish = false;

					for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
					{
						package = this.database.Packages[i];

						for (int j = 0, max2 = package.versions.Length; j < max2; ++j)
						{
							if (package.versions[j].status == Status.Pending)
							{
								canPublish = true;
								break;
							}
						}
					}

					if (canPublish == true && GUILayout.Button("Multi Publish", EditorStyles.toolbarButton) == true)
						this.mainTab = MainTab.Publish;
				}
			}

			if (language != null)
				this.OnGUIVersionLanguage(version, language);
			else if (version != null)
				this.OnGUIVersion(version);
			else
				this.OnGUIPackages();
		}

		private void	OnGUIAuthentication()
		{
			using (LabelWidthRestorer.Get(70F))
			{
				GUILayout.Label("Publisher Login", GeneralStyles.Title1);

				this.username = EditorGUILayout.TextField("Username", this.username);
				this.password = EditorGUILayout.PasswordField("Password", this.password);

				using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(this.username) == true || string.IsNullOrEmpty(this.password) == true || this.publisherAPI.IsConnecting() == true))
				{
					if (GUILayout.Button("Connect") == true)
					{
						this.publisherAPI.Connect(this.username, this.password, (request, result) =>
						{
							this.Repaint();

							if (request == null || request.StatusCode != HttpStatusCode.OK)
							{
								Debug.LogError(result);
								return;
							}

							if (result[0] == '{') // Let's consider it is JSON and try to get a response if available.
							{
								try
								{
									result = Response.MergeErrors(result);

									Response	response = JsonUtility.FromJson<Response>(result);

									if (response.Succeeded == false)
									{
										if (response.errors.error != null)
											Debug.LogError(response.errors.error.Replace("<br>", "\n"));
										return;
									}
								}
								catch (Exception ex)
								{
									Debug.LogError(result);
									Debug.LogException(ex);
								}
							}

							if (Conf.DebugMode == Conf.DebugState.Verbose)
							{
								//if (result[0] == '{' || result[0] == '[')
								//	NGDebug.LogJSON(result);
								//else
									Debug.Log(result);
							}

							if (this.publisherAPI.IsConnected == false)
							{
								this.ShowNotification(new GUIContent("Credentials not recognized."));
								Debug.Log("Credentials not recognized.");
							}
						});
						return;
					}

					if (this.publisherAPI.IsConnecting() == true)
					{
						GUI.Label(GUILayoutUtility.GetLastRect(), GeneralStyles.StatusWheel);
						this.Repaint();
					}

					if (this.publisherAPI.WaitingTFACode == true)
					{
						this.TFACode = EditorGUILayout.TextField("Code", this.TFACode);

						using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(this.TFACode) == true))
						{
							if (GUILayout.Button("Verify") == true)
							{
								this.publisherAPI.ValidateTFACode(this.TFACode, (request, result) =>
								{
									Debug.Log("Connected!");
								});
							}
						}
					}
				}

				if (Conf.DebugMode != Conf.DebugState.None)
				{
					GUILayout.Space(10F);

					if (GUILayout.Button("Connect Via Unity") == true)
					{
						this.publisherAPI.ConnectViaUnity((request, result) =>
						{
						});
					}

					GUILayout.Space(10F);

					GUILayout.Label("Cookie Credential", GeneralStyles.Title1);
					this.kharma_session = EditorGUILayout.TextField("Session", this.kharma_session);
					this.kharma_token = EditorGUILayout.TextField("Token", this.kharma_token);

					using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(this.kharma_session) == true || string.IsNullOrEmpty(this.kharma_token) == true))
					{
						if (GUILayout.Button("Use") == true)
							this.publisherAPI.Use(this.kharma_session, this.kharma_token, null);
					}
				}
			}
		}

		private void	OnGUIPackages()
		{
			GUILayout.Space(5F);

			Rect		r;
			float		totalHeight = 0F;
			string[]	statusNames = Enum.GetNames(typeof(Statuses));
			Array		statusValues = Enum.GetValues(typeof(Statuses));
			int[]		statusCount = new int[Status.GetEnumMaxIndex()];

			this.packagesScrollbar.ClearInterests();

			for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
			{
				Package	package = this.database.Packages[i];

				for (int j = 0, k = 0, max2 = package.versions.Length; j < max2; ++j)
				{
					Version	version = package.versions[j];

					++statusCount[Status.GetEnumIndex(version.status)];

					if ((this.displayStatus & version.Status) != 0)
					{
						this.packagesScrollbar.AddInterest(totalHeight + 2F + 16F + 2F + 16F + 2F + k * (6F + 24F + 2F), Status.GetColor(version.status));
						++k;
					}
				}

				totalHeight += this.GetPackageHeight(package);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				for (int i = 0, max = statusCount.Length; i < max; ++i)
				{
					if (statusCount[i] > 0)
					{
						bool	isDisplayed = (this.displayStatus & (Statuses)statusValues.GetValue(i)) != 0;

						using (BgColorContentRestorer.Get(Status.GetColor(statusNames[i])))
						{
							EditorGUI.BeginChangeCheck();
							GUILayout.Toggle(isDisplayed, statusNames[i], "buttonmid");
							if (EditorGUI.EndChangeCheck() == true)
								this.displayStatus ^= (Statuses)statusValues.GetValue(i);
						}

						r = GUILayoutUtility.GetLastRect();
						r.x += 5F;
						r.y += 1F;
						GUI.Toggle(r, isDisplayed, GUIContent.none);
					}
				}
			}

			GUILayout.Space(5F);

			r = GUILayoutUtility.GetLastRect();
			r.y += r.height;

			r.width = this.position.width;
			r.height = this.position.height - r.y;

			this.packagesScrollbar.RealHeight = totalHeight;
			this.packagesScrollbar.SetPosition(r.width - this.packagesScrollbar.MaxWidth, r.y);
			this.packagesScrollbar.SetSize(r.height);
			this.packagesScrollbar.OnGUI();

			if (this.restoreOffset > 0F && Event.current.type != EventType.Layout)
			{
				this.packagesScrollbar.Offset = this.restoreOffset;
				this.restoreOffset = -1F;
			}

			r.width -= this.packagesScrollbar.MaxWidth;
			GUI.BeginGroup(r);
			{
				r.x = 0F;
				r.y = -this.packagesScrollbar.Offset;
				r.height = this.packagesScrollbar.RealHeight;
				GUILayout.BeginArea(r);
				{
					Event	currentEvent = Event.current;

					for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
					{
						Package	package = this.database.Packages[i];

						this.OnGUIPackage(package);
					}
				}
				GUILayout.EndArea();
			}
			GUI.EndGroup();
		}

		private float	GetPackageHeight(Package package)
		{
			float	versionsHeight = 2F + 16F + 2F + 16F + 2F + 2F + 8F; // GUILayout spacing + Title + Spacing + Ratings + Versions + Spacing + inter packages Spacing
			int		displayedVersionsCount = 0;

			for (int i = 0, max = package.versions.Length; i < max; ++i)
			{
				Version	version = package.versions[i];

				if ((this.displayStatus & version.Status) != 0)
				{
					versionsHeight += 6F + 24F + 2F;
					++displayedVersionsCount;
				}
			}

			if (displayedVersionsCount == 0)
				return 0F;
			return versionsHeight;
		}

		private void	OnGUIPackage(Package package)
		{
			int		displayedVersionsCount = 0;
			float	versionsHeight = 0F;
			bool	hasDraft = false;

			for (int i = 0, max = package.versions.Length; i < max; ++i)
			{
				Version	version = package.versions[i];

				if (version.status == Status.Draft)
					hasDraft = true;

				if ((this.displayStatus & version.Status) != 0)
				{
					versionsHeight += 6F + 24F + 2F;
					++displayedVersionsCount;
				}
			}

			if (displayedVersionsCount == 0)
				return;

			Rect	rBox = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutOptionPool.Height(16F + 2F + 16F + 2F + versionsHeight + 2F));
			Rect	r3 = rBox;

			rBox.yMin += 8F;

			BgColorContentAnimator	anim = this.targetHighlightPackage == package ? targetHighlightPackageAnim : null;

			using (anim != null ? anim.Restorer(NGPublisherWindow.HighlightColor * anim.Value) : null)
			{
				if (anim != null && anim.af.isAnimating == false)
				{
					this.targetHighlightPackage = null;
					this.targetHighlightPackageAnim = null;
				}

				this.Box(rBox, GUIContent.none);
			}

			r3.xMin += 6F;
			r3.height = 16F;

			string	category = package.Category;
			Event	currentEvent = Event.current;

			if (currentEvent.type != EventType.Repaint && currentEvent.type != EventType.Layout)
			{
				if (currentEvent.keyCode == KeyCode.LeftAlt)
					this.Repaint();
			}

			if (category != string.Empty)
				Utility.content.text = package.name + " (" + category + ")";
			else
				Utility.content.text = package.name;

			if (currentEvent.alt == true)
				Utility.content.text = Utility.content.text + " (" + package.id + ")";

			r3.width = EditorStyles.label.CalcSize(Utility.content).x;
			this.Box(r3, GUIContent.none); // Double the call to overdraw the background.
			this.Box(r3, GUIContent.none);
			EditorGUI.SelectableLabel(r3, Utility.content.text, EditorStyles.label);

			r3.x = rBox.width - 50F;

			r3.y -= 2F;
			r3.width = 50F;
			r3.height = 20F;
			Utility.content.text = "Go    ";
			Utility.content.tooltip = package.short_url;
			if (GUI.Button(r3, Utility.content) == true)
				Application.OpenURL(package.short_url);

			r3.x += 28F + this.linkIcon.width;
			r3.y += 2F;
			r3.width = -this.linkIcon.width;
			r3.height = this.linkIcon.height;
			GUI.DrawTexture(r3, this.linkIcon, ScaleMode.ScaleToFit);

			r3.y += 2F;
			Utility.content.text = package.short_url;
			r3.width = EditorStyles.miniTextField.CalcSize(Utility.content).x + 2F;
			r3.x = rBox.width - 50F - r3.width;
			EditorGUI.TextField(r3, package.short_url, EditorStyles.miniTextField);
			Utility.content.image = null;

			if (this.database.HasPackageVouchers(package) == true)
			{
				r3.y -= 2F;
				Utility.content.text = package.short_url;
				r3.width = 80F;
				r3.x -= r3.width + 4F;
				if (GUI.Button(r3, "Vouchers") == true)
				{
					VouchersWindow.Open(this.database, this.publisherAPI, package);
					return;
				}
			}

			StringBuilder	buffer = Utility.GetBuffer();

			if (package.average_rating == 0)
				buffer.Append("N/A, ");
			else
			{
				buffer.Append(package.average_rating);
				buffer.Append("/5, ");
			}

			if (package.count_ratings == 0)
				buffer.Append("0 rating");
			else if (package.count_ratings == 1)
				buffer.Append("1 rating");
			else
			{
				buffer.Append(package.count_ratings);
				buffer.Append(" ratings");
			}

			Utility.content.text = Utility.ReturnBuffer(buffer);
			Utility.content.tooltip = null;
			Utility.content.image = UtilityResources.Star;
			r3 = rBox;
			r3.y += 8F + 2F;
			r3.height = 16F;
			r3.width = EditorStyles.label.CalcSize(Utility.content).x;
			EditorGUI.LabelField(r3, Utility.content);
			Utility.content.image = null;

			if (package.count_ratings != 0)
			{
				Rating[]	ratings = package.Ratings;
				if (ratings == null || ratings.Length > 0)
					TooltipHelper.Custom(r3, this.OnGUIRatingsTooltip, UtilityResources.Star.width * 5 + 30F, 18F * (ratings != null ? ratings.Length : 1), package);
			}
			
			if (hasDraft == false)
			{
				Version	publishedVersion = null;

				for (int i = 0, max = package.versions.Length; i < max; ++i)
				{
					if (package.versions[i].status == Status.Published)
					{
						publishedVersion = package.versions[i];
						break;
					}
				}

				bool	isCreatingDraft = publishedVersion != null && this.publisherAPI.IsCreatingDraft(publishedVersion.id);
				Rect	r4 = r3;

				r4.x += r4.width;
				r4.y += 1F;
				r4.width = 100F;
				r4.height = 12F;

				using (new EditorGUI.DisabledScope(isCreatingDraft))
				{
					if (GUI.Button(r4, "Create Draft", EditorStyles.miniButton) == true)
					{
						publishedVersion.RequestCreateDraft(this.publisherAPI, response =>
						{
							if (this.CheckRequestResponse(response) == true)
							{
								this.displayStatus |= Statuses.Draft;
								XGUIHighlightManager.Highlight(publishedVersion.name);
							}
							this.Repaint();
						});
						return;
					}

					if (isCreatingDraft == true)
					{
						GUI.DrawTexture(r4, GeneralStyles.StatusWheel.image, ScaleMode.ScaleToFit);
						this.Repaint();
					}
				}
			}

			r3.x += 4F;
			r3.y += r3.height + 2F;
			r3.width = rBox.width - 8F;

			for (int j = 0, max2 = package.versions.Length; j < max2; ++j)
			{
				Version	version = package.versions[j];

				if ((this.displayStatus & version.Status) == 0)
					continue;

				this.OnGUIShortVersion(r3, j, version);

				r3.y += 6F + 24F + 2F;
			}

			GUILayout.Space(8F);
		}

		private void	OnGUIRatingsTooltip(Rect r, object o)
		{
			Package		package = o as Package;
			Rating[]	ratings = package.Ratings;

			if (ratings == null)
			{
				if (this.storeAPI.IsGettingRatings(package.id) == false)
					package.RequestRatings(this.storeAPI);
				else
				{
					EditorGUI.DrawRect(r, Color.grey * .5F);
					GUI.Label(r, GeneralStyles.StatusWheel);
					this.Repaint();
				}

				return;
			}
			
			EditorGUI.DrawRect(r, Color.grey * .9F);
			Utility.DrawUnfillRect(r, Color.grey);

			Texture2D	starIcon = UtilityResources.Star;

			r.x += 2F;
			r.y += 1F;
			r.height = 16F;

			for (int i = 0, max = ratings.Length; i < max; ++i)
			{
				Rating	rating = ratings[i];
				Rect	r2 = r;

				r2.width = starIcon.width;
				r2.height = starIcon.height;

				for (int j = 0, max2 = rating.value; j < max2; ++j)
				{
					GUI.DrawTexture(r2, starIcon);
					r2.x += r2.width;
				}

				r2.x = r2.width * 5F + 6F;
				r2.width = r.width - r2.x;
				EditorGUI.IntField(r2, rating.count, EditorStyles.label);

				r.y += r.height + 2F;
			}
		}

		private void	OnGUIShortVersion(Rect r3, int j, Version version)
		{
			Rect	r = r3;
			Event	currentEvent = Event.current;

			r.y += 6F;
			r.height = 24F;
			this.Box(r, GUIContent.none);
			if (version.status == Status.Draft)
				XGUIHighlightManager.DrawHighlight(version.name, this, r, XGUIHighlights.Glow);
			r.y += 4F;

			Rect	rStatus = r;

			r.height = 16F;
			if (currentEvent.alt == true)
				Utility.content.text = version.name + " (" + version.id + ")";
			else
				Utility.content.text = version.name;
			r.width = GUI.skin.button.CalcSize(Utility.content).x;

			r.x += 4F;
			if (GUI.Button(r, Utility.content) == true)
			{
				for (int i = 0, max = this.database.Packages.Length; i < max; ++i)
				{
					if (this.database.Packages[i] == version.package)
					{
						this.selectedPackage = i;
						break;
					}
				}

				this.selectedVersion = j;
				return;
			}

			Utility.content.text = "[" + version.status + "]";

			rStatus.x += 2F;
			rStatus.y -= 14F;
			rStatus.width = EditorStyles.miniBoldLabel.CalcSize(Utility.content).x;
			rStatus.height = 16F;
			using (ColorStyleRestorer.Get(EditorStyles.miniBoldLabel, Status.GetColor(version.status)))
			{
				this.Box(rStatus, GUIContent.none);
				GUI.Label(rStatus, Utility.content, EditorStyles.miniBoldLabel);
			}

			r.x = r3.xMax - 80F - 10F - 80F - 4F;
			r.width = 80F;
			r.height = 16F;

			Rect	rApply = r;
			rApply.y -= 1F;
			rApply.x -= 50F + 4F;
			rApply.width = 50F;

			var	version_name = this.Edit(r, version.id + "version_name".GetHashCode(), version.version_name);
			r.x += r.width + 10F;
			var	price = this.Edit(r, version.id + "price".GetHashCode(), version.price);

			using (LabelWidthRestorer.Get(13F))
			{
				if (version.status == Status.Draft)
				{
					Utility.content.text = "V";
					Utility.content.tooltip = "Version name";
					{
						this.Box(version_name.Rect, GUIContent.none);
						version_name.EndWithOutput = EditorGUI.TextField(version_name.Rect, Utility.content, version_name.BeginWithInput);
					}

					Utility.content.text = "$";
					Utility.content.tooltip = "Price";
					this.Box(price.Rect, GUIContent.none);
					price.EndWithOutput = EditorGUI.FloatField(price.Rect, Utility.content, price.BeginWithInput);
				}
				else
				{
					Utility.content.text = "V";
					Utility.content.tooltip = "Version name";
					this.Box(version_name.Rect, GUIContent.none);
					EditorGUI.TextField(version_name.Rect, Utility.content, version.version_name, EditorStyles.label);

					Utility.content.text = "$";
					Utility.content.tooltip = "Price";
					this.Box(price.Rect, GUIContent.none);
					EditorGUI.FloatField(price.Rect, Utility.content, version.price, EditorStyles.label);
				}
			}

			if (version.status == Status.Draft)
			{
				if (version_name.Modified == true ||
					price.Modified == true)
				{
					if (GUI.Button(rApply, NGPublisherWindow.applyContent, GeneralStyles.ToolbarValidButton) == true)
					{
						version.RequestSetPackage(this.publisherAPI,
												  version_name.input,
												  version.publishnotes,
												  version.category_id,
												  price.input,
												  response =>
						{
							if (this.CheckRequestResponse(response) == true)
							{
								EditorGUIUtility.editingTextField = false;
								version.version_name = version_name.Apply();
								version.price = price.Apply();
							}

							this.Repaint();
						});
					}
				}
			}
			Utility.content.tooltip = null;
		}

		private void	Box(Rect r, GUIContent none)
		{
			if (Application.unityVersion.CompareTo("2019.3") >= 0)
			{
				Color	c = GUI.color;
				GUI.color = Color.white * .1F;
				GUI.Box(r, none);
				GUI.color = c;
			}
			else
			{
				if (EditorGUIUtility.isProSkin == true)
					GUI.Box(r, none);
				else
				{
					Color	c = GUI.color;
					GUI.color = Color.white * .1F;
					GUI.Box(r, none);
					GUI.color = c;
					Utility.DrawUnfillRect(r, Color.grey);
				}
			}
		}

		private void	OnGUIVersion(Version version)
		{
			bool	isGettingVersion = this.publisherAPI.IsGettingPackageVersion(version.id);
			bool	isDeletingDraft = version.status == Status.Draft && this.publisherAPI.IsDeletingingDraft(version.id);
			Event	currentEvent = Event.current;

			if (version.detailed == null && isGettingVersion == false)
			{
				this.database.RequestGetPackageVersion(this.publisherAPI, version, this.HandleErrorOnly);
				isGettingVersion = true;
			}

			if (currentEvent.type != EventType.Repaint && currentEvent.type != EventType.Layout)
			{
				if (currentEvent.keyCode == KeyCode.LeftAlt)
					this.Repaint();
				else if (currentEvent.type == EventType.KeyDown)
				{
					if (currentEvent.control == true)
					{
						if (currentEvent.keyCode == KeyCode.LeftArrow)
						{
							if (this.selectedPackage > 0)
							{
								--this.selectedPackage;

								Package	newPackage = this.database.Packages[this.selectedPackage];

								for (int i = 0, max = newPackage.versions.Length; i < max; ++i)
								{
									Version	element = newPackage.versions[i];

									if (version.status == element.status)
									{
										this.selectedVersion = i;
										break;
									}
								}

								this.Repaint();
							}
						}
						else if (currentEvent.keyCode == KeyCode.RightArrow)
						{
							if (this.selectedPackage + 1 < this.database.Packages.Length)
							{
								++this.selectedPackage;

								Package	newPackage = this.database.Packages[this.selectedPackage];

								for (int i = 0, max = newPackage.versions.Length; i < max; ++i)
								{
									Version	element = newPackage.versions[i];

									if (version.status == element.status)
									{
										this.selectedVersion = i;
										break;
									}
								}

								this.Repaint();
							}
						}
					}
					else if (currentEvent.shift == true)
					{
						if (currentEvent.keyCode == KeyCode.LeftArrow)
						{
							this.selectedVersion = Mathf.Clamp(this.selectedVersion - 1, 0, this.database.Packages[this.selectedPackage].versions.Length - 1);
							this.Repaint();
						}
						else if (currentEvent.keyCode == KeyCode.RightArrow)
						{
							this.selectedVersion = Mathf.Clamp(this.selectedVersion + 1, 0, this.database.Packages[this.selectedPackage].versions.Length - 1);
							this.Repaint();
						}
					}
				}
			}

			using (var scroll = new EditorGUILayout.ScrollViewScope(this.versionScrollPosition))
			{
				this.versionScrollPosition = scroll.scrollPosition;

				using (new EditorGUI.DisabledScope(isGettingVersion  == true || isDeletingDraft == true))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						Utility.content.text = "Refresh";

						if (isGettingVersion == true)
						{
							Utility.content.image = GeneralStyles.StatusWheel.image;
							this.Repaint();
						}
						else
							Utility.content.image = UtilityResources.Refresh;

						if (GUILayout.Button(Utility.content, GUILayoutOptionPool.Height(Constants.SingleLineHeight)) == true || (version.detailed == null && isGettingVersion == false && Event.current.type != EventType.Layout))
						{
							Utility.content.image = null;
							this.database.RequestGetPackageVersion(this.publisherAPI, version, this.HandleErrorOnly);
							return;
						}

						Utility.content.image = null;

						GUILayout.FlexibleSpace();

						if (version.status == Status.Published)
						{
							bool	isCreatingDraft = this.publisherAPI.IsCreatingDraft(version.id);

							using (new EditorGUI.DisabledScope(isCreatingDraft))
							{
								if (GUILayout.Button("Create Draft") == true)
								{
									version.RequestCreateDraft(this.publisherAPI, response =>
									{
										if (this.CheckRequestResponse(response) == true)
										{
											XGUIHighlightManager.Highlight(version.name);

											Package	package = version.package;

											for (int i = 0, max = package.versions.Length; i < max; ++i)
											{
												if (package.versions[i].status == Status.Draft)
												{
													this.selectedVersion = i;
													break;
												}
											}
										}
										this.Repaint();
									});
									return;
								}

								if (isCreatingDraft == true)
								{
									GUI.Label(GUILayoutUtility.GetLastRect(), GeneralStyles.StatusWheel);
									this.Repaint();
								}
							}
						}
						else if (version.status == Status.Draft)
						{
							bool	isCanSubmitting = this.publisherAPI.IsCanSubmit(version.id) == true || this.publisherAPI.IsSubmitting(version.id) == true;

							using (new EditorGUI.DisabledScope(version.detailed == null || version.detailed.allow_submit == false || isCanSubmitting))
							{
								if (GUILayout.Button("Submit") == true && (this.modifiedValues.Count == 0 || EditorUtility.DisplayDialog(NGPublisherWindow.Title, "You modified values and did not validate them.\n\nAre you sure you would like to submit?", "Submit", "Cancel") == true))
								{
									version.RequestCanSubmit(this.publisherAPI, response =>
									{
										if (this.CheckRequestResponse(response) == true)
										{
											SubmissionWindow.Start(version.name, (autoPublish, comments) =>
											{
												version.RequestSubmit(this.publisherAPI, autoPublish, comments, submissionResponse =>
												{
													if (this.CheckRequestResponse(submissionResponse) == true)
														this.database.RequestAllPackages(this.publisherAPI, this.HandleErrorOnly);
												});
											});
										}

										this.Repaint();
									});
									return;
								}

								if (isCanSubmitting)
								{
									GUI.Label(GUILayoutUtility.GetLastRect(), GeneralStyles.StatusWheel);
									this.Repaint();
								}
							}

							using (new EditorGUI.DisabledScope(isDeletingDraft))
							{
								if (GUILayout.Button("Delete Draft") == true && EditorUtility.DisplayDialog(NGPublisherWindow.Title, "Confirm delete draft " + version.name + " (" + version.id + ") ?", "Yes", "No") == true)
								{
									version.RequestDeleteDraft(this.publisherAPI, response =>
									{
										if (this.CheckRequestResponse(response) == true)
											this.selectedVersion = -1;
										this.Repaint();
									});
									return;
								}

								if (isDeletingDraft == true)
								{
									GUI.Label(GUILayoutUtility.GetLastRect(), GeneralStyles.StatusWheel);
									this.Repaint();
								}
							}
						}
						else if (version.status == Status.Pending)
						{
							bool	isPublishing = this.publisherAPI.IsCreatingDraft(version.id);

							using (new EditorGUI.DisabledScope(isPublishing == true || version.detailed == null || version.detailed.allow_publish == false))
							{
								if (GUILayout.Button("Publish") == true && EditorUtility.DisplayDialog(NGPublisherWindow.Title, "Confirm publish draft " + version.name + " (" + version.id + ") ?", "Yes", "No") == true)
								{
									version.RequestPublish(this.publisherAPI, string.Empty, response =>
									{
										if (this.CheckRequestResponse(response) == true)
											this.database.RequestAllPackages(this.publisherAPI, this.HandleErrorOnly);
										this.Repaint();
									});
									return;
								}

								if (isPublishing == true)
								{
									GUI.Label(GUILayoutUtility.GetLastRect(), GeneralStyles.StatusWheel);
									this.Repaint();
								}
							}
						}
					}

					Rect	rApply = GUILayoutUtility.GetLastRect();
					var		category_id = this.Edit(version.id + "category_id".GetHashCode(), version.category_id);
					var		version_name = this.Edit(version.id + "version_name".GetHashCode(), version.version_name);
					var		price = this.Edit(version.id + "price".GetHashCode(), version.price);
					var		publishnotes = this.Edit(version.id + "publishnotes".GetHashCode(), version.publishnotes);

					using (LabelWidthRestorer.Get(100F))
					{
						using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
						{
							// Use detailed's name in case english has a different name.
							if (currentEvent.alt == true)
								Utility.content.text = (version.detailed != null ? version.detailed.package.version.name : version.name) + " (" + version.id + ")";
							else
								Utility.content.text = version.detailed != null ? version.detailed.package.version.name : version.name;
							EditorGUILayout.LabelField(Utility.content.text);
						}

						if (version.Category == string.Empty)
						{
							if (Event.current.type == EventType.Layout && this.publisherAPI.IsGettingCategories(version.id) == false)
								version.RequestCategories(this.publisherAPI, this.HandleErrorOnly);

							EditorGUILayout.LabelField(new GUIContent("Category"), GeneralStyles.StatusWheel);
							this.Repaint();
						}
						else
						{
							if (version.status == Status.Draft)
							{
								using (category_id.Start())
								{
									int	selected = 0;

									for (int max = version.Categories.Length; selected < max; ++selected)
									{
										if (version.Categories[selected].id == category_id.input)
											break;
									}

									EditorGUI.BeginChangeCheck();
									int	newValue = NGEditorGUILayout.Popup("Category", selected, version.Categories, s => s.name);
									if (EditorGUI.EndChangeCheck() == true)
										category_id.output = version.Categories[newValue].id;
								}
							}
							else
								EditorGUILayout.TextField("Category", version.Category, EditorStyles.label);
						}

						if (version.status == Status.Draft)
						{
							version_name.EndWithOutput = EditorGUILayout.TextField("Version Name", version_name.BeginWithInput);
							price.EndWithOutput = EditorGUILayout.FloatField("Price", price.BeginWithInput);
						}
						else
						{
							EditorGUILayout.TextField("Version Name", version.version_name, EditorStyles.label);
							EditorGUILayout.FloatField("Price", version.price, EditorStyles.label);
						}

						Rect	rTime = GUILayoutUtility.GetRect(0F, Constants.SingleLineHeight + Constants.SingleLineHeight, EditorStyles.label);
						float	x = rTime.x;
						float	w = rTime.width;

						this.Box(rTime, GUIContent.none);
						{
							rTime.height = Constants.SingleLineHeight;
							rTime.width = Mathf.RoundToInt(w * .333F);
							this.Box(rTime, GUIContent.none);
							GUI.Label(rTime, "Created");
							rTime.x += rTime.width;

							this.Box(rTime, GUIContent.none);
							GUI.Label(rTime, "Modified");
							rTime.x += rTime.width;

							rTime.width = w - (rTime.x - x);
							this.Box(rTime, GUIContent.none);
							GUI.Label(rTime, "Published");
							rTime.y += rTime.height;
						}

						{
							rTime.x = x;
							rTime.width = Mathf.RoundToInt(w * .333F);
							EditorGUI.TextField(rTime, version.created, EditorStyles.label);
							rTime.x += rTime.width;

							EditorGUI.TextField(rTime, version.modified, EditorStyles.label);
							rTime.x += rTime.width;

							rTime.width = w - rTime.x;
							EditorGUI.TextField(rTime, version.published, EditorStyles.label);
						}

						Rect	rLabel = GUILayoutUtility.GetRect(0F, Constants.SingleLineHeight, EditorStyles.foldout);

						rLabel.width = EditorGUIUtility.labelWidth;
						this.displayPublishNotes = EditorGUI.Foldout(rLabel, this.displayPublishNotes, "Publish Notes", true);

						if (version.status == Status.Draft)
						{
							rLabel.x += EditorGUIUtility.labelWidth;
							rLabel.width = AutoEditValue<string>.RestoreButtonWidth;
							publishnotes.hasCustomResetRect = true;
							publishnotes.resetRect = rLabel;

							using (publishnotes.Start())
							{
								if (this.displayPublishNotes == true)
									publishnotes.output = EditorGUILayout.TextArea(publishnotes.input, GeneralStyles.WrappedTextArea, GUILayoutOptionPool.Width(this.position.width - 25F));
							}
						}
						else if (this.displayPublishNotes == true)
							EditorGUILayout.TextArea(version.publishnotes, GeneralStyles.WrappedTextArea, GUILayoutOptionPool.Width(this.position.width - 25F));

						if (version.status == Status.Draft)
						{
							if (category_id.Modified == true ||
								version_name.Modified == true ||
								price.Modified == true ||
								publishnotes.Modified == true)
							{
								rApply.x = rApply.center.x - 25F;
								rApply.width = 50F;
								if (GUI.Button(rApply, NGPublisherWindow.applyContent, GeneralStyles.ToolbarValidButton) == true)
								{
									version.RequestSetPackage(this.publisherAPI,
															  version_name.input,
															  publishnotes.input,
															  category_id.input,
															  price.input,
															  response =>
									{
										if (this.CheckRequestResponse(response) == true)
										{
											EditorGUIUtility.editingTextField = false;
											version.version_name = version_name.Apply();
											version.publishnotes = publishnotes.Apply();
											version.category_id = category_id.Apply();
											version.price = price.Apply();
										}

										this.Repaint();
									});
									return;
								}
							}
						}

						EditorGUILayout.Separator();

						if (version.detailed != null)
						{
							using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
							{
								EditorGUILayout.LabelField("Metadata & Artwork");
							}

							foreach (VersionDetailed.Package.Version.Language language in version.detailed.package.version.EachLanguage)
							{
								using (new EditorGUILayout.HorizontalScope())
								{
									EditorGUILayout.LabelField(this.database.GetLanguage(language.languageCode), GUILayoutOptionPool.Width(EditorGUIUtility.labelWidth));

									if (GUILayout.Button("Metadata") == true)
									{
										this.selectedLanguage = language.languageCode;
										this.languageTab = LanguageTab.Metadata;
									}

									if (GUILayout.Button("Key Images") == true)
									{
										this.selectedLanguage = language.languageCode;
										this.languageTab = LanguageTab.KeyImages;
									}

									if (GUILayout.Button("Audio/Video") == true)
									{
										this.selectedLanguage = language.languageCode;
										this.languageTab = LanguageTab.AudioVideo;
									}

									if (GUILayout.Button("Screenshots") == true)
									{
										this.selectedLanguage = language.languageCode;
										this.languageTab = LanguageTab.Screenshots;
									}
								}
							}

							Language[]	languages = this.database.Languages;

							if (languages != null)
							{
								for (int i = 0, max = languages.Length; i < max; ++i)
								{
									Language	language = languages[i];

									if (version.detailed.package.version[language.code] == null)
									{
										Utility.content.text = "Add " + this.database.GetLanguage(language.code);

										if (this.publisherAPI.IsAddingLanguage(version.id, language.code) == true)
										{
											Utility.content.image = GeneralStyles.StatusWheel.image;
											this.Repaint();
										}

										if (GUILayout.Button(Utility.content, GUILayoutOptionPool.ExpandWidthFalse) == true)
										{
											version.RequestAddLanguage(this.publisherAPI, language.code, response =>
											{
												if (this.CheckRequestResponse(response) == true)
													this.database.RequestGetPackageVersion(this.publisherAPI, response.context as Version, this.HandleErrorOnly);
											});
											return;
										}

										Utility.content.image = null;
									}
								}
							}

							EditorGUILayout.Separator();

							using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
							{
								if (version.status == Status.Draft)
									EditorGUILayout.LabelField("Package Upload");
								else 
									EditorGUILayout.LabelField("Package Test Status");

								if (Event.current.alt == true)
								{
									if (GUILayout.Button("Auto Set Unity") == true)
									{
										foreach (VersionDetailed.Package.Version.UnityPackage unityPackage in version.detailed.package.version.unitypackages)
										{
											if (Event.current.control == false && string.IsNullOrEmpty(unityPackage.vetting.unity_versions) == false)
												continue;

											var					vetting = this.Edit(unityPackage.package_upload_id.GetHashCode() + "vetting".GetHashCode(), unityPackage.vetting);
											List<Vets.Option>	sortedVersions = new List<Vets.Option>(this.database.Vets.unity_versions);

											sortedVersions.Add(new Vets.Option() { label = unityPackage.unity_version });
											sortedVersions.Sort((a, b) => this.CompareVersion(a.label, b.label));

											for (int i = 0, max = sortedVersions.Count; i < max; ++i)
											{
												if (sortedVersions[i].label == unityPackage.unity_version)
												{
													vetting.input.unity_versions = sortedVersions[i + 1].code;
													break;
												}
											}

											version.RequestVetVersion(this.publisherAPI,
																	  unityPackage,
																	  vetting.input.platforms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
																	  vetting.input.unity_versions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
																	  vetting.input.srp_type.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
																	  vetting.input.dependencies,
																	  response =>
																	  {
																		  if (this.CheckRequestResponse(response) == true)
																			  unityPackage.vetting = vetting.Apply();

																		  this.Repaint();
																	  });
										}
									}
								}
							}

							foreach (VersionDetailed.Package.Version.UnityPackage unityPackage in version.detailed.package.version.unitypackages)
								this.OnGUIUnityPackage(version, unityPackage);
						}
					}
				}
			}
		}

		private void	OnGUIUnityPackage(Version version, VersionDetailed.Package.Version.UnityPackage unityPackage)
		{
			GUILayout.Space(4F);

			Event	currentEvent = Event.current;
			Rect	rBox = GUILayoutUtility.GetRect(0F, 20F + 2F + 18F + 4F, EditorStyles.label);
			Rect	r = rBox;
			string	shortSize;

			if (unityPackage.size >= 1024 * 1024)
				shortSize = (unityPackage.size / (1024F * 1024F)).ToString("N1") + " MiB";
			else if (unityPackage.size >= 1024)
				shortSize = (unityPackage.size / 1024F).ToString("N1") + " KiB";
			else
				shortSize = unityPackage.size + " B";

			DateTime	dt = DateTime.Parse(unityPackage.timestamp);

			this.Box(r, GUIContent.none);

			if (Event.current.alt == true)
				Utility.content.text = unityPackage.unity_version + " (" + unityPackage.package_upload_id + ')';
			else
				Utility.content.text = unityPackage.unity_version;
			r.size = GeneralStyles.Title1.CalcSize(Utility.content);
			GUI.Label(r, Utility.content, GeneralStyles.Title1);

			if (version.status != Status.Draft && string.IsNullOrEmpty(unityPackage.vetting.status) == false)
			{
				r.x += r.width;

				Utility.content.text = " [" + unityPackage.vetting.status + "]";
				r.width = GeneralStyles.SmallLabel.CalcSize(Utility.content).x;
				GUI.Label(r, Utility.content, GeneralStyles.SmallLabel);
				r.x = rBox.x;
			}

			bool	isVetting = this.publisherAPI.IsVettingVersion(version.id, unityPackage.package_upload_id);
			var		vetting = this.Edit(unityPackage.package_upload_id.GetHashCode() + "vetting".GetHashCode(), unityPackage.vetting);

			using (vetting.Start())
			using (new EditorGUI.DisabledScope(isVetting))
			{
				if (vetting.Modified == true)
				{
					Rect	r2 = r;
					r2.x += r2.width;
					r2.width = 50F;

					if (GUI.Button(r2, NGPublisherWindow.applyContent, GeneralStyles.ToolbarValidButton) == true)
					{
						version.RequestVetVersion(this.publisherAPI,
												  unityPackage,
												  vetting.input.platforms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
												  vetting.input.unity_versions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
												  vetting.input.srp_type.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
												  vetting.input.dependencies,
												  response =>
						{
							if (this.CheckRequestResponse(response) == true)
							{
								unityPackage.vetting = vetting.Apply();
								EditorGUIUtility.editingTextField = false;
							}

							this.Repaint();
						});
					}

					if (isVetting == true)
					{
						GUI.Label(r2, GeneralStyles.StatusWheel);
						this.Repaint();
					}
				}

				r.y += r.height + 2F;
				r.x += 4F;

				Utility.content.text = "Platforms";
				r.size = EditorStyles.textField.CalcSize(Utility.content);
				using (LabelWidthRestorer.Get(r.size.x))
				{
					Utility.content.text = this.CountCommas(vetting.input.platforms).ToString();
					Vector2	size = EditorStyles.textField.CalcSize(Utility.content);
					r.width += size.x;
					this.Box(r, GUIContent.none);

					EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

					if (currentEvent.type == EventType.MouseDown &&
						r.Contains(currentEvent.mousePosition) == true)
					{
						PopupWindow.Show(r, new VetPopup(vetting, nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.platforms), this.database.Vets.platforms));
						GUIUtility.ExitGUI();
					}

					r.width -= size.x;
					EditorGUI.LabelField(r, "Platforms");
					r.x += r.width;
					r.width = size.x;
					EditorGUI.LabelField(r, Utility.content.text, EditorStyles.textField);
					r.x += r.width + 10F;
				}

				Utility.content.text = "Unity";
				r.size = EditorStyles.textField.CalcSize(Utility.content);
				using (LabelWidthRestorer.Get(r.size.x))
				{
					Utility.content.text = this.CountCommas(vetting.input.unity_versions).ToString();
					Vector2	size = EditorStyles.textField.CalcSize(Utility.content);
					r.width += size.x;
					this.Box(r, GUIContent.none);

					EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

					if (currentEvent.type == EventType.MouseDown &&
						r.Contains(currentEvent.mousePosition) == true)
					{
						PopupWindow.Show(r, new VetPopup(vetting, nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.unity_versions), this.database.Vets.unity_versions));
						GUIUtility.ExitGUI();
					}

					r.width -= size.x;
					EditorGUI.LabelField(r, "Unity");
					r.x += r.width;
					r.width = size.x;
					EditorGUI.LabelField(r, Utility.content.text, EditorStyles.textField);
					r.x += r.width + 10F;
				}
				
				Utility.content.text = "SRP";
				r.size = EditorStyles.textField.CalcSize(Utility.content);
				using (LabelWidthRestorer.Get(r.size.x))
				{
					Utility.content.text = this.CountCommas(vetting.input.srp_type).ToString();
					Vector2	size = EditorStyles.textField.CalcSize(Utility.content);
					r.width += size.x;
					this.Box(r, GUIContent.none);

					EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

					if (currentEvent.type == EventType.MouseDown &&
						r.Contains(currentEvent.mousePosition) == true)
					{
						PopupWindow.Show(r, new VetPopup(vetting, nameof(VersionDetailed.Package.Version.UnityPackage.Vetting.srp_type), this.database.Vets.srps));
						GUIUtility.ExitGUI();
					}

					r.width -= size.x;
					EditorGUI.LabelField(r, "SRP");
					r.x += r.width;
					r.width = size.x;
					EditorGUI.LabelField(r, Utility.content.text, EditorStyles.textField);
					r.x += r.width + 10F;
				}
				
				Utility.content.text = "Deps";
				r.size = EditorStyles.textField.CalcSize(Utility.content);
				using (LabelWidthRestorer.Get(r.size.x))
				{
					Utility.content.text = vetting.input.dependencies.Length.ToString();
					Vector2	size = EditorStyles.textField.CalcSize(Utility.content);
					r.width += size.x;
					this.Box(r, GUIContent.none);

					EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

					if (currentEvent.type == EventType.MouseDown &&
						r.Contains(currentEvent.mousePosition) == true)
					{
						PopupWindow.Show(r, new DependenciesPopup(vetting));
						GUIUtility.ExitGUI();
					}

					r.width -= size.x;
					EditorGUI.LabelField(r, "Deps");
					r.x += r.width;
					r.width = size.x;
					EditorGUI.LabelField(r, Utility.content.text, EditorStyles.textField);
				}

				r.x += r.width + 5F;
				r.width = AutoEditValue<VersionDetailed.Package.Version.UnityPackage.Vetting>.RestoreButtonWidth;
				vetting.hasCustomResetRect = true;
				vetting.resetRect = r;

				r = rBox;
				r.x += r.width;
				r.y += 4F;
				r.height = 20F;

				if (vetting.input.allow_edit == true)
				{
					r.width = 70F;
					r.x -= r.width + 5F;

					bool	isDeletingUnityPackage = this.publisherAPI.IsDeletingingUnityPackage(version.id, unityPackage);

					using (new EditorGUI.DisabledScope(isDeletingUnityPackage))
					{
						Utility.content.text = "Remove";

						if (isDeletingUnityPackage == true)
						{
							Utility.content.image = GeneralStyles.StatusWheel.image;
							this.Repaint();
						}

						if (GUI.Button(r, Utility.content) == true && EditorUtility.DisplayDialog(NGPublisherWindow.Title, "Confirm delete package " + unityPackage.unity_version + " (" + unityPackage.package_upload_id + ") ?", "Yes", "No") == true)
						{
							version.RequestDeleteUnityPackage(this.publisherAPI, unityPackage, this.HandleErrorOnly);
							return;
						}

						if (isDeletingUnityPackage == true)
							Utility.content.image = null;
					}
				}
				else
					r.width = 0F;

				if (Event.current.alt == true)
					Utility.content.text = dt.ToString();
				else
				{
					Utility.content.text = dt.ToShortDateString();
					Utility.content.tooltip = dt.ToString();
				}

				r.size = GUI.skin.box.CalcSize(Utility.content);
				r.x -= r.width + 5F;
				this.Box(r, GUIContent.none);
				GUI.Label(r, Utility.content);

				if (Event.current.alt == true)
					Utility.content.text = unityPackage.size + " bytes";
				else
				{
					Utility.content.text = shortSize;
					Utility.content.tooltip = unityPackage.size + " bytes";
				}

				r.size = GUI.skin.box.CalcSize(Utility.content);
				r.x -= r.width + 5F;
				this.Box(r, GUIContent.none);
				GUI.Label(r, Utility.content);
				Utility.content.tooltip = null;
			}
		}

		private void	OnGUIVersionLanguage(Version version, VersionDetailed.Package.Version.Language language)
		{
			GUILayout.Space(4F);

			using (var scroll = new EditorGUILayout.ScrollViewScope(this.versionLanguageScrollPosition))
			{
				this.versionLanguageScrollPosition = scroll.scrollPosition;

				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Toggle(this.languageTab == LanguageTab.Metadata, "Metadata", "buttonleft") == true)
						this.languageTab = LanguageTab.Metadata;
					if (GUILayout.Toggle(this.languageTab == LanguageTab.KeyImages, "Key Images", "buttonmid") == true)
						this.languageTab = LanguageTab.KeyImages;
					if (GUILayout.Toggle(this.languageTab == LanguageTab.AudioVideo, "Audio/Video", "buttonmid") == true)
						this.languageTab = LanguageTab.AudioVideo;
					if (GUILayout.Toggle(this.languageTab == LanguageTab.Screenshots, "Screenshots", "buttonright") == true)
						this.languageTab = LanguageTab.Screenshots;
				}

				GUILayout.Space(4F);

				if (this.languageTab == LanguageTab.Metadata)
					this.OnGUIVersionLanguageMetadata(version, language);
				else if (this.languageTab == LanguageTab.KeyImages)
					this.OnGUIVersionLanguageKeyImages(version, language);
				else if (this.languageTab == LanguageTab.AudioVideo)
					this.OnGUIVersionLanguageAudioVideo(version, language);
				else if (this.languageTab == LanguageTab.Screenshots)
					this.OnGUIVersionLanguageScreenshots(version, language);
			}
		}

		private void	OnGUIVersionLanguageMetadata(Version version, VersionDetailed.Package.Version.Language language)
		{
			var name = this.Edit(version.id + "name".GetHashCode(), language.name);
			var description = this.Edit(version.id + "description".GetHashCode(), language.description);
			var keywords = this.Edit(version.id + "keywords".GetHashCode(), language.keywords);

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			using (new EditorGUI.DisabledScope(this.publisherAPI.IsSettingPackageMetadata(version.id)))
			{
				GUILayout.Label("Metadata");

				Utility.content.text = "Go     ";
				Utility.content.tooltip = "https://publisher.assetstore.unity3d.com/package-metadata.html?id=" + version.id + "&lang=" + language.languageCode;
				if (GUILayout.Button(Utility.content, EditorStyles.toolbarButton) == true)
					Application.OpenURL(Utility.content.tooltip);

				Rect	r3 = GUILayoutUtility.GetLastRect();
				r3.x += 20F + this.linkIcon.width;
				r3.y += 1F;
				r3.width = -this.linkIcon.width;
				r3.height = this.linkIcon.height;
				GUI.DrawTexture(r3, this.linkIcon, ScaleMode.ScaleToFit);
				Utility.content.tooltip = null;

				if (name.Modified == true ||
					description.Modified == true ||
					keywords.Modified == true)
				{
					Rect	r2 = GUILayoutUtility.GetRect(0F, 16F, GeneralStyles.ToolbarValidButton, GUILayoutOptionPool.Width(50F));

					if (GUI.Button(r2, NGPublisherWindow.applyContent, GeneralStyles.ToolbarValidButton) == true)
					{
						version.RequestSetPackageMetadata(this.publisherAPI,
														  language,
														  name.input,
														  description.input,
														  keywords.input,
														  response =>
						{
							if (this.CheckRequestResponse(response) == true)
							{
								EditorGUIUtility.editingTextField = false;
								language.keywords = keywords.Apply();
								language.name = name.Apply();
								language.description = description.Apply();
							}

							this.Repaint();
						});
					}
				}

				GUILayout.FlexibleSpace();
			}

			using (LabelWidthRestorer.Get(110F))
			{
				name.EndWithOutput = EditorGUILayout.TextField("Name", name.BeginWithInput);

				EditorGUILayout.PrefixLabel("Description");
				Rect	rDescription = GUILayoutUtility.GetLastRect();
				float	width = this.position.width;
				bool	wideDisplay = width > NGPublisherWindow.DescriptionPreviewMinWidth;

				using (description.Start())
				using (var scroll = new EditorGUILayout.ScrollViewScope(this.descriptionPosition))
				{
					rDescription.x += rDescription.width;
					rDescription.width = AutoEditValue<string>.RestoreButtonWidth;
					description.hasCustomResetRect = true;
					description.resetRect = rDescription;

					this.descriptionPosition = scroll.scrollPosition;

					string	input = description.input;

					if (NGEditorGUILayout.BigText(version.id + "Descripion".GetHashCode(), ref input) == true)
					{
						if (wideDisplay == true)
						{
							using (new EditorGUILayout.HorizontalScope())
							{
								description.output = EditorGUILayout.TextArea(description.input, GeneralStyles.WrappedTextArea, GUILayoutOptionPool.Width(Mathf.RoundToInt(width * .5F)));
								GUILayout.Box(this.ConvertHTML(description.input), GeneralStyles.RichLabel, GUILayoutOptionPool.Width(Mathf.RoundToInt(width * .5F)));
							}
						}
						else
							description.output = EditorGUILayout.TextArea(description.input, GeneralStyles.WrappedTextArea);
					}
					else
					{
						description.output = input;
						wideDisplay = true;

						GUILayout.Label("Preview can not be displayed. Too much characters.", EditorStyles.miniLabel);
					}
				}

				if (wideDisplay == false)
				{
					Rect	r = GUILayoutUtility.GetLastRect();

					GUILayout.Label("Preview requires " + NGPublisherWindow.DescriptionPreviewMinWidth + "px width.", EditorStyles.miniLabel);

					r.y = r.yMax + 1F;
					r.height = 1F;
					r.width = width * r.width / 600F;
					EditorGUI.DrawRect(r, Color.yellow);
				}

				EditorGUILayout.PrefixLabel("Keywords");

				Rect	rKeywords = GUILayoutUtility.GetLastRect();

				rKeywords.x += rKeywords.width;
				rKeywords.width = AutoEditValue<string>.RestoreButtonWidth;
				keywords.hasCustomResetRect = true;
				keywords.resetRect = rKeywords;
				keywords.EndWithOutput = EditorGUILayout.TextArea(keywords.BeginWithInput, GeneralStyles.WrappedTextArea);
			}
		}

		private Vector2	descriptionPosition;

		private void	OnGUIVersionLanguageKeyImages(Version version, VersionDetailed.Package.Version.Language language)
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				GUILayout.Label("Key Images");

				Utility.content.text = "Go     ";
				Utility.content.tooltip = "https://publisher.assetstore.unity3d.com/package-keyimages.html?id=" + version.id + "&lang=" + language.languageCode;
				if (GUILayout.Button(Utility.content, EditorStyles.toolbarButton) == true)
					Application.OpenURL(Utility.content.tooltip);

				Rect	r = GUILayoutUtility.GetLastRect();
				r.x += 20F + this.linkIcon.width;
				r.y += 1F;
				r.width = -this.linkIcon.width;
				r.height = this.linkIcon.height;
				GUI.DrawTexture(r, this.linkIcon, ScaleMode.ScaleToFit);
				Utility.content.tooltip = null;

				GUILayout.FlexibleSpace();
			}

			float	topLayoutYOffset = -this.versionLanguageScrollPosition.y + Utility.GetTopLevelGUILayoutYOffset();

			this.OnGUIKeyImage(version, language, "Icon", NGPublisherWindow.IconImageDescription, ref language.key_images.icon, "icon", NGPublisherWindow.IconImageWidthRequired, NGPublisherWindow.IconImageHeightRequired, topLayoutYOffset);
			this.OnGUIKeyImage(version, language, "Card Image", NGPublisherWindow.UnsupportedImageDescription, ref language.key_images.small, "small", NGPublisherWindow.CardImageWidthRequired, NGPublisherWindow.CardImageHeightRequired, topLayoutYOffset);
			this.OnGUIKeyImage(version, language, "Card Image v2", NGPublisherWindow.CardImageDescription, ref language.key_images.small_v2, "small_v2", NGPublisherWindow.CardImageWidthRequired, NGPublisherWindow.CardImageHeightRequired, topLayoutYOffset);
			this.OnGUIKeyImage(version, language, "Cover Image", NGPublisherWindow.UnsupportedImageDescription, ref language.key_images.big, "big", NGPublisherWindow.CoverImageWidthRequired, NGPublisherWindow.CoverImageHeightRequired, topLayoutYOffset);
			this.OnGUIKeyImage(version, language, "Cover Image v2", NGPublisherWindow.CoverImageDescription, ref language.key_images.big_v2, "big_v2", NGPublisherWindow.CoverImageWidthRequired, NGPublisherWindow.CoverImageHeightRequired, topLayoutYOffset);
			this.OnGUIKeyImage(version, language, "Social Media Image", NGPublisherWindow.SocialMediaImageDescription, ref language.key_images.facebook, "facebook", NGPublisherWindow.SocialMediaImageWidthRequired, NGPublisherWindow.SocialMediaImageHeightRequired, topLayoutYOffset);

			EditorGUILayout.Separator();
		}

		private void	OnGUIKeyImage(Version version, VersionDetailed.Package.Version.Language language, string label, string description, ref string image_url, string type, int requiredWidth, int requiredHeight, float topLayoutYOffset)
		{
			EditorGUILayout.Separator();

			using (new EditorGUILayout.HorizontalScope(GUILayoutOptionPool.Height(NGPublisherWindow.ThumbnailHeight)))
			{
				float	width = NGPublisherWindow.ThumbnailHeight * requiredWidth / requiredHeight;
				Rect	rPreview = GUILayoutUtility.GetRect(NGPublisherWindow.PreviewMargin + width + NGPublisherWindow.PreviewMargin, NGPublisherWindow.ThumbnailHeight, GUILayoutOptionPool.ExpandWidthFalse);
				Event	currentEvent = Event.current;

				rPreview.x += NGPublisherWindow.PreviewMargin;
				rPreview.width = width;

				if (string.IsNullOrEmpty(image_url) == false)
				{
					Texture2D	preview = TextureCache.Get(image_url);

					if (preview != null)
					{
						TexturePreview.HandleMouseHoverPreview(rPreview, topLayoutYOffset, preview);
						GUI.DrawTexture(rPreview, preview, ScaleMode.ScaleToFit);
					}
				}
				else
				{
					Utility.DrawUnfillRect(rPreview, Color.grey);
					GUI.Label(rPreview, "Drop", GeneralStyles.CenterText);
				}

				if (this.publisherAPI.IsSettingKeyImage(version.id, language.languageCode, type) == true)
				{
					GUI.Label(rPreview, GeneralStyles.StatusWheel);
					this.Repaint();
				}

				if (currentEvent.type == EventType.Repaint)
				{
					if (DragAndDrop.paths.Length > 0)
					{
						if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
							Utility.DropZone(rPreview, "Drop " + label, Color.green);
						else if (DragAndDrop.visualMode == DragAndDropVisualMode.Rejected)
							Utility.DropZone(rPreview, "Drop " + label, Color.red);
					}
				}
				else if (currentEvent.type == EventType.DragUpdated)
				{
					if (rPreview.Contains(currentEvent.mousePosition) == true && DragAndDrop.paths.Length == 1)
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}
				else if (currentEvent.type == EventType.DragPerform)
				{
					if (rPreview.Contains(currentEvent.mousePosition) == true)
					{
						DragAndDrop.AcceptDrag();

						Texture2D	newTexture = TextureCache.LoadTexture(DragAndDrop.paths[0]);

						if (newTexture.width != requiredWidth ||
							newTexture.height != requiredHeight)
						{
							this.ShowNotification("Image must be " + requiredWidth + "x" + requiredHeight + ".", 1F);
						}
						else
						{
							this.messageCenter.Clear(label);

							version.RequestSetKeyImage(this.publisherAPI, language, DragAndDrop.paths[0], type, response =>
							{
								if (response.ok == false)
									messageCenter.Add(label, response.error, MessageType.Error, this);
								else
									messageCenter.Add(label, "Update succeeded.", MessageType.Info, this);

								this.Repaint();
							});
						}

						DragAndDrop.PrepareStartDrag();

						this.Repaint();
						return;
					}
				}

				using (new EditorGUILayout.VerticalScope())
				{
					GUILayout.Label(label + " (" + requiredWidth + "x" + requiredHeight + ")", GeneralStyles.Title1);
					GUILayout.Label(description, EditorStyles.wordWrappedLabel);
				}
			}

			Message	message = this.messageCenter.Get(label);

			if (message != null)
			{
				EditorGUILayout.HelpBox(message.message, message.type);

				Rect	r = GUILayoutUtility.GetLastRect();
				r.xMin = r.xMax - 18F;
				r.yMax = r.yMin + 18F;
				if (GUI.Button(r, "X") == true)
					this.messageCenter.Clear(label);
			}
		}

		private void	OnGUIVersionLanguageAudioVideo(Version version, VersionDetailed.Package.Version.Language language)
		{
			EditorGUILayout.HelpBox("Readonly section, because Unity did a good job on their publisher portal.\nClick on \"Go\" below to open the portal.", MessageType.Info);

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				GUILayout.Label("Audio/Video");

				Utility.content.text = "Go     ";
				Utility.content.tooltip = "https://publisher.assetstore.unity3d.com/package-artwork.html?type=av&id=" + version.id + "&lang=" + language.languageCode;
				if (GUILayout.Button(Utility.content, EditorStyles.toolbarButton) == true)
					Application.OpenURL(Utility.content.tooltip);

				Rect	r = GUILayoutUtility.GetLastRect();
				r.x += 20F + this.linkIcon.width;
				r.y += 1F;
				r.width = -this.linkIcon.width;
				r.height = this.linkIcon.height;
				GUI.DrawTexture(r, this.linkIcon, ScaleMode.ScaleToFit);
				Utility.content.tooltip = null;

				GUILayout.FlexibleSpace();
			}

			for (int k = 0, max = language.artwork.Length; k < max; ++k)
			{
				var	artwork = language.artwork[k];

				if (artwork.type != "screenshot")
				{
					EditorGUILayout.IntField("Id", artwork.id, EditorStyles.label);
					EditorGUILayout.TextField("Type", artwork.type, EditorStyles.label);
					EditorGUILayout.TextField("Reference", artwork.reference, EditorStyles.label);
					EditorGUILayout.TextField("Uri", artwork.uri, EditorStyles.label);
				}
			}
		}

		private void	OnGUIVersionLanguageScreenshots(Version version, VersionDetailed.Package.Version.Language language)
		{
			EditorGUILayout.HelpBox("Readonly section, because Unity did a good job on their publisher portal.\nClick on \"Go\" below to open the portal.", MessageType.Info);

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				GUILayout.Label("Screenshots");

				Utility.content.text = "Go     ";
				Utility.content.tooltip = "https://publisher.assetstore.unity3d.com/package-screenshots.html?type=img&id=" + version.id + "&lang=" + language.languageCode;
				if (GUILayout.Button(Utility.content, EditorStyles.toolbarButton) == true)
					Application.OpenURL(Utility.content.tooltip);

				Rect	r = GUILayoutUtility.GetLastRect();
				r.x += 20F + this.linkIcon.width;
				r.y += 1F;
				r.width = -this.linkIcon.width;
				r.height = this.linkIcon.height;
				GUI.DrawTexture(r, this.linkIcon, ScaleMode.ScaleToFit);
				Utility.content.tooltip = null;

				GUILayout.FlexibleSpace();
			}

			float	topLayoutYOffset = -this.versionLanguageScrollPosition.y + Utility.GetTopLevelGUILayoutYOffset();

			for (int k = 0, max = language.artwork.Length; k < max; ++k)
			{
				var	artwork = language.artwork[k];

				if (artwork.type == "screenshot")
					this.OnGUIScreenshot(version, language, "Uri", artwork, topLayoutYOffset);
			}
		}

		private void	OnGUIScreenshot(Version version, VersionDetailed.Package.Version.Language language, string label, VersionDetailed.Package.Version.Language.Artwork artwork, float topLayoutYOffset)
		{
			EditorGUILayout.Separator();

			using (new EditorGUILayout.HorizontalScope(GUILayoutOptionPool.Height(NGPublisherWindow.ThumbnailHeight)))
			{
				float	width = 200F;
				Rect	rPreview = GUILayoutUtility.GetRect(NGPublisherWindow.PreviewMargin + width + NGPublisherWindow.PreviewMargin, NGPublisherWindow.ThumbnailHeight, GUILayoutOptionPool.ExpandWidthFalse);
				Event	currentEvent = Event.current;

				rPreview.x += NGPublisherWindow.PreviewMargin;
				rPreview.width = width;

				if (string.IsNullOrEmpty(artwork.uri) == false)
				{
					Texture2D	preview = TextureCache.Get(artwork.uri);

					if (preview != null)
					{
						rPreview.width = rPreview.height * preview.width / preview.height;
						TexturePreview.HandleMouseHoverPreview(rPreview, topLayoutYOffset, preview);
						GUI.DrawTexture(rPreview, preview, ScaleMode.ScaleToFit);
					}

					if (preview == null/* || this.publisherAPI.IsSettingKeyImage(version, language.languageCode, type) == true*/)
					{
						GUI.Label(rPreview, GeneralStyles.StatusWheel);
						this.Repaint();
					}
				}
				else
				{
					//Utility.DrawUnfillRect(rPreview, Color.grey);
					//GUI.Label(rPreview, "Drop", GeneralStyles.CenterText);
				}

				//if (currentEvent.type == EventType.Repaint)
				//{
				//	if (DragAndDrop.paths.Length > 0)
				//	{
				//		if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
				//			Utility.DropZone(rPreview, "Drop " + label, Color.green);
				//		else if (DragAndDrop.visualMode == DragAndDropVisualMode.Rejected)
				//			Utility.DropZone(rPreview, "Drop " + label, Color.red);
				//	}
				//}
				//else if (currentEvent.type == EventType.DragUpdated)
				//{
				//	if (rPreview.Contains(currentEvent.mousePosition) == true && DragAndDrop.paths.Length == 1)
				//		DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				//	else
				//		DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
				//}
				//else if (currentEvent.type == EventType.DragPerform)
				//{
				//	if (rPreview.Contains(currentEvent.mousePosition) == true)
				//	{
				//		DragAndDrop.AcceptDrag();

				//		Texture2D	newTexture = TextureCache.LoadTexture(DragAndDrop.paths[0]);

				//		if (newTexture.width < 1200)
				//		{
				//			this.ShowNotification("Image must be minimum " + 1200 + " width pixels.", 1F);
				//		}
				//		else
				//		{
				//			messageCenter.Clear(image_url);
				//			this.publisherAPI.SetKeyImage(version, language.languageCode, DragAndDrop.paths[0], type, NewMethod(image_url));
				//		}

				//		DragAndDrop.PrepareStartDrag();

				//		this.Repaint();
				//		return;
				//	}
				//}

				using (new EditorGUILayout.VerticalScope())
				{
					//GUILayout.Label(label + " (min. width " + 1200 + ")", GeneralStyles.Title1);
					EditorGUILayout.IntField("Id", artwork.id, EditorStyles.label);
					EditorGUILayout.TextField("Type", artwork.type, EditorStyles.label);
					EditorGUILayout.TextField("Reference", artwork.reference, EditorStyles.label);
					EditorGUILayout.TextField("Scaled", artwork.scaled, EditorStyles.label);
					EditorGUILayout.TextField("Uri", artwork.uri, EditorStyles.label);

				}
			}

			Message	message = this.messageCenter.Get(language.key_images.icon);

			if (message != null)
			{
				EditorGUILayout.HelpBox(message.message, message.type);

				Rect	r = GUILayoutUtility.GetLastRect();
				r.xMin = r.xMax - 18F;
				r.yMax = r.yMin + 18F;
				if (GUI.Button(r, "X") == true)
					this.messageCenter.Clear(language.key_images.icon);
			}
		}

		private string	ConvertHTML(string content)
		{
			StringBuilder	buffer = Utility.GetBuffer(content);

			for (int i = 0; i < buffer.Length; i++)
			{
				// We assume, HTML is valid.
				if (buffer[i] == '<' && i + 9 < buffer.Length) // a href="
				{
					if (buffer[i + 1] == 'a' &&
						buffer[i + 2] == ' ' &&
						buffer[i + 3] == 'h' &&
						buffer[i + 4] == 'r' &&
						buffer[i + 5] == 'e' &&
						buffer[i + 6] == 'f' &&
						buffer[i + 7] == '=' &&
						buffer[i + 8] == '"')
					{
						int		start = i;
						bool	backslashed = false;

						i += 9;

						// Skip URL.
						for (; i < buffer.Length; i++)
						{
							if (buffer[i] == '"' && backslashed == false)
								break;
							else if (buffer[i] == '\\')
								backslashed = !backslashed;
						}

						// Seek closing bracket.
						for (; i < buffer.Length; i++)
						{
							if (buffer[i] == '>')
								break;
						}

						buffer.Remove(start, i - start + 1);
						i = start;
					}
				}
			}

			buffer.Replace("</a>", string.Empty);
			buffer.Replace("\r", string.Empty);
			buffer.Replace("\n", string.Empty);
			buffer.Replace("<strong>", "<b>");
			buffer.Replace("</strong>", "</b>");
			buffer.Replace("<em>", "<i>");
			buffer.Replace("</em>", "</i>");
			buffer.Replace("<br>", "\n");

			return Utility.ReturnBuffer(buffer);
		}

		private int		CountCommas(string input)
		{
			if (input == null || input.Length == 0)
				return 0;

			int	result = 1;

			for (int i = 0, max = input.Length; i < max; ++i)
			{
				if (input[i] == ',')
					++result;
			}

			return result;
		}

		private IEnumerable	ExportMetrics()
		{
			List<Period>	periods = new List<Period>(this.selectedPeriodForExport + 1);
			int				i = this.selectedPeriodForExport;

			for (; i >= 0; --i)
				periods.Add(this.database.Periods[i]);

			periods.Reverse();

			i = 0;
			int	max = periods.Count;

			while (i < max)
			{
				Period	period = periods[i];
				int		missingCount = 0;

				if (period.HasSales == false)
				{
					++missingCount;

					if (this.publisherAPI.IsGettingSaleCounts(this.publisherAPI.Session.publisher, period.value) == false)
						this.database.RequestSaleCounts(this.publisherAPI, period, this.HandleErrorOnly);
				}

				if (period.HasDownloads == false)
				{
					++missingCount;

					if (this.publisherAPI.IsGettingFreeDownloads(this.publisherAPI.Session.publisher, period.value) == false)
						this.database.RequestFreeDownloads(this.publisherAPI, period, this.HandleErrorOnly);
				}

				if (missingCount > 0)
				{
					Utility.AsyncProgressBarDisplay("Fetching " + period.name + " (" + (i + i + 2 - missingCount) + " / " + (max + max) + ")", (i + i + 2 - missingCount) / (float)(max + max));

					yield return null;
					continue;
				}

				++i;
			}

			this.ExportPeriodsToClipboard(periods);
			this.Repaint();

			this.exportRoutine = null;
			Utility.AsyncProgressBarClear();
		}

		private void	ExportPeriodsToClipboard(IEnumerable<Period> periods)
		{
			StringBuilder	buffer = Utility.GetBuffer();

			this.exporters[this.selectedExporter].ExportPeriod(buffer, periods, this.selectedAssetForExport == 0 ? null : this.assetsAsLabel[this.selectedAssetForExport]);

			EditorGUIUtility.systemCopyBuffer = Utility.ReturnBuffer(buffer);
			this.ShowNotification(new GUIContent("Metrics copied to clipboard."));
		}

		/// <summary>Returns false if the response is invalid and feeds the error to the user.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		private bool	CheckRequestResponse<T>(RequestResponse<T> response)
		{
			if (response.ok == false)
			{
				this.ShowNotification(new GUIContent(response.error));
				Debug.LogError(response.error);
				return false;
			}

			return true;
		}

		private void	HandleErrorOnly<T>(RequestResponse<T> response)
		{
			this.CheckRequestResponse(response);
			this.Repaint();
		}

		private AutoEditValue<T>	Edit<T>(Rect r, int id, T input)
		{
			object	a;

			if (this.modifiedValues.TryGetValue(id, out a) == false)
				a = new AutoEditValue<T>(this.modifiedValues, id, input);

			AutoEditValue<T>	aev = a as AutoEditValue<T>;

			aev.isLayout = false;
			aev.layoutRect = r;

			return aev;
		}

		private AutoEditValue<T>	Edit<T>(int id, T input)
		{
			object	a;

			if (this.modifiedValues.TryGetValue(id, out a) == false)
				a = new AutoEditValue<T>(this.modifiedValues, id, input);

			AutoEditValue<T>	aev = a as AutoEditValue<T>;

			aev.isLayout = true;

			return aev;
		}

		private int	CompareVersion(string a, string b)
		{
			string[]	aParts = a.Split('.', 'a', 'b', 'f', 'r', 'p', 'x');
			string[]	bParts = b.Split('.', 'a', 'b', 'f', 'r', 'p', 'x');
			int			max = aParts.Length > bParts.Length ? bParts.Length : aParts.Length;

			try
			{
				if (max != 4)
				{
					for (int i = 0; i < max; ++i)
					{
						int	aNum = this.ParseInt(aParts[i]);
						int	bNum = this.ParseInt(bParts[i]);

						if (aNum != bNum)
							return bNum - aNum;
					}
				}
				else
				{
					char	aExt = this.GetVersionExtension(a);
					char	bExt = this.GetVersionExtension(b);

					// Default Major.Minor.Patch{Ext}Iteration
					for (int i = 0; i < 3; ++i)
					{
						int	aNum = this.ParseInt(aParts[i]);
						int	bNum = this.ParseInt(bParts[i]);

						if (aNum != bNum)
							return bNum - aNum;
					}

					if (aExt != bExt)
						return bExt - aExt;

					return this.ParseInt(bParts[3]) - this.ParseInt(aParts[3]);
				}
			}
			catch (Exception)
			{
				return 0;
			}

			return b.CompareTo(a);
		}

		private char	GetVersionExtension(string version)
		{
			for (int i = 0, max = NGPublisherWindow.Extensions.Length; i < max; ++i)
			{
				int	n = version.IndexOf(NGPublisherWindow.Extensions[i]);

				if (n != -1)
					return version[n];
			}

			return (char)0;
		}

		private int	ParseInt(string input, int i = 0)
		{
			int	n = 0;

			for (int max = input.Length; i < max; ++i)
			{
				char	c = input[i];

				if (c >= '0' && c <= '9')
					n = n * 10 + c - '0';
				else
					break;
			}

			return n;
		}

		protected virtual void	ShowButton(Rect r)
		{
			WindowTipsWindow.DefaultShowButton(r, this, NGPublisherWindow.Title, NGPublisherWindow.Tips);
		}
		
		void	IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
		{
			Utility.AddNGMenuItems(menu,
								   NGAssemblyInfo.Name,
								   NGAssemblyInfo.WikiURL);
		}
	}
}
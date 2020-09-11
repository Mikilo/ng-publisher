using NGToolsStandalone_For_NGPublisherEditor;
using System;
using UnityEditor;
using UnityEngine;

namespace NGPublisher
{
	public class SubmissionWindow : EditorWindow
	{
		public const string	Title = "Submitting";
		public const string	AutoPublishKeyPref = "SubmissionWindow_Comments";

		private Action<bool, string>	onPublish;
		private bool					autoPublish;
		private string					comments;
		private string					assetName;

		public static void	Start(string assetName, Action<bool, string> onPublish)
		{
			SubmissionWindow[]	windows = Resources.FindObjectsOfTypeAll<SubmissionWindow>();

			for (int i = 0, max = windows.Length; i < max; ++i)
				windows[i].Close();

			SubmissionWindow	window = EditorWindow.GetWindow<SubmissionWindow>(true, SubmissionWindow.Title + ' ' + assetName, true);

			window.onPublish = onPublish;
			window.assetName = assetName;
			window.Show();
		}

		protected virtual void	OnEnable()
		{
			this.autoPublish = NGEditorPrefs.GetBool(SubmissionWindow.AutoPublishKeyPref, this.autoPublish);

			EditorApplication.delayCall += () =>
			{
				this.comments = NGEditorPrefs.GetString(SubmissionWindow.AutoPublishKeyPref + this.assetName, this.comments);
				this.Repaint();
			};
		}

		protected virtual void	OnDisable()
		{
			NGEditorPrefs.SetBool(SubmissionWindow.AutoPublishKeyPref, this.autoPublish);
			NGEditorPrefs.SetString(SubmissionWindow.AutoPublishKeyPref + this.assetName, this.comments);
		}

		protected virtual void	OnGUI()
		{
			using (LabelWidthRestorer.Get(this.position.width))
			{
				EditorGUILayout.PrefixLabel("Submit package for approval?");
			}
			this.comments = EditorGUILayout.TextArea(this.comments, GUILayoutOptionPool.ExpandHeightTrue);
			this.autoPublish = EditorGUILayout.Toggle("Auto Publish", this.autoPublish);

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Submit") == true)
				{
					onPublish(this.autoPublish, this.comments);
					this.Close();
				}
			}
		}

		protected virtual void	Update()
		{
			if (EditorApplication.isCompiling == true)
				this.Close();
		}
	}
}
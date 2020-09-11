using NGToolsStandalone_For_NGPublisher;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NGPublisher
{
	using NGToolsStandalone_For_NGPublisherEditor;

	public class VouchersWindow : EditorWindow
	{
		public const string	Title = "Vouchers";

		private static GUIStyle	miniTextField;

		private PublisherDatabase	database;
		private IPublisherAPI		api;
		private Package				package;

		private Voucher[]	vouchers;
		private Vector2		scrollPosition;
		private string		filterVoucherCode;

		public static void	Open(PublisherDatabase database, IPublisherAPI api, Package package)
		{
			Utility.OpenWindow<VouchersWindow>(true, package.name, true, null, w =>
			{
				List<Voucher>	vouchers = new List<Voucher>();
				Voucher[]		allVouchers = database.Vouchers;

				if (allVouchers != null)
				{
					for (int i = 0, max = allVouchers.Length; i < max; ++i)
					{
						Voucher	voucher = allVouchers[i];

						if (voucher.package == package.name)
							vouchers.Add(voucher);
					}
				}

				w.database = database;
				w.api = api;
				w.package = package;

				w.vouchers = vouchers.ToArray();
			});
		}

		protected virtual void	OnGUI()
		{
			if (VouchersWindow.miniTextField == null)
			{
				VouchersWindow.miniTextField = new GUIStyle(EditorStyles.textField);
				VouchersWindow.miniTextField.fontSize = 9;
			}

			using (var scroll = new EditorGUILayout.ScrollViewScope(this.scrollPosition))
			{
				this.scrollPosition = scroll.scrollPosition;

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUI.BeginChangeCheck();
					this.filterVoucherCode = EditorGUILayout.TextField(this.filterVoucherCode, UnityStyles.ToolbarSearchTextField, GUILayoutOptionPool.Width(155F));
					if (EditorGUI.EndChangeCheck() == true)
					{
					}

					Rect	r = GUILayoutUtility.GetLastRect();

					if (GUILayout.Button("", UnityStyles.ToolbarSearchCancelButton, GUILayoutOptionPool.Width(16F)) == true)
					{
						this.filterVoucherCode = string.Empty;
						GUI.FocusControl(null);
					}

					if (string.IsNullOrEmpty(this.filterVoucherCode) == true)
					{
						r.y -= 2F;
						r.xMin += 15F;
						GUI.enabled = false;
						EditorGUI.LabelField(r, "Voucher Code", EditorStyles.miniBoldLabel);
						GUI.enabled = true;
					}

					bool	isCreatingVoucher = this.api.IsCreatingVoucher(this.api.Session.publisher, this.package.id);

					using (new EditorGUI.DisabledScope(isCreatingVoucher))
					{
						if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayoutOptionPool.Width(50F)) == true)
						{
							this.database.CreateVoucher(this.api, this.package, response =>
							{
								if (response.ok == false)
									EditorUtility.DisplayDialog(NGAssemblyInfo.Name, response.error, "OK");
								else
								{
									EditorUtility.DisplayDialog(NGAssemblyInfo.Name, "Voucher \"" + response.result.voucherCode + "\" created.", "OK");
									Debug.Log("Voucher \"" + response.result.voucherCode + "\" created.");
								}

								this.Repaint();
							});
						}

						if (isCreatingVoucher == true)
						{
							GUI.DrawTexture(GUILayoutUtility.GetLastRect(), GeneralStyles.StatusWheel.image, ScaleMode.ScaleToFit);
							this.Repaint();
						}
					}

					EditorGUILayout.LabelField("Issued By", EditorStyles.boldLabel, GUILayoutOptionPool.Width(130F));
					EditorGUILayout.LabelField("Issued Date", EditorStyles.boldLabel, GUILayoutOptionPool.Width(90F));
					EditorGUILayout.LabelField("Invoice", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Redeemed Date", EditorStyles.boldLabel, GUILayoutOptionPool.Width(110F));
				}

				for (int i = 0, max = this.vouchers.Length; i < max; ++i)
				{
					Voucher	voucher = this.vouchers[i];

					if (string.IsNullOrEmpty(this.filterVoucherCode) == false &&
						voucher.voucherCode.FastContainsInsensitive(this.filterVoucherCode) == false)
					{
						continue;
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.TextField(voucher.voucherCode, VouchersWindow.miniTextField, GUILayoutOptionPool.Width(220F));
						EditorGUILayout.TextField(voucher.issuedBy, GUILayoutOptionPool.Width(130F));
						EditorGUILayout.TextField(voucher.issuedDate, GUILayoutOptionPool.Width(90F));
						EditorGUILayout.TextField(voucher.invoice);
						EditorGUILayout.TextField(voucher.redeemedDate, GUILayoutOptionPool.Width(110F));
					}
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
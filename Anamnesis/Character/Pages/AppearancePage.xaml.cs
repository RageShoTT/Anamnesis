﻿// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Pages
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Character.Dialogs;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Character.Views;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using Serilog;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AppearancePage : UserControl
	{
		private static CharacterFile.SaveModes lastSaveMode = CharacterFile.SaveModes.All;
		private static CharacterFile.SaveModes lastLoadMode = CharacterFile.SaveModes.All;

		public AppearancePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GposeService GPoseService => GposeService.Instance;
		public ActorViewModel? Actor { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!this.IsVisible)
				return;

			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private void OnClearClicked(object sender, RoutedEventArgs e)
		{
			this.Actor?.Equipment?.Arms?.Clear();
			this.Actor?.Equipment?.Chest?.Clear();
			this.Actor?.Equipment?.Ear?.Clear();
			this.Actor?.Equipment?.Feet?.Clear();
			this.Actor?.Equipment?.Head?.Clear();
			this.Actor?.Equipment?.Legs?.Clear();
			this.Actor?.Equipment?.LFinger?.Clear();
			this.Actor?.Equipment?.Neck?.Clear();
			this.Actor?.Equipment?.RFinger?.Clear();
			this.Actor?.Equipment?.Wrist?.Clear();

			this.Actor?.ModelObject?.Weapons?.Hide();
			this.Actor?.ModelObject?.Weapons?.SubModel?.Hide();
		}

		private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
		{
			this.Actor?.Equipment?.Ear?.Clear();
			this.Actor?.Equipment?.Head?.Clear();
			this.Actor?.Equipment?.LFinger?.Clear();
			this.Actor?.Equipment?.Neck?.Clear();
			this.Actor?.Equipment?.RFinger?.Clear();
			this.Actor?.Equipment?.Wrist?.Clear();

			this.Actor?.Equipment?.Arms?.Equip(ItemUtility.NpcBodyItem);
			this.Actor?.Equipment?.Chest?.Equip(ItemUtility.NpcBodyItem);
			this.Actor?.Equipment?.Legs?.Equip(ItemUtility.NpcBodyItem);
			this.Actor?.Equipment?.Feet?.Equip(ItemUtility.NpcBodyItem);
		}

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.Load(false);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load appearance");
			}
		}

		private async void OnAdvLoadClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.Load(true);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load appearance");
			}
		}

		private void OnLoadNpcClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<NpcSelector, INpcResident>(null, (v) => { this.ApplyNpc(v); });
		}

		private async void ApplyNpc(INpcResident? npc)
		{
			if (this.Actor == null || npc == null)
				return;

			if (npc.Appearance == null)
				return;

			CharacterFile apFile = npc.Appearance.ToFile();
			await apFile.Apply(this.Actor, CharacterFile.SaveModes.All);
		}

		private async Task Load(bool advanced)
		{
			if (this.Actor == null)
				return;

			OpenResult result = await FileService.Open<LegacyEquipmentSetFile, LegacyCharacterFile, DatCharacterFile, CharacterFile>();

			if (result.File == null)
				return;

			if (result.File is LegacyCharacterFile legacyAllFile)
				result.File = legacyAllFile.Upgrade();

			if (result.File is LegacyEquipmentSetFile legacyEquipmentFile)
				result.File = legacyEquipmentFile.Upgrade();

			if (result.File is DatCharacterFile datFile)
				result.File = datFile.Upgrade();

			if (result.File is CharacterFile apFile)
			{
				CharacterFile.SaveModes mode = CharacterFile.SaveModes.All;

				if (advanced)
				{
					CharacterFile.SaveModes newmode = await ViewService.ShowDialog<AppearanceModeSelectorDialog, CharacterFile.SaveModes>("Load Character...", lastLoadMode);

					if (newmode == CharacterFile.SaveModes.None)
						return;

					lastLoadMode = newmode;
					mode = lastLoadMode;
				}

				await apFile.Apply(this.Actor, mode);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.Save(false);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save appearance");
			}
		}

		private async void OnAdvSaveClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.Save(true);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save appearance");
			}
		}

		private async Task Save(bool advanced)
		{
			if (this.Actor == null)
				return;

			CharacterFile.SaveModes mode = CharacterFile.SaveModes.All;

			if (advanced)
			{
				CharacterFile.SaveModes newMode = await ViewService.ShowDialog<AppearanceModeSelectorDialog, CharacterFile.SaveModes>("Save Character...", lastSaveMode);

				if (newMode == CharacterFile.SaveModes.None)
					return;

				mode = newMode;
			}

			CharacterFile file = new CharacterFile();
			file.WriteToFile(this.Actor, mode);

			await FileService.Save(file);
		}

		private void OnActorChanged(ActorViewModel? actor)
		{
			this.Actor = actor;
			bool hasValidSelection = actor != null && (actor.ObjectKind == ActorTypes.Player || actor.ObjectKind == ActorTypes.BattleNpc || actor.ObjectKind == ActorTypes.EventNpc);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}
	}
}

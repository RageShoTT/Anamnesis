﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Navigation;
using Anamnesis.Services;
using Anamnesis.Updater;
using Anamnesis.Views;
using PropertyChanged;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf.Extensions;
using System;

/// <summary>
/// Interaction logic for Navigation.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class NavigationPanel : PanelBase
{
	private bool expanded = true;

	public NavigationPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.NavigationContextMenu.DataContext = this;
	}

	public bool Expanded
	{
		get => this.expanded;
		set
		{
			this.expanded = value;
			this.Host.TitleKey = value ? "        Anamnesis" : string.Empty;
		}
	}

	public bool IsAccordian { get; set; } = true;

	public override Point GetSubPanelDockOffset()
	{
		return new Point(this.BackgroundBorder.ActualWidth + 12, this.TopBar.ActualHeight + 12);
	}

	private void OnIconMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.DragMove();
	}

	private void OnActorEntryExpanded(object sender, RoutedEventArgs e)
	{
		if (!this.IsAccordian)
		{
		}
		else
		{
			for (int i = 0; i < this.PinnedActorList.Items.Count; i++)
			{
				DependencyObject? container = this.PinnedActorList.ItemContainerGenerator.ContainerFromIndex(i);
				ActorEntry? entry = container?.FindChild<ActorEntry>();

				if (entry == null)
					continue;

				entry.IsExpanded = sender == entry;
			}
		}
	}

	private void OnTitlebarContextButtonClicked(object sender, RoutedEventArgs e)
	{
		this.NavigationContextMenu.IsOpen = true;
	}
}

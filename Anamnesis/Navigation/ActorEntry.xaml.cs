﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Anamnesis.Extensions;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for ActorEntry.xaml.
/// </summary>
public partial class ActorEntry : UserControl
{
	public static readonly IBind<bool> IsExpandedDp = Binder.Register<bool, ActorEntry>(nameof(IsExpanded), OnIsExpandedChanged);
	public static readonly IBind<bool> ShowTextDp = Binder.Register<bool, ActorEntry>(nameof(ShowText), OnShowTextChanged);

	public static readonly RoutedEvent? CollapsedEvent;
	public static readonly RoutedEvent? ExpandedEvent;

	public ActorEntry()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.ContentScale.ScaleY = this.IsExpanded ? 1 : 0;
	}

	public event RoutedEventHandler? Collapsed;
	public event RoutedEventHandler? Expanded;

	public PinnedActor? Actor => this.DataContext as PinnedActor;

	public bool IsExpanded
	{
		get => IsExpandedDp.Get(this);
		set => IsExpandedDp.Set(this, value);
	}

	public bool ShowText
	{
		get => ShowTextDp.Get(this);
		set => ShowTextDp.Set(this, value);
	}

	public NavigationService.Request AppearanceNav => new NavigationService.Request("Appearance", this.Actor?.Id);
	public NavigationService.Request ActionNav => new NavigationService.Request("Action", this.Actor?.Id);
	public NavigationService.Request PoseNav => new NavigationService.Request("Pose", this.Actor?.Id);

	private static void OnIsExpandedChanged(ActorEntry sender, bool value)
	{
		if (value)
		{
			sender.Expanded?.Invoke(sender, new());
			sender.ExpanderContent.IsHitTestVisible = true;
			sender.ExpanderContent.Visibility = Visibility.Visible;
			sender.ExpanderContent.BeginStoryboard("ExpandStoryboard");
		}
		else
		{
			sender.Collapsed?.Invoke(sender, new());
			sender.ExpanderContent.IsHitTestVisible = false;
			sender.ExpanderContent.BeginStoryboard("CollapseStoryboard");
		}
	}

	private static void OnShowTextChanged(ActorEntry sender, bool value)
	{
		if (value)
		{
			sender.BeginStoryboard("ShowLabel");
		}
		else
		{
			sender.BeginStoryboard("HideLabel");
		}
	}

	private void OnUnpinActorClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		TargetService.UnpinActor(this.Actor);
	}

	private void OnTargetActorClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		TargetService.SetPlayerTarget(this.Actor);
	}

	private void OnActorPinPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle)
		{
			this.OnUnpinActorClicked(sender, new RoutedEventArgs());
		}
	}

	private void OnCollapseStoryboardCompleted(object sender, EventArgs e)
	{
		this.ExpanderContent.Visibility = Visibility.Collapsed;
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (!this.ShowText)
		{
			this.BeginStoryboard("ShowLabel");
		}
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (!this.ShowText)
		{
			this.BeginStoryboard("HideLabel");
		}
	}
}

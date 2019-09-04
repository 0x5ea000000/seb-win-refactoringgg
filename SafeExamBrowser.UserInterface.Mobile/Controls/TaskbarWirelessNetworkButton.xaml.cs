﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class TaskbarWirelessNetworkButton : UserControl
	{
		private IWirelessNetwork network;

		public event EventHandler NetworkSelected;

		public TaskbarWirelessNetworkButton(IWirelessNetwork network)
		{
			this.network = network;

			InitializeComponent();
			InitializeNetworkButton();
		}

		private void InitializeNetworkButton()
		{
			Button.Click += (o, args) => NetworkSelected?.Invoke(this, EventArgs.Empty);
			IsCurrentTextBlock.Visibility = network.Status == WirelessNetworkStatus.Connected ? Visibility.Visible : Visibility.Hidden;
			NetworkNameTextBlock.Text = network.Name;
			SignalStrengthTextBlock.Text = $"{network.SignalStrength}%";
		}
	}
}

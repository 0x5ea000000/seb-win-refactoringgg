﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// The control for a notification which can be loaded into the shell.
	/// </summary>
	public interface INotificationControl
	{
		/// <summary>
		/// Event fired when the user clicked on the notification control.
		/// </summary>
		event NotificationControlClickedEventHandler Clicked;
	}
}

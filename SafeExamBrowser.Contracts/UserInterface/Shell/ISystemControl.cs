﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Shell
{
	/// <summary>
	/// The control of a system component which can be loaded into the shell.
	/// </summary>
	public interface ISystemControl
	{
		/// <summary>
		/// Closes the control and / or any associated user interface elements.
		/// </summary>
		void Close();

		/// <summary>
		/// Sets the tooltip text of the system control.
		/// </summary>
		void SetTooltip(string text);
	}
}

﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.UserInterface.Contracts.Shell.Events
{
	/// <summary>
	/// Indicates that a keyboard layout has been selected by the user.
	/// </summary>
	public delegate void KeyboardLayoutSelectedEventHandler(Guid id);
}

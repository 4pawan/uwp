﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Unicorn.UWP.View
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ExtendedSplash : Page
	{
		internal Rect splashImageRect; // Rect to store splash screen image coordinates.
		private SplashScreen splash; // Variable to hold the splash screen object.
		internal bool dismissed = false; // Variable to track splash screen dismissal status.
		internal Frame rootFrame;

		public ExtendedSplash()
		{
			this.InitializeComponent();
		}



		public ExtendedSplash(SplashScreen splashscreen, bool loadState)
		{
			InitializeComponent();

			// Listen for window resize events to reposition the extended splash screen image accordingly.
			// This ensures that the extended splash screen formats properly in response to window resizing.
			Window.Current.SizeChanged += new WindowSizeChangedEventHandler(ExtendedSplash_OnResize);

			splash = splashscreen;
			if (splash != null)
			{
				// Register an event handler to be executed when the splash screen has been dismissed.
				splash.Dismissed += new TypedEventHandler<SplashScreen, Object>(DismissedEventHandler);

				// Retrieve the window coordinates of the splash screen image.
				splashImageRect = splash.ImageLocation;
				PositionImage();

				// If applicable, include a method for positioning a progress control.
				PositionRing();
			}

			// Create a Frame to act as the navigation context
			rootFrame = new Frame();
		}



	}
}

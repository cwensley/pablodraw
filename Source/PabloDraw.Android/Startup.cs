using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;

namespace PabloDraw.Android
{
	[Activity ( Label = "PabloDraw", MainLauncher = true)]
	public class Startup : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			var layout = new LinearLayout(this){
				Orientation = Orientation.Vertical
			};
			
			//layout.LayoutParameters = new LayoutParams()
			//layout.LayoutParameters.Width = ViewGroup.LayoutParams.FillParent;
			//layout.LayoutParameters.Height = ViewGroup.LayoutParams.FillParent;
			
			
			var button = new Button(this) { Text = "Click me you fools bools!" };
			button.SetWidth(ViewGroup.LayoutParams.FillParent);
			button.SetHeight(ViewGroup.LayoutParams.WrapContent);
			
			layout.AddView (button);
			
			var image = BitmapFactory.DecodeResource (this.Resources, Resource.Drawable.Icon);
			var imageView = new ImageView(this);
			imageView.SetMaxWidth (24);
			imageView.SetMaxHeight (24);
			imageView.SetImageBitmap (image);
			layout.AddView (imageView);
			
			this.SetContentView (layout);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};
		}
	}
}



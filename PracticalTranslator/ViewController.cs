using System;
using UIKit;
using Foundation;
using System.Threading.Tasks;

namespace PracticalTranslator
{
	public partial class ViewController : UIViewController
	{
		Location location = Location.Japanese;

		UIImage SpeakerImage = new UIImage("speaker.png"); 
		UIImage MicImage = new UIImage("mic.png");
		UIColor EnglishMoodeColor = UIColor.FromRGB(0.04f, 0.26f, 0.13f);
		UIColor JapaneseMoodeColor = UIColor.FromRGB(0.32f, 0.0f, 0.20f);
		Recognizer recognizer = new Recognizer();
		Translator translator = new Translator();
		Speech speech = new Speech();

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			NSNotificationCenter.DefaultCenter.AddObserver((Foundation.NSString)"UIDeviceOrientationDidChangeNotification", DidRotate);

			recognizer.Recognition += (buffer) => // マイクからの入力が解析できしだい、テキストに表示する
			{
				textField.Text = buffer;
			};
			speech.OnFinsh += () => // 読み上げが終わったら入力モードに移行する
			{
				image.Image = MicImage;
				recognizer.StartRecording();
			};
			recognizer.StartRecording(); // 起動時は、入力モード
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			NSNotificationCenter.DefaultCenter.RemoveObserver(this);
		}

		// 上下の回転を有効にする
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			return UIInterfaceOrientationMask.All;
		}

		// 回転時のイベント
		public async void DidRotate(NSNotification notification)
		{
			var newLocation = (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.PortraitUpsideDown) ? Location.English : Location.Japanese;

			if (location != newLocation)
			{
				location = newLocation;
				if (recognizer.InputBuffer == "")
				{
					textField.Text = "";
				}
				recognizer.StopRecording();

				recognizer.SetLocation(location);
				translator.SetLocation(location);
				speech.SetLocation(location);

				// モードに応じた画面に変更する
				textView.BackgroundColor = (location == Location.English) ? EnglishMoodeColor : JapaneseMoodeColor;

				image.Image = SpeakerImage;
				var str = await translator.Conversion(textField.Text);
				speech.Start(str);
				textField.Text = str;
			}
		}
	}
}

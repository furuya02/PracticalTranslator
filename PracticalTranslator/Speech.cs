using System;
using AVFoundation;

namespace PracticalTranslator
{
	public class Speech
	{
		String fm;

		public delegate void FinishHandler();
		public FinishHandler OnFinsh = null;

		public Speech()
		{
			SetLocation(Location.Japanese);
		}

		public void SetLocation(Location location)
		{
			fm = (location == Location.English) ? "en" : "ja";
		}

		public void Start(string str)
		{
			var audioSession = AVAudioSession.SharedInstance();
			audioSession.SetCategory(AVAudioSessionCategory.Ambient);

			var speechSynthesizer = new AVSpeechSynthesizer();
			speechSynthesizer.DidFinishSpeechUtterance += speechSynthesizer_StoppedSpeechUtterance;
			var speechUtterance = new AVSpeechUtterance(str)
			{
				Voice = AVSpeechSynthesisVoice.FromLanguage(fm),
				Volume = 1.0f,
			};
			speechSynthesizer.SpeakUtterance(speechUtterance);
		}

		void speechSynthesizer_StoppedSpeechUtterance(object sender, AVSpeechSynthesizerUteranceEventArgs e)
		{
			if (OnFinsh != null)
			{
				OnFinsh();
			}
		}
	}
}

using System;
using AVFoundation;
using Foundation;
using Speech;

namespace PracticalTranslator
{
	public class Recognizer
	{

		public delegate void RecognitionHandler(string msg); 
		public RecognitionHandler Recognition = null;


		SFSpeechRecognizer speechRecognizerJp = new SFSpeechRecognizer(new NSLocale("ja-JP"));
		SFSpeechRecognizer speechRecognizerEn = new SFSpeechRecognizer(new NSLocale("en_US"));
		SFSpeechRecognizer speechRecognizer = null;

		readonly AVAudioEngine audioEngine = new AVAudioEngine(); // マイク

		SFSpeechAudioBufferRecognitionRequest recognitionRequest;
		SFSpeechRecognitionTask recognitionTask;
		Location location = Location.English;

		public String InputBuffer = "";
		public bool isReading = false;


		public Recognizer()
		{
			SetLocation(Location.Japanese);
		}

		public void SetLocation(Location location)
		{
			if (this.location  == location)
			{
				return;
			}

			this.location = location;
			if (isReading)
			{
				StopRecording();
				speechRecognizer = (location == Location.English) ? speechRecognizerEn : speechRecognizerJp;
				StartRecording();
			}
			else
			{
				speechRecognizer = (location == Location.English) ? speechRecognizerEn : speechRecognizerJp;
			}
		}

		public void StartRecording()
		{
			isReading = true;
			InputBuffer = "";

			recognitionTask?.Cancel();
			recognitionTask = null;


			var audioSession = AVAudioSession.SharedInstance();
			NSError err;
			err = audioSession.SetCategory(AVAudioSessionCategory.Record);
			audioSession.SetMode(AVAudioSession.ModeMeasurement, out err);
			err = audioSession.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);

			recognitionRequest = new SFSpeechAudioBufferRecognitionRequest
			{
				ShouldReportPartialResults = true
			};

			var inputNode = audioEngine.InputNode;
			if (inputNode == null)
			{
				throw new InvalidProgramException("Audio engine has no input node");
			}

			recognitionTask = speechRecognizer.GetRecognitionTask(recognitionRequest, (result, error) =>
			{
				var isFinal = false;
				if (result != null)
				{
					InputBuffer = result.BestTranscription.FormattedString;
					if (Recognition != null)
					{
						Recognition(InputBuffer);
					}
					isFinal = result.Final;
				}

				if (error != null || isFinal)
				{
					audioEngine.Stop();
					inputNode.RemoveTapOnBus(0);
					recognitionRequest = null;
					recognitionTask = null;
				}
			});

			var recordingFormat = inputNode.GetBusOutputFormat(0);
			inputNode.InstallTapOnBus(0, 1024, recordingFormat, (buffer, when) =>
			{
				recognitionRequest?.Append(buffer);
			});

			// マイクの利用開始
			this.audioEngine.Prepare();
			this.audioEngine.StartAndReturnError(out err);
		}

		public void StopRecording()
		{
			// マイクの停止
			audioEngine.Stop();
			recognitionRequest?.EndAudio();

			isReading = false;
		}
	}
}

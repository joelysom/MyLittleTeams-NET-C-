using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MeuApp
{
    internal sealed class ChatMicrophoneRecorder : IDisposable
    {
        private readonly string _alias = "choasrec_" + Guid.NewGuid().ToString("N");
        private readonly string _outputFilePath;
        private bool _deviceOpen;
        private bool _isRecording;
        private bool _saved;

        public ChatMicrophoneRecorder(string outputFilePath)
        {
            _outputFilePath = outputFilePath;
        }

        public bool IsRecording => _isRecording;

        public string OutputFilePath => _outputFilePath;

        public void Start()
        {
            if (_isRecording)
            {
                return;
            }

            EnsureOutputDirectory();
            SendCommand($"open new type waveaudio alias {_alias}");
            _deviceOpen = true;
            SendCommand($"record {_alias}");
            _isRecording = true;
            _saved = false;
        }

        public void StopAndSave()
        {
            if (!_deviceOpen)
            {
                return;
            }

            if (_isRecording)
            {
                SendCommand($"stop {_alias}");
            }

            SendCommand($"save {_alias} \"{_outputFilePath}\"");
            _saved = true;
            _isRecording = false;
            CloseDevice();
        }

        public void Cancel()
        {
            try
            {
                if (_deviceOpen)
                {
                    if (_isRecording)
                    {
                        SendCommand($"stop {_alias}");
                    }

                    CloseDevice();
                }
            }
            finally
            {
                _isRecording = false;
                if (!_saved && File.Exists(_outputFilePath))
                {
                    try
                    {
                        File.Delete(_outputFilePath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_saved)
            {
                try
                {
                    CloseDevice();
                }
                catch
                {
                }

                return;
            }

            Cancel();
        }

        private void EnsureOutputDirectory()
        {
            var directory = Path.GetDirectoryName(_outputFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void CloseDevice()
        {
            if (!_deviceOpen)
            {
                return;
            }

            try
            {
                SendCommand($"close {_alias}");
            }
            finally
            {
                _deviceOpen = false;
                _isRecording = false;
            }
        }

        private static void SendCommand(string command)
        {
            var buffer = new StringBuilder(260);
            var errorCode = mciSendString(command, buffer, buffer.Capacity, IntPtr.Zero);
            if (errorCode == 0)
            {
                return;
            }

            var errorBuffer = new StringBuilder(260);
            mciGetErrorString(errorCode, errorBuffer, errorBuffer.Capacity);
            var errorMessage = errorBuffer.ToString();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorMessage)
                ? $"Falha ao acessar o microfone (MCI {errorCode})."
                : errorMessage);
        }

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr winHandle);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern bool mciGetErrorString(int errorCode, StringBuilder errorText, int errorTextSize);
    }
}

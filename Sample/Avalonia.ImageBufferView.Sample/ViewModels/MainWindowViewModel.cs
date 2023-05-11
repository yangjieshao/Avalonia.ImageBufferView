using System;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.ImageBufferView.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// 当前播放的图片
        /// </summary>
        [Reactive]
        public byte[]? ImageBuffer { get; private set; }

        /// <summary>
        /// 待播放图片流缓存
        /// </summary>

        private readonly List<byte[]> _buffers = new();

        private CancellationTokenSource? _cancellationTokenSource;

        public void Start()
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            LoadImage(_cancellationTokenSource.Token);
        }

        private void LoadImage(CancellationToken token)
        {
            Task.Run(() =>
            {
                if (!Directory.Exists("Images"))
                {
                    return;
                }
                var files = new DirectoryInfo("Images").GetFiles("*.jpeg");

                // Ready buffers
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var buffer = File.ReadAllBytes(file.FullName);
                    _buffers.Add(buffer);
                }

                if (_buffers.Count == 0)
                {
                    return;
                }
                while (!token.IsCancellationRequested)
                {
                    foreach (var buffer in _buffers.TakeWhile(buffer => !token.IsCancellationRequested))
                    {
                        ImageBuffer = buffer;
                        try
                        {
                            Task.Delay(1, token).Wait(token);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }, token);
        }
    }
}
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System;

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
        /// 代播放图片流缓存
        /// </summary>

        private List<byte[]> buffers = new List<byte[]>();

        private CancellationTokenSource? _cancellationTokenSource;

        public void Start()
        {
            if (_cancellationTokenSource is { })
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
                    try
                    {
                        var buffer = File.ReadAllBytes(file.FullName);
                        buffers.Add(buffer);
                    }
                    catch
                    {
                    }
                }

                if (buffers.Count == 0)
                {
                    return;
                }
                while (!token.IsCancellationRequested)
                {
                    foreach (var buffer in buffers)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        ImageBuffer = buffer;

                        Task.Delay(1).Wait();
                    }
                }
            }, token);
        }
    }
}
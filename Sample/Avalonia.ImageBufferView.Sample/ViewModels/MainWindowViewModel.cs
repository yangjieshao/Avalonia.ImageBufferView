using ReactiveUI;
using System;
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
        public ArraySegment<byte>? ImageBuffer
        {
            get => _ImageBuffer;
            set => this.RaiseAndSetIfChanged(ref _ImageBuffer, value);
        }
        private ArraySegment<byte>? _ImageBuffer;

        /// <summary>
        /// 待播放图片流缓存
        /// </summary>

        private readonly List<ArraySegment<byte>> _buffers = new();

        private CancellationTokenSource? _cancellationTokenSource;

        public async void Start()
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            await LoadImage(_cancellationTokenSource.Token);
        }

        private async ValueTask LoadImage(CancellationToken token)
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
                    var buffer = await File.ReadAllBytesAsync(file.FullName, token);
                    _buffers.Add(buffer);
                }
                catch (Exception)
                {
                    // ignored
                }
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
                        await Task.Delay(1, token);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
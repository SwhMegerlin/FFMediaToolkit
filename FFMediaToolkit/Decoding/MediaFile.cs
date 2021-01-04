﻿namespace FFMediaToolkit.Decoding
{
    using System;
    using System.IO;
    using System.Linq;
    using FFMediaToolkit.Common;
    using FFMediaToolkit.Decoding.Internal;

    /// <summary>
    /// Represents a multimedia file.
    /// </summary>
    public class MediaFile : IDisposable
    {
        private readonly InputContainer container;
        private bool isDisposed;

        private unsafe MediaFile(InputContainer container, MediaOptions options)
        {
            this.container = container;

            var video = container.Decoders.Where(codec => codec.Info.Type == MediaType.Video);
            var audio = container.Decoders.Where(codec => codec.Info.Type == MediaType.Audio);

            if (video.Any())
            {
                Video = video.Select(codec => new VideoStream(codec, options)).ToArray();
            }

            if (audio.Any())
            {
                Audio = audio.Select(codec => new AudioStream(codec, options)).ToArray();
            }

            Info = new MediaInfo(container.Pointer);
        }

        /// <summary>
        /// Gets the video streams.
        /// </summary>
        public VideoStream[] Video { get; }

        /// <summary>
        /// Gets the audio streams.
        /// </summary>
        public AudioStream[] Audio { get; }

        /// <summary>
        /// Gets informations about the media container.
        /// </summary>
        public MediaInfo Info { get; }

        /// <summary>
        /// Opens a media file from the specified path with default settings.
        /// </summary>
        /// <param name="path">A path to the media file.</param>
        /// <returns>The opened <see cref="MediaFile"/>.</returns>
        public static MediaFile Open(string path) => Open(path, new MediaOptions());

        /// <summary>
        /// Opens a media file from the specified path.
        /// </summary>
        /// <param name="path">A path to the media file.</param>
        /// <param name="options">The decoder settings.</param>
        /// <returns>The opened <see cref="MediaFile"/>.</returns>
        public static MediaFile Open(string path, MediaOptions options)
        {
            try
            {
                var container = InputContainer.LoadFile(path, options);
                return new MediaFile(container, options);
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to open the media file", ex);
            }
        }

        /// <summary>
        /// Opens a media stream with default settings.
        /// </summary>
        /// <param name="stream">A stream of the multimedia file.</param>
        /// <returns>The opened <see cref="MediaFile"/>.</returns>
        public static MediaFile Open(Stream stream) => Open(stream, new MediaOptions());

        /// <summary>
        /// Opens a media stream.
        /// </summary>
        /// <param name="stream">A stream of the multimedia file.</param>
        /// <param name="options">The decoder settings.</param>
        /// <returns>The opened <see cref="MediaFile"/>.</returns>
        public static MediaFile Open(Stream stream, MediaOptions options)
        {
            try
            {
                var container = InputContainer.LoadStream(stream, options);
                return new MediaFile(container, options);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to open the media stream", ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            var video = Video.Cast<MediaStream>();
            var audio = Audio.Cast<MediaStream>();

            var streams = video.Concat(audio);

            foreach (var stream in streams)
                stream.Dispose();

            container.Dispose();

            isDisposed = true;
        }
    }
}